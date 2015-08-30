using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.S3D;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.DataTypes;
using Adjutant.Library.Controls;
using System.IO;

using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Adjutant.Library.Controls
{
    public partial class BSPViewer : UserControl
    {
        #region Init
        private CacheFile cache;
        private CacheFile.IndexItem tag;
        private scenario_structure_bsp sbsp;

        private bool isWorking = false;
        private Dictionary<scenario_structure_bsp.Cluster, Model3DGroup> clusterDic = new Dictionary<scenario_structure_bsp.Cluster, Model3DGroup>();
        private Dictionary<scenario_structure_bsp.InstancedGeometry, Model3DGroup> igDic = new Dictionary<scenario_structure_bsp.InstancedGeometry, Model3DGroup>();
        private List<Model3DGroup> sectList = new List<Model3DGroup>();
        private List<MaterialGroup> shaders = new List<MaterialGroup>();

        private PakFile pak;
        private PakFile.PakTag item;
        private Scene atpl;
        private Dictionary<int, Model3DGroup> atplDic = new Dictionary<int, Model3DGroup>();

        public ModelFormat DefaultModeFormat = ModelFormat.AMF;
        public float TextureScale = 1;

        public System.Drawing.Color RenderBackColor
        {
            get { return ehRenderer.BackColor; }
            set { ehRenderer.BackColor = value; }
        }

        public BSPViewer()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods

        #region Map
        public void LoadBSPTag(CacheFile Cache, CacheFile.IndexItem Tag, bool Force)
        {
            try
            {
                Clear();
                loadBspTag(Cache, Tag, false, Force);
            }
            catch (Exception ex)
            {
                renderer1.ClearViewport();
                Clear();
                renderer1.Stop("Loading failed: " + ex.Message);
                tvRegions.Nodes.Clear();
                this.Enabled = false;
            }
        }

        private void loadBspTag(CacheFile Cache, CacheFile.IndexItem Tag, bool Specular, bool Force)
        {
            if (!this.Enabled) this.Enabled = true;
            tvRegions.Nodes.Clear();
            if (renderer1.Running) renderer1.Stop("Loading...");
            Refresh();

            cache = Cache;
            tag = Tag;

            sbsp = DefinitionsManager.sbsp(cache, tag);
            sbsp.BSPName = Path.GetFileNameWithoutExtension(tag.Filename + "." + tag.ClassCode);
            sbsp.LoadRaw();

            isWorking = true;

            #region Build Tree
            TreeNode ClusterNode = new TreeNode("Clusters") { Checked = true };
            foreach(var clust in sbsp.Clusters)
            {
                if (clust.SectionIndex >= sbsp.ModelSections.Count) continue;
                if (sbsp.ModelSections[clust.SectionIndex].Submeshes.Count > 0)
                    ClusterNode.Nodes.Add(new TreeNode(sbsp.Clusters.IndexOf(clust).ToString("D3")) { Tag = clust, Checked = true });
            }
            if (ClusterNode.Nodes.Count > 0)
                tvRegions.Nodes.Add(ClusterNode);

            TreeNode IGnode = new TreeNode("Instances") { Checked = true };
            foreach (var IG in sbsp.GeomInstances)
            {
                if (IG.SectionIndex >= sbsp.ModelSections.Count) continue;
                if (sbsp.ModelSections[IG.SectionIndex].Submeshes.Count > 0)
                    IGnode.Nodes.Add(new TreeNode(IG.Name) { Tag = IG, Checked = true });
            }
            if (IGnode.Nodes.Count > 0)
                tvRegions.Nodes.Add(IGnode);

            tvRegions.Sort(); //much easier for looking through IGs
            #endregion

            isWorking = false;

            #region Load Stuff
            LoadShaders(false);
            LoadSections();

            foreach (var clust in sbsp.Clusters)
                AddCluster(clust, Force);

            foreach (var ig in sbsp.GeomInstances)
                AddGeomInstance(ig, Force);
            #endregion

            #region BoundingBox Stuff
            PerspectiveCamera camera = (PerspectiveCamera)renderer1.Viewport.Camera;

            var XBounds = new RealBounds(float.MaxValue, float.MinValue); 
            var YBounds = new RealBounds(float.MaxValue, float.MinValue);
            var ZBounds = new RealBounds(float.MaxValue, float.MinValue);

            #region Get Bounds
            foreach (var c in sbsp.Clusters)
            {
                if (c.SectionIndex >= sbsp.ModelSections.Count) continue;
                if (sbsp.ModelSections[c.SectionIndex].Submeshes.Count == 0) continue;

                if (c.XBounds.Min < XBounds.Min) XBounds.Min = c.XBounds.Min;
                if (c.YBounds.Min < YBounds.Min) YBounds.Min = c.YBounds.Min;
                if (c.ZBounds.Min < ZBounds.Min) ZBounds.Min = c.ZBounds.Min;

                if (c.XBounds.Max > XBounds.Max) XBounds.Max = c.XBounds.Max;
                if (c.YBounds.Max > YBounds.Max) YBounds.Max = c.YBounds.Max;
                if (c.ZBounds.Max > ZBounds.Max) ZBounds.Max = c.ZBounds.Max;
            }

            //foreach (var bb in sbsp.BoundingBoxes)
            //{
            //    if (bb.XBounds.Min < XBounds.Min) XBounds.Min = bb.XBounds.Min;
            //    if (bb.YBounds.Min < YBounds.Min) YBounds.Min = bb.YBounds.Min;
            //    if (bb.ZBounds.Min < ZBounds.Min) ZBounds.Min = bb.ZBounds.Min;

            //    if (bb.XBounds.Max > XBounds.Max) XBounds.Max = bb.XBounds.Max;
            //    if (bb.YBounds.Max > YBounds.Max) YBounds.Max = bb.YBounds.Max;
            //    if (bb.ZBounds.Max > ZBounds.Max) ZBounds.Max = bb.ZBounds.Max;
            //}
            #endregion

            double pythagoras3d = Math.Sqrt(
                Math.Pow(XBounds.Length, 2) +
                Math.Pow(YBounds.Length, 2) +
                Math.Pow(ZBounds.Length, 2));

            if (double.IsInfinity(pythagoras3d) || pythagoras3d == 0) //no clusters
            {
                XBounds = sbsp.XBounds;
                YBounds = sbsp.YBounds;
                ZBounds = sbsp.ZBounds;

                pythagoras3d = Math.Sqrt(
                Math.Pow(XBounds.Length, 2) +
                Math.Pow(YBounds.Length, 2) +
                Math.Pow(ZBounds.Length, 2));
            }

            if (XBounds.Length / 2 > (YBounds.Length)) //side view
            {
                var p = new Point3D(
                XBounds.MidPoint,
                YBounds.Max + pythagoras3d * 0.5,
                ZBounds.MidPoint);
                renderer1.MoveCamera(p, new Vector3D(0, 0, -2));
            }
            else //normal camera position
            {
                var p = new Point3D(
                XBounds.Max + pythagoras3d * 0.5,
                YBounds.MidPoint,
                ZBounds.MidPoint);
                renderer1.MoveCamera(p, new Vector3D(-1, 0, 0));
            }

            renderer1.CameraSpeed = Math.Ceiling(pythagoras3d * 3) / 1000;
            renderer1.MaxCameraSpeed = Math.Ceiling(pythagoras3d * 3) * 5 / 1000;
            renderer1.MaxPosition = new Point3D(
                sbsp.XBounds.Max + pythagoras3d * 2,
                sbsp.YBounds.Max + pythagoras3d * 2,
                sbsp.ZBounds.Max + pythagoras3d * 2);
            renderer1.MinPosition = new Point3D(
                sbsp.XBounds.Min - pythagoras3d * 2,
                sbsp.YBounds.Min - pythagoras3d * 2,
                sbsp.ZBounds.Min - pythagoras3d * 2);
            #endregion

            renderer1.Start();
            RenderSelected();
        }

        private void LoadShaders(bool spec)
        {
            var errMat = GetErrorMaterial();
            
            if (sbsp.Shaders.Count == 0)
            {
                var matGroup = new MaterialGroup();
                matGroup.Children.Add(errMat);
                shaders.Add(matGroup);
                return;
            }

            foreach (scenario_structure_bsp.Shader s in sbsp.Shaders)
            {
                #region Skip Unused
                bool found = false;
                foreach (var sec in sbsp.ModelSections)
                {
                    foreach (var sub in sec.Submeshes)
                    {
                        if (sub.ShaderIndex == sbsp.Shaders.IndexOf(s))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }

                if (!found)
                {
                    shaders.Add(null);
                    continue;
                }
                #endregion

                var matGroup = new MaterialGroup();
                try
                {
                    var rmshTag = cache.IndexItems.GetItemByID(s.tagID);
                    var rmsh = DefinitionsManager.rmsh(cache, rmshTag);

                    int mapIndex = 0;
                    if (cache.Version >= DefinitionSet.Halo3Beta && cache.Version <= DefinitionSet.HaloReachRetail)
                    {
                        var rmt2Tag = cache.IndexItems.GetItemByID(rmsh.Properties[0].TemplateTagID);
                        var rmt2 = DefinitionsManager.rmt2(cache, rmt2Tag);

                        for (int i = 0; i < rmt2.UsageBlocks.Count; i++)
                        {
                            if (rmt2.UsageBlocks[i].Usage.Contains("base_map") || rmt2.UsageBlocks[i].Usage == "foam_texture")
                                mapIndex = i;
                        }
                    }

                    var bitmTag = cache.IndexItems.GetItemByID(rmsh.Properties[0].ShaderMaps[mapIndex].BitmapTagID);

                    var image = BitmapExtractor.GetBitmapByTag(cache, bitmTag, 0, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    if (TextureScale < 1) image = new Bitmap(image, new Size((int)(image.Width * TextureScale), (int)(image.Height * TextureScale)));

                    if (image == null)
                    {
                        matGroup.Children.Add(errMat);
                        shaders.Add(matGroup);
                        continue;
                    }

                    int tileIndex = rmsh.Properties[0].ShaderMaps[mapIndex].TilingIndex;
                    float uTiling;
                    try { uTiling = rmsh.Properties[0].Tilings[tileIndex].UTiling; }
                    catch { uTiling = 1; }

                    float vTiling;
                    try { vTiling = rmsh.Properties[0].Tilings[tileIndex].VTiling; }
                    catch { vTiling = 1; }

                    MemoryStream stream = new MemoryStream();                                                        //PNG for transparency
                    //image.Save(stream, (rmshTag.ClassCode == "rmsh" || rmshTag.ClassCode == "rmtr" || rmshTag.ClassCode == "rmt" || rmshTag.ClassCode == "mat") ? ImageFormat.Bmp : ImageFormat.Bmp);
                    image.Save(stream, ImageFormat.Bmp);
                    image.Dispose();

                    var diffuse = new BitmapImage();

                    diffuse.BeginInit();
                    diffuse.StreamSource = stream;
                    diffuse.EndInit();

                    matGroup.Children.Add(new DiffuseMaterial()
                    {
                        Brush = new ImageBrush(diffuse)
                        {
                            ViewportUnits = BrushMappingMode.Absolute,
                            TileMode = TileMode.Tile,
                            Viewport = new System.Windows.Rect(0, 0, 1f / Math.Abs(uTiling), 1f / Math.Abs(vTiling))
                        }
                    });

                    shaders.Add(matGroup);
                }
                catch
                {
                    matGroup.Children.Add(errMat);
                    shaders.Add(matGroup);
                }
            }
        }

        private void LoadSections()
        {
            for (int x = 0; x < sbsp.ModelSections.Count; x++)
            {
                try
                {
                    var part = sbsp.ModelSections[x];

                    if (part.Submeshes.Count == 0)
                    {
                        sectList.Add(null);
                        continue;
                    }

                    var group = new Model3DGroup();
                    var verts = part.Vertices;

                    for (int i = 0; i < part.Submeshes.Count; i++)
                    {
                        var submesh = part.Submeshes[i];
                        var geom = new MeshGeometry3D();

                        var iList = ModelFunctions.GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, sbsp.IndexInfoList[part.FacesIndex].FaceFormat);

                        int min = iList.Min();
                        int max = iList.Max();

                        for (int j = 0; j < iList.Count; j++)
                            iList[j] -= min;

                        var vArray = new Vertex[(max - min) + 1];
                        Array.Copy(verts, min, vArray, 0, (max - min) + 1);

                        foreach (var vertex in vArray)
                        {
                            VertexValue pos, tex, norm;
                            vertex.TryGetValue("position", 0, out pos);
                            vertex.TryGetValue("texcoords", 0, out tex);

                            geom.Positions.Add(new Point3D(pos.Data.x, pos.Data.y, pos.Data.z));
                            geom.TextureCoordinates.Add(new System.Windows.Point(tex.Data.x, 1f - tex.Data.y));
                            if (vertex.TryGetValue("normal", 0, out norm)) geom.Normals.Add(new Vector3D(norm.Data.x, norm.Data.y, norm.Data.z));
                        }

                        foreach (var index in iList)
                            geom.TriangleIndices.Add(index);

                        GeometryModel3D modeld = new GeometryModel3D(geom, shaders[submesh.ShaderIndex])
                        {
                            //BackMaterial = shaders[submesh.ShaderIndex]
                        };

                        group.Children.Add(modeld);
                    }
                    sectList.Add(group);
                }
                catch { 
                    sectList.Add(null); 
                }
            }
        }

        private void AddCluster(scenario_structure_bsp.Cluster cluster, bool force)
        {
            try
            {
                if (sectList[cluster.SectionIndex] == null)
                    return;

                var group = new Model3DGroup();
                group.Children.Add(sectList[cluster.SectionIndex]);

                clusterDic.Add(cluster, group);
            }
            catch (Exception ex) { if (!force) throw ex; }
        }

        private void AddGeomInstance(scenario_structure_bsp.InstancedGeometry IG, bool force)
        {
            try
            {
                if (sectList[IG.SectionIndex] == null) 
                    return;

                var mat = Matrix3D.Identity;
                mat.M11 = IG.TransformMatrix.m11;
                mat.M12 = IG.TransformMatrix.m12;
                mat.M13 = IG.TransformMatrix.m13;

                mat.M21 = IG.TransformMatrix.m21;
                mat.M22 = IG.TransformMatrix.m22;
                mat.M23 = IG.TransformMatrix.m23;

                mat.M31 = IG.TransformMatrix.m31;
                mat.M32 = IG.TransformMatrix.m32;
                mat.M33 = IG.TransformMatrix.m33;

                mat.OffsetX = IG.TransformMatrix.m41;
                mat.OffsetY = IG.TransformMatrix.m42;
                mat.OffsetZ = IG.TransformMatrix.m43;

                var mGroup = new Transform3DGroup();
                mGroup.Children.Add(new ScaleTransform3D(IG.TransformScale, IG.TransformScale, IG.TransformScale));
                mGroup.Children.Add(new MatrixTransform3D(mat));

                var group = new Model3DGroup();

                group.Children.Add(sectList[IG.SectionIndex]);
                group.Transform = mGroup;
                
                igDic.Add(IG, group);
            }
            catch (Exception ex) { 
                if (!force) throw ex; 
            }
        }
        #endregion

        #region Pak
        public void LoadBSPTag(PakFile Pak, PakFile.PakTag Item, bool Force)
        {
            try
            {
                Clear();
                loadBspTag(Pak, Item, false, Force);
            }
            catch (Exception ex)
            {
                renderer1.ClearViewport();
                Clear();
                renderer1.Stop("Loading failed: " + ex.Message);
                tvRegions.Nodes.Clear();
                this.Enabled = false;
            }
        }

        private void loadBspTag(PakFile Pak, PakFile.PakTag Item, bool Specular, bool Force)
        {
            if (!this.Enabled) this.Enabled = true;
            tvRegions.Nodes.Clear();
            if (renderer1.Running) renderer1.Stop("Loading...");
            Refresh();

            pak = Pak;
            item = Item;

            atpl = new Scene(pak, item, true);

            var shList = new List<int>();

            isWorking = true;

            #region Build Tree

            #region non-shared nodes
            TreeNode pNode = new TreeNode(atpl.Name) { Tag = atpl };
            foreach (var obj in atpl.Objects)
            {
                if (obj.isInherited || obj.isInheritor) continue;
                if (obj.Submeshes == null) continue;

                pNode.Nodes.Add(new TreeNode(obj.Name) { Tag = obj });

                foreach (var sub in obj.Submeshes)
                    if (!shList.Contains(sub.MaterialIndex)) shList.Add(sub.MaterialIndex);
            }
            if (pNode.Nodes.Count > 0) tvRegions.Nodes.Add(pNode);
            #endregion

            #region shared nodes
            foreach (var pObj in atpl.Objects)
            {
                if (!pObj.isInherited) continue;

                TreeNode iNode = new TreeNode(pObj.Name) { Tag = pObj };

                foreach (var cObj in atpl.Objects)
                {
                    if (!cObj.isInheritor) continue;

                    if (cObj.inheritID == pObj.ID)
                    {
                        iNode.Nodes.Add(new TreeNode(cObj.Name) { Tag = cObj });

                        foreach (var sub in cObj.Submeshes)
                            if (!shList.Contains(sub.MaterialIndex)) shList.Add(sub.MaterialIndex);
                    }
                }

                if (iNode.Nodes.Count > 0) tvRegions.Nodes.Add(iNode);
            }
            #endregion

            foreach (TreeNode node in tvRegions.Nodes)
                node.Nodes[0].Checked = node.Checked = true;

            //tvRegions.Sort(); //much easier for looking through IGs
            #endregion

            isWorking = false;

            #region Load Stuff
            LoadS3DShaders(false, shList);
            LoadS3DMeshes(Force);
            #endregion

            #region BoundingBox Stuff
            PerspectiveCamera camera = (PerspectiveCamera)renderer1.Viewport.Camera;

            var XBounds = atpl.RenderBounds.XBounds;
            var YBounds = atpl.RenderBounds.ZBounds;
            var ZBounds = atpl.RenderBounds.YBounds;

            #region Get Bounds
            //foreach (var c in sbsp.Clusters)
            //{
            //    if (c.SectionIndex >= sbsp.ModelSections.Count) continue;
            //    if (sbsp.ModelSections[c.SectionIndex].Submeshes.Count == 0) continue;

            //    if (c.XBounds.Min < XBounds.Min) XBounds.Min = c.XBounds.Min;
            //    if (c.YBounds.Min < YBounds.Min) YBounds.Min = c.YBounds.Min;
            //    if (c.ZBounds.Min < ZBounds.Min) ZBounds.Min = c.ZBounds.Min;

            //    if (c.XBounds.Max > XBounds.Max) XBounds.Max = c.XBounds.Max;
            //    if (c.YBounds.Max > YBounds.Max) YBounds.Max = c.YBounds.Max;
            //    if (c.ZBounds.Max > ZBounds.Max) ZBounds.Max = c.ZBounds.Max;
            //}

            //foreach (var bb in sbsp.BoundingBoxes)
            //{
            //    if (bb.XBounds.Min < XBounds.Min) XBounds.Min = bb.XBounds.Min;
            //    if (bb.YBounds.Min < YBounds.Min) YBounds.Min = bb.YBounds.Min;
            //    if (bb.ZBounds.Min < ZBounds.Min) ZBounds.Min = bb.ZBounds.Min;

            //    if (bb.XBounds.Max > XBounds.Max) XBounds.Max = bb.XBounds.Max;
            //    if (bb.YBounds.Max > YBounds.Max) YBounds.Max = bb.YBounds.Max;
            //    if (bb.ZBounds.Max > ZBounds.Max) ZBounds.Max = bb.ZBounds.Max;
            //}
            #endregion

            double pythagoras3d = Math.Sqrt(
                Math.Pow(XBounds.Length, 2) +
                Math.Pow(YBounds.Length, 2) +
                Math.Pow(ZBounds.Length, 2));

            if (double.IsInfinity(pythagoras3d) || pythagoras3d == 0) //no clusters
            {
                XBounds = sbsp.XBounds;
                YBounds = sbsp.YBounds;
                ZBounds = sbsp.ZBounds;

                pythagoras3d = Math.Sqrt(
                Math.Pow(XBounds.Length, 2) +
                Math.Pow(YBounds.Length, 2) +
                Math.Pow(ZBounds.Length, 2));
            }

            if (XBounds.Length / 2 > (YBounds.Length)) //side view
            {
                var p = new Point3D(
                XBounds.MidPoint,
                YBounds.Max + pythagoras3d * 0.5,
                ZBounds.MidPoint);
                renderer1.MoveCamera(p, new Vector3D(0, 0, -2));
            }
            else //normal camera position
            {
                var p = new Point3D(
                XBounds.Max + pythagoras3d * 0.5,
                YBounds.MidPoint,
                ZBounds.MidPoint);
                renderer1.MoveCamera(p, new Vector3D(-1, 0, 0));
            }

            renderer1.CameraSpeed = Math.Ceiling(pythagoras3d * 3) / 1000;
            renderer1.MaxCameraSpeed = Math.Ceiling(pythagoras3d * 3) * 5 / 1000;
            renderer1.MaxPosition = new Point3D(
                atpl.RenderBounds.XBounds.Max + pythagoras3d * 2,
                atpl.RenderBounds.YBounds.Max + pythagoras3d * 2,
                atpl.RenderBounds.ZBounds.Max + pythagoras3d * 2);
            renderer1.MinPosition = new Point3D(
                atpl.RenderBounds.XBounds.Min - pythagoras3d * 2,
                atpl.RenderBounds.YBounds.Min - pythagoras3d * 2,
                atpl.RenderBounds.ZBounds.Min - pythagoras3d * 2);
            #endregion

            renderer1.Start();
            RenderSelected();
        }

        private void LoadS3DShaders(bool spec, List<int> indices)
        {
            var errMat = GetErrorMaterial();

            var matGroup = new MaterialGroup();
            matGroup.Children.Add(errMat);
            shaders.Add(matGroup);

            var texpath = pak.FilePath + "\\" + "pak_stream_decompressed.s3dpak";
            var sPak = (File.Exists(texpath)) ? new PakFile(texpath) : pak;

            foreach (var mat in atpl.Materials)
            {
                if (!indices.Contains(atpl.Materials.IndexOf(mat)))
                {
                    shaders.Add(null);
                    continue;
                }
                matGroup = new MaterialGroup();

                try
                {
                    var pict = new Texture(sPak, sPak.GetItemByName(mat.Name));
                    var image = BitmapExtractor.GetBitmapByTag(sPak, pict, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    if (TextureScale < 1) image = new Bitmap(image, new Size((int)(image.Width * TextureScale), (int)(image.Height * TextureScale)));


                    if (image == null)
                    {
                        matGroup.Children.Add(errMat);
                        shaders.Add(matGroup);
                        continue;
                    }

                    float uTiling = 1, vTiling = 1;

                    MemoryStream stream = new MemoryStream();
                    image.Save(stream, ImageFormat.Bmp);

                    var diffuse = new BitmapImage();

                    diffuse.BeginInit();
                    diffuse.StreamSource = stream;
                    diffuse.EndInit();

                    matGroup.Children.Add(new DiffuseMaterial()
                    {
                        Brush = new ImageBrush(diffuse)
                        {
                            ViewportUnits = BrushMappingMode.Absolute,
                            TileMode = TileMode.Tile,
                            Viewport = new System.Windows.Rect(0, 0, 1f / Math.Abs(uTiling), 1f / Math.Abs(vTiling))
                        }
                    });

                    shaders.Add(matGroup);
                }
                catch
                {
                    matGroup.Children.Add(errMat);
                    shaders.Add(matGroup);
                }
            }
            if(sPak != pak) sPak.Close();
        }

        private void LoadS3DMeshes(bool force)
        {
            atplDic = new Dictionary<int, Model3DGroup>();

            foreach (var obj in atpl.Objects)
            {
                if (obj.isInherited || obj.Submeshes == null) continue;

                try
                {
                    var group = new Model3DGroup();
                    foreach (var submesh in obj.Submeshes)
                        AddS3DMesh(group, obj, submesh, force);

                    var mGroup = new Transform3DGroup();
                    var pObj = (obj.inheritID == -1) ? obj : atpl.ObjectByID(obj.inheritID);

                    //Matrix3D mat0 = ModelFunctions.MatrixFromBounds(obj.BoundingBox);
                    //Matrix3D mat1 = (obj.isInheritor) ? ModelFunctions.MatrixFromBounds(pObj.BoundingBox) : Matrix3D.Identity;
                    //Matrix3D mat2 = ModelFunctions.MatrixFromBounds(atpl.RenderBounds);

                    var mat3 = new Matrix3D(1, 0, 0, 0, 0, 0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 1);
                    var mat4 = (pObj.geomUnk01 == 134) ? Matrix3D.Identity : new Matrix3D(500, 0, 0, 0, 0, 500, 0, 0, 0, 0, 500, 0, 0, 0, 0, 1);


                    mGroup.Children.Add(new MatrixTransform3D(mat4 * mat3));
                    group.Transform = mGroup;

                    atplDic.Add(atpl.Objects.IndexOf(obj), group);
                }
                catch (Exception ex) { if (!force)throw ex; }
            }
        }

        private void AddS3DMesh(Model3DGroup group, Node obj, Node.Submesh submesh, bool force)
        {
            try
            {
                var pObj = (obj.inheritID == -1) ? obj : atpl.ObjectByID(obj.inheritID);
                
                var geom = new MeshGeometry3D();
                var iList = ModelFunctions.GetTriangleList(pObj.Indices, (obj.faceOffset + submesh.FaceStart) * 3, submesh.FaceLength * 3, 3);

                var vArray = new Vertex[submesh.VertLength];
                Array.Copy(pObj.Vertices.ToArray(), (obj.vertOffset + submesh.VertStart), vArray, 0, submesh.VertLength);

                foreach (var vertex in vArray)
                {
                    if (vertex == null) continue;
                    VertexValue pos, tex, norm;
                    vertex.TryGetValue("position", 0, out pos);
                    vertex.TryGetValue("texcoords", 0, out tex);

                    geom.Positions.Add(new Point3D(pos.Data.x, pos.Data.y, pos.Data.z));
                    geom.TextureCoordinates.Add(new System.Windows.Point(tex.Data.x * obj.unkC0, tex.Data.y * obj.unkC0));
                    if (vertex.TryGetValue("normal", 0, out norm)) geom.Normals.Add(new Vector3D(norm.Data.x, norm.Data.y, norm.Data.z));
                }

                foreach (var index in iList)
                    geom.TriangleIndices.Add(index);

                GeometryModel3D modeld = new GeometryModel3D(geom, shaders[submesh.MaterialIndex + 1])
                {
                    //BackMaterial = shaders[submesh.MaterialIndex + 1]
                };

                group.Children.Add(modeld);
            }
            catch (Exception ex) { if (!force) throw ex; }
        }

        #endregion

        private void RenderSelected()
        {
            renderer1.ClearViewport();
            Model3DGroup group = new Model3DGroup();

            foreach (TreeNode pnode in tvRegions.Nodes)
            {
                if (!pnode.Checked) continue;
                foreach (TreeNode cnode in pnode.Nodes)
                {
                    if (!cnode.Checked) continue;
                    Model3DGroup mesh;

                    if (cnode.Tag is scenario_structure_bsp.Cluster)
                    {
                        var cluster = cnode.Tag as scenario_structure_bsp.Cluster;

                        if (clusterDic.TryGetValue(cluster, out mesh))
                            group.Children.Add(mesh);
                    }
                    else if (cnode.Tag is scenario_structure_bsp.InstancedGeometry)
                    {
                        var ig = cnode.Tag as scenario_structure_bsp.InstancedGeometry;

                        if (igDic.TryGetValue(ig, out mesh))
                            group.Children.Add(mesh);
                    }
                    else //S3D
                    {
                        var obj = cnode.Tag as Node;
                        if (atplDic.TryGetValue(atpl.Objects.IndexOf(obj), out mesh))
                            group.Children.Add(mesh);
                    }
                }
            }

            ModelVisual3D visuald = new ModelVisual3D
            {
                Content = group
            };

            renderer1.Viewport.Children.Add(visuald);
        }

        private DiffuseMaterial GetErrorMaterial()
        {
            return new DiffuseMaterial(new SolidColorBrush(Colors.Gold));
        }
        #endregion

        #region Events
        private void tvRegions_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (isWorking) return;

            int num;
            isWorking = true;
            TreeNode node = e.Node;
            if (node.Parent == null)
            {
                if (!node.Checked)
                    for (num = 0; num < node.Nodes.Count; num++)
                        node.Nodes[num].Checked = false;
                else if (node.Nodes.Count > 0)
                    node.Nodes[0].Checked = true;
            }
            else if (node.Checked)
                node.Parent.Checked = true;
            else
            {
                bool flag = false;
                for (num = 0; num < node.Parent.Nodes.Count; num++)
                {
                    if (node.Parent.Nodes[num].Checked)
                    {
                        flag = true;
                        break;
                    }
                }
                node.Parent.Checked = flag;
            }
            isWorking = false;

            RenderSelected();
        }

        private void tvRegions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectAllChildrenToolStripMenuItem.Visible = (tvRegions.SelectedNode != null && tvRegions.SelectedNode.Parent == null);
        }

        private void selectAllChildrenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isWorking = true;

            foreach (TreeNode cnode in tvRegions.SelectedNode.Nodes)
                cnode.Checked = true;

            tvRegions.SelectedNode.Checked = true;

            isWorking = false;
            RenderSelected();
        }

        private void selectResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isWorking = true;

            foreach (TreeNode pnode in tvRegions.Nodes)
                foreach (TreeNode cnode in pnode.Nodes)
                    if (cnode.Text.Contains(txtSearch.Text) && txtSearch.Text != "")
                        cnode.Checked = pnode.Checked = true;

            isWorking = false;
            RenderSelected();
        }

        private void deselectResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isWorking = true;

            foreach (TreeNode pnode in tvRegions.Nodes)
                foreach (TreeNode cnode in pnode.Nodes)
                    if (cnode.Text.Contains(txtSearch.Text) && txtSearch.Text != "")
                        cnode.Checked = false;

            isWorking = false;
            RenderSelected();
        }

        private void btnSelAll_Click(object sender, EventArgs e)
        {
            isWorking = true;
            foreach (TreeNode parent in tvRegions.Nodes)
            {
                foreach (TreeNode child in parent.Nodes)
                    child.Checked = true;

                parent.Checked = true;
            }
            isWorking = false;
            RenderSelected();
        }

        private void btnSelNone_Click(object sender, EventArgs e)
        {
            isWorking = true;
            foreach (TreeNode parent in tvRegions.Nodes)
            {
                foreach (TreeNode child in parent.Nodes)
                    child.Checked = false;

                parent.Checked = false;
            }
            isWorking = false;
            RenderSelected();
        }

        private void btnExportBSP_Click(object sender, EventArgs e)
        {
            var clusts = new List<int>();
            var igs = new List<int>();

            foreach (TreeNode pnode in tvRegions.Nodes)
            {
                if (!pnode.Checked) continue;

                foreach (TreeNode cnode in pnode.Nodes)
                {
                    if (!cnode.Checked) continue;
                    if (cnode.Tag is scenario_structure_bsp.Cluster)
                    {
                        var cluster = cnode.Tag as scenario_structure_bsp.Cluster;
                        clusts.Add(sbsp.Clusters.IndexOf(cluster));
                    }
                    else if (cnode.Tag is scenario_structure_bsp.InstancedGeometry)
                    {
                        var ig = cnode.Tag as scenario_structure_bsp.InstancedGeometry;
                        igs.Add(sbsp.GeomInstances.IndexOf(ig));
                    }
                    else if (cnode.Tag is Node)
                    {
                        var obj = cnode.Tag as Node;
                        clusts.Add(atpl.Objects.IndexOf(obj));
                    }
                }
            }

            var sfd = new SaveFileDialog()
            {
                Filter = "EMF Files|*.emf|OBJ Files|*.obj|AMF Files|*.amf",
                FilterIndex = (int)DefaultModeFormat + 1,
                FileName = (tag != null) ? tag.Filename.Substring(tag.Filename.LastIndexOf("\\") + 1) : atpl.Name
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var format = (ModelFormat)(sfd.FilterIndex - 1);
                if (cache != null)
                {
                    BSPExtractor.SaveBSPParts(sfd.FileName, cache, sbsp, format, clusts, igs);
                    TagExtracted(this, tag);
                }
                else
                {
                    ModelFunctions.WriteAMF(sfd.FileName, pak, atpl, clusts);
                    TagExtracted(this, item);
                }
            }
            catch (Exception ex) { ErrorExtracting(this, (tag != null) ? (object)tag : (object)item, ex); }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            selectResultsToolStripMenuItem.Visible =
            deselectResultsToolStripMenuItem.Visible
               = (txtSearch.Text != "");

            foreach (TreeNode pnode in tvRegions.Nodes)
            {
                foreach (TreeNode cnode in pnode.Nodes)
                {
                    if (cnode.Text.Contains(txtSearch.Text) && txtSearch.Text != "")
                    {
                        cnode.ForeColor = System.Drawing.Color.Green;
                        cnode.NodeFont = new Font(tvRegions.Font, FontStyle.Underline);
                    }
                    else
                    {
                        cnode.ForeColor = System.Drawing.Color.Black;
                        cnode.NodeFont = new Font(tvRegions.Font, FontStyle.Regular);
                    }
                }
            }
        }
        #endregion

        public void Clear()
        {
            renderer1.ClearViewport();
            sectList.Clear();

            foreach (var val in igDic)
                val.Value.Children.Clear();

            foreach (var val in clusterDic)
                val.Value.Children.Clear();

            foreach (var val in atplDic)
                val.Value.Children.Clear();

            foreach (var val in shaders)
                if (val != null) val.Children.Clear();

            igDic.Clear();
            clusterDic.Clear();
            atplDic.Clear();
            shaders.Clear();

            GC.Collect();
        }

        public event TagExtractedEventHandler TagExtracted;
        public event ErrorExtractingEventHandler ErrorExtracting;
    }
}
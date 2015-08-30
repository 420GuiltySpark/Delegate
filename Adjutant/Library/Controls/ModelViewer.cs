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
    public partial class ModelViewer : UserControl
    {
        #region Init
        private CacheFile cache;
        private CacheFile.IndexItem tag;
        private render_model mode;

        private bool isWorking = false;
        private Dictionary<render_model.Region.Permutation, Model3DGroup> meshDic = new Dictionary<render_model.Region.Permutation, Model3DGroup>();
        private Dictionary<int, Model3DGroup> geomDic = new Dictionary<int, Model3DGroup>();

        private PakFile pak;
        private PakFile.PakTag item;
        private Template atpl;
        private Dictionary<int, Model3DGroup> atplDic = new Dictionary<int, Model3DGroup>();

        private List<MaterialGroup> shaders = new List<MaterialGroup>();

        public ModelFormat DefaultModeFormat = ModelFormat.AMF;

        public System.Drawing.Color RenderBackColor
        {
            get { return ehRenderer.BackColor; }
            set { ehRenderer.BackColor = value; }
        }
        public List<string> PermutationFilter = new List<string>();

        public ModelViewer()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods

        #region MAP
        public void LoadModelTag(CacheFile Cache, CacheFile.IndexItem Tag, bool Specular, bool UserPermFilter, bool Force)
        {
            try
            {
                Clear();
                loadModelTag(Cache, Tag, Specular, UserPermFilter, Force);
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

        private void loadModelTag(CacheFile Cache, CacheFile.IndexItem Tag, bool Specular, bool UserPermFilter, bool Force)
        {
            if (!this.Enabled) this.Enabled = true;
            tvRegions.Nodes.Clear();
            if (renderer1.Running) renderer1.Stop("Loading...");
            Refresh();

            cache = Cache;
            tag = Tag;

            mode = DefinitionsManager.mode(cache, tag);
            mode.LoadRaw();

            isWorking = true;

            #region Build Tree
            foreach (var region in mode.Regions)
            {
                TreeNode node = new TreeNode(region.Name) { Tag = region };
                foreach (var perm in region.Permutations)
                {
                    if (perm.PieceIndex != -1)
                        if (mode.ModelSections[perm.PieceIndex].Submeshes.Count > 0)
                            node.Nodes.Add(new TreeNode(perm.Name) { Tag = perm });
                }
                if (node.Nodes.Count > 0)
                    tvRegions.Nodes.Add(node);
            }

            if (UserPermFilter)
            {
                foreach (TreeNode node in tvRegions.Nodes)
                {
                    foreach (TreeNode child in node.Nodes)
                    {
                        if (PermutationFilter.Contains((child.Tag as render_model.Region.Permutation).Name))
                            child.Checked = node.Checked = true;
                    }
                }
            }
            else
            {
                foreach (TreeNode node in tvRegions.Nodes)
                    node.Nodes[0].Checked = node.Checked = true;
            }
            #endregion

            isWorking = false;

            LoadShaders(Specular);
            LoadMeshes(Force);

            #region BoundingBox Stuff
            PerspectiveCamera camera = (PerspectiveCamera)renderer1.Viewport.Camera;

            var bb = mode.BoundingBoxes[0];

            double pythagoras3d = Math.Sqrt(
                Math.Pow(bb.XBounds.Length, 2) +
                Math.Pow(bb.YBounds.Length, 2) +
                Math.Pow(bb.ZBounds.Length, 2));

            if (bb.XBounds.Length / 2 > (bb.YBounds.Length)) //side view for long models like weapons
            {
                var p = new Point3D(
                bb.XBounds.MidPoint,
                bb.YBounds.Max + pythagoras3d * 0.75,
                bb.ZBounds.MidPoint);
                renderer1.MoveCamera(p, new Vector3D(0, 0, -2));
            }
            else //normal camera position
            {
                var p = new Point3D(
                bb.XBounds.Max + pythagoras3d * 0.75,
                bb.YBounds.MidPoint,
                bb.ZBounds.MidPoint);
                renderer1.MoveCamera(p, new Vector3D(-1, 0, 0));
            }

            renderer1.CameraSpeed = Math.Ceiling(pythagoras3d * 5) / 1000;
            renderer1.MaxCameraSpeed = Math.Ceiling(pythagoras3d * 5) * 7 / 1000;
            renderer1.MaxPosition = new Point3D(
                bb.XBounds.Max + pythagoras3d * 3,
                bb.YBounds.Max + pythagoras3d * 3,
                bb.ZBounds.Max + pythagoras3d * 3);
            renderer1.MinPosition = new Point3D(
                bb.XBounds.Min - pythagoras3d * 3,
                bb.YBounds.Min - pythagoras3d * 3,
                bb.ZBounds.Min - pythagoras3d * 3);
            #endregion

            RenderSelected();
            renderer1.Start();
        }

        private void LoadShaders(bool spec)
        {
            var errMat = GetErrorMaterial();

            if (mode.Shaders.Count == 0)
            {
                var matGroup = new MaterialGroup();
                matGroup.Children.Add(errMat);
                shaders.Add(matGroup);
            }

            foreach (render_model.Shader s in mode.Shaders)
            {
                #region Skip Unused
                bool found = false;
                foreach (var sec in mode.ModelSections)
                {
                    foreach (var sub in sec.Submeshes)
                    {
                        if (sub.ShaderIndex == mode.Shaders.IndexOf(s))
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
                            if (rmt2.UsageBlocks[i].Usage == "base_map")
                            {
                                mapIndex = i;
                                break;
                            }
                        }
                    }

                    var bitmTag = cache.IndexItems.GetItemByID(rmsh.Properties[0].ShaderMaps[mapIndex].BitmapTagID);
                    var image = BitmapExtractor.GetBitmapByTag(cache, bitmTag, 0, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

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
                    image.Save(stream, (rmshTag.ClassCode == "rmsh" || rmshTag.ClassCode == "mat") ? ImageFormat.Bmp : ImageFormat.Bmp);

                    var diffuse = new BitmapImage();

                    diffuse.BeginInit();
                    diffuse.StreamSource = new MemoryStream(stream.ToArray());
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

                    if (spec && rmshTag.ClassCode == "rmsh")
                    {
                        stream = new MemoryStream();
                        image.Save(stream, ImageFormat.Png);

                        var specular = new BitmapImage();

                        specular.BeginInit();
                        specular.StreamSource = new MemoryStream(stream.ToArray());
                        specular.EndInit();

                        matGroup.Children.Add(new SpecularMaterial()
                        {
                            SpecularPower = 10,
                            Brush = new ImageBrush(specular)
                            {
                                ViewportUnits = BrushMappingMode.Absolute,
                                TileMode = TileMode.Tile,
                                Viewport = new System.Windows.Rect(0, 0, 1f / Math.Abs(uTiling), 1f / Math.Abs(vTiling))
                            }
                        });
                    }

                    shaders.Add(matGroup);
                }
                catch
                {
                    matGroup.Children.Add(errMat);
                    shaders.Add(matGroup);
                }
            }
        }

        private void LoadMeshes(bool force)
        {
            meshDic = new Dictionary<render_model.Region.Permutation, Model3DGroup>();
            geomDic = new Dictionary<int, Model3DGroup>();

            foreach (var region in mode.Regions)
            {
                foreach (var perm in region.Permutations)
                {
                    if (perm.PieceIndex != -1)
                    {
                        var ModelPart = mode.ModelSections[perm.PieceIndex];

                        if (ModelPart.Submeshes.Count == 0) continue;

                        var group = new Model3DGroup();
                        var oldGroup = new Model3DGroup();

                        if (perm.PieceIndex >= mode.InstancedGeometryIndex && mode.InstancedGeometryIndex != -1)
                        {
                            foreach (var Submesh in ModelPart.Submeshes)
                                AddMesh(group, ModelPart, Submesh, force);
                        }
                        else if (geomDic.TryGetValue(ModelPart.FacesIndex, out oldGroup))
                        {
                            for (int i = 0; i < ModelPart.Submeshes.Count; i++)
                                AddCopy(group, oldGroup, ModelPart, i, force);
                        }
                        else
                        {
                            foreach (var Submesh in ModelPart.Submeshes)
                                AddMesh(group, ModelPart, Submesh, force);
                            geomDic.Add(ModelPart.FacesIndex, group);
                        }

                        meshDic.Add(perm, group);
                    }
                }
            }
        }

        private void AddMesh(Model3DGroup group, render_model.ModelSection part, render_model.ModelSection.Submesh submesh, bool force)
        {
            try
            {
                var geom = new MeshGeometry3D();

                var iList = ModelFunctions.GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, mode.IndexInfoList[part.FacesIndex].FaceFormat);

                int min = iList.Min();
                int max = iList.Max();

                for (int i = 0; i < iList.Count; i++)
                    iList[i] -= min;

                var vArray = new Vertex[(max - min) + 1];
                Array.Copy(part.Vertices.ToArray(), min, vArray, 0, (max - min) + 1);

                foreach (var vertex in vArray)
                {
                    if (vertex == null) continue;
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
                    BackMaterial = shaders[submesh.ShaderIndex]
                };

                group.Children.Add(modeld);
            }
            catch (Exception ex) { if (!force) throw ex; }
        }

        private void AddCopy(Model3DGroup newGroup, Model3DGroup oldGroup, render_model.ModelSection part, int submeshIndex, bool force)
        {
            try
            {
                GeometryModel3D modeld = new GeometryModel3D();
                modeld.Geometry = ((GeometryModel3D)oldGroup.Children[submeshIndex]).Geometry;
                modeld.Material = modeld.BackMaterial = shaders[part.Submeshes[submeshIndex].ShaderIndex];

                newGroup.Children.Add(modeld);
            }
            catch (Exception ex) { if (!force) throw ex; }
        }
        #endregion

        #region PAK
        public void LoadModelTag(PakFile Pak, PakFile.PakTag Item, bool Specular, bool Force)
        {
            try
            {
                Clear();
                loadModelTag(Pak, Item, Specular, Force);
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

        private void loadModelTag(PakFile Pak, PakFile.PakTag Item, bool Specular, bool Force)
        {
            if (!this.Enabled) this.Enabled = true;
            tvRegions.Nodes.Clear();
            if (renderer1.Running) renderer1.Stop("Loading...");
            Refresh();

            pak = Pak;
            item = Item;

            atpl = new Template(pak, item, true);
            atpl.Parse();

            isWorking = true;

            #region Build Tree

            TreeNode pNode = new TreeNode(atpl.Name) { Tag = atpl };
            foreach (var obj in atpl.Objects)
            {
                if (obj.VertCount == 0 /*&& obj.Submeshes == null*/) continue;
                //if (obj.BoundingBox.Length == 0) continue;

                pNode.Nodes.Add(new TreeNode(obj.Name) { Tag = obj });
            }
            if (pNode.Nodes.Count > 0) tvRegions.Nodes.Add(pNode);

            foreach (TreeNode node in tvRegions.Nodes)
                node.Nodes[0].Checked = node.Checked = true;

            #endregion

            isWorking = false;

            LoadS3DShaders(Specular);
            LoadS3DMeshes(Force);

            #region BoundingBox Stuff
            PerspectiveCamera camera = (PerspectiveCamera)renderer1.Viewport.Camera;

            var bb = new render_model.BoundingBox();
            //bb.XBounds = new RealBounds(float.PositiveInfinity, float.NegativeInfinity);
            //bb.YBounds = new RealBounds(float.PositiveInfinity, float.NegativeInfinity);
            //bb.ZBounds = new RealBounds(float.PositiveInfinity, float.NegativeInfinity);

            //foreach (var obj in atpl.Objects)
            //{
            //    if (obj.VertCount == 0) continue;
            //    bb.XBounds.Min = Math.Min(bb.XBounds.Min, obj.BoundingBox.XBounds.Min);
            //    bb.YBounds.Min = Math.Min(bb.YBounds.Min, obj.BoundingBox.YBounds.Min);
            //    bb.ZBounds.Min = Math.Min(bb.ZBounds.Min, obj.BoundingBox.ZBounds.Min);
            //    bb.XBounds.Max = Math.Max(bb.XBounds.Max, obj.BoundingBox.XBounds.Max);
            //    bb.YBounds.Max = Math.Max(bb.YBounds.Max, obj.BoundingBox.YBounds.Max);
            //    bb.ZBounds.Max = Math.Max(bb.ZBounds.Max, obj.BoundingBox.ZBounds.Max);
            //}

            //bb.XBounds = new RealBounds(0, 50);
            //bb.YBounds = new RealBounds(0, 50);
            //bb.ZBounds = new RealBounds(0, 50);

            bb.XBounds = atpl.RenderBounds.XBounds;
            bb.YBounds = atpl.RenderBounds.ZBounds;
            bb.ZBounds = atpl.RenderBounds.YBounds;

            if (bb.XBounds.Length / 2 > (bb.YBounds.Length)) //side view for long models like weapons
            {
                var p = new Point3D(
                bb.XBounds.MidPoint,
                bb.YBounds.Max + bb.Length * 0.75,
                bb.ZBounds.MidPoint);
                renderer1.MoveCamera(p, new Vector3D(0, 0, -2));
            }
            else //normal camera position
            {
                var p = new Point3D(
                bb.XBounds.Max + bb.Length * 0.75,
                bb.YBounds.MidPoint,
                bb.ZBounds.MidPoint);
                renderer1.MoveCamera(p, new Vector3D(-1, 0, 0));
            }

            renderer1.CameraSpeed = Math.Ceiling(bb.Length * 5) / 1000;
            renderer1.MaxCameraSpeed = Math.Ceiling(bb.Length * 5) * 7 / 1000;
            renderer1.MaxPosition = new Point3D(
                bb.XBounds.Max + bb.Length * 3,
                bb.YBounds.Max + bb.Length * 3,
                bb.ZBounds.Max + bb.Length * 3);
            renderer1.MinPosition = new Point3D(
                bb.XBounds.Min - bb.Length * 3,
                bb.YBounds.Min - bb.Length * 3,
                bb.ZBounds.Min - bb.Length * 3);
            #endregion

            RenderSelected();
            renderer1.Start();
        }

        private void LoadS3DShaders(bool spec)
        {
            var errMat = GetErrorMaterial();

            var matGroup = new MaterialGroup();
            matGroup.Children.Add(errMat);
            shaders.Add(matGroup);

            var texpath = pak.FilePath + "\\" + "pak_stream_decompressed.s3dpak";
            var sPak = (File.Exists(texpath)) ? new PakFile(texpath) : pak;

            foreach (var mat in atpl.Materials)
            {
                matGroup = new MaterialGroup();

                try
                {
                    var pict = new Texture(sPak, sPak.GetItemByName(mat.Name));
                    var image = BitmapExtractor.GetBitmapByTag(sPak, pict, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

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
                if (obj.VertCount == 0 /*&& obj.Submeshes == null*/) continue;
                //if (obj.BoundingBox.Length == 0) continue;

                var group = new Model3DGroup();
                foreach (var submesh in obj.Submeshes)
                    AddS3DMesh(group, obj, submesh, force);

                int xx = atpl.Objects.IndexOf(obj);

                Matrix3D mat0 = ModelFunctions.MatrixFromBounds(obj.BoundingBox);
                Matrix3D mat1 = obj.Transform;

                Matrix3D pMat = atpl.HierarchialTransformUp(obj);

                var mat2 = new Matrix3D(1, 0, 0, 0, 0, 0, 1, 0, 0, -1, 0, 0, 0, 0, 0, 1);
                var mat3 = new Matrix3D(100, 0, 0, 0, 0, 100, 0, 0, 0, 0, 0, 100, 0, 0, 0, 1);

                int id = obj.ParentID;
                var mGroup = new Transform3DGroup();
                mGroup.Children.Add(new MatrixTransform3D(pMat * mat2));
                group.Transform = mGroup;

                atplDic.Add(atpl.Objects.IndexOf(obj), group);
            }
        }

        private void AddS3DMesh(Model3DGroup group, Node obj, Node.Submesh submesh, bool force)
        {
            try
            {
                var geom = new MeshGeometry3D();

                int[] indcs = obj.Indices;
                Vertex[] vrtcs = obj.Vertices;

                var iList = ModelFunctions.GetTriangleList(indcs, submesh.FaceStart * 3, submesh.FaceLength * 3, 3);

                int min = iList.Min();
                int max = iList.Max();

                for (int i = 0; i < iList.Count; i++)
                    iList[i] -= min;

                var vArray = new Vertex[(max - min) + 1];
                Array.Copy(ModelFunctions.DeepClone(vrtcs), min, vArray, 0, (max - min) + 1);

                foreach (var vertex in vArray)
                {
                    if (vertex == null) continue;
                    VertexValue pos, tex, norm;
                    vertex.TryGetValue("position", 0, out pos);
                    vertex.TryGetValue("texcoords", 0, out tex);

                    geom.Positions.Add(new Point3D(pos.Data.x*1, pos.Data.y*1, pos.Data.z*1));
                    geom.TextureCoordinates.Add(new System.Windows.Point(tex.Data.x, 1f - tex.Data.y));
                    if (vertex.TryGetValue("normal", 0, out norm)) geom.Normals.Add(new Vector3D(norm.Data.x, norm.Data.y, norm.Data.z));
                }

                foreach (var index in iList)
                    geom.TriangleIndices.Add(index);

                GeometryModel3D modeld = new GeometryModel3D(geom, shaders[submesh.MaterialIndex + 1])
                {
                    BackMaterial = shaders[submesh.MaterialIndex + 1]
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

            foreach(TreeNode parent in tvRegions.Nodes)
            {
                if (!parent.Checked) continue;

                foreach(TreeNode child in parent.Nodes)
                {
                    if (!child.Checked) continue;

                    Model3DGroup mesh;
                    if (child.Tag is render_model.Region.Permutation)
                    {
                        var perm = child.Tag as render_model.Region.Permutation;

                        if (meshDic.TryGetValue(perm, out mesh))
                            group.Children.Add(mesh);
                    }
                    else //atpl
                    {
                        var obj = child.Tag as Node;
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
            var mat = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            var brush = (SolidColorBrush)mat.Brush;

            var anim0 = new ColorAnimation
            {
                From = new System.Windows.Media.Color?(brush.Color),
                To = new System.Windows.Media.Color?(Colors.Gold)
            };

            anim0.Duration = new System.Windows.Duration(TimeSpan.FromMilliseconds(500.0));
            anim0.AutoReverse = true;
            anim0.RepeatBehavior = RepeatBehavior.Forever;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, anim0);

            return mat;
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
            //if (tvRegions.SelectedNode != null && tvRegions.SelectedNode.Parent != null)
            //    tvRegions.ContextMenuStrip = contextMenuStrip1;
            //else
            //    tvRegions.ContextMenuStrip = null;

            selectPermutationToolStripMenuItem.Visible = deselectPermutationToolStripMenuItem.Visible
                = (tvRegions.SelectedNode != null && tvRegions.SelectedNode.Parent != null);
        }

        private void selectPermutationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (TreeNode pNode in tvRegions.Nodes)
            {
                foreach (TreeNode cNode in pNode.Nodes)
                {
                    if (cNode.Text == tvRegions.SelectedNode.Text)
                        cNode.Checked = true;
                }
            }
        }

        private void deselectPermutationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (TreeNode pNode in tvRegions.Nodes)
            {
                foreach (TreeNode cNode in pNode.Nodes)
                {
                    if (cNode.Text == tvRegions.SelectedNode.Text)
                        cNode.Checked = false;
                }
            }
        }

        private void btnBDS_Click(object sender, EventArgs e)
        {
            isWorking = true;
            foreach (TreeNode node in tvRegions.Nodes)
            {
                node.Checked = false;

                foreach (TreeNode child in node.Nodes)
                {
                    if (PermutationFilter.Contains((child.Tag as render_model.Region.Permutation).Name))
                        child.Checked = node.Checked = true;
                    else child.Checked = false;
                }
            }
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

        private void btnExportModel_Click(object sender, EventArgs e)
        {
            List<int> parts = new List<int>();

            foreach (TreeNode parent in tvRegions.Nodes)
            {
                if (!parent.Checked) continue;

                foreach (TreeNode child in parent.Nodes)
                {
                    if (!child.Checked) continue;
                    if (cache != null) parts.Add((child.Tag as render_model.Region.Permutation).PieceIndex);
                    else parts.Add(atpl.Objects.IndexOf(child.Tag as Node));
                }
            }

            var sfd = new SaveFileDialog()
            {
                Filter = "EMF Files|*.emf|OBJ Files|*.obj|AMF Files|*.amf|JMS Files|*.jms",
                FilterIndex = (int)DefaultModeFormat + 1,
                FileName = (tag != null) ? tag.Filename.Substring(tag.Filename.LastIndexOf("\\") + 1) : atpl.Name
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var format = (ModelFormat)(sfd.FilterIndex - 1);
                if (cache != null)
                {
                    ModelExtractor.SaveModelParts(sfd.FileName, cache, mode, format, parts, false);
                    TagExtracted(this, tag);
                }
                else
                {
                    ModelFunctions.WriteAMF(sfd.FileName, pak, atpl, parts);
                    TagExtracted(this, item);
                }
            }
            catch (Exception ex) { ErrorExtracting(this, (tag!= null) ? (object)tag : (object)item, ex); }
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
        #endregion

        public void Clear()
        {
            foreach (var val in meshDic)
                val.Value.Children.Clear();

            foreach (var val in geomDic)
                val.Value.Children.Clear();

            foreach (var val in shaders)
                if (val != null) val.Children.Clear();

            meshDic.Clear();
            geomDic.Clear();
            shaders.Clear();

            GC.Collect();
        }

        public event TagExtractedEventHandler TagExtracted;
        public event ErrorExtractingEventHandler ErrorExtracting;
    }
}
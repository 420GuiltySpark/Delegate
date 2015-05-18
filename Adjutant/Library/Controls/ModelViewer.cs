using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
        public void LoadModelTag(CacheFile Cache, CacheFile.IndexItem Tag, bool Specular, bool UserPermFilter, bool Force)
        {
            try { loadModelTag(Cache, Tag, Specular, UserPermFilter, Force); }
            catch (Exception ex)
            {
                renderer1.ClearViewport();
                meshDic.Clear();
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
            ModelFunctions.LoadModelRaw(cache, ref mode);
            ModelFunctions.LoadModelExtras(ref mode);

            isWorking = true;

            foreach (var region in mode.Regions)
            {
                TreeNode node = new TreeNode(region.Name) { Tag = region };
                foreach (var perm in region.Permutations)
                {
                    if (perm.PieceIndex != -1)
                        if (mode.ModelParts[perm.PieceIndex].ValidPartIndex != 255 && mode.ModelParts[perm.PieceIndex].TotalVertexCount > 0)
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

            isWorking = false;

            LoadMeshes(GetShaders(Specular), Force);

            #region BoundingBox Stuff
            PerspectiveCamera camera = (PerspectiveCamera)renderer1.Viewport.Camera;

            var bb = mode.BoundingBoxs[0];

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
                //var dist = (bb.XBounds.Length() * 0.75) / Math.Tan(camera.FieldOfView / 2);
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

        private List<MaterialGroup> GetShaders(bool spec)
        {
            var shaders = new List<MaterialGroup>();

            if (mode.Shaders.Count == 0)
            {
                var matGroup = new MaterialGroup();
                matGroup.Children.Add(GetErrorMaterial());
                shaders.Add(matGroup);
                return shaders;
            }

            foreach (render_model.Shader s in mode.Shaders)
            {
                var matGroup = new MaterialGroup();

                try
                {

                    var rmshTag = cache.IndexItems.GetItemByID(s.tagID);
                    var rmsh = DefinitionsManager.rmsh(cache, rmshTag);

                    var bitmTag = cache.IndexItems.GetItemByID(rmsh.Properties[0].ShaderMaps[0].BitmapTagID);
                    var image = BitmapExtractor.GetBitmapByTag(cache, bitmTag, 0, true);

                    if (image == null)
                    {
                        matGroup.Children.Add(GetErrorMaterial());
                        shaders.Add(matGroup);
                        continue;
                    }

                    int tileIndex = rmsh.Properties[0].ShaderMaps[0].TilingIndex;
                    float uTiling;
                    try { uTiling = rmsh.Properties[0].Tilings[tileIndex].UTiling; }
                    catch { uTiling = 1; }

                    float vTiling;
                    try { vTiling = rmsh.Properties[0].Tilings[tileIndex].VTiling; }
                    catch { vTiling = 1; }

                    MemoryStream stream = new MemoryStream();
                    image.Save(stream, (rmshTag.ClassCode == "rmsh" || rmshTag.ClassCode == "mat") ? ImageFormat.Bmp : ImageFormat.Png);

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
                    matGroup.Children.Add(GetErrorMaterial());
                    shaders.Add(matGroup);
                }
            }

            return shaders;
            
        }

        private void LoadMeshes(List<MaterialGroup> shaders, bool force)
        {
            meshDic = new Dictionary<render_model.Region.Permutation, Model3DGroup>();

            foreach (var region in mode.Regions)
            {
                foreach (var perm in region.Permutations)
                {
                    if (perm.PieceIndex != -1)
                    {
                        var group = new Model3DGroup();
                        var ModelPart = mode.ModelParts[perm.PieceIndex];
                        foreach (var Submesh in ModelPart.Submeshes)
                            AddMesh(group, ModelPart, Submesh, shaders, force);
                        meshDic.Add(perm, group);
                    }
                }
            }
        }

        private void AddMesh(Model3DGroup group, render_model.ModelPart part, render_model.ModelPart.Submesh submesh, List<MaterialGroup> shaders, bool force)
        {
            try
            {
                var geom = new MeshGeometry3D();

                var iList = ModelFunctions.GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, part);

                int min = iList.Min();
                int max = iList.Max();

                for (int i = 0; i < iList.Count; i++)
                    iList[i] -= min;

                var vArray = new Vertex[(max - min) + 1];
                Array.Copy(part.Vertices.ToArray(), min, vArray, 0, (max - min) + 1);

                foreach (var vertex in vArray)
                {
                    if (vertex == null) continue;
                    geom.Positions.Add(new Point3D(vertex.Positions[0].x, vertex.Positions[0].y, vertex.Positions[0].z));
                    geom.TextureCoordinates.Add(new System.Windows.Point(vertex.TexPos[0].x, 1f - vertex.TexPos[0].y));
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

        private void RenderSelected()
        {
            renderer1.ClearViewport();
            Model3DGroup group = new Model3DGroup();
            List<Model3DGroup> decals = new List<Model3DGroup>();

            foreach(TreeNode parent in tvRegions.Nodes)
            {
                if (!parent.Checked) continue;

                foreach(TreeNode child in parent.Nodes)
                {
                    if (!child.Checked) continue;

                    var perm = child.Tag as render_model.Region.Permutation;
                    Model3DGroup mesh;

                    if(meshDic.TryGetValue(perm, out mesh))
                        group.Children.Add(mesh);
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
                    parts.Add((child.Tag as render_model.Region.Permutation).PieceIndex);
                }
            }

            var sfd = new SaveFileDialog()
            {
                Filter = "EMF Files|*.emf|OBJ Files|*.obj|JMS Files|*.jms",
                FileName = tag.Filename.Substring(tag.Filename.LastIndexOf("\\") + 1)
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var format = (ModelFormat)(sfd.FilterIndex - 1);
                ModelExtractor.SaveModelParts(sfd.FileName, cache, mode, format, parts, false);
                TagExtracted(this, tag);
            }
            catch (Exception ex) { ErrorExtracting(this, tag, ex); }
        }
        #endregion

        new public void Dispose()
        {
            renderer1.viewport.Children.Clear();
            renderer1.Stop("");
            meshDic.Clear();

            base.Dispose();
        }

        public event TagExtractedEventHandler TagExtracted;
        public event ErrorExtractingEventHandler ErrorExtracting;
    }
}
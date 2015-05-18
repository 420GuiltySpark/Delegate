using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using System.Threading;
using System.IO;

namespace Adjutant.Library.Controls
{    
    public partial class ModelExtractor : UserControl
    {
        private CacheFile cache;
        private CacheFile.IndexItem tag;
        private render_model mode;
        private bool isWorking = false;
        private string filter = "";

        public ModelFormat DefaultModeFormat = ModelFormat.EMF;
        public BitmapFormat DefaultBitmFormat = BitmapFormat.TIF;

        public string DataFolder = "";
        public string PermFilter
        {
            get { return filter; }
            set
            {
                filter = value;
                selectBDSToolStripMenuItem.Visible = filter != "";
            }
        }

        public ModelExtractor()
        {
            InitializeComponent();
        }

        #region Methods
        public void LoadModelTag(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            cache = Cache;
            tag = Tag;

            mode = DefinitionsManager.mode(cache, tag);

            if (mode.InstancedGeometryIndex != -1) mode.LoadRaw();

            lblName.Text = mode.Name;

            tvRegions.Nodes.Clear();
            foreach (var region in mode.Regions)
            {
                TreeNode node = new TreeNode(region.Name) { Checked = true, Tag = region };
                foreach (var perm in region.Permutations)
                {
                    if (perm.PieceIndex != -1)
                        if (mode.ModelSections[perm.PieceIndex].Submeshes.Count > 0 /*&& mode.ModelParts[perm.PieceIndex].TotalVertexCount > 0*/)
                            node.Nodes.Add(new TreeNode(perm.Name) { Checked = true, Tag = perm });
                }
                if(node.Nodes.Count > 0)
                    tvRegions.Nodes.Add(node);
            }
        }

        private void RecursiveExtract(object SaveFolder)
        {
            //var mode = DefinitionsManager.mode(cache, tag);

            List<CacheFile.IndexItem> tagsDone = new List<CacheFile.IndexItem>();

            foreach (render_model.Shader shader in mode.Shaders)
            {
                var rmshTag = cache.IndexItems.GetItemByID(shader.tagID);
                if (rmshTag == null) continue;
                var rmsh = DefinitionsManager.rmsh(cache, rmshTag);

                #region Halo 2 Extract
                if (cache.Version == DefinitionSet.Halo2Xbox)
                {
                    var h2rmsh = rmsh as Definitions.Halo2Xbox.shader;
                    for (int i = 0; i < h2rmsh.BitmIDs.Length; i++)
                    {
                        var bitmTag = cache.IndexItems.GetItemByID(h2rmsh.BitmIDs[i]);
                        if (bitmTag == null) continue;

                        //dont need to waste time extracting the same ones over and over
                        if (tagsDone.Contains(bitmTag)) continue;

                        try
                        {
                            BitmapExtractor.SaveAllImages((string)SaveFolder + "\\" + bitmTag.Filename, cache, bitmTag, DefaultBitmFormat, true);
                            TagExtracted(this, bitmTag);
                        }
                        catch (Exception ex) { ErrorExtracting(this, bitmTag, ex); }

                        tagsDone.Add(bitmTag);
                    }
                    continue;
                }
                #endregion

                foreach (Definitions.shader.ShaderProperties prop in rmsh.Properties)
                {
                    foreach (Definitions.shader.ShaderProperties.ShaderMap map in prop.ShaderMaps)
                    {
                        var bitmTag = cache.IndexItems.GetItemByID(map.BitmapTagID);
                        if (bitmTag == null) continue;

                        //dont need to waste time extracting the same ones over and over
                        if (tagsDone.Contains(bitmTag)) continue;

                        try
                        {
                            BitmapExtractor.SaveAllImages((string)SaveFolder + "\\" + bitmTag.Filename, cache, bitmTag, DefaultBitmFormat, true);
                            TagExtracted(this, bitmTag);
                        }
                        catch (Exception ex) { ErrorExtracting(this, bitmTag, ex); }

                        tagsDone.Add(bitmTag);
                    }
                }
            }
            FinishedRecursiveExtract(this, tag);
        }

        #region Static Methods

        /// <summary>
        /// Saves selected pieces of the model from a render_model tag to disk.
        /// </summary>
        /// <param name="Filename">The full path and filename to save to.</param>
        /// <param name="Cache">The CacheFile containing the render_model tag.</param>
        /// <param name="Tag">The render_model tag.</param>
        /// <param name="Format">The format to save the model in.</param>
        /// <param name="SplitMeshes">Whether to split the pieces into individual submeshes. Only applies when saving in EMF format.</param>
        /// <param name="PartIndices">A List containing the indices of the render_model.ModelParts to save.</param>
        public static void SaveModelParts(string Filename, CacheFile Cache, render_model Model, ModelFormat Format, List<int> PartIndices, bool SplitMeshes)
        {
            switch (Format)
            {
                case ModelFormat.EMF:
                    ModelFunctions.WriteEMF3(Filename, Cache, Model, SplitMeshes, PartIndices);
                    break;
                case ModelFormat.JMS:
                    ModelFunctions.WriteJMS(Filename, Cache, Model, PartIndices); 
                    break;
                case ModelFormat.OBJ:
                    ModelFunctions.WriteOBJ(Filename, Cache, Model, PartIndices);
                    break;
                case ModelFormat.AMF:
                    ModelFunctions.WriteAMF(Filename, Cache, Model, PartIndices);
                    break;
            }
        }

        /// <summary>
        /// Saves all pieces of the model from a render_model tag to disk.
        /// </summary>
        /// <param name="Filename">The full path and filename to save to.</param>
        /// <param name="Cache">The CacheFile containing the render_model tag.</param>
        /// <param name="Tag">The render_model tag.</param>
        /// <param name="Format">The format to save the model in.</param>
        /// <param name="SplitMeshes">Whether to split the pieces into individual submeshes. Only applies when saving in EMF format.</param>
        public static void SaveAllModelParts(string Filename, CacheFile Cache, CacheFile.IndexItem Tag, ModelFormat Format, bool SplitMeshes)
        {
            var mode = DefinitionsManager.mode(Cache, Tag);
            mode.LoadRaw();

            List<int> Parts = new List<int>();
            for (int i = 0; i < mode.ModelSections.Count; i++)
            {
                if (mode.ModelSections[i].Submeshes.Count > 0 /*&& mode.ModelParts[i].TotalVertexCount > 0*/)
                    Parts.Add(i);
            }

            SaveModelParts(Filename, Cache, mode, Format, Parts, SplitMeshes);
        }
        #endregion
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
        }

        private void tvRegions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvRegions.SelectedNode != null && tvRegions.SelectedNode.Parent != null)
                selectPermutationToolStripMenuItem.Visible = deselectPermutationToolStripMenuItem.Visible = true;
            else
                selectPermutationToolStripMenuItem.Visible = deselectPermutationToolStripMenuItem.Visible = false;
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
                Filter = "EMF Files|*.emf|OBJ Files|*.obj|AMF Files|*.amf|JMS Files|*.jms",
                FilterIndex = (int)DefaultModeFormat + 1,
                FileName = tag.Filename.Substring(tag.Filename.LastIndexOf("\\") + 1)
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            var format = (ModelFormat)(sfd.FilterIndex - 1);
            try
            {
                SaveModelParts(sfd.FileName, cache, mode, format, parts, chkSplit.Checked);
                TagExtracted(this, tag);
            }
            catch (Exception ex) { ErrorExtracting(this, tag, ex); }
        }

        private void btnExportBitmaps_Click(object sender, EventArgs e)
        {
            
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                SelectedPath = DataFolder,
                Description = "This will attempt to extract all bitmaps related to this model. The bitmaps will be placed in their tag paths using the selected folder as the root."
            };
            
            if (dialog.ShowDialog() != DialogResult.OK) return;

            //can take a while to do this, might as well thread it to prevent freezing
            ThreadPool.QueueUserWorkItem(RecursiveExtract, dialog.SelectedPath);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isWorking = true;
            foreach (TreeNode parent in tvRegions.Nodes)
            {
                foreach (TreeNode child in parent.Nodes)
                    child.Checked = true;

                parent.Checked = true;
            }
            isWorking = false;
        }

        private void selectNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isWorking = true;
            foreach (TreeNode parent in tvRegions.Nodes)
            {
                foreach (TreeNode child in parent.Nodes)
                    child.Checked = false;

                parent.Checked = false;
            }
            isWorking = false;
        }

        private void selectBDSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isWorking = true;
            foreach (TreeNode node in tvRegions.Nodes)
            {
                node.Checked = false;

                foreach (TreeNode child in node.Nodes)
                {
                    if (filter.Contains((child.Tag as render_model.Region.Permutation).Name))
                        child.Checked = node.Checked = true;
                    else child.Checked = false;
                }
            }
            isWorking = false;
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
        #endregion

        public event ErrorExtractingEventHandler ErrorExtracting;
        public event TagExtractedEventHandler TagExtracted;
        public event FinishedRecursiveExtractEventHandler FinishedRecursiveExtract;
    }
}

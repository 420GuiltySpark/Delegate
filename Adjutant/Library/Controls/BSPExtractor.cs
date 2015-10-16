using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.Definitions;
using System.Threading;
using System.IO;

namespace Adjutant.Library.Controls
{    
    public partial class BSPExtractor : UserControl
    {
        private CacheBase cache;
        private CacheBase.IndexItem tag;
        private scenario_structure_bsp sbsp;
        private bool isWorking = false;

        public ModelFormat DefaultModeFormat = ModelFormat.AMF;
        public BitmapFormat DefaultBitmFormat = BitmapFormat.TIF;

        public string DataFolder = "";

        public BSPExtractor()
        {
            InitializeComponent();
        }

        #region Methods
        public void LoadBSPTag(CacheBase Cache, CacheBase.IndexItem Tag)
        {
            cache = Cache;
            tag = Tag;

            sbsp = DefinitionsManager.sbsp(cache, tag);
            sbsp.BSPName = Path.GetFileNameWithoutExtension(tag.Filename + "." + tag.ClassCode);

            lblName.Text = sbsp.BSPName;
            if (cache.Version <= DefinitionSet.Halo2Vista) sbsp.LoadRaw();

            isWorking = true;
            tvRegions.Nodes.Clear();

            TreeNode ClusterNode = new TreeNode("Clusters") { Checked = true };
            foreach (var clust in sbsp.Clusters)
            {
                if (sbsp.ModelSections[clust.SectionIndex].Submeshes.Count > 0)
                    ClusterNode.Nodes.Add(new TreeNode(sbsp.Clusters.IndexOf(clust).ToString("D3")) { Tag = clust, Checked = true });
            }
            if (ClusterNode.Nodes.Count > 0)
                tvRegions.Nodes.Add(ClusterNode);

            TreeNode IGnode = new TreeNode("Instances") { Checked = true };
            foreach (var IG in sbsp.GeomInstances)
            {
                if (sbsp.ModelSections[IG.SectionIndex].Submeshes.Count > 0)
                    IGnode.Nodes.Add(new TreeNode(IG.Name) { Tag = IG, Checked = true });
            }
            if (IGnode.Nodes.Count > 0)
                tvRegions.Nodes.Add(IGnode);

            isWorking = false;
        }

        private void RecursiveExtract(object SaveFolder)
        {
            List<CacheBase.IndexItem> tagsDone = new List<CacheBase.IndexItem>();

            foreach (var shader in sbsp.Shaders)
            {
                var rmshTag = cache.IndexItems.GetItemByID(shader.tagID);

                if (rmshTag == null) continue;

                var rmsh = DefinitionsManager.rmsh(cache, rmshTag);

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
        /// Saves selected pieces of the model from a scenario_structure_bsp tag to disk.
        /// </summary>
        /// <param name="Filename">The full path and filename to save to.</param>
        /// <param name="Cache">The CacheFile containing the scenario_structure_bsp tag.</param>
        /// <param name="Tag">The scenario_structure_bsp tag.</param>
        /// <param name="Format">The format to save the model in.</param>
        /// <param name="ClusterIndices">A List containing the indices of the scenario_structure_bsp.Clusters to save.</param>
        /// <param name="InstanceIndices">A List containing the indices of the scenario_structure_bsp.GeomInstances to save.</param>
        public static void SaveBSPParts(string Filename, CacheBase Cache, scenario_structure_bsp BSP, ModelFormat Format, List<int> ClusterIndices, List<int> InstanceIndices)
        {
            switch (Format)
            {
                case ModelFormat.OBJ:
                    ModelFunctions.WriteOBJ(Filename, Cache, BSP, ClusterIndices, InstanceIndices);
                    break;
                case ModelFormat.EMF:
                    ModelFunctions.WriteEMF3(Filename, Cache, BSP, ClusterIndices, InstanceIndices);
                    break;
                default:
                    ModelFunctions.WriteAMF(Filename, Cache, BSP, ClusterIndices, InstanceIndices);
                    break;
            }
        }

        /// <summary>
        /// Saves all pieces of the model from a scenario_structure_bsp tag to disk.
        /// </summary>
        /// <param name="Filename">The full path and filename to save to.</param>
        /// <param name="Cache">The CacheFile containing the scenario_structure_bsp tag.</param>
        /// <param name="Tag">The scenario_structure_bsp tag.</param>
        /// <param name="Format">The format to save the model in.</param>
        public static void SaveAllBSPParts(string Filename, CacheBase Cache, CacheBase.IndexItem Tag, ModelFormat Format)
        {
            var sbsp = DefinitionsManager.sbsp(Cache, Tag);
            sbsp.LoadRaw();

            var clusters = new List<int>();
            var geoms = new List<int>();

            for (int i = 0; i < sbsp.Clusters.Count; i++)
                clusters.Add(i);

            for (int i = 0; i < sbsp.GeomInstances.Count; i++)
                geoms.Add(i);

            SaveBSPParts(Filename, Cache, sbsp, Format, clusters, geoms);
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
            if (tvRegions.SelectedNode != null && tvRegions.SelectedNode.Parent == null)
                selectAllChildrenToolStripMenuItem.Visible = true;
            else
                selectAllChildrenToolStripMenuItem.Visible = false;
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
                }
            }

            var sfd = new SaveFileDialog()
            {
                Filter = "EMF Files|*.emf|OBJ Files|*.obj|AMF Files|*.amf",
                FilterIndex = (int)DefaultModeFormat + 1,
                FileName = tag.Filename.Substring(tag.Filename.LastIndexOf("\\") + 1)
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            var format = (ModelFormat)(sfd.FilterIndex - 1);
            try
            {
                SaveBSPParts(sfd.FileName, cache, sbsp, format, clusts, igs);
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

        private void selectAllChildrenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isWorking = true;

            foreach (TreeNode cnode in tvRegions.SelectedNode.Nodes)
                cnode.Checked = true;

            tvRegions.SelectedNode.Checked = true;

            isWorking = false;
        }
        #endregion

        public event ErrorExtractingEventHandler ErrorExtracting;
        public event TagExtractedEventHandler TagExtracted;
        public event FinishedRecursiveExtractEventHandler FinishedRecursiveExtract;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.Cache;
using Adjutant.Library.Controls;
using Adjutant.Library.Definitions;

namespace Adjutant.Controls
{
    public partial class MapViewer : UserControl
    {
        private CacheFile cache;
        private TagViewer tv;
        private Settings settings;
        public List<string> ClassFilter = new List<string>();

        public string Filename;
        public bool HierarchyMode { get; private set; }

        public MapViewer(Settings Settings)
        {
            InitializeComponent();
            settings = Settings;
        }

        #region Methods
        public void LoadMap(string Filename, bool Hierarchy)
        {
            this.Filename = Filename;
            HierarchyMode = Hierarchy;

            cache = new CacheFile(Filename);

            //if (cache.Version == DefinitionSet.Halo4Retail && !settings.Reflex ||
            //    cache.Version == DefinitionSet.Halo3Beta   && !settings.Reflex)
            //{
            //    cache = null;
            //    throw new Exception();
            //}
            //else
            //{
                tv = new TagViewer(settings, extractor1);

                tvTags.Nodes.Clear();

                if (HierarchyMode)
                    LoadHierarchy();
                else
                    LoadClasses();
            //}
        }

        public void ReloadMap(bool Hierarchy)
        {
            HierarchyMode = Hierarchy;

            tv = new TagViewer(settings, extractor1);
            tvTags.Nodes.Clear();

            if (HierarchyMode)
                LoadHierarchy();
            else
                LoadClasses();
        }

        public void CloseMap()
        {
            if (cache == null) return;

            splitContainer2.Panel2.Controls.Clear();
            tv.Dispose();
            tvTags.Nodes.Clear();
            cache.Close();
            cache = null;
        }

        public void ViewStrings()
        {
            if (cache == null) throw new InvalidOperationException("Cannot load strings without a cache file!");

            splitContainer2.Panel2.Controls.Clear();
            var sv = new StringsViewer();
            sv.LoadStrings(cache);
            splitContainer2.Panel2.Controls.Add(sv);
            sv.Dock = DockStyle.Fill;
            sv.FixSize();
        }

        public void ViewLocales()
        {
            if (cache == null) throw new InvalidOperationException("Cannot load locales without a cache file!");

            splitContainer2.Panel2.Controls.Clear();
            var lv = new StringsViewer();
            lv.LoadLocales(cache);
            splitContainer2.Panel2.Controls.Add(lv);
            lv.Dock = DockStyle.Fill;
            lv.FixSize();
        }

        public void SearchTags(string Key)
        {
            if (HierarchyMode)
                SearchHierarchy(Key);
            else
                SearchClasses(Key);
        }

        private void LoadClasses()
        {
            var nList = new List<TreeNode>();
            var dic = new Dictionary<string, TreeNode>();

            foreach (CacheFile.IndexItem tag in cache.IndexItems)
            {
                if (settings.Flags.HasFlag(SettingsFlags.UseClassFilter) && ClassFilter.Count > 0)
                {
                    if (!(ClassFilter.Contains(tag.ClassCode) || ClassFilter.Contains(tag.ClassName)))
                        continue;
                }

                if (tag.ClassCode == "____") continue;

                TreeNode n;

                if (dic.TryGetValue(tag.ClassCode, out n))
                    n.Nodes.Add(new TreeNode(tag.Filename) { Name = tag.Filename, Tag = tag, ImageIndex = 1, SelectedImageIndex = 1 });
                else
                {
                    n = new TreeNode(settings.Flags.HasFlag(SettingsFlags.ShortClassNames) ? tag.ClassCode : tag.ClassName) 
                    { Name = settings.Flags.HasFlag(SettingsFlags.ShortClassNames) ? tag.ClassCode : tag.ClassName, ImageIndex = 0, SelectedImageIndex = 0 };
                    n.Nodes.Add(new TreeNode(tag.Filename) { Name = tag.Filename, Tag = tag, ImageIndex = 1, SelectedImageIndex = 1 });
                    nList.Add(n);
                    dic.Add(tag.ClassCode, n);
                }
            }

            tvTags.Nodes.Clear();
            tvTags.Nodes.AddRange(nList.ToArray());

            if (settings.Flags.HasFlag(SettingsFlags.SortTags))
                tvTags.Sort();
        }

        private void LoadHierarchy()
        {
            List<TreeNode> tree = new List<TreeNode>();
            Dictionary<string, TreeNode> path_dic = new Dictionary<string, TreeNode>();

            foreach (CacheFile.IndexItem tag in cache.IndexItems)
            {
                if (settings.Flags.HasFlag(SettingsFlags.UseClassFilter) && ClassFilter.Count > 0)
                {
                    if (!(ClassFilter.Contains(tag.ClassCode) || ClassFilter.Contains(tag.ClassName)))
                        continue;
                }

                if (tag.ClassCode == "____") continue;

                var path = tag.Filename.Split('\\');
                var tagName = path[path.Length - 1];
                var node = new TreeNode(path[0]) { Name = path[0] };

                if (path.Length == 1)
                {
                    node.Text = node.Name = path[0] + "." + tag.ClassCode;
                    node.Tag = tag;
                    node.ImageIndex = node.SelectedImageIndex = 1;
                    tree.Add(node);
                    continue;
                }

                if (!path_dic.TryGetValue(path[0], out node))
                {
                    node = new TreeNode(path[0]) { Name = path[0], ImageIndex = 0, SelectedImageIndex = 0 };
                    path_dic.Add(path[0], node);
                    tree.Add(node);
                }

                var current = path[0];

                for (int i = 1; i < path.Length; i++)
                {
                    current += "\\" + path[i];
                    try { node = node.Nodes.Find(current, false)[0]; }
                    catch
                    {
                        node = node.Nodes.Add(path[i]);
                        node.Name = current;
                        node.ImageIndex = node.SelectedImageIndex = 0;
                    }
                }

                node.Name = node.Text = current + "." + tag.ClassCode;
                node.ImageIndex = node.SelectedImageIndex = 1;
                node.Tag = tag;
            }

            tvTags.Nodes.Clear();
            tvTags.Nodes.AddRange(tree.ToArray());

            if (settings.Flags.HasFlag(SettingsFlags.SortTags))
                tvTags.Sort();
        }

        private void SearchClasses(string key)
        {
            if (key == "")
            {
                LoadClasses();
                return;
            }

            var nList = new List<TreeNode>();
            var dic = new Dictionary<string, TreeNode>();

            foreach (CacheFile.IndexItem tag in cache.IndexItems)
            {
                if (settings.Flags.HasFlag(SettingsFlags.UseClassFilter) && ClassFilter.Count > 0)
                {
                    if (!(ClassFilter.Contains(tag.ClassCode) || ClassFilter.Contains(tag.ClassName)))
                        continue;
                }

                bool match = false;
                string[] parts = key.Split(' ');
                foreach (string part in parts)
                {
                    if (part != "")
                        match = tag.Filename.ToLower().Contains(part.ToLower());
                    if (!match) break;
                }

                if (!match) continue;

                //if (!tag.Filename.ToLower().Contains(key.ToLower())) continue;

                TreeNode n;

                if (dic.TryGetValue(tag.ClassCode, out n))
                    n.Nodes.Add(new TreeNode(tag.Filename) { Name = tag.Filename, Tag = tag, ImageIndex = 1, SelectedImageIndex = 1 });
                else
                {
                    n = new TreeNode(settings.Flags.HasFlag(SettingsFlags.ShortClassNames) ? tag.ClassCode : tag.ClassName) { Name = settings.Flags.HasFlag(SettingsFlags.ShortClassNames) ? tag.ClassCode : tag.ClassName, ImageIndex = 0, SelectedImageIndex = 0 };
                    n.Nodes.Add(new TreeNode(tag.Filename) { Name = tag.Filename, Tag = tag, ImageIndex = 1, SelectedImageIndex = 1 });
                    nList.Add(n);
                    dic.Add(tag.ClassCode, n);
                }
            }

            tvTags.Nodes.Clear();
            tvTags.Nodes.AddRange(nList.ToArray());

            if (settings.Flags.HasFlag(SettingsFlags.SortTags))
                tvTags.Sort();
        }

        private void SearchHierarchy(string key)
        {
            if (key == "")
            {
                LoadHierarchy();
                return;
            }

            List<TreeNode> tree = new List<TreeNode>();
            Dictionary<string, TreeNode> path_dic = new Dictionary<string, TreeNode>();

            foreach (CacheFile.IndexItem tag in cache.IndexItems)
            {
                if (settings.Flags.HasFlag(SettingsFlags.UseClassFilter) && ClassFilter.Count > 0)
                {
                    if (!(ClassFilter.Contains(tag.ClassCode) || ClassFilter.Contains(tag.ClassName)))
                        continue;
                }

                bool match = false;
                string[] parts = key.Split(' ');
                foreach (string part in parts)
                {
                    if (part != "")
                        match = tag.Filename.ToLower().Contains(part.ToLower());
                    if (!match) break;
                }

                if (!match) continue;

                //if (!tag.Filename.ToLower().Contains(key.ToLower())) continue;

                var path = tag.Filename.Split('\\');
                var tagName = path[path.Length - 1];
                var node = new TreeNode(path[0]) { Name = path[0] };

                if (path.Length == 1)
                {
                    node.Text = node.Name = path[0] + "." + tag.ClassCode;
                    node.Tag = tag;
                    node.ImageIndex = node.SelectedImageIndex = 1;
                    tree.Add(node);
                    continue;
                }

                if (!path_dic.TryGetValue(path[0], out node))
                {
                    node = new TreeNode(path[0]) { Name = path[0], ImageIndex = 0, SelectedImageIndex = 0 };
                    path_dic.Add(path[0], node);
                    tree.Add(node);
                }

                var current = path[0];

                for (int i = 1; i < path.Length; i++)
                {
                    current += "\\" + path[i];
                    try { node = node.Nodes.Find(current, false)[0]; }
                    catch
                    {
                        node = node.Nodes.Add(path[i]);
                        node.Name = current;
                        node.ImageIndex = node.SelectedImageIndex = 0;
                    }
                }

                node.Name = node.Text = current + "." + tag.ClassCode;
                node.ImageIndex = node.SelectedImageIndex = 1;
                node.Tag = tag;
            }

            tvTags.Nodes.Clear();
            tvTags.Nodes.AddRange(tree.ToArray());

            if (settings.Flags.HasFlag(SettingsFlags.SortTags))
                tvTags.Sort();
        }
        #endregion

        #region Events
        private void tvTags_AfterSelect(object sender, TreeViewEventArgs e)
        {
            extractToolStripMenuItem.Visible = tvTags.SelectedNode.Nodes.Count > 0;

            if (tvTags.SelectedNode.Nodes.Count > 0) return;

            var tag = tvTags.SelectedNode.Tag as CacheFile.IndexItem;

            if (!splitContainer2.Panel2.Controls.Contains(tv))
            {
                splitContainer2.Panel2.Controls.Clear();
                splitContainer2.Panel2.Controls.Add(tv);
                tv.Dock = DockStyle.Fill;
            }

            tv.LoadTag(cache, tag);
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dest = settings.dataFolder;

            if (!settings.Flags.HasFlag(SettingsFlags.QuickExtract))
            {
                var fbd = new FolderBrowserDialog()
                {
                    Description = "Select your data folder",
                    SelectedPath = dest
                };

                if (fbd.ShowDialog() != DialogResult.OK) return;

                dest = fbd.SelectedPath + "\\";
            }

            extractor1.BeginExtraction(cache, tvTags.SelectedNode, settings, dest);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(tvTags.SelectedNode.Name);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvTags_AfterSelect(null, null);
        }

        #region Search Box
        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if ((string)txtSearch.Tag == "0" || txtSearch.ForeColor == Color.DimGray)
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if ((string)txtSearch.Tag == "0")
            {
                txtSearch.Text = "Search...";
                txtSearch.ForeColor = Color.DimGray;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.ForeColor == Color.DimGray || txtSearch.Text == "")
                txtSearch.Tag = "0";
            else
                txtSearch.Tag = "-1";
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                SearchTags(txtSearch.Text);
        }
        #endregion
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.S3D;
using Adjutant.Library.Cache;
using Adjutant.Library.Controls;
using Adjutant.Library.Definitions;

namespace Adjutant.Controls
{
    public partial class MapViewer : UserControl
    {
        private PakFile pak;
        private CacheFile cache;
        private TagViewer tv;
        private Settings settings;
        public List<string> ClassFilter = new List<string>();

        public string Filename;
        public bool HierarchyMode { get; private set; }
        public bool isCache;

        public MapViewer(Settings Settings)
        {
            InitializeComponent();
            settings = Settings;
        }

        #region Methods
        public void LoadMap(string Filename, bool Hierarchy)
        {
            if (cache != null) cache.Close();
            cache = null;
            if (pak != null) pak.Close();
            pak = null;

            this.Filename = Filename;
            HierarchyMode = Hierarchy;

            cache = new CacheFile(Filename);
            tv = new TagViewer(settings, extractor1);

            tvTags.Nodes.Clear();

            if (HierarchyMode)
                LoadHierarchy();
            else
                LoadClasses();

            isCache = true;
        }

        public void LoadPak(string Filename)
        {
            if (cache != null) cache.Close();
            cache = null;
            if (pak != null) pak.Close();
            pak = null;

            this.Filename = Filename;
            HierarchyMode = false;
 
            pak = new PakFile(Filename);
            tv = new TagViewer(settings, extractor1);

            tvTags.Nodes.Clear();

            LoadPakItems();

            isCache = false;
        }

        public void ReloadMap(bool Hierarchy)
        {
            if (cache == null) return;

            HierarchyMode = Hierarchy;

            extractor1.CancelExtraction();
            tv = new TagViewer(settings, extractor1);
            tvTags.Nodes.Clear();

            if (HierarchyMode)
                LoadHierarchy();
            else
                LoadClasses();
        }

        public void CloseFile()
        {
            if (cache == null && pak == null) return;

            extractor1.CancelExtraction();
            splitContainer2.Panel2.Controls.Clear();
            tv.Dispose();
            tvTags.Nodes.Clear();
            if (cache != null) cache.Close();
            if (pak != null) pak.Close();
            cache = null;
            pak = null;
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
            if (pak != null)
            {
                SearchPakItems(Key);
                return;
            }

            if (HierarchyMode)
                SearchHierarchy(Key);
            else
                SearchClasses(Key);
        }

        public void LoadPakItems()
        {
            var nList = new List<TreeNode>();
            var dic = new Dictionary<int, TreeNode>();

            foreach (var item in pak.PakItems)
            {
                TreeNode pnode;
                var node = new TreeNode(item.Name) { Name = item.Name, Tag = item, ImageIndex = 1, SelectedImageIndex = 1 };

                if (dic.TryGetValue(item.unk0, out pnode))
                    pnode.Nodes.Add(node);
                else
                {
                    pnode = new TreeNode(item.unk0.ToString("D2")+ " [" + item.Type.ToString() + "]") 
                    { Name = item.unk0.ToString(), ImageIndex = 0, SelectedImageIndex = 0 };
                    pnode.Nodes.Add(node);
                    nList.Add(pnode);
                    dic.Add(item.unk0, pnode);
                }
            }

            tvTags.Nodes.Clear();
            tvTags.Nodes.AddRange(nList.ToArray());

            if (settings.Flags.HasFlag(SettingsFlags.SortTags))
                tvTags.Sort();
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

                var path = tag.Filename.Split('\\');
                var tagName = path[path.Length - 1];
                var node = new TreeNode(path[0]) { Name = path[0] };

                if (path.Length == 1)
                {
                    node.Text = node.Name = path[0] + "." + tag.ClassCode;
                    node.Tag = tag;
                    node.ImageIndex = node.SelectedImageIndex = 1;
                    nList.Add(node);
                    continue;
                }

                if (!dic.TryGetValue(path[0], out node))
                {
                    node = new TreeNode(path[0]) { Name = path[0], ImageIndex = 0, SelectedImageIndex = 0 };
                    dic.Add(path[0], node);
                    nList.Add(node);
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
            tvTags.Nodes.AddRange(nList.ToArray());

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

            List<TreeNode> nList = new List<TreeNode>();
            Dictionary<string, TreeNode> dic = new Dictionary<string, TreeNode>();

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
                    nList.Add(node);
                    continue;
                }

                if (!dic.TryGetValue(path[0], out node))
                {
                    node = new TreeNode(path[0]) { Name = path[0], ImageIndex = 0, SelectedImageIndex = 0 };
                    dic.Add(path[0], node);
                    nList.Add(node);
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
            tvTags.Nodes.AddRange(nList.ToArray());

            if (settings.Flags.HasFlag(SettingsFlags.SortTags))
                tvTags.Sort();
        }

        private void SearchPakItems(string key)
        {
            if (key == "")
            {
                LoadPakItems();
                return;
            }

            var nList = new List<TreeNode>();
            var dic = new Dictionary<int, TreeNode>();

            foreach (var item in pak.PakItems)
            {
                bool match = false;
                string[] parts = key.Split(' ');
                foreach (string part in parts)
                {
                    if (part != "")
                        match = item.Name.ToLower().Contains(part.ToLower());
                    if (!match) break;
                }

                if (!match) continue;

                TreeNode pnode;
                var node = new TreeNode(item.Name) { Name = item.Name, Tag = item, ImageIndex = 1, SelectedImageIndex = 1 };

                if (dic.TryGetValue(item.unk0, out pnode))
                    pnode.Nodes.Add(node);
                else
                {
                    pnode = new TreeNode(item.unk0.ToString("D2") + " [" + item.Type.ToString() + "]")
                    { Name = item.unk0.ToString(), ImageIndex = 0, SelectedImageIndex = 0 };
                    pnode.Nodes.Add(node);
                    nList.Add(pnode);
                    dic.Add(item.unk0, pnode);
                }
            }

            tvTags.Nodes.Clear();
            tvTags.Nodes.AddRange(nList.ToArray());

            if (settings.Flags.HasFlag(SettingsFlags.SortTags))
                tvTags.Sort();
        }
        #endregion

        #region Events
        private void tvTags_AfterSelect(object sender, TreeViewEventArgs e)
        {
            extractSelectedToolStripMenuItem.Visible = tvTags.SelectedNode.Nodes.Count > 0;
            dumpFileToolStripMenuItem.Visible = (!isCache && tvTags.SelectedNode.Nodes.Count == 0);
            dumpFolderToolStripMenuItem.Visible = (!isCache && tvTags.SelectedNode.Nodes.Count > 0);

            if (tvTags.SelectedNode.Nodes.Count > 0) return;

            var tag = tvTags.SelectedNode.Tag as CacheFile.IndexItem;
            var item = tvTags.SelectedNode.Tag as PakFile.PakTag;

            if (!splitContainer2.Panel2.Controls.Contains(tv))
            {
                splitContainer2.Panel2.Controls.Clear();
                splitContainer2.Panel2.Controls.Add(tv);
                tv.Dock = DockStyle.Fill;
            }

            if (cache != null)
                tv.LoadTag(cache, tag);
            else if (item != null)
                tv.LoadPakItem(pak, item);
        }

        private void tvTags_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //left click will select the node by default
            //anyway but since this activates regardless
            //of mouse button it ensures right-click selects
            //the node before showing the context menu
            tvTags.SelectedNode = e.Node;
        }

        private void extractSelectedToolStripMenuItem_Click(object sender, EventArgs e)
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

            if (isCache) extractor1.BeginExtraction(cache, new List<TreeNode>() { tvTags.SelectedNode }, settings, dest);
            else extractor1.BeginExtraction(pak, new List<TreeNode>() { tvTags.SelectedNode }, settings, dest);
        }

        private void extractAllToolStripMenuItem_Click(object sender, EventArgs e)
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

            var nodeList = new List<TreeNode>();

            foreach (TreeNode node in tvTags.Nodes)
                if (node.Nodes.Count > 0) nodeList.Add(node);

            if (isCache) extractor1.BeginExtraction(cache, nodeList, settings, dest);
            else extractor1.BeginExtraction(pak, nodeList, settings, dest);
        }

        private void dumpFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = tvTags.SelectedNode.Tag as PakFile.PakTag;

            var sfd = new SaveFileDialog()
            {
                FileName = item.Name,
                Filter = "Binary Files|*.bin"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            var reader = pak.Reader;

            reader.SeekTo(item.Offset);
            var data = reader.ReadBytes(item.Size);

            File.WriteAllBytes(sfd.FileName, data);

            MessageBox.Show("Done!");
        }

        private void dumpFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dest = settings.dataFolder;

            var fbd = new FolderBrowserDialog()
            {
                Description = "Select your data folder",
                SelectedPath = dest
            };

            if (fbd.ShowDialog() != DialogResult.OK) return;

            dest = fbd.SelectedPath + "\\";

            foreach (TreeNode node in tvTags.SelectedNode.Nodes)
            {
                var item = node.Tag as PakFile.PakTag;

                var reader = pak.Reader;

                reader.SeekTo(item.Offset);
                var data = reader.ReadBytes(item.Size);

                File.WriteAllBytes(dest + item.Name + ".bin", data);
            }

            MessageBox.Show("Done!");
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

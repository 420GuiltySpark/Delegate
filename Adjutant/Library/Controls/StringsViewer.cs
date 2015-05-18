using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;

namespace Adjutant.Library.Controls
{
    public partial class StringsViewer : UserControl
    {
        private CacheFile cache;
        private CacheFile.IndexItem tag;
        private multilingual_unicode_string_list unic;
        private List<string> sList;

        public StringsViewer()
        {
            InitializeComponent();
        }

        #region Load Strings
        public void LoadStrings(CacheFile Cache)
        {
            cache = Cache;
            tag = null;

            label1.Enabled = cmbLang.Enabled = false;
            
            var strings = sList = cache.Strings;

            lstStrings.Items.Clear();
            for (int i = 0; i < strings.Count; i++)
                lstStrings.Items.Add(new ListViewItem(new string[] { i.ToString("D6"), strings[i] }));
        }
        #endregion

        #region Load Locales
        public void LoadLocales(CacheFile Cache)
        {
            LoadLocales(Cache, Language.English);
        }

        public void LoadLocales(CacheFile Cache, Language Language)
        {
            cache = Cache;
            tag = null;

            label1.Enabled = cmbLang.Enabled = true;

            cmbLang.SelectedIndex = -1;
            cmbLang.SelectedIndex = (int)Language;
        }
        #endregion

        #region Load Unic Tags
        public void LoadUnicTag(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            LoadUnicTag(Cache, Tag, Language.English);
        }

        public void LoadUnicTag(CacheFile Cache, CacheFile.IndexItem Tag, Language Language)
        {
            cache = Cache;
            tag = Tag;

            label1.Enabled = cmbLang.Enabled = true;

            cmbLang.SelectedIndex = -1;

            var reader = Cache.Reader;
            unic = DefinitionsManager.unic(Cache, tag);

            cmbLang.SelectedIndex = (int)Language;
        }
        #endregion

        #region Static Methods
        public static List<string> GetUnicStrings(CacheFile Cache, CacheFile.IndexItem Tag, Language Language)
        {
            List<string> strings = new List<string>();

            var reader = Cache.Reader;
            var unic = DefinitionsManager.unic(Cache, Tag);

            int index = unic.Indices[(int)Language];
            int length = unic.Lengths[(int)Language];

            for (int i = index; i < (index + length); i++)
                strings.Add(Cache.LocaleTables[(int)Language][i]);


            return strings;
        }

        public static void SaveUnicStrings(string Filename, CacheFile Cache, CacheFile.IndexItem Tag, Language Language)
        {
            List<string> sList = new List<string>();

            var reader = Cache.Reader;
            var unic = DefinitionsManager.unic(Cache, Tag);

            int index = unic.Indices[(int)Language];
            int length = unic.Lengths[(int)Language];

            for (int i = index; i < (index + length); i++)
                sList.Add(Cache.LocaleTables[(int)Language][i]);

            if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();
            if (!Filename.EndsWith(".txt")) Filename += ".txt";

            var fs = new FileStream(Filename, FileMode.Create);
            int start = unic.Indices[(int)Language];

            for (int i = 0; i < sList.Count; i++)
            {
                string line = (i + start).ToString("D6") + "\t" + sList[i].Replace("\r\n", " ") + "\r\n";
                byte[] buffer = Encoding.UTF8.GetBytes(line);
                fs.Write(buffer, 0, buffer.Length);
            }

            fs.Close();
            fs.Dispose();
        }
        #endregion

        #region Events
        private void cmbLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!cmbLang.Enabled)
            {
                var strings = sList = cache.Strings;
                lstStrings.Items.Clear();
                for (int i = 0; i < strings.Count; i++)
                    lstStrings.Items.Add(new ListViewItem(new string[] { i.ToString("D6"), strings[i] }));
            }
            
            if (cmbLang.SelectedIndex == -1) return;
            
            if (cmbLang.SelectedIndex >= cache.LocaleTables.Count)
            {
                lstStrings.Items.Clear();
                return;
            }

            if (tag != null)
            {
                var strings = sList = GetUnicStrings(cache, tag, (Language)cmbLang.SelectedIndex);

                if (txtSearch.Text != "")
                {
                    txtSearch_TextChanged(null, null);
                    return;
                }

                lstStrings.Items.Clear();
                for (int i = 0; i < strings.Count; i++)
                    lstStrings.Items.Add(new ListViewItem(new string[] { (i + unic.Indices[cmbLang.SelectedIndex]).ToString("D6"), strings[i] }));
            }
            else
            {
                var strings = sList = cache.LocaleTables[cmbLang.SelectedIndex];

                if (txtSearch.Text != "")
                {
                    txtSearch_TextChanged(null, null);
                    return;
                }

                lstStrings.Items.Clear();
                for (int i = 0; i < strings.Count; i++)
                    lstStrings.Items.Add(new ListViewItem(new string[] { i.ToString("D6"), strings[i] }));
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text == "")
            {
                cmbLang_SelectedIndexChanged(null, null);
                return;

            }

            int start = (tag == null) ? 0 : unic.Indices[cmbLang.SelectedIndex];

            lstStrings.Items.Clear();
            for (int i = 0; i < sList.Count; i++)
            {
                bool match = false;
                string[] parts = txtSearch.Text.Split(' ');
                foreach (string part in parts)
                {
                    if (part != "")
                        match = sList[i].ToLower().Contains(part.ToLower());
                    if (!match) break;
                }

                if (!match) continue;

                string[] item = new string[2];
                item[0] = (i + start).ToString("D6");
                item[1] = sList[i];
                lstStrings.Items.Add(new ListViewItem(item));
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog() { Filter = "Text Files|*.txt|All Files|*.*" };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            var fs = new FileStream(sfd.FileName, FileMode.Create);
            int start = (tag == null) ? 0 : unic.Indices[cmbLang.SelectedIndex];

            for (int i = 0; i < sList.Count; i++)
            {
                string line = (i + start).ToString("D6") + "\t" + sList[i].Replace("\r\n", " ") + "\r\n";
                byte[] buffer = Encoding.UTF8.GetBytes(line);
                fs.Write(buffer, 0, buffer.Length);
            }

            fs.Close();
            fs.Dispose();

            MessageBox.Show("Done!");
        }

        private void lstStrings_ClientSizeChanged(object sender, EventArgs e)
        {
            FixSize();
        }
        #endregion

        public void FixSize()
        {
            lstStrings.Columns[1].Width = lstStrings.ClientSize.Width - 46;
        }
    }
}

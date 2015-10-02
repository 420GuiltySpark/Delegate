using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using System.IO;
using System.Diagnostics;
using Composer;
using Composer.Wwise;
using bink = Adjutant.Library.Definitions.bink;

namespace Adjutant.Library.Controls
{
    public partial class BinkExtractor : UserControl
    {
        private CacheBase cache;
        private CacheBase.IndexItem tag;

        public BinkExtractor()
        {
            InitializeComponent();
        }

        public void LoadBinkTag(CacheBase Cache, CacheBase.IndexItem Tag)
        {
            cache = Cache;
            tag = Tag;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var fName = tag.Filename.Substring(tag.Filename.LastIndexOf('\\') + 1);

                var sfd = new SaveFileDialog()
                {
                    FileName = fName,
                    Filter = "BIK Files|*.bik",
                };

                if (sfd.ShowDialog() != DialogResult.OK) return;

                SaveBink(sfd.FileName, cache, tag);
                TagExtracted(this, tag);
            }
            catch (Exception ex)
            {
                ErrorExtracting(this, tag, ex);
            }

        }

        public static void SaveBink(string Filename, CacheBase Cache, CacheBase.IndexItem Tag)
        {
            var bik = DefinitionsManager.bink(Cache, Tag);
            var raw = Cache.GetRawFromID(bik.RawID);

            if (!Filename.EndsWith(".bik")) Filename += ".bik";

            if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();

            var fs = new FileStream(Filename, FileMode.Create, FileAccess.Write);
            var bw = new BinaryWriter(fs);

            for (int i = 0; i < (raw.Length); i += 4)
                Array.Reverse(raw, i, 4);

            bw.Write(raw);

            bw.Close();
            bw.Dispose();
        }

        public event TagExtractedEventHandler TagExtracted;
        public event ErrorExtractingEventHandler ErrorExtracting;
    }
}

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

namespace Adjutant.Library.Controls.MetaViewerControls
{
    public delegate void RequestTagLoadEventHandler(int tagID);

    internal partial class mTagRef : MetaViewerControl
    {
        private int val = -1;

        public mTagRef(iValue Value, CacheBase Cache)
        {
            InitializeComponent();
            value = Value;
            cache = Cache;

            lblName.Text = value.Node.Attributes["name"].Value;
        }

        public override void Reload(int ParentAddress)
        {
            var reader = cache.Reader;

            int offset;

            try { offset = int.Parse(value.Node.Attributes["offset"].Value); }
            catch { offset = Convert.ToInt32(value.Node.Attributes["offset"].Value, 16); }

            reader.BaseStream.Position = ParentAddress + offset;

            switch (value.Type)
            {
                case iValue.ValueType.TagRef:
                    reader.BaseStream.Position += (cache.Version >= DefinitionSet.Halo3Beta) ? 12 : 4; //doesn't always work from here, use the tag class instead

                    try
                    {
                        val = reader.ReadInt32();
                        var tag = cache.IndexItems.GetItemByID(val);
                        txtClass.Text = tag.ClassCode;
                        txtPath.Text = tag.Filename;
                    }
                    catch { txtClass.Text = txtPath.Text = ""; }
                    
                    break;

                default:
                    throw new InvalidOperationException("Cannot load " + value.Type.ToString() + " values using mTagRef.");
            }
        }

        private void loadTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RequestTagLoad(val);
        }

        public event RequestTagLoadEventHandler RequestTagLoad;
    }
}
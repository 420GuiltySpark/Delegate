using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Adjutant.Library.Cache;

namespace Adjutant.Library.Controls.MetaViewerControls
{
    internal partial class mValue : MetaViewerControl
    {
        public int val = -1;
        public mValue(iValue Value, CacheFile Cache)
        {
            InitializeComponent();
            value = Value;
            cache = Cache;

            ContextMenuStrip = null;
            lblName.Text = value.Node.Attributes["name"].Value;

            try { lblDesc.Text = value.Node.Attributes["desc"].Value; }
            catch { lblDesc.Text = ""; }
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
                case iValue.ValueType.Int8:
                    txtValue.Text = reader.ReadByte().ToString();
                    break;

                case iValue.ValueType.Int16:
                    txtValue.Text = reader.ReadInt16().ToString();
                    break;

                case iValue.ValueType.Int32:
                case iValue.ValueType.Undefined:
                    txtValue.Text = reader.ReadInt32().ToString();
                    break;

                case iValue.ValueType.Float32:
                    txtValue.Text = reader.ReadSingle().ToString();
                    break;

                case iValue.ValueType.UInt16:
                    txtValue.Text = reader.ReadUInt16().ToString();
                    break;

                case iValue.ValueType.UInt32:
                    txtValue.Text = reader.ReadUInt32().ToString();
                    break;

                case iValue.ValueType.RawID:
                    val = reader.ReadInt32();
                    ContextMenuStrip = contextMenuStrip1;
                    txtValue.Text = val.ToString();
                    break;

                default:
                    throw new InvalidOperationException("Cannot load " + value.Type.ToString() + " values using mValue.");
            }
        }

        private void mValue_DoubleClick(object sender, EventArgs e)
        {
            if(value.Type == iValue.ValueType.RawID)
                txtValue.Text = (txtValue.Text == val.ToString()) ? (val & ushort.MaxValue).ToString() : val.ToString();
        }

        private void dumpRawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog()
            {
                Filter = "Binary Files|*.bin"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            var data = cache.GetRawFromID(val);
            File.WriteAllBytes(sfd.FileName, data);

            MessageBox.Show("Done!");
        }
    }
}

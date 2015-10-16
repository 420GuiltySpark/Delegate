using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.Definitions;
using System.Xml;

namespace Adjutant.Library.Controls.MetaViewerControls
{
    internal partial class mEnum : MetaViewerControl
    {
        private int index;

        public mEnum(iValue Value, CacheBase Cache)
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

            reader.SeekTo(ParentAddress + offset);

            int count;

            switch (value.Type)
            {
                case iValue.ValueType.Enum8:
                    count = byte.MaxValue;
                    index = reader.ReadByte();
                    break;

                case iValue.ValueType.Enum16:
                    count = short.MaxValue;
                    index = reader.ReadInt16();
                    break;

                case iValue.ValueType.Enum32:
                    count = int.MaxValue;
                    index = reader.ReadInt32();
                    break;

                default:
                    throw new InvalidOperationException("Cannot load " + value.Type.ToString() + " values using mEnum.");
            }

            var dic = new Dictionary<int, string>();

            foreach (XmlNode n in value.Node.ChildNodes)
            {
                if (n.Name.ToLower() == "option")
                {
                    int val;
                    try { val = int.Parse(n.Attributes["value"].Value); }
                    catch { val = Convert.ToInt32(n.Attributes["value"].Value, 16); }
                    dic.Add(val, n.Attributes["name"].Value);
                }
            }

            try { count = int.Parse(value.Node.Attributes["count"].Value); }
            catch { }


            string str;
            cmbOptions.Items.Clear();

            if (dic.TryGetValue(index, out str))
                cmbOptions.Items.Add(str);
            else
                cmbOptions.Items.Add("<undefined> (" + index.ToString() + ")");

            cmbOptions.SelectedIndex = 0;
        }

        private void cmbOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (cmbOptions.SelectedIndex != index)
            //    cmbOptions.SelectedIndex = index;
        }
    }
}

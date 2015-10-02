using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.DataTypes;

namespace Adjutant.Library.Controls.MetaViewerControls
{
    internal partial class mBitmask : MetaViewerControl
    {
        private bool isLoaded = false;

        public mBitmask(iValue Value, CacheBase Cache)
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

            Bitmask mask;

            switch (value.Type)
            {
                case iValue.ValueType.Bitmask8:
                    mask = new Bitmask(reader.ReadByte());
                    break;

                case iValue.ValueType.Bitmask16:
                    mask = new Bitmask(reader.ReadInt16());
                    break;

                case iValue.ValueType.Bitmask32:
                    mask = new Bitmask(reader.ReadInt32());
                    break;

                default:
                    throw new InvalidOperationException("Cannot load " + value.Type.ToString() + " values using mBitmask.");
            }

            if (isLoaded)
            {
                for (int i = 0; i < mask.Values.Length; i++)
                    clbOptions.SetItemChecked(i, mask.Values[i]);
                return;
            }

            Dictionary<int, string> dic = new Dictionary<int, string>();

            foreach (XmlNode n in value.Node.ChildNodes)
            {
                if(n.Name.ToLower() == "option")
                    dic.Add(int.Parse(n.Attributes["value"].Value), n.Attributes["name"].Value);
            }

            int count;
            try { count = int.Parse(value.Node.Attributes["count"].Value); }
            catch { count = mask.Values.Length; }

            clbOptions.Items.Clear();
            for (int i = 0; i < mask.Values.Length; i++)
            {
                if (i == count)
                    break;

                string label;
                if (!dic.TryGetValue(i, out label)) label = "Bit #" + i.ToString("D2");
                
                clbOptions.Items.Add(label, mask.Values[i]);

                if (clbOptions.Items.Count <= 8)
                {
                    this.Height = 15 * (i + 1) + 13;
                    clbOptions.Height = 15 * (i + 1) + 4;
                }
            }

            isLoaded = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.Cache;
using Adjutant.Library.Imaging;
using Adjutant.Library.DataTypes;

namespace Adjutant.Library.Controls.MetaViewerControls
{
    internal partial class mMultiValue : MetaViewerControl
    {
        public mMultiValue(iValue Value, CacheBase Cache)
        {
            InitializeComponent();
            value = Value;
            cache = Cache;

            lblName.Text = value.Node.Attributes["name"].Value;
            try { lblDesc.Text = value.Node.Attributes["desc"].Value; }
            catch { lblDesc.Text = ""; }
        }

        public override void Reload(int ParentAddress)
        {
            foreach (Control c in this.Controls)
                c.Visible = true;

            var reader = cache.Reader;

            int offset;

            try { offset = int.Parse(value.Node.Attributes["offset"].Value); }
            catch { offset = Convert.ToInt32(value.Node.Attributes["offset"].Value, 16); }

            reader.SeekTo(ParentAddress + offset);

            switch (value.Type)
            {
                case iValue.ValueType.ShortBounds:
                    var sbounds = new RealBounds(reader.ReadInt16(), reader.ReadInt16());

                    lblA.Text = "";
                    txtA.Text = sbounds.Min.ToString();
                    lblB.Text = "to";
                    txtB.Text = sbounds.Max.ToString();

                    lblC.Visible = txtC.Visible = lblD.Visible = txtD.Visible = btnColour.Visible = false;

                    lblDesc.Visible = true;
                    break;

                case iValue.ValueType.RealBounds:
                    var rbounds = new RealBounds(reader.ReadSingle(), reader.ReadSingle());
                    
                    lblA.Text = "";
                    txtA.Text = rbounds.Min.ToString();
                    lblB.Text = "to";
                    txtB.Text = rbounds.Max.ToString();

                    lblC.Visible = txtC.Visible = lblD.Visible = txtD.Visible = btnColour.Visible = false;

                    lblDesc.Visible = true;
                    break;

                case iValue.ValueType.ShortPoint2D:
                    var sp2 = new RealQuat(reader.ReadInt16(), reader.ReadInt16());

                    lblA.Text = "x";
                    txtA.Text = sp2.x.ToString();
                    lblB.Text = "y";
                    txtB.Text = sp2.y.ToString();

                    lblC.Visible = txtC.Visible = lblD.Visible = txtD.Visible = btnColour.Visible = false;
                    break;

                case iValue.ValueType.RealPoint2D:
                    var p2 = new RealQuat(reader.ReadSingle(), reader.ReadSingle());

                    lblA.Text = "x";
                    txtA.Text = p2.x.ToString();
                    lblB.Text = "y";
                    txtB.Text = p2.y.ToString();

                    lblC.Visible = txtC.Visible = lblD.Visible = txtD.Visible = btnColour.Visible = false;
                    break;

                case iValue.ValueType.RealPoint3D:
                    var p3 = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    lblA.Text = "x";
                    txtA.Text = p3.x.ToString();
                    lblB.Text = "y";
                    txtB.Text = p3.y.ToString();
                    lblC.Text = "z";
                    txtC.Text = p3.z.ToString();

                    lblD.Visible = txtD.Visible = btnColour.Visible = false;
                    break;

                case iValue.ValueType.RealPoint4D:
                    var p4 = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    lblA.Text = "x";
                    txtA.Text = p4.x.ToString();
                    lblB.Text = "y";
                    txtB.Text = p4.y.ToString();
                    lblC.Text = "z";
                    txtC.Text = p4.z.ToString();
                    lblD.Text = "w";
                    txtD.Text = p4.w.ToString();

                    btnColour.Visible = false;
                    break;

                case iValue.ValueType.RealVector2D:
                    var v2 = new RealQuat(reader.ReadSingle(), reader.ReadSingle());

                    lblA.Text = "i";
                    txtA.Text = v2.i.ToString();
                    lblB.Text = "j";
                    txtB.Text = v2.j.ToString();
                    
                    lblC.Visible = txtC.Visible = lblD.Visible = txtD.Visible = btnColour.Visible = false;
                    break;

                case iValue.ValueType.RealVector3D:
                    var v3 = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    lblA.Text = "i";
                    txtA.Text = v3.i.ToString();
                    lblB.Text = "j";
                    txtB.Text = v3.j.ToString();
                    lblC.Text = "k";
                    txtC.Text = v3.k.ToString();
                    
                    lblD.Visible = txtD.Visible = btnColour.Visible = false;
                    break;

                case iValue.ValueType.RealVector4D:
                    var v4 = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    lblA.Text = "i";
                    txtA.Text = v4.i.ToString();
                    lblB.Text = "j";
                    txtB.Text = v4.j.ToString();
                    lblC.Text = "k";
                    txtC.Text = v4.k.ToString();
                    lblD.Text = "w";
                    txtD.Text = v4.w.ToString();

                    btnColour.Visible = false;
                    break;

                case iValue.ValueType.Colour32RGB:
                    var c1 = Color.FromArgb((int)(255 * reader.ReadSingle()), (int)(255 * reader.ReadSingle()), (int)(255 * reader.ReadSingle()));

                    lblA.Text = "r";
                    txtA.Text = ((float)c1.R / 255).ToString();
                    lblB.Text = "g";
                    txtB.Text = ((float)c1.G / 255).ToString();
                    lblC.Text = "b";
                    txtC.Text = ((float)c1.B / 255).ToString();
                    
                    lblD.Visible = txtD.Visible = false;

                    btnColour.BackColor = c1;
                    break;

                case iValue.ValueType.Colour32ARGB:
                    var c2 = Color.FromArgb((int)(255 * reader.ReadSingle()), (int)(255 * reader.ReadSingle()), (int)(255 * reader.ReadSingle()), (int)(255 * reader.ReadSingle()));

                    lblA.Text = "a";
                    txtA.Text = ((float)c2.A / 255).ToString();
                    lblB.Text = "r";
                    txtB.Text = ((float)c2.R / 255).ToString();
                    lblC.Text = "g";
                    txtC.Text = ((float)c2.G / 255).ToString();
                    lblD.Text = "b";
                    txtD.Text = ((float)c2.B / 255).ToString();

                    btnColour.BackColor = c2;
                    break;

                default:
                    throw new InvalidOperationException("Cannot load " + value.Type.ToString() + " values using mMultiValue.");
            }
        }
    }
}

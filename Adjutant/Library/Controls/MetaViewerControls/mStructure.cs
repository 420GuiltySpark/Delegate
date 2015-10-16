using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Adjutant.Library.Definitions;

namespace Adjutant.Library.Controls.MetaViewerControls
{
    public delegate void ResizeNeededEvent();

    internal partial class mStructure : MetaViewerControl
    {
        private int chunkOffset, chunkCount, chunkSize;
        private bool showInvis, isLoaded, isLoading;
        private CacheBase.IndexItem tag;

        public mStructure(iValue Value, CacheBase Cache, CacheBase.IndexItem Tag, bool ShowInvisibles)
        {
            InitializeComponent();
            value = Value;
            cache = Cache;
            showInvis = ShowInvisibles;
            tag = Tag;

            lblName.Text = value.Node.Attributes["name"].Value.ToUpper();

            try { chunkSize = int.Parse(value.Node.Attributes["size"].Value); }
            catch { chunkSize = Convert.ToInt32(value.Node.Attributes["size"].Value, 16); }

            isLoaded = isLoading = false;
        }

        public override void Reload(int ParentAddress)
        {
            var reader = cache.Reader;

            int offset;

            try { offset = int.Parse(value.Node.Attributes["offset"].Value); }
            catch { offset = Convert.ToInt32(value.Node.Attributes["offset"].Value, 16); }

            reader.SeekTo(ParentAddress + offset);
            chunkCount = reader.ReadInt32();

            if (chunkCount <= 0)
            {
                Enabled = false;
                return;
            }

            chunkOffset = reader.ReadInt32();
            if (tag.Magic != 0)
                chunkOffset = chunkOffset - tag.Magic;
            else
                chunkOffset = chunkOffset - cache.Magic;
            reader.ReadInt32();

            LoadLabels();

            if (!isLoaded)
            {
                LoadControls();
                isLoaded = true;
            }
            else
                ReloadControls(0);

            
            Enabled = true;
        }

        new private void Resize()
        {
            int total = 34;
            foreach (Control c in pnlContainer.Controls)
            {
                c.Width = (c is mComment) ? this.Width - 30 : this.Width - 15;
                total += (c is mStructure) ? c.Height + 10 : c.Height;
            }

            this.Height = total;
            pnlContainer.Height = total + 5;
        }

        private void LoadLabels()
        {
            isLoading = true;
            cmbChunks.Items.Clear();

            string label;
            try { label = value.Node.Attributes["label"].Value; }
            catch
            {
                for (int i = 0; i < chunkCount; i++)
                    cmbChunks.Items.Add(i.ToString());

                cmbChunks.SelectedIndex = 0;
                isLoading = false;

                return;
            }

            foreach (XmlNode n in value.Node.ChildNodes)
            {
                if (n.Attributes["name"].Value == label)
                {
                    if (n.Name.ToLower() != "stringid" && n.Name.ToLower() != "tagref" && n.Name.ToLower() != "string")
                        throw new NotSupportedException("Struct labelling does not support " + n.Name + " values.");

                    var reader = cache.Reader;

                    int offset;

                    try { offset = int.Parse(n.Attributes["offset"].Value); }
                    catch { offset = Convert.ToInt32(n.Attributes["offset"].Value, 16); }

                    for (int i = 0; i < chunkCount; i++)
                    {
                        reader.SeekTo(chunkOffset + i * chunkSize + offset);
                        string s = "";
                        switch (n.Name.ToLower())
                        {
                            case "stringid":
                                s = cache.Strings.GetItemByID(reader.ReadInt32());
                                break;
                            case "tagref":
                                reader.Skip(12);
                                s = cache.IndexItems.GetItemByID(reader.ReadInt32()).Filename;
                                s = s.Substring(s.LastIndexOf('\\') + 1);
                                break;
                            case "string":
                                int length = int.Parse(n.Attributes["length"].Value);
                                s = reader.ReadNullTerminatedString(length);
                                break;
                                

                        }
                        cmbChunks.Items.Add(cmbChunks.Items.Count.ToString() + " : " + s);
                    }

                    cmbChunks.SelectedIndex = 0;
                    isLoading = false;
                    return;
                }
            }

            for (int i = 0; i < chunkCount; i++)
                cmbChunks.Items.Add(i.ToString());
            
            cmbChunks.SelectedIndex = 0;
            isLoading = false;
        }

        private void LoadControls()
        {
            pnlContainer.Controls.Clear();

            int yLoc = 0;

            foreach (XmlNode n in value.Node.ChildNodes)
            {
                iValue val;
                try { val = new iValue(n); }
                catch { continue; }
                if (!val.Visible && !showInvis) continue;

                MetaViewerControl mvc = new MetaViewerControl();

                switch (val.Type)
                {
                    case iValue.ValueType.Comment:
                        mvc = new mComment(val, cache);
                        break;

                    case iValue.ValueType.Struct:
                        mvc = new mStructure(val, cache, tag, showInvis);
                        ((mStructure)mvc).RequestTagLoad += new RequestTagLoadEventHandler(mTagRef_RequestTagLoad);
                        ((mStructure)mvc).ResizeNeeded += new ResizeNeededEvent(mStructure_ResizeNeeded);
                        break;
                        
                    case iValue.ValueType.TagRef:
                        mvc = new mTagRef(val, cache);
                        ((mTagRef)mvc).RequestTagLoad += new RequestTagLoadEventHandler(mTagRef_RequestTagLoad);
                        break;

                    case iValue.ValueType.String:
                    case iValue.ValueType.StringID:
                        mvc = new mString(val, cache);
                        break;

                    case iValue.ValueType.Bitmask8:
                    case iValue.ValueType.Bitmask16:
                    case iValue.ValueType.Bitmask32:
                        mvc = new mBitmask(val, cache);
                        break;

                    case iValue.ValueType.Int8:
                    case iValue.ValueType.Int16:
                    case iValue.ValueType.Int32:
                    case iValue.ValueType.Float32:
                    case iValue.ValueType.UInt16:
                    case iValue.ValueType.UInt32:
                    case iValue.ValueType.Undefined:
                    case iValue.ValueType.RawID:
                        mvc = new mValue(val, cache);
                        break;

                    case iValue.ValueType.Enum8:
                    case iValue.ValueType.Enum16:
                    case iValue.ValueType.Enum32:
                        mvc = new mEnum(val, cache);
                        break;

                    case iValue.ValueType.ShortBounds:
                    case iValue.ValueType.RealBounds:
                    case iValue.ValueType.ShortPoint2D:
                    case iValue.ValueType.RealPoint2D:
                    case iValue.ValueType.RealPoint3D:
                    case iValue.ValueType.RealPoint4D:
                    case iValue.ValueType.RealVector2D:
                    case iValue.ValueType.RealVector3D:
                    case iValue.ValueType.RealVector4D:
                    case iValue.ValueType.Colour32RGB:
                    case iValue.ValueType.Colour32ARGB:
                        mvc = new mMultiValue(val, cache);
                        break;

                    default:
                        continue;
                }

                mvc.Reload(chunkOffset);
                if (mvc is mStructure || mvc is mComment) yLoc += 5;
                mvc.Location = new Point(5, yLoc);
                yLoc += mvc.Height;
                if (mvc is mStructure || mvc is mComment) yLoc += 5;
                pnlContainer.Controls.Add(mvc);
            }

            Resize();
        }

        private void ReloadControls(int index)
        {
            foreach (MetaViewerControl c in pnlContainer.Controls)
                c.Reload(chunkOffset + index * chunkSize);
        }

        private void cmbChunks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!isLoading)
                ReloadControls(cmbChunks.SelectedIndex);
        }

        private void mStructure_ResizeNeeded()
        {
            int yLoc = 0;
            for (int i = 0; i < pnlContainer.Controls.Count; i++)
            {
                var c = pnlContainer.Controls[i];

                if (c is mStructure || c is mComment) yLoc += 5;
                c.Location = new Point(c.Location.X, yLoc);
                c.Width = (c is mComment) ? this.Width - 30 : this.Width - 15;
                yLoc += c.Height;
                if (c is mStructure || c is mComment) yLoc += 5;
            }

            Resize();
            ResizeNeeded();
        }

        private void mStructure_EnabledChanged(object sender, EventArgs e)
        {
            if (Enabled == false)
            {
                isLoading = true;
                cmbChunks.SelectedIndex = -1;
                Height = 34;
                isLoading = false;
            }
            else
                Resize();

            ResizeNeeded();
        }

        private void mTagRef_RequestTagLoad(int tagID)
        {
            RequestTagLoad(tagID);
        }

        public event ResizeNeededEvent ResizeNeeded;

        public event RequestTagLoadEventHandler RequestTagLoad;
    }
}

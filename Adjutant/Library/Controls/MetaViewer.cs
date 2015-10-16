using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.Controls.MetaViewerControls;
using Adjutant.Library.Definitions;
using System.IO;
using System.Xml;

namespace Adjutant.Library.Controls
{
    public partial class MetaViewer : UserControl
    {
        private CacheBase cache;
        private CacheBase.IndexItem tag;
        private XmlElement element;
        private bool showInvis, isWorking;
        private string pluginDir;

        public MetaViewer()
        {
            InitializeComponent();
        }

        public void LoadTagMeta(CacheBase Cache, CacheBase.IndexItem Tag, bool ShowInvisibles, string PluginsFolder)
        {
            cache = Cache;
            tag = Tag;
            pluginDir = PluginsFolder;

            chkInvis.Visible = true;

            isWorking = true;
            chkInvis.Checked = showInvis = ShowInvisibles;
            isWorking = false;

            var path = pluginDir + "\\" + cache.PluginDir + "\\" + tag.ClassCode.Replace("<", "_").Replace(">", "_").PadRight(4) + ".xml";

            if (File.Exists(path))
            {
                try
                {
                    var fs = new FileStream(path, FileMode.Open);
                    var doc = new XmlDocument();
                    doc.Load(fs);
                    element = doc.DocumentElement;
                    fs.Close();
                    fs.Dispose();

                    LoadControls();
                }
                catch
                {
                    pnlContainer.Controls.Clear();
                    var header = new mComment() { Location = new Point(5, 5) };
                    pnlContainer.Controls.Add(header);
                    header.SetText("Error loading plugin: " + cache.PluginDir + "\\" + tag.ClassCode.Replace("<", "_").Replace(">", "_").PadRight(4) + ".xml", "");
                }
            }
            else
            {
                pnlContainer.Controls.Clear();
                var header = new mComment() { Location = new Point(5, 5) };
                pnlContainer.Controls.Add(header);
                header.SetText("Plugin not found: " + cache.PluginDir + "\\" + tag.ClassCode.Replace("<", "_").Replace(">", "_").PadRight(4) + ".xml", "");
            }
        }

        private void LoadControls()
        {
            pnlContainer.Controls.Clear();

            int yLoc = 5;

            foreach (XmlNode n in element.ChildNodes)
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
                        ((mStructure)mvc).RequestTagLoad += new RequestTagLoadEventHandler(MetaViewer_TagLoadRequested);
                        ((mStructure)mvc).ResizeNeeded += new ResizeNeededEvent(mStructure_ResizeNeeded);
                        break;

                    case iValue.ValueType.TagRef:
                        mvc = new mTagRef(val, cache);
                        ((mTagRef)mvc).RequestTagLoad += new RequestTagLoadEventHandler(MetaViewer_TagLoadRequested);
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
                }

                mvc.Reload(tag.Offset);
                if (mvc is mStructure || mvc is mComment) yLoc += 5;
                mvc.Location = new Point(5, yLoc);
                yLoc += mvc.Height;
                if (mvc is mStructure || mvc is mComment) yLoc += 5;
                pnlContainer.Controls.Add(mvc);
            }
        }

        private void chkInvis_CheckedChanged(object sender, EventArgs e)
        {
            if (!isWorking)
                LoadTagMeta(cache, tag, chkInvis.Checked, pluginDir);
        }

        private void mStructure_ResizeNeeded()
        {
            int yLoc = (pnlContainer.Controls.Count > 0) ? pnlContainer.Controls[0].Location.Y : 5; //why is the location relative to the scroll position?
            for (int i = 0; i < pnlContainer.Controls.Count; i++)
            {
                var c = pnlContainer.Controls[i];

                if (c is mStructure && i > 0) yLoc += 5;
                c.Location = new Point(c.Location.X, yLoc);
                yLoc += c.Height;
                if (c is mStructure) yLoc += 5;
            }
        }

        private void MetaViewer_TagLoadRequested(int tagID)
        {
            LoadTagMeta(cache, cache.IndexItems.GetItemByID(tagID), showInvis, pluginDir);
        }
    }
}

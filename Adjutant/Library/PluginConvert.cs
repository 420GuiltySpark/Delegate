using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;

namespace Adjutant.Library
{
    public class PluginConverter
    {
        public void ConvertPlugins()
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "XML Files|*.xml|All Files|*.*",
                Multiselect = true
            };

            if (ofd.ShowDialog() != DialogResult.OK) return;

            var fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() != DialogResult.OK) return;

            foreach (string s in ofd.FileNames)
            {
                var xml = new FileStream(s, FileMode.Open, FileAccess.Read);
                var doc = new XmlDocument();
                doc.Load(xml);
                ConvertPlugin(doc, fbd.SelectedPath, Path.GetFileNameWithoutExtension(s));
                xml.Close();
                xml.Dispose();
            }
        }

        private void ConvertPlugin(XmlDocument Doc, string saveTo, string cls)
        {
            var element = Doc.DocumentElement;

            string path;
            try { path = saveTo + "\\" + element.Attributes["class"].Value.Replace("<", "_").Replace(">", "_") + ".xml"; }
            catch { path = saveTo + "\\" + cls + ".xml"; }

            var fs = new FileStream(path, FileMode.OpenOrCreate);
            XmlTextWriter xtw = new XmlTextWriter(fs, Encoding.ASCII) { Formatting = Formatting.Indented };

            xtw.WriteStartElement("plugin");

            try { xtw.WriteAttributeString("class", element.Attributes["class"].Value); }
            catch { xtw.WriteAttributeString("class", cls); }

            try { xtw.WriteAttributeString("headersize", element.Attributes["headersize"].Value); }
            catch { }

            #region write revisions
            xtw.WriteStartElement("revisions");

            foreach (XmlNode n in element.ChildNodes)
            {
                if (n.Name.ToLower() != "revision") continue;

                xtw.WriteStartElement("revision");

                foreach (XmlAttribute atr in n.Attributes)
                    xtw.WriteAttributeString(atr.Name, atr.Value);

                xtw.WriteString(n.InnerText);

                xtw.WriteEndElement();
            }

            xtw.WriteEndElement();
            #endregion

            foreach (XmlNode n in element.ChildNodes)
                WriteNode(xtw, n);

            xtw.WriteEndElement();

            xtw.Close();
        }

        private void WriteNode(XmlTextWriter xtw, XmlNode node)
        {
            bool vis;
            switch (node.Name.ToLower())
            {
                case "reflexive":
                case "structure":
                case "reflex":
                case "struct":
                    xtw.WriteStartElement("struct");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    xtw.WriteAttributeString("size", node.Attributes["entrySize"].Value);
                    foreach (XmlNode n in node.ChildNodes)
                        WriteNode(xtw, n);
                    xtw.WriteEndElement();
                    break;

                case "tagref":
                case "tag":
                    xtw.WriteStartElement("tagRef");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    xtw.WriteEndElement();
                    break;

                case "ident":
                case "id":
                case "tagid":
                    return;

                case "sid":
                case "stringid":
                case "stringidentifier":
                    xtw.WriteStartElement("stringID");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    xtw.WriteEndElement();
                    break;

                case "enum8":
                    xtw.WriteStartElement("enum8");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    foreach (XmlNode n in node.ChildNodes)
                    {
                        if (n.Name.ToLower() != "option") continue;

                        xtw.WriteStartElement("option");
                        xtw.WriteAttributeString("name", n.Attributes["name"].Value);
                        xtw.WriteAttributeString("value", n.Attributes["value"].Value);
                        xtw.WriteEndElement();
                    }
                    xtw.WriteEndElement();
                    break;

                case "enum16":
                    xtw.WriteStartElement("enum16");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    foreach (XmlNode n in node.ChildNodes)
                    {
                        if (n.Name.ToLower() != "option") continue;

                        xtw.WriteStartElement("option");
                        xtw.WriteAttributeString("name", n.Attributes["name"].Value);
                        xtw.WriteAttributeString("value", n.Attributes["value"].Value);
                        xtw.WriteEndElement();
                    }
                    xtw.WriteEndElement();
                    break;

                case "enum32":
                    xtw.WriteStartElement("enum32");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    foreach (XmlNode n in node.ChildNodes)
                    {
                        if (n.Name.ToLower() != "option") continue;

                        xtw.WriteStartElement("option");
                        xtw.WriteAttributeString("name", n.Attributes["name"].Value);
                        xtw.WriteAttributeString("value", n.Attributes["value"].Value);
                        xtw.WriteEndElement();
                    }
                    xtw.WriteEndElement();
                    break;

                case "bitmask8":
                case "bit8":
                    xtw.WriteStartElement("bitmask8");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    foreach (XmlNode n in node.ChildNodes)
                    {
                        if (n.Name.ToLower() != "option") continue;

                        xtw.WriteStartElement("option");
                        xtw.WriteAttributeString("name", n.Attributes["name"].Value);
                        xtw.WriteAttributeString("value", n.Attributes["value"].Value);
                        xtw.WriteEndElement();
                    }
                    xtw.WriteEndElement();
                    break;

                case "bitmask16":
                case "bit16":
                    xtw.WriteStartElement("bitmask16");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    foreach (XmlNode n in node.ChildNodes)
                    {
                        if (n.Name.ToLower() != "option") continue;

                        xtw.WriteStartElement("option");
                        xtw.WriteAttributeString("name", n.Attributes["name"].Value);
                        xtw.WriteAttributeString("value", n.Attributes["value"].Value);
                        xtw.WriteEndElement();
                    }
                    xtw.WriteEndElement();
                    break;

                case "bitmask32":
                case "bit32":
                    xtw.WriteStartElement("bitmask32");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    foreach (XmlNode n in node.ChildNodes)
                    {
                        if (n.Name.ToLower() != "option") continue;

                        xtw.WriteStartElement("option");
                        xtw.WriteAttributeString("name", n.Attributes["name"].Value);
                        xtw.WriteAttributeString("value", n.Attributes["value"].Value);
                        xtw.WriteEndElement();
                    }
                    xtw.WriteEndElement();
                    break;

                case "int8":
                case "byte":
                    xtw.WriteStartElement("int8");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    xtw.WriteEndElement();
                    break;

                case "int16":
                case "short":
                    xtw.WriteStartElement("int16");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    xtw.WriteEndElement();
                    break;

                case "uint16":
                case "ushort":
                    xtw.WriteStartElement("uint16");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    xtw.WriteEndElement();
                    break;

                case "int32":
                case "long":
                case "int":
                    xtw.WriteStartElement("int32");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    xtw.WriteEndElement();
                    break;

                case "uint32":
                case "ulong":
                case "uint":
                    xtw.WriteStartElement("uint32");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    xtw.WriteEndElement();
                    break;

                case "single":
                case "float":
                    xtw.WriteStartElement("float32");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    xtw.WriteEndElement();
                    break;

                case "string32":
                case "32string":
                    xtw.WriteStartElement("string");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    xtw.WriteAttributeString("length", "32");
                    xtw.WriteEndElement();
                    break;

                case "string256":
                case "256string":
                    xtw.WriteStartElement("string");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    xtw.WriteAttributeString("length", "256");
                    xtw.WriteEndElement();
                    break;

                case "unic64":
                case "unicode64":
                case "64unic":
                case "64unicode":
                    break;

                case "unic256":
                case "unicode256":
                case "256unic":
                case "256unicode":
                    break;

                case "undefined":
                case "unknown":
                    xtw.WriteStartElement("undefined");
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    xtw.WriteEndElement();
                    break;

                case "unused":
                case "blank":
                    return;

                case "bounds":
                case "vector2":
                case "vector3":
                case "vector4":
                case "point2":
                case "point3":
                case "point4":
                    xtw.WriteStartElement(node.Name);
                    xtw.WriteAttributeString("name", node.Attributes["name"].Value);
                    xtw.WriteAttributeString("offset", node.Attributes["offset"].Value);
                    vis = Convert.ToBoolean(node.Attributes["visible"].Value);
                    xtw.WriteAttributeString("visible", vis.ToString());
                    xtw.WriteEndElement();
                    break;

                default:
                    return;
            }
        }
    }
}

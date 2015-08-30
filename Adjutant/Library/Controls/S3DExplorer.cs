using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.S3D;

namespace Adjutant.Library.Controls
{
    public partial class S3DExplorer : UserControl
    {
        private PakFile pak;
        private PakFile.PakTag item;
        private TemplateBase model;

        public S3DExplorer()
        {
            InitializeComponent();
        }

        public void LoadModelHierarchy(PakFile Pak, PakFile.PakTag Item)
        {
            pak = Pak;
            item = Item;

            bool useInherit = (item.Class == TagType.Scene);

            switch (item.Class)
            {
                case TagType.Templates:
                    model = new Template(Pak, item, false);
                    break;
                case TagType.Scene:
                    model = new Scene(Pak, item, false);
                    break;
                default:
                    return;
            }


            tvSub.Nodes.Clear();
            richTextBox1.Clear();
            splitContainer1.Panel1Collapsed = false;

            var root = new TreeNode(model.Name) { Tag = model, ImageIndex = 7, SelectedImageIndex = 7 };

            #region Materials
            var mNode = new TreeNode("Materials") { ImageIndex = 1, SelectedImageIndex = 1 };
            int i = 0;
            foreach (var mat in model.Materials)
                mNode.Nodes.Add(new TreeNode("[" + (i++).ToString("d3") + "] " + mat.Name) { ImageIndex = 2, SelectedImageIndex = 2, Tag = mat });
            root.Nodes.Add(mNode);
            #endregion

            #region Scripts
            if (item.Class == TagType.Scene)
            {
                var sNode = new TreeNode("Scripts") { ImageIndex = 1, SelectedImageIndex = 1 };
                i = 0;
                foreach (var sc in ((Scene)model).Scripts)
                    sNode.Nodes.Add(new TreeNode("[" + (i++).ToString("d3") + "]") { ImageIndex = 7, SelectedImageIndex = 7, Tag = sc });
                root.Nodes.Add(sNode);
            }
            #endregion

            var nList = new List<TreeNode>();
            var dic = new Dictionary<int, TreeNode>();

            foreach (var obj in model.Objects)
            {
                var node = new TreeNode("[" + obj.ID.ToString("d3") + "] " + obj.Name) { Tag = obj };
                if (obj.VertCount > 0 || obj.Submeshes != null) node.ImageIndex = node.SelectedImageIndex = 3;
                if (obj.VertCount == 0 && obj.Submeshes != null || obj.isInheritor) node.ImageIndex = node.SelectedImageIndex = 4;

                if (node.ImageIndex > 0 || !useInherit)
                    dic.Add(obj.ID, node);
            }

            foreach (var pair in dic)
            {
                var obj = (Node)pair.Value.Tag;
                int id = (obj.inheritID != -1) ? obj.inheritID : obj.ParentID;

                if (id == -1)
                {
                    nList.Add(pair.Value);
                    continue;
                }

                TreeNode pNode;
                if (dic.TryGetValue(id, out pNode))
                    pNode.Nodes.Add(pair.Value);
                else nList.Add(pair.Value);
            }

            root.Nodes.AddRange(nList.ToArray());
            tvSub.Nodes.Add(root);
            root.Expand();

            if (!useInherit)
                foreach (TreeNode node in root.Nodes)
                    node.Expand();

            tvSub.SelectedNode = root;
        }

        public void DisplayTagInfo(PakFile Pak, PakFile.PakTag Item)
        {
            pak = Pak;
            item = Item;

            var reader = pak.Reader;
            int baseAddress = item.Offset;
            reader.SeekTo(baseAddress);
            richTextBox1.Clear();
            splitContainer1.Panel1Collapsed = true;

            switch (item.Class)
            {
                #region None
                case TagType.None:
                    richTextBox1.Text = reader.ReadString(reader.ReadInt32());
                    break;
                #endregion

                #region SceneData
                case TagType.SceneData:
                    var data = new SceneData(Pak, Item);

                    richTextBox1.Text += "Header:\t" + "[" + BitConverter.ToString(data.unmapped0, 0).Replace("-", string.Empty) + "]\r\n";

                    richTextBox1.Text += "x0700:\t\t" + data.x0700.ToString("d4") + "\t(" + leString(data.x0700, 2) + ")\r\n";
                    richTextBox1.Text += "xADDE:\t\t" + data.xADDE.ToString("d4") + "\t(" + leString(data.xADDE, 2) + ")\r\n";

                    richTextBox1.Text += "=======Bounds=======\r\n";
                    richTextBox1.Text += string.Format("{0, 13}\t{1, 13}\t{2, 13}\r\n{3, 13}\t{4, 13}\t{5, 13}\r\n",
                        data.unkBounds.XBounds.Min.ToString("F6"), data.unkBounds.YBounds.Min.ToString("F6"), data.unkBounds.ZBounds.Min.ToString("F6"),
                        data.unkBounds.XBounds.Max.ToString("F6"), data.unkBounds.YBounds.Max.ToString("F6"), data.unkBounds.ZBounds.Max.ToString("F6"));
                    richTextBox1.Text += "\r\n";

                    richTextBox1.Text += "Index Count:\t" + data.indices.Count.ToString("d5") + "\r\n";
                    richTextBox1.Text += "Struct Count:\t" + data.unkS0.Count.ToString("d5") + "\r\n";

                    richTextBox1.Text += "Footer:\t" + "[" + BitConverter.ToString(data.unmapped1, 0).Replace("-", string.Empty) + "]\r\n";
                    richTextBox1.Text += "\r\n";
                    //return;

                    richTextBox1.Text += "=======Indices=======\r\n";

                    for (int i = 0; i < Math.Min(data.indices.Count, 650); i++)
                        richTextBox1.Text += data.indices[i].ToString("d5") + ((i % 12 == 11) ? "\r\n" : " ");
                    richTextBox1.Text += "\r\n\r\n";

                    richTextBox1.Text += "=======???????=======\r\n";

                    for (int x = 0; x < Math.Min(data.unkS0.Count, 650); x++)
                    {
                        var dat = data.unkS0[x];
                        //for (int i = 0; i < 12; i += 3)
                        //{
                        //    int a = dat.unk0[i + 0];
                        //    int b = dat.unk0[i + 1];
                        //    int c = dat.unk0[i + 2];

                        //    richTextBox1.Text += string.Format("{0} [{1,6}, {2,6}, {3,6}]", (((i + 1) % 12 == 1) ? x.ToString("d5") : " "),
                        //        a.ToString("d5"), b.ToString("d5"), c.ToString("d5"))
                        //        + (((i + 1) % 6 == 5) ? "\r\n" : "");
                        //}
                        //richTextBox1.Text += "\r\n";
                        //continue;

                        for (int i = 0; i < 12; i += 3)
                        {
                            float a = dat.unk0[i + 0];
                            float b = dat.unk0[i + 1];
                            float c = dat.unk0[i + 2];

                            a = (a + (float)0x7FFF) / (float)0xFFFF;
                            b = (b + (float)0x7FFF) / (float)0xFFFF;
                            c = (c + (float)0x7FFF) / (float)0xFFFF;

                            a = a * data.unkBounds.XBounds.Length + data.unkBounds.XBounds.Min;
                            b = b * data.unkBounds.YBounds.Length + data.unkBounds.YBounds.Min;
                            c = c * data.unkBounds.ZBounds.Length + data.unkBounds.ZBounds.Min;

                            richTextBox1.Text += string.Format("{0} [{1,11}, {2,11}, {3,11}]", (((i + 1) % 12 == 1) ? x.ToString("d5") : " "),
                                a.ToString("F4"), b.ToString("F4"), c.ToString("F4"))
                                + (((i + 1) % 6 == 5) ? "\r\n" : "");
                        }
                        richTextBox1.Text += "\r\n";
                    }
                    break;
                #endregion

                #region Strings
                case TagType.Strings:
                    richTextBox1.Text = Encoding.Unicode.GetString(reader.ReadBytes(Item.Size));
                    break;
                #endregion

                #region SceneCDT
                case TagType.SceneCDT:
                    var cdt = new SceneCDT(Pak, Item);

                    richTextBox1.Text += "Header:\t" + "[" + BitConverter.ToString(cdt.unmapped0, 0).Replace("-", string.Empty) + "]\r\n";
                    richTextBox1.Text += "\r\n";

                    richTextBox1.Text += "Struct Count:\t" + cdt.unkS0.Count.ToString("d5") + "\r\n";

                    richTextBox1.Text += "\r\n=======???????=======\r\n";

                    for (int i = 0; i < Math.Min(cdt.unkS0.Count, 250); i++)
                    {
                        var dat = cdt.unkS0[i];
                        richTextBox1.Text += "unkIndex:\t" + dat.unkIndex.ToString("d4") + "\t\t(" + leString(dat.unkIndex, 2) + ")\r\n";
                        richTextBox1.Text += "unk0:\t\t" + dat.unk0.ToString("d6") + "\t\t(" + leString(dat.unk0, 4) + ")\r\n";
                        richTextBox1.Text += "unk1:\t\t" + dat.unk1.ToString("d6") + "\t\t(" + leString(dat.unk1, 4) + ")\r\n";
                        richTextBox1.Text += "unkf0:\t\t" + dat.unkf0.ToString("F4") + "\r\n\r\n";
                    }
                    break;
                #endregion

                default:
                    richTextBox1.Text += "\r\n  [NO DATA]";
                    break;
            }
        }

        private void displaySelectedNodeInfo(object obj, int baseAddress)
        {
            richTextBox1.Clear();

            #region Template
            if (obj is Template)
            {
                var atpl = (Template)obj;
                richTextBox1.Text += "Name:\t\t" + atpl.Name + "\r\n";
                richTextBox1.Text += "Mat Count:\t" + atpl.Materials.Count.ToString("d3") + "\r\n";
                richTextBox1.Text += "Obj Count:\t" + atpl.Objects.Count.ToString("d3") + "\r\n";
                richTextBox1.Text += "Node Count:\t" + atpl.NodeInfo.Count.ToString("d3") + "\r\n";
                richTextBox1.Text += "Matrix Count:\t" + atpl.unkMatList.Count.ToString("d3") + "\r\n";
                richTextBox1.Text += "\r\n";
                richTextBox1.Text += "unk0:\t\t" + atpl.unk0.ToString("d4") + "\t\t(" + leString(atpl.unk0, 4) + ")\r\n";
                richTextBox1.Text += "xF000:\t\t" + atpl.xF000.ToString("d4") + "\t\t(" + leString(atpl.xF000, 2) + ")\r\n";
                richTextBox1.Text += "x2C01:\t\t" + atpl.x2C01.ToString("d4") + "\t\t(" + leString(atpl.x2C01, 2) + ")\r\n";
                richTextBox1.Text += "unk1:\t\t" + atpl.unk1.ToString("d4") + "\t\t(" + leString(atpl.unk1, 4) + ")\r\n";
                richTextBox1.Text += "\r\n";

                richTextBox1.Text += "=======Bounds=======\r\n";
                richTextBox1.Text += string.Format("{0, 11}\t{1, 11}\t{2, 11}\r\n{3, 11}\t{4, 11}\t{5, 11}\r\n",
                    atpl.RenderBounds.XBounds.Min.ToString("F6"), atpl.RenderBounds.YBounds.Min.ToString("F6"), atpl.RenderBounds.ZBounds.Min.ToString("F6"),
                    atpl.RenderBounds.XBounds.Max.ToString("F6"), atpl.RenderBounds.YBounds.Max.ToString("F6"), atpl.RenderBounds.ZBounds.Max.ToString("F6"));
                richTextBox1.Text += "\r\n";

                richTextBox1.Text += "=======Node Data=======\r\n";
                foreach (var info in atpl.NodeInfo)
                {
                    richTextBox1.Text += string.Format("{0, 11}\r\n{1, 11}\t{2, 11}\t{3, 11}\r\n{4, 11}\t{5, 11}\t{6, 11}\t{7, 11}\r\n{8, 11}\t{9, 11}\t{10, 11}\r\n{11, 11}\r\n\r\n",
                        info.unk00.ToString("F6"), info.unk01.ToString("F6"), info.unk02.ToString("F6"), info.unk03.ToString("F6"), info.unk04.ToString("F6"), info.unk05.ToString("F6"), info.unk06.ToString("F6"), info.unk07.ToString("F6"), info.unk08.ToString("F6"), info.unk09.ToString("F6"), info.unk10.ToString("F6"), info.unk11.ToString("F6"));
                }

                richTextBox1.Text += "=======Matrices=======\r\n";
                foreach (var mat in atpl.unkMatList)
                {
                    richTextBox1.Text += string.Format("{0, 11}\t{1, 11}\t{2, 11}\t{3, 11}\r\n{4, 11}\t{5, 11}\t{6, 11}\t{7, 11}\r\n{8, 11}\t{9, 11}\t{10, 11}\t{11, 11}\r\n{12, 11}\t{13, 11}\t{14, 11}\t{15, 11}\r\n\r\n",
                        mat.m11.ToString("F6"), mat.m12.ToString("F6"), mat.m13.ToString("F6"), 0f.ToString("F6"),
                        mat.m21.ToString("F6"), mat.m22.ToString("F6"), mat.m23.ToString("F6"), 0f.ToString("F6"),
                        mat.m31.ToString("F6"), mat.m32.ToString("F6"), mat.m33.ToString("F6"), 0f.ToString("F6"),
                        mat.m41.ToString("F6"), mat.m42.ToString("F6"), mat.m43.ToString("F6"), 1f.ToString("F6"));
                }
            }
            #endregion

            #region Scene
            else if (obj is Scene)
            {
                var bsp = (Scene)obj;
                richTextBox1.Text += "Name:\t\t" + bsp.Name + "\r\n";
                richTextBox1.Text += "Mat Count:\t" + bsp.Materials.Count.ToString("d3") + "\r\n";
                richTextBox1.Text += "Obj Count:\t" + bsp.Objects.Count.ToString("d3") + "\r\n";
                richTextBox1.Text += "\r\n";
                richTextBox1.Text += "xF000:\t\t" + bsp.xF000.ToString("d4") + "\t\t(" + leString(bsp.xF000, 2) + ")\r\n";
                richTextBox1.Text += "x2C01:\t\t" + bsp.x2C01.ToString("d4") + "\t\t(" + leString(bsp.x2C01, 2) + ")\r\n";
                richTextBox1.Text += "unk1:\t\t" + bsp.unk1.ToString("d4") + "\t\t(" + leString(bsp.unk1, 4) + ")\r\n";
                richTextBox1.Text += "\r\n";

                richTextBox1.Text += "=======Bounds=======\r\n";
                richTextBox1.Text += string.Format("{0, 11}\t{1, 11}\t{2, 11}\r\n{3, 11}\t{4, 11}\t{5, 11}\r\n",
                    bsp.RenderBounds.XBounds.Min.ToString("F6"), bsp.RenderBounds.YBounds.Min.ToString("F6"), bsp.RenderBounds.ZBounds.Min.ToString("F6"),
                    bsp.RenderBounds.XBounds.Max.ToString("F6"), bsp.RenderBounds.YBounds.Max.ToString("F6"), bsp.RenderBounds.ZBounds.Max.ToString("F6"));
                richTextBox1.Text += "\r\n";
            }
            #endregion

            #region Material
            else if (obj is Material)
            {
                var mat = (Material)obj;
                richTextBox1.Text += "x5601:\t\t" + mat.x5601.ToString("d4") + "\t\t(" + leString(mat.x5601, 2) + ")\r\n";
                richTextBox1.Text += "Next Address:\t" + mat.AddressOfNext.ToString("d9") + "\t(" + (mat.AddressOfNext + baseAddress).ToString("d9") + ")\r\n";
                richTextBox1.Text += "Name:\t\t" + mat.Name + "\r\n";
            }
            #endregion

            #region Script
            else if (obj is Script)
            {
                var sc = (Script)obj;
                richTextBox1.Text += "xBA01:\t\t" + sc.xBA01.ToString("d4") + "\t\t(" + leString(sc.xBA01, 2) + ")\r\n";
                richTextBox1.Text += "Next Address:\t" + sc.AddressOfNext.ToString("d9") + "\t(" + (sc.AddressOfNext + baseAddress).ToString("d9") + ")\r\n";
                richTextBox1.Text += "\r\n=======Script Data=====\r\n\r\n" + sc.Data.Replace("\\t", "\t").Replace("\\n", "\n") + "\r\n";
            }
            #endregion

            #region Node
            else if (obj is Node)
            {
                var node = (Node)obj;
                richTextBox1.Text += "Base Address:\t" + node.mainAddress.ToString("d9") + "\t(" + (node.mainAddress + baseAddress).ToString("d9") + ")\r\n";
                richTextBox1.Text += "xF000:\t\t" + node.xF000.ToString("d4") + "\t\t(" + leString(node.xF000, 2) + ")\r\n";
                richTextBox1.Text += "Next Address:\t" + node.PreNextAddress.ToString("d9") + "\t(" + (node.PreNextAddress + baseAddress).ToString("d9") + ")\r\n";
                richTextBox1.Text += "xB903:\t\t" + node.xB903.ToString("d4") + "\t\t(" + leString(node.xB903, 2) + ")\r\n";
                richTextBox1.Text += "Name:\t\t" + node.Name + "\r\n";
                richTextBox1.Text += "ID:\t\t" + node.ID.ToString("d3") + "\t\t(" + leString(node.ID, 2) + ")\r\n";
                richTextBox1.Text += "x2400:\t\t" + node.x2400.ToString("d4") + "\t\t(" + leString(node.x2400, 2) + ")\r\n";
                richTextBox1.Text += "unk0:\t\t" + node.unk0.ToString("d3") + "\t\t(" + leString(node.unk0, 1) + ")\r\n";
                richTextBox1.Text += "unk1:\t\t" + node.unk1.ToString("d4") + "\t\t(" + leString(node.unk1, 2) + ")\r\n";
                richTextBox1.Text += "unk2:\t\t" + node.unk2.ToString("d4") + "\t\t(" + leString(node.unk2, 2) + ")\r\n";
                richTextBox1.Text += "Vert Count:\t" + node.VertCount.ToString("d6") + "\r\n";
                richTextBox1.Text += "Face Count:\t" + node.FaceCount.ToString("d6") + "\r\n";
                richTextBox1.Text += "unk3:\t\t" + node.unk3.ToString("d4") + "\t\t(" + leString(node.unk3, 2) + ")\r\n";
                richTextBox1.Text += "unk4:\t\t" + node.unk4.ToString("d4") + "\t\t(" + leString(node.unk4, 2) + ")\r\n";
                richTextBox1.Text += "unk5:\t\t" + node.unk5.ToString("d4") + "\t\t(" + leString(node.unk5, 2) + ")\r\n";
                richTextBox1.Text += "unk5a:\t\t" + node.unk5a.ToString("d4") + "\t\t(" + leString(node.unk5a, 4) + ")\r\n";
                richTextBox1.Text += "\r\n";

                richTextBox1.Text += "=======Matrix=======\r\n";
                richTextBox1.Text += string.Format("{0, 11}\t{1, 11}\t{2, 11}\t{3, 11}\r\n{4, 11}\t{5, 11}\t{6, 11}\t{7, 11}\r\n{8, 11}\t{9, 11}\t{10, 11}\t{11, 11}\r\n{12, 11}\t{13, 11}\t{14, 11}\t{15, 11}\r\n",
                    node.Transform.m11.ToString("F6"), node.Transform.m12.ToString("F6"), node.Transform.m13.ToString("F6"), 0f.ToString("F6"),
                    node.Transform.m21.ToString("F6"), node.Transform.m22.ToString("F6"), node.Transform.m23.ToString("F6"), 0f.ToString("F6"),
                    node.Transform.m31.ToString("F6"), node.Transform.m32.ToString("F6"), node.Transform.m33.ToString("F6"), 0f.ToString("F6"),
                    node.Transform.m41.ToString("F6"), node.Transform.m42.ToString("F6"), node.Transform.m43.ToString("F6"), 1f.ToString("F6"));
                richTextBox1.Text += "\r\n";

                richTextBox1.Text += "xFA00:\t\t" + node.xFA00.ToString("d4") + "\t\t(" + leString(node.xFA00, 2) + ")\r\n";
                richTextBox1.Text += "NodeIndex:\t" + node.NodeIndex.ToString() + "\r\n";
                richTextBox1.Text += "Mesh Count:\t" + ((node.Submeshes == null) ? "0" : node.Submeshes.Count.ToString()) + "\r\n";
                richTextBox1.Text += "Parent ID:\t" + node.ParentID.ToString("d3") + "\t\t(" + leString(node.ParentID, 2) + ")\r\n";
                richTextBox1.Text += "Inherits ID:\t" + node.inheritID.ToString("d2") + "\t\t(" + leString(node.inheritID, 2) + ")\r\n";
                richTextBox1.Text += "\r\n";

                if (node.BoundingBox != null)
                {
                    richTextBox1.Text += "=======Bounds=======\r\n";
                    richTextBox1.Text += string.Format("{0, 11}\t{1, 11}\t{2, 11}\r\n{3, 11}\t{4, 11}\t{5, 11}\r\n",
                        node.BoundingBox.XBounds.Min.ToString("F6"), node.BoundingBox.YBounds.Min.ToString("F6"), node.BoundingBox.ZBounds.Min.ToString("F6"),
                        node.BoundingBox.XBounds.Max.ToString("F6"), node.BoundingBox.YBounds.Max.ToString("F6"), node.BoundingBox.ZBounds.Max.ToString("F6"));
                    richTextBox1.Text += "\r\n";
                }

                if (node.VertCount > 0 || node.Submeshes != null)
                {
                    richTextBox1.Text += "======Vert Info======\r\n";
                    richTextBox1.Text += "x1200:\t\t" + node.x1200.ToString("d4") + "\t\t(" + leString(node.x1200, 2) + ")\r\n";
                    richTextBox1.Text += "geomUnk01:\t" + node.geomUnk01.ToString("d3") + "\t\t(" + leString(node.geomUnk01, 1) + ")\r\n";
                    richTextBox1.Text += "x4001:\t\t" + node.x4001.ToString("d4") + "\t\t(" + leString(node.x4001, 2) + ")\r\n";
                    richTextBox1.Text += "CentreX:\t" + node.CentreX.ToString("d4") + "\t\t(" + leString(node.CentreX, 2) + ")\r\n";
                    richTextBox1.Text += "CentreY:\t" + node.CentreY.ToString("d4") + "\t\t(" + leString(node.CentreY, 2) + ")\r\n";
                    richTextBox1.Text += "CentreZ:\t" + node.CentreZ.ToString("d4") + "\t\t(" + leString(node.CentreZ, 2) + ")\r\n";
                    richTextBox1.Text += "RadiusX:\t" + node.RadiusX.ToString("d4") + "\t\t(" + leString(node.RadiusX, 2) + ")\r\n";
                    richTextBox1.Text += "RadiusY:\t" + node.RadiusY.ToString("d4") + "\t\t(" + leString(node.RadiusY, 2) + ")\r\n";
                    richTextBox1.Text += "RadiusZ:\t" + node.RadiusZ.ToString("d4") + "\t\t(" + leString(node.RadiusZ, 2) + ")\r\n";
                    richTextBox1.Text += "unkUV0:\t" + node.unkUV0.ToString("d3") + "\t\t(" + leString(node.unkUV0, 1) + ")\r\n";
                    if (node.preUV != null) richTextBox1.Text += "preUV:\t\t" + "[" + BitConverter.ToString(node.preUV, 0).Replace("-", string.Empty) + "]\r\n" + node.UVsize.ToString() + "\r\n";
                    else richTextBox1.Text += "preUV:\t\t[NA]\r\n\r\n";

                    richTextBox1.Text += "\r\n";
                }

                if (node.Submeshes == null) return;

                richTextBox1.Text += "unkCC:\t\t" + node.unkCC.ToString("d3") + "\t\t(" + leString(node.unkCC, 1) + ")\r\n";
                richTextBox1.Text += "unkC0:\t\t" + node.unkC0.ToString("d3") + "\t\t(" + leString(node.unkC0, 4) + ")\r\n";
                for (int i = 0; i < 5; i++)
                    richTextBox1.Text += "unkC1[" + i.ToString() + "]:\t" + node.unkC1[i].ToString("d3") + "\t\t(" + leString(node.unkC1[i], 4) + ")\r\n";
                richTextBox1.Text += "\r\n";

                richTextBox1.Text += "======Submeshes======\t" + node.subAddress.ToString("d9") + "\t(" + (node.subAddress + baseAddress).ToString("d9") + ")\r\n";
                foreach (var sub in node.Submeshes)
                {
                    richTextBox1.Text += "Vert Bounds:\t" + sub.VertStart.ToString("d6") + " - " + (sub.VertStart + sub.VertLength).ToString("d6") + "  (" + sub.VertLength.ToString("d6") + ")\r\n";
                    richTextBox1.Text += "Face Bounds:\t" + sub.FaceStart.ToString("d6") + " - " + (sub.FaceStart + sub.FaceLength).ToString("d6") + "  (" + sub.FaceLength.ToString("d6") + ")\r\n";
                    richTextBox1.Text += "Mat Count:\t" + sub.MaterialCount.ToString("d3") + "\r\n";
                    richTextBox1.Text += "Mat Index:\t" + sub.MaterialIndex.ToString("d3") + "\r\n";

                    richTextBox1.Text += "unk0:\t\t" + sub.unk0.ToString("d4") + "\t\t(" + leString((int)sub.unk0, 2) + ")\r\n";
                    richTextBox1.Text += "unkID0:\t" + sub.unkID0.ToString("d3") + "\t\t(" + leString(sub.unkID0, 2) + ")\r\n";

                    if (sub.unk0 == 306) //3201
                    {
                        richTextBox1.Text += "unkCount0:\t" + sub.unkCount0.ToString("d4") + "\t\t(" + leString(sub.unkCount0, 1) + ")\r\n";
                        richTextBox1.Text += "unkID1:\t" + sub.unkID1.ToString("d3") + "\t\t(" + leString(sub.unkID1, 2) + ")\r\n";
                        richTextBox1.Text += "unkCount1:\t" + sub.unkCount1.ToString("d4") + "\t\t(" + leString(sub.unkCount1, 1) + ")\r\n";
                    }
                    else richTextBox1.Text += "\r\n\r\n\r\n";

                    richTextBox1.Text += "unkfAdr:\t" + sub.unkfAddress.ToString("d6") + "\r\n";
                    richTextBox1.Text += "unkf0,1:\t" + sub.unkf0.ToString("F6") + ", " + sub.unkf1.ToString("F6") + "\r\n";

                    richTextBox1.Text += "x2001:\t\t" + sub.x2001.ToString("d4") + "\t\t(" + leString(sub.x2001, 2) + ")\r\n";
                    richTextBox1.Text += "unkb0:\t\t" +
                        BitConverter.ToSingle(sub.unkb0, 0).ToString("F6") + ", " + BitConverter.ToSingle(sub.unkb0, 4).ToString("F6") + ", " +
                        BitConverter.ToSingle(sub.unkb0, 8).ToString("F6") + ", " + BitConverter.ToSingle(sub.unkb0, 12).ToString("F6") + ", " +
                        BitConverter.ToSingle(sub.unkb0, 16).ToString("F6") + ", " + BitConverter.ToSingle(sub.unkb0, 20).ToString("F6") + ", " +
                        BitConverter.ToSingle(sub.unkb0, 24).ToString("F6") + ", " + BitConverter.ToSingle(sub.unkb0, 28).ToString("F6") + "\r\n";

                    if (sub.x2801 != 1)
                    {
                        richTextBox1.Text += "x2801:\t\t" + sub.x2801.ToString("d4") + "\t\t(" + leString(sub.x2801, 2) + ")\r\n";
                        richTextBox1.Text += "x81:\t\t" + sub.x81.ToString("d3") + "\t\t(" + leString(sub.x81, 1) + ")\r\n";
                        richTextBox1.Text += "unk4:\t\t" + sub.unk4.ToString("d4") + "\t\t(" + leString(sub.unk4, 4) + ")\r\n";
                        richTextBox1.Text += "xFF:\t\t" + sub.xFF.ToString("d3") + "\t\t(" + leString(sub.xFF, 1) + ")\r\n";
                        richTextBox1.Text += "x1300:\t\t" + sub.x1300.ToString("d4") + "\t\t(" + leString(sub.x1300, 2) + ")\r\n";
                        richTextBox1.Text += "VertexCount:\t" + sub.VertexCount.ToString("d4") + "\t\t(" + leString(sub.VertexCount, 2) + ")\r\n";
                        richTextBox1.Text += "IndexCount:\t" + sub.IndexCount.ToString("d4") + "\t\t(" + leString(sub.IndexCount, 2) + ")\r\n";
                        richTextBox1.Text += "unkID2:\t" + sub.unkID2.ToString("d3") + "\t\t(" + leString(sub.unkID2, 4) + ")\r\n";
                        richTextBox1.Text += "unk7:\t\t" + sub.unk7.ToString("d6") + "\t\t(" + leString(sub.unk7, 4) + ")\r\n";
                        richTextBox1.Text += "unk8:\t\t" + sub.unk8.ToString("d6") + "\t\t(" + leString(sub.unk8, 4) + ")\r\n";
                        richTextBox1.Text += "unk9a:\t\t" + sub.unk9a.ToString("d4") + "\t\t(" + leString(sub.unk9a, 2) + ")\r\n";
                        richTextBox1.Text += "unk9b:\t\t" + sub.unk9b.ToString("d4") + "\t\t(" + leString(sub.unk9b, 2) + ")\r\n";
                    }
                    else richTextBox1.Text += "\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n";

                    richTextBox1.Text += "\r\n";
                }
            }
            #endregion
        }

        private string leString(int value, int byteLength)
        {
            var b = BitConverter.GetBytes(value);
            return BitConverter.ToString(b, 0).Replace("-", string.Empty).Substring(0, byteLength * 2);
        }

        private void tvSub_AfterSelect(object sender, TreeViewEventArgs e)
        {
            displaySelectedNodeInfo(tvSub.SelectedNode.Tag, item.Offset);
        }
    }
}

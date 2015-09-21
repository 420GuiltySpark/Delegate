using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.S3D;
using Adjutant.Library.S3D.Blocks;

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
                mNode.Nodes.Add(new TreeNode("[" + (i++).ToString("d3") + "] " + mat.Reference) { ImageIndex = 2, SelectedImageIndex = 2, Tag = mat });
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
                var node = new TreeNode("[" + obj._B903.ID.ToString("d3") + "] " + obj._B903.Name) { Tag = obj };
                if (obj._B903.VertCount > 0 || obj.Submeshes != null) node.ImageIndex = node.SelectedImageIndex = 3;
                if (obj._B903.VertCount == 0 && obj.Submeshes != null || obj.isInheritor) node.ImageIndex = node.SelectedImageIndex = 4;

                if (node.ImageIndex > 0 || !useInherit)
                    dic.Add(obj._B903.ID, node);
            }

            foreach (var pair in dic)
            {
                var obj = (Node)pair.Value.Tag;
                int id = (obj._2901 != null) ? obj._2901.InheritID : obj.ParentID;

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
                case TagType.AnimStream:
                    richTextBox1.Text = reader.ReadString(reader.ReadInt32());
                    break;
                #endregion

                #region SceneData
                case TagType.SceneData:
                    var data = Pak.SceneData;

                    richTextBox1.Text += "Header:\t" + "[" + BitConverter.ToString(data.unmapped0, 0).Replace("-", string.Empty) + "]\r\n";

                    richTextBox1.Text += string.Format("x0700:\t\t{0:d4}\r\n", data.x0700, leString(data.x0700, 2));
                    richTextBox1.Text += string.Format("xADDE:\t\t{0:d4}\r\n", data.xADDE, leString(data.xADDE, 2));

                    richTextBox1.Text += "=======Bounds=======\r\n";
                    richTextBox1.Text += string.Format(
                        "{0,13:F6}\t{1,13:F6}\t{2,13:F6}\r\n{3,13:F6}\t{4,13:F6}\t{5,13:F6}\r\n",
                        data.unkBounds.XBounds.Min, data.unkBounds.YBounds.Min, data.unkBounds.ZBounds.Min,
                        data.unkBounds.XBounds.Max, data.unkBounds.YBounds.Max, data.unkBounds.ZBounds.Max);
                    richTextBox1.Text += "\r\n";

                    richTextBox1.Text += string.Format("Index Count:\t{0:d5}\r\n", data.indices.Count);
                    richTextBox1.Text += string.Format("Struct Count:\t{0:d5}\r\n", data.unkS0.Count);

                    richTextBox1.Text += "Footer:\t" + "[" + BitConverter.ToString(data.unmapped1, 0).Replace("-", string.Empty) + "]\r\n";
                    richTextBox1.Text += "\r\n";

                    richTextBox1.Text += "=======Indices=======\r\n";

                    for (int i = 0; i < Math.Min(data.indices.Count, 650); i++)
                        richTextBox1.Text += data.indices[i].ToString("d5") + ((i % 12 == 11) ? "\r\n" : " ");
                    richTextBox1.Text += "\r\n\r\n";

                    richTextBox1.Text += "=======???????=======\r\n";

                    for (int x = 0; x < Math.Min(data.unkS0.Count, 650); x++)
                    {
                        var dat = data.unkS0[x];
                        for (int i = 0; i < 12; i += 3)
                        {
                            int a = dat.unk0[i + 0];
                            int b = dat.unk0[i + 1];
                            int c = dat.unk0[i + 2];

                            richTextBox1.Text += string.Format("{0} [{1,6}, {2,6}, {3,6}]", (((i + 1) % 12 == 1) ? x.ToString("d5") : " "),
                                a.ToString("d5"), b.ToString("d5"), c.ToString("d5"))
                                + (((i + 1) % 6 == 5) ? "\r\n" : "");

                            //richTextBox1.Text += string.Format("{0} [{1,4}, {2,4}, {3,4}]", (((i + 1) % 12 == 1) ? x.ToString("d5") : " "),
                            //    leString(a, 2), leString(b, 2), leString(c, 2))
                            //    + (((i + 1) % 6 == 5) ? "\r\n" : "");
                        }
                        richTextBox1.Text += "\r\n";
                        continue;

                        for (int i = 0; i < 12; i += 3)
                        {
                            float a = dat.unk0[i + 0];
                            float b = dat.unk0[i + 1];
                            float c = dat.unk0[i + 2];

                            a = (a + (float)0x7FFF) / (float)0xFFFF;
                            b = (b + (float)0x7FFF) / (float)0xFFFF;
                            c = (c + (float)0x7FFF) / (float)0xFFFF;

                            //a = a * data.unkBounds.XBounds.Length + data.unkBounds.XBounds.Min;
                            //b = b * data.unkBounds.YBounds.Length + data.unkBounds.YBounds.Min;
                            //c = c * data.unkBounds.ZBounds.Length + data.unkBounds.ZBounds.Min;

                            richTextBox1.Text += string.Format("{0} [{1,11}, {2,11}, {3,11}]", (((i + 1) % 12 == 1) ? x.ToString("d5") : " "),
                                a.ToString("f6"), b.ToString("f6"), c.ToString("f6"))
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
                    var cdt = Pak.SceneCDT;

                    richTextBox1.Text += string.Format("Header:\t[{0}]\r\n", BitConverter.ToString(cdt.unmapped0, 0).Replace("-", string.Empty));
                    richTextBox1.Text += "\r\n";

                    richTextBox1.Text += string.Format("Set Count:\t{0:d5}\r\n", cdt.sets.Count.ToString("d5"));
                    foreach (var set in cdt.sets)
                    {
                        richTextBox1.Text += "\r\n=======DataSet=======\r\n";
                        richTextBox1.Text += string.Format("Struct Count:\t{0:d5}\r\n\r\n", set.unkS0.Count.ToString("d5"));

                        if (set.unkS0.Count == 0) continue;

                        richTextBox1.Text += string.Format("FaceTotal:\t{0:d8}\t({1})\r\n", set.unk0, leString(set.unk0, 4));
                        richTextBox1.Text += string.Format("MinBound:\t{0}\r\n", set.MinBound.ToString());
                        richTextBox1.Text += string.Format("unkf0:\t\t{0:F6}\r\n", set.unkf0);
                        richTextBox1.Text += string.Format("DataLength:\t{0:d8}\t({1})\r\n", set.DataLength, leString(set.DataLength, 4));

                        richTextBox1.Text += "\r\n\r\n";
                        richTextBox1.Text += "\r\n=======Set Structs=======\r\n";
                        for (int i = 0; i < Math.Min(set.unkS0.Count, 250); i++)
                        {
                            var dat = set.unkS0[i];
                            richTextBox1.Text += string.Format("NodeID:\t{0:d4}\t\t({1})\r\n", dat.NodeID, leString(dat.NodeID, 2));
                            richTextBox1.Text += string.Format("NodeFaces:\t{0:d6}\t\t({1})\r\n", dat.NodeFaces, leString(dat.NodeFaces, 4));
                            richTextBox1.Text += string.Format("unk1:\t\t{0:d6}\t\t({1})\r\n", dat.unk1, leString(dat.unk1, 4));
                            richTextBox1.Text += string.Format("unkf0:\t\t{0:F4}\r\n\r\n", dat.unkf0);
                        }
                        richTextBox1.Text += "\r\n\r\n";
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
                richTextBox1.Text += string.Format("Name:\t\t{0}\r\n", atpl.Name);
                richTextBox1.Text += string.Format("Mat Count:\t{0:d3}\r\n", atpl.Materials.Count);
                richTextBox1.Text += string.Format("Obj Count:\t{0:d3}\r\n", atpl.Objects.Count);
                richTextBox1.Text += string.Format("Bone Count:\t{0:d3}\r\n", atpl.Bones.Count);
                richTextBox1.Text += string.Format("Matrix Count:\t{0:d3}\r\n", atpl._0503 == null ? 0 : atpl._0503.DataCount);
                richTextBox1.Text += "\r\n";

                richTextBox1.Text += "=======Bounds=======\r\n";
                richTextBox1.Text += string.Format("{0,11:F6}\t{1,11:F6}\t{2,11:F6}\r\n{3,11:F6}\t{4,11:F6}\t{5,11:F6}\r\n",
                    atpl.RenderBounds.XBounds.Min, atpl.RenderBounds.YBounds.Min, atpl.RenderBounds.ZBounds.Min,
                    atpl.RenderBounds.XBounds.Max, atpl.RenderBounds.YBounds.Max, atpl.RenderBounds.ZBounds.Max);
                richTextBox1.Text += "\r\n";

                //richTextBox1.Text += "=======Node Data=======\r\n";
                //foreach (var info in atpl.Bones)
                //{
                //    richTextBox1.Text += string.Format("{0,10:F6}\r\n", info.unk00);
                //    richTextBox1.Text += string.Format("{0,10:F6},\t{1,10:F6},\t{2,10:F6}\r\n", info._FA02.Data.x, info._FA02.Data.y, info._FA02.Data.z);
                //    if (info._FB02 != null) richTextBox1.Text += string.Format("{0,10:F6},\t{1,10:F6},\t{2,10:F6},\t{3,10:F6}\r\n", info._FB02.Data.x, info._FB02.Data.y, info._FB02.Data.z, info._FB02.Data.w);
                //    if (info._FC02 != null) richTextBox1.Text += string.Format("{0,10:F6},\t{1,10:F6},\t{2,10:F6}\r\n", info._FC02.unk08, info._FC02.unk09, info._FC02.unk10);
                //    if (info._0A03 != null) richTextBox1.Text += string.Format("{0,10:F6}\r\n", info._0A03.unk11);
                //    richTextBox1.Text += "\r\n";
                //}

                //if (atpl._0503 != null)
                //{
                //    richTextBox1.Text += "=======Matrices=======\r\n";
                //    richTextBox1.Text += string.Format("unk0:\t\t{0:d3}\r\n", atpl._0503.unk0);
                //    richTextBox1.Text += string.Format("unk1:\t\t{0:d3}\r\n", atpl._0503.unk1);
                //    richTextBox1.Text += "\r\n";

                //    foreach (var mat in atpl._0503.Data)
                //    {
                //        richTextBox1.Text += string.Format(
                //            "{0,11:F6}\t{1,11:F6}\t{2,11:F6}\t{3,11:F6}\r\n" +
                //            "{4,11:F6}\t{5,11:F6}\t{6,11:F6}\t{7,11:F6}\r\n" +
                //            "{8,11:F6}\t{9,11:F6}\t{10,11:F6}\t{11,11:F6}\r\n" +
                //            "{12,11:F6}\t{13,11:F6}\t{14,11:F6}\t{15,11:F6}\r\n\r\n",
                //            mat.m11, mat.m12, mat.m13, 0f,
                //            mat.m21, mat.m22, mat.m23, 0f,
                //            mat.m31, mat.m32, mat.m33, 0f,
                //            mat.m41, mat.m42, mat.m43, 1f);
                //    }
                //}
            }
            #endregion

            #region Scene
            else if (obj is Scene)
            {
                var bsp = (Scene)obj;
                richTextBox1.Text += string.Format("Name:\t\t{0}\r\n", bsp.Name);
                richTextBox1.Text += string.Format("Mat Count:\t{0:d3}\r\n", bsp.Materials.Count);
                richTextBox1.Text += string.Format("Obj Count:\t{0:d3}\r\n", bsp.Objects.Count);
                richTextBox1.Text += "\r\n";

                richTextBox1.Text += "=======2002=======\r\n";
                richTextBox1.Text += string.Format("unk0:\t\t{0}\r\nunk1:\t\t{1}\r\nunk2:\t\t{2}\r\n", bsp._2002.unk0, bsp._2002.unk1, bsp._2002.unk2);
                richTextBox1.Text += "bounds:\r\n";
                richTextBox1.Text += string.Format("{0,11:F6}\t{1,11:F6}\t{2,11:F6}\r\n{3,11:F6}\t{4,11:F6}\t{5,11:F6}\r\n",
                    bsp._2002.Bounds.XBounds.Min, bsp._2002.Bounds.YBounds.Min, bsp._2002.Bounds.ZBounds.Min,
                    bsp._2002.Bounds.XBounds.Max, bsp._2002.Bounds.YBounds.Max, bsp._2002.Bounds.ZBounds.Max);
                richTextBox1.Text += "\r\n";
                richTextBox1.Text += string.Format("unkPos0:\t{0}\r\n", bsp._2002.unkPos0.ToString(3, ", "));
                richTextBox1.Text += "\r\n";

                richTextBox1.Text += "=======2102=======\r\n";
                richTextBox1.Text += string.Format("count\t\t{0}\r\n", bsp._2102.unk0);
                richTextBox1.Text += string.Format("size\t\t{0}\r\n", bsp._2102.BlockSize);
                richTextBox1.Text += "\r\n";


                richTextBox1.Text += "=======2202=======\r\n";
                richTextBox1.Text += string.Format("unk0\t\t{0}\r\n", bsp._2202.unk0);
                richTextBox1.Text += string.Format("unk1:\t\t{0}\r\nunk2:\t\t{1}\r\nunk3:\t\t{2}\r\n", bsp._2202.unk1, bsp._2202.unk2, bsp._2202.unk3);
                richTextBox1.Text += "list:\r\n";

                for (int i = 0; i < Math.Min(bsp._2202.unkList.Length, 1000); i++)
                    richTextBox1.Text += bsp._2202.unkList[i].ToString("d7") + ((i % 10 == 9) ? "\r\n" : " ");
                
            }
            #endregion

            #region Script
            else if (obj is StringBlock_BA01)
            {
                var sc = (StringBlock_BA01)obj;
                richTextBox1.Text += sc.Data.Replace("\\t", "\t").Replace("\\n", "\n");
            }
            #endregion

            #region Node
            else if (obj is Node)
            {
                var node = (Node)obj;

                #region Experimental
                //richTextBox1.Text += "=======SceneData=======\r\n";
                //var idx = pak.SceneData.indices[node._B903.ID];
                //var dat = pak.SceneData.unkS0[idx];
                //var sbb = pak.SceneData.unkBounds;
                //var fmt = "[{0,8}, {1,8}, {2,8}] ";

                //richTextBox1.Text += "index:\t" + idx.ToString("d5") + "\r\n\r\n";
                //richTextBox1.Text += "\r\n";

                //richTextBox1.Text += "sint16:  ";
                //for (int i = 0; i < 12; i += 3)
                //{
                //    int a = dat.unk0[i + 0];
                //    int b = dat.unk0[i + 1];
                //    int c = dat.unk0[i + 2];
                //    richTextBox1.Text += string.Format(fmt, a.ToString("d6"), b.ToString("d6"), c.ToString("d6"));
                //}
                //richTextBox1.Text += "\r\n";
                //richTextBox1.Text += "uint16:  ";
                //for (int i = 0; i < 12; i += 3)
                //{
                //    int a = BitConverter.ToUInt16(BitConverter.GetBytes((short)dat.unk0[i + 0]), 0);
                //    int b = BitConverter.ToUInt16(BitConverter.GetBytes((short)dat.unk0[i + 1]), 0);
                //    int c = BitConverter.ToUInt16(BitConverter.GetBytes((short)dat.unk0[i + 2]), 0);
                //    richTextBox1.Text += string.Format(fmt, a.ToString("d6"), b.ToString("d6"), c.ToString("d6"));
                //}
                //richTextBox1.Text += "\r\n";
                //richTextBox1.Text += "sfloat:  ";
                //for (int i = 0; i < 12; i += 3)
                //{
                //    float a = dat.unk0[i + 0];
                //    float b = dat.unk0[i + 1];
                //    float c = dat.unk0[i + 2];

                //    a = (a + (float)0x7FFF) / (float)0xFFFF;
                //    b = (b + (float)0x7FFF) / (float)0xFFFF;
                //    c = (c + (float)0x7FFF) / (float)0xFFFF;

                //    //a = a * sbb.XBounds.Length + sbb.XBounds.Min;
                //    //b = b * sbb.YBounds.Length + sbb.YBounds.Min;
                //    //c = c * sbb.ZBounds.Length + sbb.ZBounds.Min;

                //    richTextBox1.Text += string.Format(fmt, a.ToString("F6"), b.ToString("F6"), c.ToString("F6"));
                //}
                //richTextBox1.Text += "\r\n";
                //richTextBox1.Text += "ufloat:  ";
                //for (int i = 0; i < 12; i += 3)
                //{
                //    float a = BitConverter.ToUInt16(BitConverter.GetBytes((short)dat.unk0[i + 0]), 0);
                //    float b = BitConverter.ToUInt16(BitConverter.GetBytes((short)dat.unk0[i + 1]), 0);
                //    float c = BitConverter.ToUInt16(BitConverter.GetBytes((short)dat.unk0[i + 2]), 0);

                //    a = a / (float)0xFFFF;
                //    b = b / (float)0xFFFF;
                //    c = c / (float)0xFFFF;

                //    //a = a * sbb.XBounds.Length + sbb.XBounds.Min;
                //    //b = b * sbb.YBounds.Length + sbb.YBounds.Min;
                //    //c = c * sbb.ZBounds.Length + sbb.ZBounds.Min;

                //    richTextBox1.Text += string.Format(fmt, a.ToString("F6"), b.ToString("F6"), c.ToString("F6"));
                //}
                //richTextBox1.Text += "\r\n\r\n\r\n"; 
                #endregion

                richTextBox1.Text += string.Format("Base Address:\t{0:d9}\t({1})\r\n", node.mainAddress, (node.mainAddress + baseAddress));
                richTextBox1.Text += string.Format("Name:\t\t{0}\r\n", node._B903.Name);
                richTextBox1.Text += string.Format("ID:\t\t{0:d3}\t\t({1})\r\n", node._B903.ID, leString(node._B903.ID, 2));
                richTextBox1.Text += string.Format("x2400:\t\t{0:d4}\t\t({1})\r\n", node._B903.x2400, leString(node._B903.x2400, 2));
                richTextBox1.Text += string.Format("unk0:\t\t{0:d3}\t\t({1})\r\n", node._B903.unk0, leString(node._B903.unk0, 1));
                richTextBox1.Text += string.Format("unk1:\t\t{0:d4}\t\t({1})\r\n", node._B903.unk1, leString(node._B903.unk1, 2));
                richTextBox1.Text += string.Format("unk2:\t\t{0:d4}\t\t({1})\r\n", node._B903.unk2, leString(node._B903.unk2, 2));
                richTextBox1.Text += string.Format("Vert Count:\t{0:d6}\r\n", node._B903.VertCount);
                richTextBox1.Text += string.Format("Face Count:\t{0:d6}\r\n", node._B903.FaceCount);
                richTextBox1.Text += "\r\n";

                if (node._3301 != null)
                {
                    richTextBox1.Text += "=======Rigging=======\r\n";
                    richTextBox1.Text += string.Format("FirstNode:\t{0:D4}\r\nNodeCount:\t{1:D4}\r\n", node._3301.FirstNodeID, node._3301.NodeCount);
                    richTextBox1.Text += string.Format("weights:\t{0}\r\n", node._1A01 != null);
                    richTextBox1.Text += "\r\n";
                }

                richTextBox1.Text += "=======Matrix=======\r\n";
                richTextBox1.Text += string.Format(
                    "{0,11:F6}\t{1,11:F6}\t{2,11:F6}\t{3,11:F6}\r\n" + 
                    "{4,11:F6}\t{5,11:F6}\t{6,11:F6}\t{7,11:F6}\r\n" + 
                    "{8,11:F6}\t{9,11:F6}\t{10,11:F6}\t{11,11:F6}\r\n" + 
                    "{12,11:F6}\t{13,11:F6}\t{14,11:F6}\t{15,11:F6}\r\n",
                    node.Transform.Data.m11, node.Transform.Data.m12, node.Transform.Data.m13, 0f,
                    node.Transform.Data.m21, node.Transform.Data.m22, node.Transform.Data.m23, 0f,
                    node.Transform.Data.m31, node.Transform.Data.m32, node.Transform.Data.m33, 0f,
                    node.Transform.Data.m41, node.Transform.Data.m42, node.Transform.Data.m43, 1f);
                richTextBox1.Text += "\r\n";

                richTextBox1.Text += string.Format("NodeIndex:\t{0}\r\n", node.BoneIndex);
                richTextBox1.Text += string.Format("Mesh Count:\t{0}\r\n", ((node.Submeshes == null) ? 0 : node.Submeshes.Count));
                richTextBox1.Text += string.Format("Parent ID:\t{0:d3}\t\t({1})\r\n", node.ParentID, leString(node.ParentID, 2));
                if (node._2901 != null) richTextBox1.Text += string.Format("Inherits ID:\t{0:d3}\t\t({1})\r\n", node._2901.InheritID, leString(node._2901.InheritID, 2));
                richTextBox1.Text += "\r\n";

                if (node.BoundingBox != null)
                {
                    richTextBox1.Text += "=======Bounds=======\r\n";
                    richTextBox1.Text += string.Format("{0,11:F6}\t{1,11:F6}\t{2,11:F6}\r\n{3,11:F6}\t{4,11:F6}\t{5,11:F6}\r\n",
                        node.BoundingBox.Data.XBounds.Min, node.BoundingBox.Data.YBounds.Min, node.BoundingBox.Data.ZBounds.Min,
                        node.BoundingBox.Data.XBounds.Max, node.BoundingBox.Data.YBounds.Max, node.BoundingBox.Data.ZBounds.Max);
                    richTextBox1.Text += "\r\n";
                }

                if (node._B903.VertCount > 0 || node.Submeshes != null)
                {
                    richTextBox1.Text += "======Vert Info======\r\n";
                    richTextBox1.Text += string.Format("x1200:\t\t{0:d4}\t\t({1})\r\n", node._2E01.x1200, leString(node._2E01.x1200, 2));
                    richTextBox1.Text += string.Format("geomUnk01:\t{0:d3}\t\t({1})\r\n", node._2E01.geomUnk01, leString(node._2E01.geomUnk01, 1));
                    richTextBox1.Text += string.Format("x4001:\t\t{0:d4}\t\t({1})\r\n", node._2E01.x4001, leString(node._2E01.x4001, 2));

                    //if (node.Vertices != null)
                    //{
                    //    richTextBox1.Text += string.Format("CentreX:\t{0:d4}\t\t({1})\r\n", node.Vertices.CentreX, leString(node.Vertices.CentreX, 2));
                    //    richTextBox1.Text += string.Format("CentreY:\t{0:d4}\t\t({1})\r\n", node.Vertices.CentreY, leString(node.Vertices.CentreY, 2));
                    //    richTextBox1.Text += string.Format("CentreZ:\t{0:d4}\t\t({1})\r\n", node.Vertices.CentreZ, leString(node.Vertices.CentreZ, 2));
                    //    richTextBox1.Text += string.Format("RadiusX:\t{0:d4}\t\t({1})\r\n", node.Vertices.RadiusX, leString(node.Vertices.RadiusX, 2));
                    //    richTextBox1.Text += string.Format("RadiusY:\t{0:d4}\t\t({1})\r\n", node.Vertices.RadiusY, leString(node.Vertices.RadiusY, 2));
                    //    richTextBox1.Text += string.Format("RadiusZ:\t{0:d4}\t\t({1})\r\n", node.Vertices.RadiusZ, leString(node.Vertices.RadiusZ, 2));
                    //}

                    if (node._3001 != null)
                    {
                        richTextBox1.Text += string.Format("unkUV0:\t{0:d4}\t\t({1})\r\n", node._3001.unkUV0, leString(node._3001.unkUV0, 2));
                        richTextBox1.Text += string.Format("unkUV1:\t{0:d3}\t\t({1})\r\n", node._3001.unkUV1, leString(node._3001.unkUV1, 1));
                        richTextBox1.Text += string.Format("unkUV2:\t{0:d3}\t\t({1})\r\n", node._3001.unkUV2, leString(node._3001.unkUV2, 1));
                        richTextBox1.Text += string.Format("unkUV3:\t{0:d3}\t\t({1})\r\n", node._3001.unkUV3, leString(node._3001.unkUV3, 1));
                        richTextBox1.Text += string.Format("unkUV4:\t{0:d3}\t\t({1})\r\n", node._3001.unkUV4, leString(node._3001.unkUV4, 1));
                        richTextBox1.Text += string.Format("UVSize:\t{0:d3}\t\t({1})\r\n", node._3001.DataSize, leString(node._3001.DataSize, 1));
                    }

                    richTextBox1.Text += "\r\n";
                }

                if (node.Submeshes == null) return;

                if (node._2F01 != null)
                {
                    richTextBox1.Text += string.Format("unkCC:\t\t{0:d3}\t\t({1})\r\n", node._2F01.unkCC, leString(node._2F01.unkCC, 1));
                    richTextBox1.Text += string.Format("unkC0:\t\t{0:d3}\t\t({1})\r\n", node._2F01.unkC0, leString(node._2F01.unkC0, 4));
                }
                
                //for (int i = 0; i < 5; i++)
                //    richTextBox1.Text += string.Format("unkC1[{0}]:\t{1:d3}\t\t({2})\r\n", i, node.unkC1[i], leString(node.unkC1[i], 4));
                richTextBox1.Text += "\r\n";

                richTextBox1.Text += string.Format("======Submeshes======\t{0:d9}\t({1:d9})\r\n", node.subAddress, (node.subAddress + baseAddress));
                richTextBox1.Text += string.Format("{0}\r\n", node.unk0);
                richTextBox1.Text += string.Format("{0}\r\n\r\n", node.unk1);
                
                foreach (var sub in node.Submeshes)
                {
                    richTextBox1.Text += string.Format("Vert Bounds:\t{0:d6} - {1:d6} ({2:d6})\r\n", sub.VertStart, (sub.VertStart + sub.VertLength - 1), sub.VertLength);
                    richTextBox1.Text += string.Format("Face Bounds:\t{0:d6} - {1:d6} ({2:d6})\r\n", sub.FaceStart, (sub.FaceStart + sub.FaceLength - 1), sub.FaceLength);
                    richTextBox1.Text += string.Format("Mat Count:\t{0:d3}\r\n", sub.MaterialCount);
                    richTextBox1.Text += string.Format("Mat Index:\t{0:d3}\r\n", sub.MaterialIndex);

                    if (sub._3401 != null) richTextBox1.Text += string.Format("unkID0:\t{0:d3}\t\t({1})\r\n", sub._3401.unkID0, leString(sub._3401.unkID0, 2));

                    if (sub._3201 != null)
                    {
                        richTextBox1.Text += string.Format("unkID0:\t{0:d3}\t\t({1})\r\n", sub._3201.unkID0, leString(sub._3201.unkID0, 2));
                        richTextBox1.Text += string.Format("unkCount0:\t{0:d4}\t\t({1})\r\n", sub._3201.unkCount0, leString(sub._3201.unkCount0, 1));
                        richTextBox1.Text += string.Format("unkID1:\t{0:d3}\t\t({1})\r\n", sub._3201.unkID1, leString(sub._3201.unkID1, 2));
                        richTextBox1.Text += string.Format("unkCount1:\t{0:d4}\t\t({1})\r\n", sub._3201.unkCount1, leString(sub._3201.unkCount1, 1));
                    }

                    richTextBox1.Text += string.Format("unkf0,1:\t{0:F6}, {1:F6}\r\n", sub._1C01.unkf0, sub._1C01.unkf1);

                    richTextBox1.Text += string.Format(
                        "unkb0:\t\t{0:F6}, {1:F6}, {2:F6}, {3:F6}, {4:F6}, {5:F6}, {6:F6}, {7:F6}\r\n",
                        sub._2001.unkf[0], sub._2001.unkf[1], sub._2001.unkf[2], sub._2001.unkf[3],
                        sub._2001.unkf[4], sub._2001.unkf[5], sub._2001.unkf[6], sub._2001.unkf[7]);

                    if (sub._2801 != null)
                    {
                        richTextBox1.Text += string.Format("x81:\t\t{0:d3}\t\t({1})\r\n", sub._2801.x81.ToString("d3"), leString(sub._2801.x81, 1));
                        richTextBox1.Text += string.Format("unk4:\t\t{0:d4}\t\t({1})\r\n", sub._2801.unk4.ToString("d4"), leString(sub._2801.unk4, 4));
                        richTextBox1.Text += string.Format("xFF:\t\t{0:d3}\t\t({1})\r\n", sub._2801.xFF.ToString("d3"), leString(sub._2801.xFF, 1));
                        richTextBox1.Text += string.Format("x1300:\t\t{0:d4}\t\t({1})\r\n", sub._2801.x1300.ToString("d4"), leString(sub._2801.x1300, 2));
                        richTextBox1.Text += string.Format("VertexCount:\t{0:d4}\t\t({1})\r\n", sub._2801.VertexCount.ToString("d4"), leString(sub._2801.VertexCount, 2));
                        richTextBox1.Text += string.Format("IndexCount:\t{0:d4}\t\t({1})\r\n", sub._2801.IndexCount.ToString("d4"), leString(sub._2801.IndexCount, 2));
                        richTextBox1.Text += string.Format("unkID2:\t{0:d3}\t\t({1})\r\n", sub._2801.unkID2.ToString("d3"), leString(sub._2801.unkID2, 4));
                        richTextBox1.Text += string.Format("unk7:\t\t{0:d6}\t\t({1})\r\n", sub._2801.unk7.ToString("d6"), leString(sub._2801.unk7, 4));
                        richTextBox1.Text += string.Format("unk8:\t\t{0:d6}\t\t({1})\r\n", sub._2801.unk8.ToString("d6"), leString(sub._2801.unk8, 4));
                        richTextBox1.Text += string.Format("unk9a:\t\t{0:d4}\t\t({1})\r\n", sub._2801.unk9a.ToString("d4"), leString(sub._2801.unk9a, 2));
                        richTextBox1.Text += string.Format("unk9b:\t\t{0:d4}\t\t({1})\r\n", sub._2801.unk9b.ToString("d4"), leString(sub._2801.unk9b, 2));
                    }

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

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
        private S3DPak pak;
        private S3DPak.PakItem item;
        private S3DModelBase model;

        public S3DExplorer()
        {
            InitializeComponent();
        }

        public void LoadModelHierarchy(S3DPak Pak, S3DPak.PakItem Item, bool useInherit)
        {
            pak = Pak;
            item = Item;

            switch (item.Type)
            {
                case PakType.Models:
                    model = new S3DATPL(Pak, item);
                    break;
                case PakType.BSP:
                    model = new S3DBSP(Pak, Item);
                    break;
                default:
                    return;
            }


            tvSub.Nodes.Clear();
            richTextBox1.Clear();

            var root = new TreeNode(model.Name) { Tag = model, ImageIndex = 7, SelectedImageIndex = 7 };

            #region Materials
            var mNode = new TreeNode("Materials") { ImageIndex = 1, SelectedImageIndex = 1 };
            int i = 0;
            foreach (var mat in model.Materials)
                mNode.Nodes.Add(new TreeNode("[" + (i++).ToString("d3") + "] " + mat.Name) { ImageIndex = 2, SelectedImageIndex = 2, Tag = mat });
            root.Nodes.Add(mNode);
            #endregion

            #region Scripts
            if (item.Type == PakType.BSP)
            {
                var sNode = new TreeNode("Scripts") { ImageIndex = 1, SelectedImageIndex = 1 };
                i = 0;
                foreach (var sc in ((S3DBSP)model).Scripts)
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
                var obj = (S3DObject)pair.Value.Tag;
                int id = (useInherit) ? obj.inheritIndex : obj.ParentID;

                if (id == -1)
                {
                    nList.Add(pair.Value);
                    continue;
                }

                TreeNode pNode;
                if (dic.TryGetValue(id, out pNode))
                    pNode.Nodes.Add(pair.Value);
            }

            root.Nodes.AddRange(nList.ToArray());
            tvSub.Nodes.Add(root);
            root.Expand();

            if (!useInherit)
                foreach (TreeNode node in root.Nodes)
                    node.Expand();

            tvSub.SelectedNode = root;
        }

        private void displayModelInfo(S3DATPL atpl, int baseAddress)
        {
            richTextBox1.Text = "";
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

        private void displayMaterialInfo(S3DMaterial mat, int baseAddress)
        {
            richTextBox1.Text = "";
            richTextBox1.Text += "x5601:\t\t" + mat.x5601.ToString("d4") + "\t\t(" + leString(mat.x5601, 2) + ")\r\n";
            richTextBox1.Text += "Next Address:\t" + mat.AddressOfNext.ToString("d9") + "\t(" + (mat.AddressOfNext + baseAddress).ToString("d9") + ")\r\n";
            richTextBox1.Text += "Name:\t\t" + mat.Name + "\r\n";
        }

        private void displayScriptInfo(S3DScript sc, int baseAddress)
        {
            richTextBox1.Text = "";
            richTextBox1.Text += "xBA01:\t\t" + sc.xBA01.ToString("d4") + "\t\t(" + leString(sc.xBA01, 2) + ")\r\n";
            richTextBox1.Text += "Next Address:\t" + sc.AddressOfNext.ToString("d9") + "\t(" + (sc.AddressOfNext + baseAddress).ToString("d9") + ")\r\n";
            richTextBox1.Text += "\r\n=======Script Data=====\r\n\r\n" + sc.Data.Replace("\\t", "\t").Replace("\\n", "\n") + "\r\n";
        }

        private void displayObjectInfo(S3DObject obj, int baseAddress)
        {
            richTextBox1.Text = "";
            richTextBox1.Text += "Base Address:\t" + obj.mainAddress.ToString("d9") + "\t(" + (obj.mainAddress + baseAddress).ToString("d9") + ")\r\n";
            richTextBox1.Text += "xF000:\t\t" + obj.xF000.ToString("d4") + "\t\t(" + leString(obj.xF000, 2) + ")\r\n";
            richTextBox1.Text += "Next Address:\t" + obj.PreNextAddress.ToString("d9") + "\t(" + (obj.PreNextAddress + baseAddress).ToString("d9") + ")\r\n";
            richTextBox1.Text += "xB903:\t\t" + obj.xB903.ToString("d4") + "\t\t(" + leString(obj.xB903, 2) + ")\r\n";
            richTextBox1.Text += "Name:\t\t" + obj.Name + "\r\n";
            richTextBox1.Text += "ID:\t\t" + obj.ID.ToString("d3") + "\t\t(" + leString(obj.ID, 2) + ")\r\n";
            richTextBox1.Text += "x2400:\t\t" + obj.x2400.ToString("d4") + "\t\t(" + leString(obj.x2400, 2) + ")\r\n";
            richTextBox1.Text += "unk0:\t\t" + obj.unk0.ToString("d3") + "\t\t(" + leString(obj.unk0, 1) + ")\r\n";
            richTextBox1.Text += "unk1:\t\t" + obj.unk1.ToString("d4") + "\t\t(" + leString(obj.unk1, 2) + ")\r\n";
            richTextBox1.Text += "unk2:\t\t" + obj.unk2.ToString("d4") + "\t\t(" + leString(obj.unk2, 2) + ")\r\n";
            richTextBox1.Text += "Vert Count:\t" + obj.VertCount.ToString("d6") + "\r\n";
            richTextBox1.Text += "Face Count:\t" + obj.FaceCount.ToString("d6") + "\r\n";
            richTextBox1.Text += "unk3:\t\t" + obj.unk3.ToString("d4") + "\t\t(" + leString(obj.unk3, 2) + ")\r\n";
            richTextBox1.Text += "\r\n";

            richTextBox1.Text += "=======Matrix=======\r\n";
            richTextBox1.Text += string.Format("{0, 11}\t{1, 11}\t{2, 11}\t{3, 11}\r\n{4, 11}\t{5, 11}\t{6, 11}\t{7, 11}\r\n{8, 11}\t{9, 11}\t{10, 11}\t{11, 11}\r\n{12, 11}\t{13, 11}\t{14, 11}\t{15, 11}\r\n",
                obj.Transform.m11.ToString("F6"), obj.Transform.m12.ToString("F6"), obj.Transform.m13.ToString("F6"), 0f.ToString("F6"),
                obj.Transform.m21.ToString("F6"), obj.Transform.m22.ToString("F6"), obj.Transform.m23.ToString("F6"), 0f.ToString("F6"),
                obj.Transform.m31.ToString("F6"), obj.Transform.m32.ToString("F6"), obj.Transform.m33.ToString("F6"), 0f.ToString("F6"),
                obj.Transform.m41.ToString("F6"), obj.Transform.m42.ToString("F6"), obj.Transform.m43.ToString("F6"), 1f.ToString("F6"));
            richTextBox1.Text += "\r\n";

            richTextBox1.Text += "xFA00:\t\t" + obj.xFA00.ToString("d4") + "\t\t(" + leString(obj.xFA00, 2) + ")\r\n";
            richTextBox1.Text += "NodeIndex:\t" + obj.NodeIndex.ToString() + "\r\n";
            richTextBox1.Text += "unk4:\t\t" + obj.unk4.ToString("d4") + "\t\t(" + leString(obj.unk4, 2) + ")\r\n";
            richTextBox1.Text += "Mesh Count:\t" + ((obj.Submeshes == null) ? "0" : obj.Submeshes.Count.ToString()) + "\r\n";
            richTextBox1.Text += "Parent ID:\t" + obj.ParentID.ToString("d3") + "\t\t(" + leString(obj.ParentID, 2) + ")\r\n";
            richTextBox1.Text += "Inherits ID:\t" + obj.inheritIndex.ToString("d2") + "\t\t(" + leString(obj.inheritIndex, 2) + ")\r\n";
            richTextBox1.Text += "\r\n";

            if (obj.BoundingBox != null)
            {
                richTextBox1.Text += "=======Bounds=======\r\n";
                richTextBox1.Text += string.Format("{0, 11}\t{1, 11}\t{2, 11}\r\n{3, 11}\t{4, 11}\t{5, 11}\r\n",
                    obj.BoundingBox.XBounds.Min.ToString("F6"), obj.BoundingBox.YBounds.Min.ToString("F6"), obj.BoundingBox.ZBounds.Min.ToString("F6"),
                    obj.BoundingBox.XBounds.Max.ToString("F6"), obj.BoundingBox.YBounds.Max.ToString("F6"), obj.BoundingBox.ZBounds.Max.ToString("F6"));
                richTextBox1.Text += "\r\n";
            }

            if (obj.VertCount > 0 || obj.Submeshes != null)
            {
                richTextBox1.Text += "======Vert Info======\r\n";
                richTextBox1.Text += "x1200:\t\t" + obj.x1200.ToString("d4") + "\t\t(" + leString(obj.x1200, 2) + ")\r\n";
                richTextBox1.Text += "geomUnk01:\t" + obj.geomUnk01.ToString("d3") + "\t\t(" + leString(obj.geomUnk01, 1) + ")\r\n";
                richTextBox1.Text += "x4001:\t\t" + obj.x4001.ToString("d4") + "\t\t(" + leString(obj.x4001, 2) + ")\r\n";
                richTextBox1.Text += "xF100:\t\t" + obj.xF100.ToString("d4") + "\t\t(" + leString(obj.xF100, 2) + ")\r\n";
                richTextBox1.Text += "CentreX:\t" + obj.CentreX.ToString("d4") + "\t\t(" + leString(obj.CentreX, 2) + ")\r\n";
                richTextBox1.Text += "CentreY:\t" + obj.CentreY.ToString("d4") + "\t\t(" + leString(obj.CentreY, 2) + ")\r\n";
                richTextBox1.Text += "CentreZ:\t" + obj.CentreZ.ToString("d4") + "\t\t(" + leString(obj.CentreZ, 2) + ")\r\n";
                richTextBox1.Text += "RadiusX:\t" + obj.RadiusX.ToString("d4") + "\t\t(" + leString(obj.RadiusX, 2) + ")\r\n";
                richTextBox1.Text += "RadiusY:\t" + obj.RadiusY.ToString("d4") + "\t\t(" + leString(obj.RadiusY, 2) + ")\r\n";
                richTextBox1.Text += "RadiusZ:\t" + obj.RadiusZ.ToString("d4") + "\t\t(" + leString(obj.RadiusZ, 2) + ")\r\n";
                if (obj.preUV != null) richTextBox1.Text += "preUV:\t\t" + "[" + BitConverter.ToString(obj.preUV, 0).Replace("-", string.Empty) + "]\r\n";
                else richTextBox1.Text += "preUV:\t\t[NA]\r\n";
                richTextBox1.Text += "\r\n";
            }

            if (obj.Submeshes == null) return;

            richTextBox1.Text += "======Submeshes======\t" + obj.subAddress.ToString("d9") + "\t(" + (obj.subAddress + baseAddress).ToString("d9") + ")\r\n";
            foreach (var sub in obj.Submeshes)
            {
                richTextBox1.Text += "Vert Bounds:\t" + sub.VertStart.ToString("d6") + " - " + (sub.VertStart + sub.VertLength).ToString("d6") + "  (" + sub.VertLength.ToString("d6") + ")\r\n";
                richTextBox1.Text += "Face Bounds:\t" + sub.FaceStart.ToString("d6") + " - " + (sub.FaceStart + sub.FaceLength).ToString("d6") + "  (" + sub.FaceLength.ToString("d6") + ")\r\n";
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

        private string leString(int value, int byteLength)
        {
            var b = BitConverter.GetBytes(value);
            return BitConverter.ToString(b, 0).Replace("-", string.Empty).Substring(0, byteLength * 2);
        }

        private void tvSub_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var tag = tvSub.SelectedNode.Tag;

            if (tag is S3DATPL)
                displayModelInfo((S3DATPL)tag, item.Offset);
            else if (tag is S3DMaterial)
                displayMaterialInfo((S3DMaterial)tag, item.Offset);
            else if (tag is S3DScript)
                displayScriptInfo((S3DScript)tag, item.Offset);
            else if (tag is S3DObject)
                displayObjectInfo((S3DObject)tag, item.Offset);
        }
    }
}

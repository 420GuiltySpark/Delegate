using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.DataTypes;
using Adjutant.Library;
using Adjutant.Library.S3D;
using Adjutant.Library.Endian;
using Adjutant.Library.Definitions;
using Adjutant.Library.Controls;

namespace Adjutant.Library.S3D
{
    public class Template : TemplateBase
    {
        public int unk0;
        public int xF000;
        public int PreNodeInfoAddress;
        public int x2C01;
        public int unk1;

        public List<NodeInfo> NodeInfo;
        public List<Matrix> unkMatList;

        public Template(PakFile Pak, PakFile.PakTag Item, bool loadMesh)
        {
            var reader = Pak.Reader;
            reader.EndianType = EndianFormat.LittleEndian;
            reader.SeekTo(Item.Offset);

            reader.ReadInt16(); //E402
            reader.ReadInt32(); //filesize
            
            reader.ReadInt16(); //E502
            reader.ReadInt32(); //address of 1603
            reader.ReadInt32(); //LPTA (probs part of the string)
            Name = reader.ReadNullTerminatedString();           
            reader.ReadByte(); //00

            reader.ReadInt16(); //1603
            reader.ReadInt32(); //address of 5501
            reader.ReadBytes(3); //02 01 01

            reader.ReadInt16(); //5501
            reader.ReadInt32(); //address to end of materials

            int count = reader.ReadInt32();
            Materials = new List<Material>();
            for (int i = 0; i < count; i++)
                Materials.Add(new Material(Pak, Item));

            reader.ReadInt16(); //0100
            unk0 = reader.ReadInt32();
            xF000 = reader.ReadInt16();
            PreNodeInfoAddress = reader.ReadInt32();
            x2C01 = reader.ReadInt16();
            unk1 = reader.ReadInt32();

            count = reader.ReadInt32();
            Objects = new List<Node>();
            for (int i = 0; i < count; i++)
                Objects.Add(new Node(Pak, Item, loadMesh));

            foreach (var obj in Objects)
                if (obj.isInheritor)
                    Objects[obj.inheritID].isInherited = true;

            NodeInfo = new List<NodeInfo>();
            unkMatList = new List<Matrix>();

            try
            {
                reader.SeekTo(Item.Offset + PreNodeInfoAddress);
                
                reader.ReadInt16(); //0100
                reader.ReadInt32(); //address
                
                reader.ReadInt16(); //E802
                var addr = reader.ReadInt32();

                count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                    NodeInfo.Add(new NodeInfo(Pak, Item));

                reader.SeekTo(Item.Offset + addr);

                reader.ReadInt16(); //E602
                addr = reader.ReadInt32();
                reader.ReadInt32(); //block count
                //havent mapped this block, assumed anim/sound related

                reader.SeekTo(Item.Offset + addr);

                reader.ReadInt16(); //0100
                reader.SeekTo(Item.Offset + reader.ReadInt32());

                if (reader.ReadInt16() == 541) //1D02
                    reader.SeekTo(Item.Offset + reader.ReadInt32());
                else //BA01
                {
                    reader.ReadInt32(); //address to end of string
                    reader.ReadNullTerminatedString(); //LOD related
                    int t = reader.ReadInt16(); //1D02 [541]
                    addr = reader.ReadInt32(); //address to end of next block
                    reader.ReadInt32(); //block count? havent seen used

                    reader.SeekTo(Item.Offset + addr);

                    reader.ReadInt16(); //1103
                    reader.SeekTo(Item.Offset + reader.ReadInt32());
                }

                reader.ReadInt16(); //0403
                reader.SeekTo(Item.Offset + reader.ReadInt32());
                // ^assume null terminated string after this address, only been 0x00 so far

                if (reader.ReadInt16() == 773) //0503
                {
                    addr = reader.ReadInt32();

                    reader.ReadInt16(); //0D03
                    reader.ReadInt32(); //points to 6 before addr

                    count = reader.ReadInt32(); //may not have matrixdata even if count > 0
                    reader.ReadInt16(); //no idea
                    reader.ReadByte();  //no idea
                    for (int i = 0; i < count; i++)
                    {
                        var mat = new Matrix();
                        
                        mat.m11 = reader.ReadSingle();
                        mat.m12 = reader.ReadSingle();
                        mat.m13 = reader.ReadSingle();
                        reader.ReadSingle(); //0.0f
                        mat.m21 = reader.ReadSingle();
                        mat.m22 = reader.ReadSingle();
                        mat.m23 = reader.ReadSingle();
                        reader.ReadSingle(); //0.0f
                        mat.m31 = reader.ReadSingle();
                        mat.m32 = reader.ReadSingle();
                        mat.m33 = reader.ReadSingle();
                        reader.ReadSingle(); //0.0f
                        mat.m41 = reader.ReadSingle();
                        mat.m42 = reader.ReadSingle();
                        mat.m43 = reader.ReadSingle();
                        reader.ReadSingle(); //1.0f

                        unkMatList.Add(mat);
                    }

                    reader.ReadInt16(); //0100
                    reader.ReadInt32(); //address, same as addr
                    reader.ReadInt16(); //0803

                    reader.SeekTo(Item.Offset + addr + 2);
                }

                reader.ReadInt32(); //address to end of bounds values
                reader.ReadInt32(); //bounds count?
                var min = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                var max = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                RenderBounds = new render_model.BoundingBox();
                RenderBounds.XBounds = new RealBounds(min.x, max.x);
                RenderBounds.YBounds = new RealBounds(min.y, max.y);
                RenderBounds.ZBounds = new RealBounds(min.z, max.z);
                RenderBounds.UBounds = new RealBounds(0f, 0f);
                RenderBounds.VBounds = new RealBounds(0f, 0f);

                reader.ReadInt16(); //0E03
                addr = reader.ReadInt32(); //address to end of next block
                reader.ReadInt32(); //block count? havent seen used

                reader.SeekTo(Item.Offset + addr);
                
                reader.ReadInt16(); //0100
                reader.ReadInt32(); //address to EOF
            }
            catch { }
        }

        public override void Parse()
        {
            foreach (var obj in Objects)
            {
                if (obj.VertCount > 0)
                    ModelFunctions.DecompressVertex(ref obj.Vertices, obj.BoundingBox);
            }

            isParsed = true;
        }
    }

    public class NodeInfo
    {
        public int xE902;
        public int addr0; //points to 0100
        public float unk00; //assumed float, but always been zero so far
        public int xFA02;
        public int addr1; //points to FB02
        public float unk01, unk02, unk03; //position xyz relative to parent
        public int xFB02;
        public int addr2; //points to FC02
        public float unk04, unk05, unk06, unk07;
        public int xFC02;
        public int addr3; //points to 0A03
        public float unk08, unk09, unk10; //always 1/1/1
        public int x0A03;
        public int addr4; //points to 0100;
        public float unk11; //always 1, possibly node scale
        public int x0100;
        public int addr5; //points to next item

        public NodeInfo(PakFile Pak, PakFile.PakTag Item)
        {
            var reader = Pak.Reader;

            xE902 = reader.ReadInt16();
            addr0 = reader.ReadInt32();
            unk00 = reader.ReadSingle();

            xFA02 = reader.ReadInt16();
            addr1 = reader.ReadInt32();
            unk01 = reader.ReadSingle();
            unk02 = reader.ReadSingle();
            unk03 = reader.ReadSingle();

            if (false) //not always accurate, needs more mapping
            {
                xFB02 = reader.ReadInt16();
                addr2 = reader.ReadInt32();
                unk04 = reader.ReadSingle();
                unk05 = reader.ReadSingle();
                unk06 = reader.ReadSingle();
                unk07 = reader.ReadSingle();

                xFC02 = reader.ReadInt16();

                addr3 = reader.ReadInt32();
                unk08 = reader.ReadSingle();
                unk09 = reader.ReadSingle();
                unk10 = reader.ReadSingle();

                x0A03 = reader.ReadInt16();
                addr4 = reader.ReadInt32();
                unk11 = reader.ReadSingle();
            }
            else reader.SeekTo(Item.Offset + addr0);

            x0100 = reader.ReadInt16();
            addr5 = reader.ReadInt32();
        }
    }
}

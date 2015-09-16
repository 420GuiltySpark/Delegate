using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.DataTypes;
using Adjutant.Library;
using Adjutant.Library.S3D;
using Adjutant.Library.Endian;
using Adjutant.Library.Controls;

namespace Adjutant.Library.S3D.Blocks
{
    public abstract class S3DBlock
    {
        public int BaseAddress;
        public int Ident, EOBOffset;
        public int BlockSize;

        protected S3DBlock(EndianReader reader, int ident)
        {
            BaseAddress = (int)reader.Position;
            Ident = reader.ReadUInt16(EndianFormat.BigEndian);

            if (Ident != ident)
                throw new InvalidOperationException(string.Format("Block identifier mismatch. Expected 0x{0:X4}, got 0x{1:X4}.", ident, Ident));

            EOBOffset = reader.ReadInt32();
            BlockSize = EOBOffset - (BaseAddress + 6);
        }
    }

    public class unkBlock_XXXX : S3DBlock
    {
        public unkBlock_XXXX(EndianReader reader, int ident)
            : base(reader, ident)
        {
            reader.SeekTo(EOBOffset);
        }
    }

    public class StringBlock_BA01 : S3DBlock
    {
        public string Data;

        public StringBlock_BA01(EndianReader reader)
            : base(reader, 0xBA01)
        {
            Data = reader.ReadNullTerminatedString();
        }

        public override string ToString()
        {
            return Data;
        }
    }

    public class MatRefBlock_5601 : S3DBlock
    {
        public string Reference;

        public MatRefBlock_5601(EndianReader reader)
            : base(reader, 0x5601)
        {
            Reference = reader.ReadNullTerminatedString();
        }

        public override string ToString()
        {
            return Reference;
        }
    }

    #region Node Blocks
    public class Block_B903 : S3DBlock
    {
        public string Name;
        public int ID;
        public int x2400;
        public int unk0, unk1, unk2;
        public int VertCount, FaceCount;

        public Block_B903(EndianReader reader)
            : base(reader, 0xB903)
        {
            Name = reader.ReadNullTerminatedString();
            ID = reader.ReadInt16();
            x2400 = reader.ReadInt16();

            unk0 = reader.ReadByte();
            unk1 = reader.ReadInt16(); //possibly flags
            unk2 = reader.ReadInt16(); //possibly flags

            VertCount = reader.ReadInt32();
            FaceCount = reader.ReadInt32();
        }
    }

    public class Block_2901 : S3DBlock
    {
        public int InheritID;
        public int VertexOffset, IndexOffset;

        public Block_2901(EndianReader reader)
            : base(reader, 0x2901)
        {
            InheritID = reader.ReadInt16();
            VertexOffset = reader.ReadInt32();
            IndexOffset = reader.ReadInt32();
        }
    }

    public class Block_2E01 : S3DBlock
    {
        //public int BaseAddress;
        //public int x2E01, EOBAddress;
        public int x1200;
        public int geomUnk01; //03 for no materials, 86 for world, 87 for one material, 8F for two, 9F for three, BF for four
        public int x4001; //not always 4001

        public Block_2E01(EndianReader reader)
            : base(reader, 0x2E01)
        {
            //BaseAddress = (int)reader.Position;
            //x2E01 = reader.ReadInt16();
            //EOBAddress = reader.ReadInt32();

            x1200 = reader.ReadInt16();
            geomUnk01 = reader.ReadByte();
            x4001 = reader.ReadInt16();
        }
    }

    public class Block_3501 : S3DBlock
    {
        public int unk0, unk1, unk2, unk3, unk4, unk5;

        public Block_3501(EndianReader reader)
            : base(reader, 0x3501)
        {
            unk0 = reader.ReadInt16();
            unk1 = reader.ReadInt16();
            unk2 = reader.ReadInt16();
            unk3 = reader.ReadInt16(); //4801
            unk4 = reader.ReadInt16(); //4801
            unk5 = reader.ReadInt16(); //4801
        }
    }

    public class VertexBlock_F100 : S3DBlock
    {
        public int DataCount;
        public int CentreX, CentreY, CentreZ;
        public int RadiusX, RadiusY, RadiusZ;
        public Vertex[] Data;

        public VertexBlock_F100(EndianReader reader, bool loadMesh, int geomUnk01)
            : base(reader, 0xF100)
        {
            DataCount = reader.ReadInt32();
            Data = new Vertex[DataCount];
            if (DataCount == 0) return;

            if (geomUnk01 != 134)
            {
                CentreX = reader.ReadInt16();
                CentreY = reader.ReadInt16();
                CentreZ = reader.ReadInt16();
                RadiusX = reader.ReadInt16();
                RadiusY = reader.ReadInt16();
                RadiusZ = reader.ReadInt16();
            }

            if (!loadMesh) reader.Skip(DataCount * ((geomUnk01 == 134) ? 12 : 8));
            else for (int i = 0; i < DataCount; i++)
                {
                    Vertex v;

                    if (geomUnk01 == 134)
                    {
                        v = new Vertex() { FormatName = "S3D_World" };
                        var data = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        v.Values.Add(new VertexValue(data, VertexValue.ValueType.Float32_3, "position", 0));
                    }
                    else
                    {
                        v = new Vertex() { FormatName = "S3D_Compressed" };
                        var data = new RealQuat(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                        v.Values.Add(new VertexValue(data, VertexValue.ValueType.Int16_N4, "position", 0));
                    }
                    Data[i] = v;
                }
        }
    }

    public class UVDataBlock_3001 : S3DBlock
    {
        public int DataCount;
        public int x2E00;

        public int unkUV0, unkUV1, unkUV2, unkUV3, unkUV4;
        public int DataSize;

        public UVDataBlock_3001(EndianReader reader, bool loadMesh, Vertex[] Vertices)
            : base(reader, 0x3001)
        {
            DataCount = reader.ReadInt32(); //vCount

            x2E00 = reader.ReadInt16(); //2E00
            reader.EndianType = EndianFormat.BigEndian;

            unkUV0 = reader.ReadInt16(); //flags? 0x1C00 when world
            unkUV1 = reader.ReadByte();
            unkUV2 = reader.ReadByte();
            unkUV3 = reader.ReadByte();
            unkUV4 = reader.ReadByte(); //0x00 when world, else 0x20
            DataSize = reader.ReadByte();

            if (!loadMesh) reader.Skip(DataCount * DataSize);
            else for (int i = 0; i < DataCount; i++)
                {
                    RealQuat tex0 = new RealQuat();

                    #region switch
                    switch (DataSize)
                    {
                        case 8:
                            tex0 = RealQuat.FromUByteN4(reader.ReadUInt32());
                            reader.Skip(0);
                            break;
                        case 12:
                            reader.Skip(4);
                            break;
                        case 16:
                            reader.Skip(12);
                            break;
                        case 20:
                            reader.Skip(16);
                            break;
                        case 24:
                            reader.Skip(16);
                            break;
                        case 28:
                            reader.Skip(20);
                            break;
                        case 32:
                            reader.Skip(16);
                            break;
                        case 36:
                            reader.Skip(24);
                            break;
                        case 44:
                            reader.Skip(28);
                            break;
                    }
                    #endregion

                    //if (tex0.w == 1)
                    //  tex0 = tex0;

                    int u = reader.ReadInt16();
                    int v = reader.ReadInt16();

                    //var tex0 = new RealQuat(((float)a + (float)0) / (float)0xFFFF, ((float)b + (float)0) / (float)0xFFFF);
                    var tex1 = new RealQuat((float)u / (float)(0x7FFF), (float)v / (float)(0x7FFF));

                    #region switch
                    switch (DataSize)
                    {
                        case 8:
                            reader.Skip(0);
                            break;
                        case 12:
                            reader.Skip(4);
                            break;
                        case 16:
                            reader.Skip(0);
                            break;
                        case 20:
                            reader.Skip(0);
                            break;
                        case 24:
                            reader.Skip(4);
                            break;
                        case 28:
                            reader.Skip(4);
                            break;
                        case 32:
                            reader.Skip(12);
                            break;
                        case 36:
                            reader.Skip(8);
                            break;
                        case 44:
                            reader.Skip(12);
                            break;
                    }
                    #endregion

                    //Vertices[i].Values.Add(new VertexValue(tex0, 0, "normal", 0));
                    Vertices[i].Values.Add(new VertexValue(tex1, VertexValue.ValueType.Int16_N2, "texcoords", 0));
                }

            reader.EndianType = EndianFormat.LittleEndian;
        }
    }

    public class IndexBlock_F200 : S3DBlock
    {
        public int DataCount;
        public int[] Data;

        public IndexBlock_F200(EndianReader reader, bool loadMesh)
            : base(reader, 0xF200)
        {
            DataCount = reader.ReadInt32();
            Data = new int[DataCount * 3];
            if (DataCount == 0) return;

            if (!loadMesh) reader.Skip(DataCount * 6);
            else for (int i = 0; i < DataCount * 3; i++)
                    Data[i] = reader.ReadUInt16();
        }
    }

    public class BoundsBlock_1D01 : S3DBlock
    {
        public int DataCount; //always 1
        public RealBoundingBox Data;

        public BoundsBlock_1D01(EndianReader reader)
            : base(reader, 0x1D01)
        {
            DataCount = reader.ReadInt32();
            var min = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            var max = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            Data.XBounds = new RealBounds(min.x, max.x);
            Data.YBounds = new RealBounds(min.y, max.y);
            Data.ZBounds = new RealBounds(min.z, max.z);
        }
    }

    public class MatrixBlock_F900 : S3DBlock
    {
        public Matrix Data;

        public MatrixBlock_F900(EndianReader reader)
            : base(reader, 0xF900)
        {
            Data.m11 = reader.ReadSingle();
            Data.m12 = reader.ReadSingle();
            Data.m13 = reader.ReadSingle();
            reader.ReadSingle(); //0.0f
            Data.m21 = reader.ReadSingle();
            Data.m22 = reader.ReadSingle();
            Data.m23 = reader.ReadSingle();
            reader.ReadSingle(); //0.0f
            Data.m31 = reader.ReadSingle();
            Data.m32 = reader.ReadSingle();
            Data.m33 = reader.ReadSingle();
            reader.ReadSingle(); //0.0f
            Data.m41 = reader.ReadSingle();
            Data.m42 = reader.ReadSingle();
            Data.m43 = reader.ReadSingle();
            reader.ReadSingle(); //1.0f
        }
    }

    public class Block_2F01 : S3DBlock
    {
        public int unkCC, unkC0;

        public Block_2F01(EndianReader reader)
            : base(reader, 0x2F01)
        {
            unkCC = reader.ReadByte(); //count

            reader.ReadByte(); //index
            unkC0 = reader.ReadInt32(); //material scale

            //first block is above, skip the rest
            for (int i = 1; i < unkCC; i++)
            {
                reader.ReadByte(); //index
                reader.ReadInt32(); //count
            }
        }
    }

    public class ScriptRefBlock_8304 : S3DBlock
    {
        public int Reference;

        public ScriptRefBlock_8304(EndianReader reader)
            : base(reader, 0x8304)
        {
            Reference = reader.ReadInt32();
        }
    }

    public class Block_FD00 : S3DBlock
    {
        public string Data;

        public Block_FD00(EndianReader reader)
            : base(reader, 0xFD00)
        {
            reader.ReadInt16(); //BA01
            reader.ReadInt32(); //address
            Data = reader.ReadNullTerminatedString();
        }
    }

    #region Submesh Blocks
    public class Block_3201 : S3DBlock
    {
        public int unkID0, unkID1;
        public int unkCount0, unkCount1;

        public Block_3201(EndianReader reader)
            : base(reader, 0x3201)
        {
            unkID0 = reader.ReadInt16(); //points to first inheritor if skincompound, otherwise parent bone
            unkCount0 = reader.ReadByte(); //number of inheritors/bones (starts at unkID0 and increments through object IDs)
            unkID1 = reader.ReadInt16(); //secondary parent bone
            unkCount1 = reader.ReadByte(); //secondary number of bones
        }
    }

    public class Block_3401 : S3DBlock
    {
        public int unkID0;

        public Block_3401(EndianReader reader)
            : base(reader, 0x3401)
        {
            unkID0 = reader.ReadInt16(); //points to inherited sharingObj
        }
    }

    public class Block_1C01 : S3DBlock
    {
        public float unkf0, unkf1; //material related

        public Block_1C01(EndianReader reader)
            : base(reader, 0x1C01)
        {
            unkf0 = reader.ReadSingle();
            unkf1 = reader.ReadSingle();
        }
    }

    public class Block_2001 : S3DBlock
    {
        public float[] unkf = new float[8]; //material related

        public Block_2001(EndianReader reader)
            : base(reader, 0x2001)
        {
            for (int i = 0; i < 8; i++)
                unkf[i] = reader.ReadSingle();
        }
    }

    public class Block_2801 : S3DBlock
    {
        public int x81;
        public int unk4;
        public int xFF;
        public int x1300;

        public int VertexCount;
        public int IndexCount;
        public int unkID2;
        public int unk7;
        public int unk8;
        public int unk9a;
        public int unk9b;

        public Block_2801(EndianReader reader)
            : base(reader, 0x2801)
        {
            x81 = reader.ReadByte();
            unk4 = reader.ReadInt32();
            xFF = reader.ReadByte();
            x1300 = reader.ReadInt16(); //mesh type enum? 16 = standard, 18 = skin, 19 = skincompound

            VertexCount = reader.ReadInt16(); //vertex count
            IndexCount = reader.ReadInt16(); //face count * 3 [usually]
            unkID2 = reader.ReadInt32(); //object ID, unknown purpose, same as parent ID, only used on vertless meshes (inheritors)
            unk7 = reader.ReadInt32(); //increases with vert count
            unk8 = reader.ReadInt32(); //seems to increase with mesh size
            unk9a = reader.ReadInt16(); //not used on standard meshes
            unk9b = reader.ReadInt16(); //not used on standard meshes
        }
    }
    #endregion

    public class Block_3301 : S3DBlock
    {
        public int FirstNodeID, NodeCount;

        public List<RealQuat> nodes = new List<RealQuat>();

        public Block_3301(EndianReader reader, bool loadSkin, Vertex[] Vertices)
            : base(reader, 0x3301)
        {
            FirstNodeID = reader.ReadInt16();
            NodeCount = reader.ReadInt16();

            if (!loadSkin) reader.Skip(Vertices.Length * 4);
            else foreach (var v in Vertices)
                {
                    var val = RealQuat.FromUByte4(reader.ReadUInt32());
                    nodes.Add(val);
                    v.Values.Add(new VertexValue(val, VertexValue.ValueType.UInt8_4, "blendindices", 0));
                }
        }
    }

    public class Block_1A01 : S3DBlock
    {
        public Block_1A01(EndianReader reader, bool loadSkin, Vertex[] Vertices)
            : base(reader, 0x1A01)
        {
            if (!loadSkin) reader.Skip(Vertices.Length * 4);
            else foreach (var v in Vertices)
                {
                    var val = RealQuat.FromUByteN4(reader.ReadUInt32());
                    v.Values.Add(new VertexValue(val, VertexValue.ValueType.UInt8_4, "blendweight", 0));
                }
        }
    }
    #endregion

    #region Template Blocks
    #region Bone Blocks
    public class BoneBlock_E902 : S3DBlock
    {
        public float unk00; //always 0
        public PosBlock_FA02 _FA02;
        public unkBlock_XXXX _EA02;
        public Block_FB02 _FB02;
        public unkBlock_XXXX _EB02;
        public Block_FC02 _FC02;
        public unkBlock_XXXX _EC02;
        public Block_0A03 _0A03;
        public unkBlock_XXXX _ED02;

        public BoneBlock_E902(EndianReader reader)
            : base(reader, 0xE902)
        {
            unk00 = reader.ReadSingle();

            _FA02 = new PosBlock_FA02(reader);

            if (reader.PeekUInt16() == 0xEA02)
                _EA02 = new unkBlock_XXXX(reader, 0xEA02);

            _FB02 = new Block_FB02(reader);

            if (reader.PeekUInt16() == 0xEB02)
                _EB02 = new unkBlock_XXXX(reader, 0xEB02);

            _FC02 = new Block_FC02(reader);

            if (reader.PeekUInt16() == 0xEC02)
                _EC02 = new unkBlock_XXXX(reader, 0xEC02);

            _0A03 = new Block_0A03(reader);

            if (reader.PeekUInt16() == 0xED02)
                _ED02 = new unkBlock_XXXX(reader, 0xED02);

            //technically not part of the block
            reader.ReadInt16(); //0100
            reader.ReadInt32(); //address
        }
    }

    public class PosBlock_FA02 : S3DBlock
    {
        public RealQuat Data; //relative position coords

        public PosBlock_FA02(EndianReader reader)
            : base(reader, 0xFA02)
        {
            Data = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }

    public class Block_FB02 : S3DBlock
    {
        public RealQuat Data; //assumed rotation

        public Block_FB02(EndianReader reader)
            : base(reader, 0xFB02)
        {
            Data = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }

    public class Block_FC02 : S3DBlock
    {
        public float unk08, unk09, unk10; //almost always 1/1/1

        public Block_FC02(EndianReader reader)
            : base(reader, 0xFC02)
        {
            unk08 = reader.ReadSingle();
            unk09 = reader.ReadSingle();
            unk10 = reader.ReadSingle();
        }
    }

    public class Block_0A03 : S3DBlock
    {
        public float unk11; //almost always 1

        public Block_0A03(EndianReader reader)
            : base(reader, 0x0A03)
        {
            unk11 = reader.ReadSingle();
        }
    }
    #endregion

    public class Block_0503 : S3DBlock
    {
        public int DataCount;
        public int unk0, unk1;
        public List<Matrix> Data = new List<Matrix>();

        public Block_0503(EndianReader reader)
            : base(reader, 0x0503)
        {
            reader.ReadInt16(); //0D03
            reader.ReadInt32(); //EOB

            DataCount = reader.ReadInt32();
            unk0 = reader.ReadInt16(); //always 3
            unk1 = reader.ReadByte(); //0, 2 or 3

            if (unk1 != 3)
            {
                for (int i = 0; i < DataCount; i++)
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

                    Data.Add(mat);
                }
            }

            //technically not part of the block
            reader.ReadInt16(); //0100
            reader.ReadInt32(); //address
        }
    }

    #region 0E03
    public class Block_0E03 : S3DBlock
    {
        public int DataCount;
        public List<Block_0F03> Sets = new List<Block_0F03>();

        public Block_0E03(EndianReader reader)
            : base(reader, 0x0E03)
        {
            DataCount = reader.ReadInt32();

            for (int i = 0; i < DataCount; i++)
                Sets.Add(new Block_0F03(reader));
        }
    }

    public class Block_0F03 : S3DBlock
    {
        public int unk0, unk1; //min/max?

        public Block_0F03(EndianReader reader)
            : base(reader, 0x0F03)
        {
            unk0 = reader.ReadInt32();
            unk1 = reader.ReadInt32();

            reader.SeekTo(EOBOffset);
            return;

            //reader.ReadSingle();
            //reader.ReadSingle();
            //reader.ReadSingle();
            //reader.ReadInt16();
            //reader.ReadSingle();
            //reader.ReadSingle();
            //reader.ReadSingle();
            //reader.ReadSingle(); //???
            //reader.ReadInt32();
        }
    }
    #endregion
    #endregion

    #region Scene Blocks
    public class Block_2002 : S3DBlock
    {
        public int unk0, unk1, unk2;
        public RealBoundingBox Bounds;
        public RealQuat unkPos0;

        public Block_2002(EndianReader reader)
            : base(reader, 0x2002)
        {
            unk0 = reader.ReadInt32(); // ]
            unk1 = reader.ReadInt32(); // ] unknown purpose, often all 60
            unk2 = reader.ReadInt32(); // ]

            var min = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            var max = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Bounds = new RealBoundingBox()
            {
                XBounds = new RealBounds(min.x, max.x),
                YBounds = new RealBounds(min.y, max.y),
                ZBounds = new RealBounds(min.z, max.z),
            };
            unkPos0 = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }
    #endregion
}
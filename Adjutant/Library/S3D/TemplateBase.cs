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
    public abstract class TemplateBase
    {
        #region Declarations
        public string Name;
        public List<Material> Materials;
        public List<Node> Objects;
        public render_model.BoundingBox RenderBounds;
        public bool isParsed = false;
        #endregion

        #region Methods
        public Node ObjectByID(int ID)
        {
            foreach (var obj in Objects)
                if (obj.ID == ID) return obj;

            return null;
        }

        public Matrix HierarchialTransformDown(Node obj)
        {
            if (obj.isInherited || obj.isInheritor) return Matrix.Identity; //these could end in infinite loop
            return (obj.ParentID == -1) ? obj.Transform : HierarchialTransformDown(ObjectByID(obj.ParentID)) * obj.Transform;
        }

        public Matrix HierarchialTransformUp(Node obj)
        {
            if (obj.isInherited || obj.isInheritor) return obj.Transform;
            return (obj.ParentID == -1) ? obj.Transform : obj.Transform * HierarchialTransformUp(ObjectByID(obj.ParentID));
        }

        public virtual void Parse()
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class Material
    {
        public int x5601;
        public int AddressOfNext;
        public string Name;

        public Material(PakFile Pak, PakFile.PakTag Item)
        {
            var reader = Pak.Reader;

            x5601 = reader.ReadInt16();
            AddressOfNext = reader.ReadInt32();
            Name = reader.ReadNullTerminatedString();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Node
    {
        #region Declarations
        public int xF000;
        public int PreNextAddress;
        public int xB903;
        public int unkAddress0; //address to F900/2E01 (249/302)
        public string Name;
        public int ID;
        public int x2400;
        public int unk0;
        public int unk1;
        public int unk2;
        public int VertCount;
        public int FaceCount;
        public int unk3; //F900 or 2E01 if verts or 2901 if inherits

        public int x1200;
        public int geomUnk01; //0x03 indicates no UV data
        public int x4001;
        public int xF100;
        public int VertCount2;
        public int CentreX;
        public int CentreY;
        public int CentreZ;
        public int RadiusX;
        public int RadiusY;
        public int RadiusZ;
        public byte[] preUV;
        public int unkUV0;
        public int UVsize;
        public int unk4;
        public int unkCC;
        public int unkC0; //possibly UV scale
        public int[] unkC1;

        public Vertex[] Vertices;
        public int[] Indices;
        public List<Submesh> Submeshes;
        public render_model.BoundingBox BoundingBox;

        public Matrix Transform;
        public int xFA00;
        public int unkAddress2;
        public int NodeIndex;

        public int unk5;
        public int unk5a;
        #endregion
        
        public int ParentID = -1;

        public int mainAddress;
        public int vertsAddress;
        public int uvsAddress;
        public int faceAddress;
        public int subAddress;

        public bool isInherited = false;
        public bool isInheritor = false;
        public int vertOffset = 0;
        public int faceOffset = 0;
        public int inheritID = -1;

        public Node(PakFile Pak, PakFile.PakTag Item, bool loadMesh)
        {
            var reader = Pak.Reader;

            mainAddress = (int)reader.Position - Item.Offset;
            reader.SeekTo(Item.Offset + mainAddress);

            xF000 = reader.ReadInt16();
            PreNextAddress = reader.ReadInt32();
            xB903 = reader.ReadInt16();
            unkAddress0 = reader.ReadInt32(); //address to next block
            Name = reader.ReadNullTerminatedString();
            ID = reader.ReadInt16();
            x2400 = reader.ReadInt16();

            unk0 = reader.ReadByte();
            unk1 = reader.ReadInt16(); //possibly flags
            unk2 = reader.ReadInt16(); //possibly flags

            VertCount = reader.ReadInt32();
            FaceCount = reader.ReadInt32();
            unk3 = reader.ReadInt16();

            if (unk3 == 297) //2901 [only on bsps]
            {
                isInheritor = true;
                reader.ReadInt32(); //address of 2E01
                inheritID = reader.ReadInt16();
                vertOffset = reader.ReadInt32();
                faceOffset = reader.ReadInt32();
                reader.ReadInt16(); //2E01
                reader.ReadInt32(); //address to 3501/2301
                reader.ReadInt16(); //1200
                reader.ReadByte();  //geomUnk01
                reader.ReadInt16(); //4001

                if (reader.ReadInt16() == 309) //3501
                {
                    reader.ReadInt32(); //address of 2301
                    reader.Skip(12);    //xxxx-xxxx-xxxx-4801-4801-4801
                    reader.ReadInt16(); //2301
                } //else 2301

                reader.ReadInt32(); //address to 3101/2A01

                if (reader.ReadInt16() == 305) //3101
                {
                    reader.ReadInt32(); //address to 2A01
                    reader.ReadInt16(); //2A01
                } //else 2A01

                reader.ReadInt32(); //address to 1D01
            }

            if (ID == 1739)
                ID = ID;

            try
            {
                #region Read Geometry
                if (unk3 == 302) //2E01
                {
                    reader.ReadInt32(); //address to next block
                    x1200 = reader.ReadInt16();
                    geomUnk01 = reader.ReadByte();
                    x4001 = reader.ReadInt16();

                    ReadVertexData(reader, loadMesh, Item);

                    if (geomUnk01 != 0 && geomUnk01 != 3)
                        ReadUVData(reader, loadMesh, Item);

                    ReadIndexData(reader, loadMesh, Item);
                }
                #endregion

                #region bounds crap
                if (unk3 == 302 || unk3 == 297) //2E01/2901
                {
                    var t = reader.ReadInt16(); //1D01
                    reader.ReadInt32(); //address to end of bounds (F800)

                    var c = reader.ReadInt32(); //bounds count?
                    var min = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    var max = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    BoundingBox = new render_model.BoundingBox();
                    BoundingBox.XBounds = new RealBounds(min.x, max.x);
                    BoundingBox.YBounds = new RealBounds(min.y, max.y);
                    BoundingBox.ZBounds = new RealBounds(min.z, max.z);

                    reader.ReadInt16(); //F800
                    reader.ReadInt32(); //address
                    reader.ReadInt32(); //FFFFFFFF
                    unk4 = reader.ReadInt16(); //2F01 or F900

                    if (unk4 != 249) //2F01
                    {
                        /*var addr = Item.Offset +*/ reader.ReadInt32(); //address to F900
                        unkCC = reader.ReadByte(); //count

                        reader.ReadByte(); //index
                        unkC0 = reader.ReadInt32() * 2; //UV related (this section is missing on meshes with no UVs) [x2 for scale fix]

                        //first block is above, skip the rest
                        for (int i = 1; i < unkCC; i++)
                        {
                            reader.ReadByte(); //index
                            reader.ReadInt32(); //count
                        }

                        reader.ReadInt16(); //F900
                    }
                    
                    BoundingBox.UBounds = new RealBounds(0, 1f * unkC0);
                    BoundingBox.VBounds = new RealBounds(0, 1f * unkC0);
                }
                #endregion

                #region transform
                reader.ReadInt32(); //address to transform end
                
                Transform.m11 = reader.ReadSingle();
                Transform.m12 = reader.ReadSingle();
                Transform.m13 = reader.ReadSingle();
                reader.ReadSingle(); //0.0f
                Transform.m21 = reader.ReadSingle();
                Transform.m22 = reader.ReadSingle();
                Transform.m23 = reader.ReadSingle();
                reader.ReadSingle(); //0.0f
                Transform.m31 = reader.ReadSingle();
                Transform.m32 = reader.ReadSingle();
                Transform.m33 = reader.ReadSingle();
                reader.ReadSingle(); //0.0f
                Transform.m41 = reader.ReadSingle();
                Transform.m42 = reader.ReadSingle();
                Transform.m43 = reader.ReadSingle();
                reader.ReadSingle(); //1.0f
                #endregion

                xFA00 = reader.ReadInt16();
                unkAddress2 = reader.ReadInt32();
                NodeIndex = reader.ReadInt32(); //node data index
                unk5 = reader.ReadInt16();

                if (unk5 == 1155) //8304, used on zone/path objects
                {
                    reader.ReadInt32(); //address
                    unk5a = reader.ReadInt32(); //index to something
                    reader.ReadInt16(); //1501
                }
                else if (unk5 == 253) //FD00, used on template root node
                {
                    reader.ReadInt32(); //address
                    reader.ReadInt16(); //BA01
                    reader.ReadInt32(); //address
                    reader.ReadNullTerminatedString();
                    reader.ReadInt16(); //1501
                }

                reader.ReadInt32(); //address to end of string
                reader.ReadNullTerminatedString();

                if (unk3 == 302 || unk3 == 297) //2E01/2901
                {
                    reader.ReadInt16(); //0701
                    reader.ReadInt32(); //address to 1601 after submeshes
                    reader.ReadInt16(); //F300
                    reader.ReadInt32(); //address to 0401
                    reader.ReadInt32(); //struct count (always 5 so far)

                    unkC1 = new int[5];

                    //aformentioned struct
                    for (int i = 0; i < 5; i++)
                    {
                        reader.ReadInt16(); //0301
                        reader.ReadInt32(); //address to 0100
                        unkC1[i] = reader.ReadInt32(); //count (always 0?)
                        reader.ReadInt16(); //0100
                        reader.ReadInt32(); //address to next
                    }

                    #region Read Submesh Data [0401]
                    reader.ReadInt16(); //0401
                    reader.ReadInt32(); //address to 0100 after submeshes (end of submesh data)

                    subAddress = (int)reader.Position - Item.Offset;
                    var count = reader.ReadInt32();
                    Submeshes = new List<Submesh>();
                    if (count < 100) //error catch
                    {
                        try
                        {
                            for (int i = 0; i < count; i++)
                                Submeshes.Add(new Submesh(Pak, Item));
                        }
                        catch { }
                    }
                    //else
                    //    count = count;
                    #endregion

                    //bone indices/weights (?) here
                }

                reader.SeekTo(Item.Offset + (PreNextAddress - 4));
                if (unk5 == 277) ParentID = reader.ReadInt32(); //1501
            }
            catch { }

            reader.SeekTo(Item.Offset + PreNextAddress + 6);
        }

        private void ReadVertexData(EndianReader reader, bool loadMesh, PakFile.PakTag Item)
        {
            xF100 = reader.ReadInt16();
            var addr = reader.ReadInt32(); //address to end of vert data

            VertCount2 = reader.ReadInt32();
            if (VertCount2 == 0) return;

            if (geomUnk01 != 134)
            {
                CentreX = reader.ReadInt16();
                CentreY = reader.ReadInt16();
                CentreZ = reader.ReadInt16();
                RadiusX = reader.ReadInt16();
                RadiusY = reader.ReadInt16();
                RadiusZ = reader.ReadInt16();
            }

            vertsAddress = (int)reader.Position - Item.Offset;

            if (!loadMesh) reader.SeekTo(Item.Offset + addr);
            else
            {
                Vertices = new Vertex[VertCount];
                for (int i = 0; i < VertCount; i++)
                {
                    var v = new Vertex() { FormatName = "S3D" };

                    if (geomUnk01 == 134)
                    {
                        var data = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        v.Values.Add(new VertexValue(data, VertexValue.ValueType.Float32_3, "position", 0));
                        Vertices[i] = v;
                    }
                    else
                    {
                        var data = new RealQuat(
                            ((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF,
                            ((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF,
                            ((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF,
                            ((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF);
                        v.Values.Add(new VertexValue(data, VertexValue.ValueType.Int16_N4, "position", 0));
                        Vertices[i] = v;
                    }
                }
            }
        }

        private void ReadUVData(EndianReader reader, bool loadMesh, PakFile.PakTag Item)
        {
            reader.ReadInt16(); //3001
            var addr = Item.Offset + reader.ReadInt32(); //address to end of UVs

            uvsAddress = (int)reader.Position - Item.Offset;
            UVsize = ((addr - Item.Offset) - (uvsAddress + 8)) / VertCount2;
            var v2 = reader.ReadInt32(); //vCount

            reader.ReadInt16(); //2E00
            preUV = reader.ReadBytes(6);
            
            if (!loadMesh) reader.SeekTo(addr - 1);
            else for (int i = 0; i < VertCount; i++)
                {
                    int a = 0, b = 0;

                    switch (UVsize)
                    {
                        case 8:
                            a = reader.ReadUInt16();
                            b = reader.ReadUInt16();
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
                        case 36:
                            reader.Skip(24);
                            break;
                        case 44:
                            reader.Skip(28);
                            break;
                    }

                    int u = reader.ReadInt16();
                    int v = reader.ReadInt16();
                    var tex0 = new RealQuat(((float)a + (float)0) / (float)0xFFFF, ((float)b + (float)0) / (float)0xFFFF);
                    var tex1 = new RealQuat(((float)u + (float)0x7FFF) / (float)0xFFFF, ((float)v + (float)0x7FFF) / (float)0xFFFF);

                    switch (UVsize)
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
                        case 36:
                            reader.Skip(8);
                            break;
                        case 44:
                            reader.Skip(12);
                            break;
                    }

                    //Vertices[i].Values.Add(new VertexValue(norm, 0, "normal", 0));
                    Vertices[i].Values.Add(new VertexValue(tex1, VertexValue.ValueType.Int16_N2, "texcoords", 0));
                }
            unkUV0 = reader.ReadByte();

            //if (UVsize > 8)
            reader.SeekTo(addr);
        }

        private void ReadIndexData(EndianReader reader, bool loadMesh, PakFile.PakTag Item)
        {
            reader.ReadInt16(); //F200
            var addr = reader.ReadInt32(); //address to end of indices

            faceAddress = (int)reader.Position - Item.Offset;
            var count = reader.ReadInt32(); //fCount
            if (count == 0) return;

            if (!loadMesh) reader.Skip(FaceCount * 6);
            else
            {
                Indices = new int[FaceCount * 3];
                for (int i = 0; i < FaceCount * 3; i++)
                    Indices[i] = reader.ReadUInt16();
            }
            reader.SeekTo(Item.Offset + addr);
        }

        public override string ToString()
        {
            return Name;
        }

        public class Submesh
        {
            public int FaceStart;
            public int FaceLength;
            public int VertStart;
            public int VertLength;
            public int MaterialCount;
            public int MaterialIndex;

            #region unknowns
            public int unk0;
            public int unkID0 = -1;
            public int unkCount0;
            public int unkID1 = -1;
            public int unkCount1;

            public int unkfAddress;
            public float unkf0, unkf1;
            public int x2001;
            public byte[] unkb0;

            public int x2801;

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
            #endregion

            public Submesh(PakFile Pak, PakFile.PakTag Item)
            {
                var reader = Pak.Reader;
                reader.ReadInt16(); //0x0501
                reader.ReadInt32(); //address of 0x0D01
                FaceStart = reader.ReadInt32();
                FaceLength = reader.ReadInt32();
                reader.ReadInt16(); //0x0D01
                reader.ReadInt32(); //address of unk0
                VertStart = reader.ReadInt32();
                VertLength = reader.ReadInt32();
                unk0 = reader.ReadInt16(); //0x0B01 or 0x3201 or 0x3401

                if (unk0 == 306) //0x3201
                {
                    reader.Skip(4);
                    unkID0 = reader.ReadInt16(); //points to first inheritor if skincompound, otherwise parent bone
                    unkCount0 = reader.ReadByte(); //number of inheritors/bones (starts at unkID0 and increments through object IDs)
                    unkID1 = reader.ReadInt16(); //secondary parent bone
                    unkCount1 = reader.ReadByte(); //secondary number of bones
                    reader.ReadInt16(); //0x0B01
                }

                if (unk0 == 308) //0x3401
                {
                    reader.Skip(4);
                    unkID0 = reader.ReadInt16(); //points to vertex storing object
                    reader.ReadInt16(); //0x0B01
                }

                var addr = reader.ReadInt32(); //address to 1C01
                MaterialCount = reader.ReadInt32();
                reader.ReadInt16(); //0x0E01
                reader.ReadInt32(); //address
                MaterialIndex = reader.ReadInt32();

                reader.SeekTo(Item.Offset + addr);
                reader.ReadInt16(); //0x1C01

                #region unknowns
                reader.ReadInt32(); //address to 2001
                unkfAddress = (int)reader.Position;
                unkf0 = reader.ReadSingle(); //UV related
                unkf1 = reader.ReadSingle(); //UV related
                x2001 = reader.ReadInt16();
                reader.ReadInt32(); //address to 2801
                unkb0 = reader.ReadBytes(32); //8 floats, UV related
                x2801 = reader.ReadInt16();
                addr = reader.ReadInt32(); //address to 0100
                if (x2801 != 1) //0100
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
                    reader.ReadInt16(); //0100
                }
                reader.ReadInt32(); //address to next
                #endregion

                //reader.SeekTo(Item.Offset + addr + 6);
            }

            public class MatInfo
            {
                public MatInfo(PakFile Pak, PakFile.PakTag Item)
                {
                    var reader = Pak.Reader;

                    reader.ReadInt16(); //0E01
                    reader.ReadInt32(); //address to 1401
                    reader.ReadInt32(); //mat ID
                    reader.ReadInt32(); //FFFFFFFF
                    reader.ReadInt16(); //00FF/FFFF
                    reader.ReadInt16(); //1401
                    reader.ReadInt32(); //address to 1F01
                    reader.ReadInt16(); //FFFF/0001
                    reader.ReadInt16(); //1F01
                    reader.ReadInt32(); //address to BA01
                    reader.ReadInt16(); //00FF
                    reader.ReadInt16(); //BA01
                    reader.ReadInt32(); //address to end of string (0100)
                    reader.ReadNullTerminatedString();
                    reader.ReadInt16(); //0100
                    reader.ReadInt16(); //address of next
                }
            }
        }
    }

}

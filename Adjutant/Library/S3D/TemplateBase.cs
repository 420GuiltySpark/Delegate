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
        public List<S3DMaterial> Materials;
        public List<S3DObject> Objects;
        public render_model.BoundingBox RenderBounds;
        public bool isParsed = false;
        #endregion

        #region Methods
        public S3DObject ObjectByID(int ID)
        {
            foreach (var obj in Objects)
                if (obj.ID == ID) return obj;

            return null;
        }

        public Matrix HierarchialTransformDown(S3DObject obj)
        {
            if (obj.isInherited || obj.isInheritor) return Matrix.Identity; //these could end in infinite loop
            return (obj.ParentID == -1) ? obj.Transform : HierarchialTransformDown(ObjectByID(obj.ParentID)) * obj.Transform;
        }

        public Matrix HierarchialTransformUp(S3DObject obj)
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

    public class S3DMaterial
    {
        public int x5601;
        public int AddressOfNext;
        public string Name;

        public S3DMaterial(PakFile Pak, PakFile.PakTag Item)
        {
            var reader = Pak.Reader;
            //reader.EndianType = EndianFormat.LittleEndian;
            //reader.SeekTo(Item.Offset);

            x5601 = reader.ReadInt16();
            AddressOfNext = reader.ReadInt32();
            Name = reader.ReadNullTerminatedString();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class S3DObject
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
        public int unkAddress1;

        public int x1200;
        public int geomUnk01; //0x03 indicates no UV data
        public int x4001;
        public int xF100;
        public int geomUnkAddress;
        public int VertCount2;
        public int CentreX;
        public int CentreY;
        public int CentreZ;
        public int RadiusX;
        public int RadiusY;
        public int RadiusZ;
        public byte[] preUV;
        public int UVsize;

        public Vertex[] Vertices;
        public int[] Indices;
        public List<Submesh> Submeshes;
        public render_model.BoundingBox BoundingBox;

        public Matrix Transform;
        public int xFA00;
        public int unkAddress2;
        public int NodeIndex;
        public int unk4;
        #endregion

        public int ParentID = -1;
        public int uvScale;

        public int mainAddress;
        public int vertsAddress;
        public int uvsAddress;
        public int faceAddress;
        public int subAddress;

        public bool isInherited = false;
        public bool isInheritor = false;
        public int vertOffset = 0;
        public int faceOffset = 0;
        public int inheritIndex = -1;

        public S3DObject(PakFile Pak, PakFile.PakTag Item)
        {
            var reader = Pak.Reader;

            mainAddress = (int)reader.Position - Item.Offset;
            reader.SeekTo(Item.Offset + mainAddress);

            xF000 = reader.ReadInt16();
            PreNextAddress = reader.ReadInt32();
            xB903 = reader.ReadInt16();
            unkAddress0 = reader.ReadInt32();
            Name = reader.ReadNullTerminatedString();
            ID = reader.ReadInt16();
            x2400 = reader.ReadInt16();

            unk0 = reader.ReadByte();
            unk1 = reader.ReadInt16(); //possibly flags
            unk2 = reader.ReadInt16(); //possibly flags

            VertCount = reader.ReadInt32();
            FaceCount = reader.ReadInt32();
            unk3 = reader.ReadInt16();
                
            if (unk3 == 297) //2901
            {
                isInheritor = true;
                reader.ReadInt32(); //address
                inheritIndex = reader.ReadInt16();
                vertOffset = reader.ReadInt32();
                faceOffset = reader.ReadInt32();
                unk3 = reader.ReadInt16();
                unkAddress1 = reader.ReadInt32();
                reader.Skip(47);
            }
            else unkAddress1 = reader.ReadInt32();

            try
            {
                if ((unk3 == 302 || unk3 == 297) && !isInheritor) //2E01/2901
                {
                    #region Read Geometry
                    x1200 = reader.ReadInt16();
                    geomUnk01 = reader.ReadByte();
                    x4001 = reader.ReadInt16();
                    xF100 = reader.ReadInt16();
                    geomUnkAddress = reader.ReadInt32();

                    VertCount2 = reader.ReadInt32();

                    if (VertCount > 0)
                    {
                        CentreX = reader.ReadInt16();
                        CentreY = reader.ReadInt16();
                        CentreZ = reader.ReadInt16();
                        RadiusX = reader.ReadInt16();
                        RadiusY = reader.ReadInt16();
                        RadiusZ = reader.ReadInt16();

                        Vertices = new Vertex[VertCount];
                        Indices = new int[FaceCount * 3];
                        vertsAddress = (int)reader.Position - Item.Offset;
                        for (int i = 0; i < VertCount; i++)
                        {
                            var v = new Vertex() { FormatName = "S3D" };
                            var data = new RealQuat(
                                ((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF, 
                                ((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF, 
                                ((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF, 
                                ((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF);
                            v.Values.Add(new VertexValue(data, VertexValue.ValueType.Int16_N4, "position", 0));
                            Vertices[i] = v;
                        }
                        reader.Skip(2); //3001
                        var addr = Item.Offset + reader.ReadInt32();
                        UVsize = 0;
                        if (geomUnk01 != 3)
                        {
                            #region Read UVs
                            uvsAddress = (int)reader.Position - Item.Offset;
                            UVsize = ((addr - Item.Offset) - (uvsAddress + 4)) / VertCount2;
                            var v2 = reader.ReadInt32(); //vCount

                            preUV = reader.ReadBytes(8);

                            for (int i = 0; i < VertCount; i++)
                            {
                                int a = reader.ReadInt16(); //bounds min?
                                int b = reader.ReadInt16(); //bounds max?
                                var texx = new RealQuat(((float)a + (float)0x7FFF) / (float)0xFFFF, ((float)b + (float)0x7FFF) / (float)0xFFFF);
                                float len = texx.b - texx.a;

                                reader.Skip(-4);
                                var n = reader.ReadUInt32();
                                var q0 = RealQuat.FromDHenN3(n);
                                var q1 = RealQuat.FromHenDN3(n);
                                var q2 = RealQuat.FromDecN4(n);

                                if (UVsize == 16)
                                {
                                    reader.ReadInt32();
                                    reader.ReadInt32();
                                }
                                int u = reader.ReadInt16();
                                int v = reader.ReadInt16();
                                var tex = new RealQuat(((float)u + (float)0) / (float)0xFFFF, ((float)v + (float)0x7FFF) / (float)0xFFFF);

                                //tex = new RealQuat(tex.x * texx.b, tex.y);

                                //Vertices[i].Values.Add(new VertexValue(norm, 0, "normal", 0));
                                Vertices[i].Values.Add(new VertexValue(tex, VertexValue.ValueType.Int16_N2, "texcoords", 0));
                            }
                            reader.Skip(1);
                            reader.ReadInt16();
                            reader.ReadInt32();
                            #endregion
                        }
                        if (UVsize > 8)
                            reader.SeekTo(addr + 6);
                        faceAddress = (int)reader.Position - Item.Offset;
                        var f2 = reader.ReadInt32(); //fCount
                        for (int i = 0; i < FaceCount * 3; i++)
                            Indices[i] = reader.ReadUInt16();
                    }
                    else reader.Skip(10);

                    reader.ReadInt16(); //1D01
                    reader.ReadInt32(); //address
                }

                if (unk3 == 302 || unk3 == 297) //2E01/2901
                {
                    var pos = reader.BaseStream.Position - Item.Offset;
                    var c = reader.ReadInt32(); //count?
                    var min = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    var max = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    BoundingBox = new render_model.BoundingBox();
                    BoundingBox.XBounds = new RealBounds(min.x, max.x);
                    BoundingBox.YBounds = new RealBounds(min.y, max.y);
                    BoundingBox.ZBounds = new RealBounds(min.z, max.z);

                    reader.ReadInt16(); //F800
                    reader.ReadInt32(); //address
                    reader.ReadInt32(); //FFFFFFFF
                    var t = reader.ReadInt16(); //2F01
                    uvScale = 1;
                    if (t != 249) //2F01
                    {
                        reader.SeekTo(Item.Offset + reader.ReadInt32() - 4); //address to F900
                        //reader.ReadInt16(); //0100
                        uvScale = reader.ReadInt32(); //not actually uv scale?
                        reader.ReadInt16(); //F900
                    }
                    reader.ReadInt32(); //address to matrix end
                    BoundingBox.UBounds = new RealBounds(-1f * uvScale, 1f * uvScale);
                    BoundingBox.VBounds = new RealBounds(-1f * uvScale, 1f * uvScale);
                    #endregion
                }

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

                xFA00 = reader.ReadInt16();
                unkAddress2 = reader.ReadInt32();
                NodeIndex = reader.ReadInt32(); //node data index
                unk4 = reader.ReadInt16();

                if (unk4 == 1155) //8304
                {
                    reader.ReadInt32(); //address
                    reader.ReadInt32(); //index/ID/count
                    reader.ReadInt16(); //1501
                }
                else if (unk4 == 253)
                {
                    reader.ReadInt32(); //address
                    reader.ReadInt16();
                    reader.ReadInt32(); //address
                    reader.ReadNullTerminatedString();
                    reader.ReadInt16(); //1501
                }

                reader.ReadInt32();
                reader.ReadNullTerminatedString();

                if (unk3 == 302 || unk3 == 297) //2E01/2901
                {
                    reader.Skip(102);
                    #region Read Submeshes
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
                }

                reader.SeekTo(Item.Offset + (PreNextAddress - 4));
                if (unk4 == 277) ParentID = reader.ReadInt32(); //1501, parent ID
            }
            catch { }

            reader.SeekTo(Item.Offset + PreNextAddress + 6);
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
            public int MaterialIndex;

            #region unknowns
            public int unk0;
            public int unkID0 = -1;
            public int unkCount0;
            public int unkID1 = -1;
            public int unkCount1;

            public float unkf0;
            public float unkf1;
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

                var addr = reader.ReadInt32();
                reader.ReadInt32(); //0x01000000 [material count, not always 1]
                reader.ReadInt16(); //0x0E01
                reader.ReadInt32(); //address
                MaterialIndex = reader.ReadInt32();

                reader.SeekTo(Item.Offset + addr);
                reader.ReadInt16(); //0x1C01
                    
                #region unknowns
                reader.ReadInt32(); //address to 2001
                unkf0 = reader.ReadSingle();
                unkf1 = reader.ReadSingle();
                x2001 = reader.ReadInt16();
                reader.ReadInt32(); //address to 2801
                unkb0 = reader.ReadBytes(32);
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
                }
                #endregion

                reader.SeekTo(Item.Offset + addr + 6);
            }
        }
    }
}

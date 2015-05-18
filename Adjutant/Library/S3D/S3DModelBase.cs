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
    public abstract class S3DModelBase
    {
        #region Declarations
        public string Name;
        public List<S3DMaterial> Materials;
        public List<S3DObject> Objects;
        public render_model.BoundingBox RenderBounds;
        public bool isParsed = false;
        #endregion

        #region SubClasses
        public class S3DMaterial
        {
            public int x5601;
            public int AddressOfNext;
            public string Name;

            public S3DMaterial(S3DPak Pak, S3DPak.PakItem Item)
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
            public int VertCount;
            public int FaceCount;
            public int unk2; //F900 or 2E01 if verts
            public int unkAddress1;

            public int x1200;
            public int geomUnk01;
            public int x4001;
            public int xF100;
            public int geomUnkAddress;
            public int geomUnk05;
            public int VertCount2;
            public int geomUnk04;
            public int geomUnk06;
            public int geomUnk07;
            public int geomUnk08;
            public int geomUnk09;
            public float geomfloat;

            public Vertex[] Vertices;
            public int[] Indices;
            public List<Submesh> Submeshes;
            public render_model.BoundingBox BoundingBox;

            public Matrix unkMatrix0;
            public int xFA00;
            public int unkAddress2;
            public int unkIndex0;
            public int unk3;


            //fuck it
            ////Type1 == FD00
            //public int unkAddress2A;
            //public int unk2A;
            //public int x2B01;
            //public int unkAddress3A;

            ////Type1 == 1501
            //public int unkAddress2B;
            //public int unk2B;
            //public int 
            #endregion
            public int ParentID = -1;
            public int uvScale;

            int mainAddress;
            int vertsAddress;
            int uvsAddress;
            int faceAddress;
            int subAddress;

            public bool isInherited = false;
            public bool isInheritor = false;
            public int vertOffset = 0;
            public int faceOffset = 0;
            public int inheritIndex;

            public S3DObject(S3DPak Pak, S3DPak.PakItem Item)
            {
                var reader = Pak.Reader;
                //reader.EndianType = EndianFormat.LittleEndian;
                //reader.SeekTo(Item.Offset);

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
                unk1 = reader.ReadInt32();

                VertCount = reader.ReadInt32();
                FaceCount = reader.ReadInt32();
                unk2 = reader.ReadInt16();
                
                if (unk2 == 297) //2901
                {
                    isInheritor = true;
                    reader.ReadInt32(); //address
                    inheritIndex = reader.ReadInt16();
                    vertOffset = reader.ReadInt32();
                    faceOffset = reader.ReadInt32();
                    unk2 = reader.ReadInt16();
                    unkAddress1 = reader.ReadInt32();
                    reader.Skip(47);
                }
                else unkAddress1 = reader.ReadInt32();

                if (VertCount > 0 && unk2 != 302)
                    VertCount = VertCount;

                try
                {
                    if (unk2 == 302 && !isInheritor) //2E01
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
                            geomUnk04 = reader.ReadInt16();
                            geomUnk06 = reader.ReadInt32();
                            geomUnk07 = reader.ReadInt16();
                            geomUnk08 = reader.ReadInt16();
                            geomUnk09 = reader.ReadInt16();

                            Vertices = new Vertex[VertCount];
                            Indices = new int[FaceCount * 3];
                            vertsAddress = (int)reader.Position - Item.Offset;
                            for (int i = 0; i < VertCount; i++)
                            {
                                var v = new Vertex() { FormatName = "S3D" };
                                //var data = new RealQuat(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                                var data = new RealQuat(((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF, ((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF, ((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF, ((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF);
                                v.Values.Add(new VertexValue(data, VertexValue.ValueType.Int16_N4, "position", 0));
                                Vertices[i] = v;
                            }
                            reader.Skip(2); //3001
                            var addr = Item.Offset + reader.ReadInt32();
                            int size = 0;
                            if (geomUnk01 != 3)
                            {
                                #region Read UVs
                                uvsAddress = (int)reader.Position - Item.Offset;
                                size = ((addr - Item.Offset) - (uvsAddress + 4)) / VertCount2;
                                var v2 = reader.ReadInt32(); //vCount

                                reader.ReadInt16(); //2E00
                                geomfloat = reader.ReadSingle();
                                reader.ReadInt16(); //0020

                                for (int i = 0; i < VertCount; i++)
                                {
                                    reader.ReadInt16();
                                    reader.ReadInt16();
                                    if (size == 16)
                                    {
                                        reader.ReadInt32();
                                        reader.ReadInt32();
                                    }
                                    short u = reader.ReadInt16();
                                    short v = reader.ReadInt16();
                                    //var norm = RealQuat.FromHenDN3(reader.ReadUInt32());
                                    var tex = new RealQuat(((float)u + (float)0x7FFF) / (float)0xFFFF, ((float)v + (float)0x7FFF) / (float)0xFFFF);
                                    //var tex = new RealQuat(((float)reader.ReadUInt16() + (float)0) / (float)0xFFFF, ((float)reader.ReadUInt16() + (float)0) / (float)0xFFFF);

                                    //Vertices[i].Values.Add(new VertexValue(norm, 0, "normal", 0));
                                    Vertices[i].Values.Add(new VertexValue(tex, VertexValue.ValueType.Int16_N2, "texcoords", 0));
                                }
                                reader.Skip(1);
                                reader.ReadInt16();
                                reader.ReadInt32();
                                #endregion
                            }
                            if (size > 8)
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

                    if (unk2 == 302) //2E01
                    {
                        var pos = reader.BaseStream.Position - Item.Offset;
                        var c = reader.ReadInt32(); //count?
                        //if (c != 1)
                        //    System.Windows.Forms.MessageBox.Show(Name + ": count is " + c.ToString());
                        var min = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        var max = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                        BoundingBox = new render_model.BoundingBox();
                        BoundingBox.XBounds = new RealBounds(min.x, max.x);
                        BoundingBox.YBounds = new RealBounds(min.y, max.y);
                        BoundingBox.ZBounds = new RealBounds(min.z, max.z);

                        //if (Name.Contains("helmet"))
                        //    Name = Name;

                        reader.ReadInt16(); //F800
                        reader.ReadInt32(); //address
                        reader.ReadInt32(); //FFFFFFFF
                        var t = reader.ReadInt16(); //2F01
                        uvScale = 1;
                        if (t != 249) //2F01
                        {
                            reader.SeekTo(Item.Offset + reader.ReadInt32() - 4); //address to F900
                            //reader.ReadInt16(); //0100
                            uvScale = reader.ReadInt32();
                            reader.ReadInt16(); //F900
                        }
                        reader.ReadInt32(); //address to matrix end

                        //reader.Skip(28);

                        BoundingBox.UBounds = new RealBounds(-1f * uvScale, 1f * uvScale);
                        BoundingBox.VBounds = new RealBounds(-1f * uvScale, 1f * uvScale);
                        //if (BoundingBox.Length > 0)
                        //{
                        //    for (int i = 0; i < VertCount; i++)
                        //        Controls.ModelFunctions.DecompressVertex(ref Vertices[i], BoundingBox);
                        //}
                        #endregion
                    }

                    unkMatrix0.m11 = reader.ReadSingle();
                    unkMatrix0.m12 = reader.ReadSingle();
                    unkMatrix0.m13 = reader.ReadSingle();
                    reader.ReadSingle();
                    unkMatrix0.m21 = reader.ReadSingle();
                    unkMatrix0.m22 = reader.ReadSingle();
                    unkMatrix0.m23 = reader.ReadSingle();
                    reader.ReadSingle();
                    unkMatrix0.m31 = reader.ReadSingle();
                    unkMatrix0.m32 = reader.ReadSingle();
                    unkMatrix0.m33 = reader.ReadSingle();
                    reader.ReadSingle();
                    unkMatrix0.m41 = reader.ReadSingle();
                    unkMatrix0.m42 = reader.ReadSingle();
                    unkMatrix0.m43 = reader.ReadSingle();
                    reader.ReadSingle();

                    xFA00 = reader.ReadInt16();
                    unkAddress2 = reader.ReadInt32();
                    unkIndex0 = reader.ReadInt32();
                    unk3 = reader.ReadInt16();

                    if (unk3 == 1155) //8304
                    {
                        reader.ReadInt32(); //address
                        reader.ReadInt32(); //index/ID/count
                        reader.ReadInt16(); //1501
                    }
                    else if (unk3 == 253)
                    {
                        reader.ReadInt32(); //address
                        reader.ReadInt16();
                        reader.ReadInt32(); //address
                        reader.ReadNullTerminatedString();
                        reader.ReadInt16(); //1501
                    }

                    reader.ReadInt32();
                    reader.ReadNullTerminatedString();

                    if (unk2 == 302) //2E01
                    //if (false)
                    {
                        #region Read Submeshes
                        //reader.SeekTo(Item.Offset + reader.ReadInt32());
                        reader.Skip(102);
                        subAddress = (int)reader.Position - Item.Offset;
                        var count = reader.ReadInt32();
                        Submeshes = new List<Submesh>();
                        if (count < 100)
                        {
                            try
                            {
                                for (int i = 0; i < count; i++)
                                    Submeshes.Add(new Submesh(Pak, Item));
                            }
                            catch { }
                        }
                        else
                            count = count;
                        #endregion
                    }

                    //if (Type1 != 253)
                    //{
                    //    reader.ReadInt16(); //2B01
                    //    reader.ReadInt32(); //address 
                    //    ParentID = reader.ReadInt32(); //1501, parent ID
                    //}
                    //reader.ReadInt16(); //0100
                    //reader.ReadInt32(); //address to next item

                    reader.SeekTo(Item.Offset + (PreNextAddress - 4));
                    if (unk3 == 277) ParentID = reader.ReadInt32(); //1501, parent ID

                    //if (isInheritor)
                    //{
                    //    //reader.Skip(12);
                    //    //inheritIndex = reader.ReadInt32();
                    //    //reader.Skip(6);

                    //    for (int i = 0; i < Submeshes.Count; i++)
                    //    {
                    //        Submeshes[i].VertStart += vertOffset;
                    //        Submeshes[i].FaceStart += faceOffset;
                    //    }
                    //}
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
                public int MeshInheritID = -1;
                public bool Inherits = false;

                public int unk0;
                public int unk1;
                public int unk2;

                public Submesh(S3DPak Pak, S3DPak.PakItem Item)
                {
                    var reader = Pak.Reader;
                    reader.ReadInt16(); //0x0501
                    reader.ReadInt32(); //address of 0x0D01
                    FaceStart = reader.ReadInt32();
                    FaceLength = reader.ReadInt32();
                    reader.ReadInt16(); //0x0D01
                    reader.ReadInt32(); //address of 0x0B01 or 0x3201
                    VertStart = reader.ReadInt32();
                    VertLength = reader.ReadInt32();
                    int unk = reader.ReadInt16(); //0x0B01 or 0x3201 or 0x3401

                    if (unk == 306) //0x3201
                    {
                        reader.Skip(4);
                        MeshInheritID = reader.ReadInt16();
                        unk0 = reader.ReadByte();
                        unk1 = reader.ReadInt16();
                        unk2 = reader.ReadByte();
                        reader.ReadInt16(); //0x0B01
                    }

                    if (unk == 308) //0x3401
                    {
                        reader.Skip(4);
                        MeshInheritID = reader.ReadInt16();
                        reader.Skip(2);
                        Inherits = true;
                    }

                    var addr = reader.ReadInt32();
                    reader.ReadInt32(); //0x01000000
                    reader.ReadInt16(); //0x0E01
                    reader.ReadInt32(); //address
                    MaterialIndex = reader.ReadInt32();

                    reader.SeekTo(Item.Offset + addr);
                    reader.ReadInt16(); //0x1C01
                    reader.Skip(52);
                    reader.SeekTo(Item.Offset + reader.ReadInt32() + 6);
                }
            }
        }
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
            return (obj.ParentID == -1) ? obj.unkMatrix0 : HierarchialTransformDown(ObjectByID(obj.ParentID)) * obj.unkMatrix0;
        }

        public Matrix HierarchialTransformUp(S3DObject obj)
        {
            if (obj.isInherited || obj.isInheritor) return obj.unkMatrix0;
            return (obj.ParentID == -1) ? obj.unkMatrix0 : obj.unkMatrix0 * HierarchialTransformUp(ObjectByID(obj.ParentID));
        }

        public void ParseTPL()
        {
            foreach (var obj in Objects)
            {
                if (obj.VertCount > 0)
                    ModelFunctions.DecompressVertex(ref obj.Vertices, obj.BoundingBox);
            }

            isParsed = true;
        }

        public void ParseBSP()
        {
            foreach (var obj in Objects)
            {
                if (!obj.isInheritor && !obj.isInherited && obj.Vertices != null && obj.BoundingBox != null)
                    ModelFunctions.DecompressVertex(ref obj.Vertices, obj.BoundingBox);
            }

            foreach (var obj in Objects)
            {
                if (obj.isInheritor && obj.Submeshes.Count > 0)
                {
                    var pObj = ObjectByID(obj.inheritIndex);
                    int maxVert = 0;
                    int maxIndx = 0;

                    foreach (var sub in obj.Submeshes)
                    {
                        maxVert = Math.Max(maxVert, sub.VertStart + obj.vertOffset + sub.VertLength);
                        maxIndx = Math.Max(maxIndx, sub.FaceStart + obj.faceOffset + sub.FaceLength);
                    }

                    int vLength = maxVert - obj.vertOffset;
                    int fLength = (maxIndx - obj.faceOffset) * 3;

                    obj.Vertices = new Vertex[vLength];
                    obj.Indices = new int[fLength];

                    Array.Copy(pObj.Vertices, obj.vertOffset, obj.Vertices, 0, vLength);
                    Array.Copy(pObj.Indices, obj.faceOffset * 3, obj.Indices, 0, fLength);

                    //if (obj.Vertices != null && obj.BoundingBox != null)
                    //    ModelFunctions.DecompressVertex(ref obj.Vertices, obj.BoundingBox);
                }
            }

            isParsed = true;
        }
        #endregion
    }
}

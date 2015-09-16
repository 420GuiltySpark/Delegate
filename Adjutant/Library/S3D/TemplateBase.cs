using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.DataTypes;
using Adjutant.Library;
using Adjutant.Library.S3D;
using Adjutant.Library.S3D.Blocks;
using Adjutant.Library.Endian;
using Adjutant.Library.Controls;

namespace Adjutant.Library.S3D
{
    public abstract class TemplateBase
    {
        #region Declarations
        public string Name;
        public List<MatRefBlock_5601> Materials;
        public List<Node> Objects;
        public List<BoneBlock_E902> Bones = new List<BoneBlock_E902>();
        public RealBoundingBox RenderBounds;
        #endregion

        #region Methods
        public Node ObjectByID(int ID)
        {
            foreach (var obj in Objects)
                if (obj._B903.ID == ID) return obj;

            return null;
        }

        public Matrix HierarchialTransformDown(Node obj)
        {
            if (obj.isInherited || obj.isInheritor) return Matrix.Identity; //these could end in infinite loop
            return (obj.ParentID == -1) ? obj.Transform.Data : HierarchialTransformDown(ObjectByID(obj.ParentID)) * obj.Transform.Data;
        }

        public Matrix HierarchialTransformUp(Node obj)
        {
            if (obj.isInherited || obj.isInheritor) return obj.Transform.Data;
            return (obj.ParentID == -1) ? obj.Transform.Data : obj.Transform.Data * HierarchialTransformUp(ObjectByID(obj.ParentID));
        }
        #endregion
    }

    public class Node
    {
        #region Declarations
        public int xF000;
        public Block_B903 _B903;
        public Block_2901 _2901;
        public Block_2E01 _2E01;
        public Block_3501 _3501;
        public VertexBlock_F100 Vertices;
        public UVDataBlock_3001 _3001;
        public IndexBlock_F200 Indices;
        public BoundsBlock_1D01 BoundingBox;
        public Block_2F01 _2F01;
        public MatrixBlock_F900 Transform;
        public ScriptRefBlock_8304 _8304;
        public Block_FD00 _FD00;

        public Block_3301 _3301;
        public Block_1A01 _1A01;

        public int[] unkC1;

        public List<Submesh> Submeshes;

        public int BoneIndex;
        #endregion

        public int ParentID = -1;

        public int mainAddress;
        public int subAddress;

        public bool isInherited = false;
        public bool isInheritor = false;

        public int unk0, unk1;

        public Node(EndianReader reader, bool loadMesh)
        {
            mainAddress = (int)reader.Position;

            xF000 = reader.ReadInt16();
            reader.ReadInt32(); //address

            _B903 = new Block_B903(reader);

            if (reader.PeekUInt16() != 0xF900)
                ReadGeomBlocks(reader, loadMesh);

            Transform = new MatrixBlock_F900(reader);

            #region Block FA00
            reader.ReadInt16(); //FA00
            reader.ReadInt32(); //EOB address
            BoneIndex = reader.ReadInt32(); //node data index
            #endregion

            if (reader.PeekUInt16() == 0x8304) //used on zone/path objects
                _8304 = new ScriptRefBlock_8304(reader);

            if (reader.PeekUInt16() == 0xFD00) //used on template root node
                _FD00 = new Block_FD00(reader);

            #region Block 1501
            reader.ReadInt16(); //1501
            reader.ReadInt32(); //EOB address
            reader.ReadNullTerminatedString();
            #endregion

            if (_2E01 != null)
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

                subAddress = (int)reader.Position;
                var count = reader.ReadInt32();
                Submeshes = new List<Submesh>();

                for (int i = 0; i < count; i++)
                    Submeshes.Add(new Submesh(reader));
                #endregion

                reader.ReadInt16(); //0100
                reader.ReadInt32(); //address

                if (Submeshes[0]._3201 != null)
                {
                    reader.ReadInt16(); //1601
                    reader.ReadInt32(); //EOB offset

                    reader.ReadInt16(); //1701
                    reader.ReadInt32(); //EOB offset
                    unk0 = reader.ReadInt32();
                    unk1 = reader.ReadInt32();

                    _3301 = new Block_3301(reader, loadMesh, Vertices.Data);

                    if (reader.PeekUInt16() == 0x1A01)
                        _1A01 = new Block_1A01(reader, loadMesh, Vertices.Data);

                    reader.ReadInt16(); //0100
                    reader.ReadInt32(); //address
                }
            }

            if (reader.PeekUInt16() == 0x2B01)
            {
                reader.ReadInt16(); //2B01
                reader.ReadInt32(); //EOB offset
                ParentID = reader.ReadInt32();
            }

            reader.ReadInt16(); //0100
            reader.ReadInt32(); //address
        }

        private void ReadGeomBlocks(EndianReader reader, bool loadmesh)
        {
            if (reader.PeekUInt16() == 0x2901)
                _2901 = new Block_2901(reader);

            _2E01 = new Block_2E01(reader);

            if(_2901 != null)
            {
                isInheritor = true;

                if (reader.PeekUInt16() == 0x3501)
                    _3501 = new Block_3501(reader);

                reader.ReadInt16(); //2301
                reader.ReadInt32(); //EOB offset

                if (reader.PeekUInt16() == 0x3101)
                {
                    reader.ReadInt16(); //3101
                    reader.ReadInt32(); //EOB offset
                }

                reader.ReadInt16(); //2A01
                reader.ReadInt32(); //EOB offset
            }
            else
            {
                Vertices = new VertexBlock_F100(reader, loadmesh, _2E01.geomUnk01);

                if (_2E01.geomUnk01 != 0 && _2E01.geomUnk01 != 3)
                {
                    _3001 = new UVDataBlock_3001(reader, loadmesh, Vertices.Data);
                    reader.SeekTo(_3001.EOBOffset); //failsafe
                }

                Indices = new IndexBlock_F200(reader, loadmesh);
            }

            BoundingBox = new BoundsBlock_1D01(reader);

            #region Block F800
            reader.ReadInt16(); //F800
            reader.ReadInt32(); //EOB address
            reader.ReadInt32(); //FFFFFFFF
            #endregion

            if (reader.PeekUInt16() == 0x2F01)
                _2F01 = new Block_2F01(reader);
        }

        public override string ToString()
        {
            return _B903.Name;
        }

        public class Submesh
        {
            public int FaceStart;
            public int FaceLength;
            public int VertStart;
            public int VertLength;
            public int MaterialCount;
            public int MaterialIndex;

            public Block_3201 _3201;
            public Block_3401 _3401;
            public Block_1C01 _1C01;
            public Block_2001 _2001;
            public Block_2801 _2801;

            public Submesh(EndianReader reader)
            {
                reader.ReadInt16(); //0x0501
                reader.ReadInt32(); //EOB offset
                FaceStart = reader.ReadInt32();
                FaceLength = reader.ReadInt32();
                reader.ReadInt16(); //0x0D01
                reader.ReadInt32(); //EOB offset
                VertStart = reader.ReadInt32();
                VertLength = reader.ReadInt32();

                if (reader.PeekUInt16() == 0x3201)
                    _3201 = new Block_3201(reader);

                if (reader.PeekUInt16() == 0x3401)
                    _3401 = new Block_3401(reader);

                #region Block 0B01
                reader.ReadInt16(); //0x0B01
                var addr = reader.ReadInt32(); //EOB offset
                MaterialCount = reader.ReadInt32();
                reader.ReadInt16(); //0x0E01
                reader.ReadInt32(); //EOB offset
                MaterialIndex = reader.ReadInt32();

                reader.SeekTo(addr); 
                #endregion

                _1C01 = new Block_1C01(reader);

                _2001 = new Block_2001(reader);

                if (reader.PeekUInt16() == 0x2801)
                    _2801 = new Block_2801(reader);

                reader.ReadInt16(); //0100
                reader.ReadInt32(); //address to next
            }

            public class MatInfo
            {
                public MatInfo(PakFile Pak)
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
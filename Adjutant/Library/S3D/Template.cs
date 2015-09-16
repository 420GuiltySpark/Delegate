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
    public class Template : TemplateBase
    {
        public int xF000;
        public int x2C01;

        public Block_0503 _0503;
        public unkBlock_XXXX _E602;
        public StringBlock_BA01 unkStrBlk;
        public unkBlock_XXXX _1D02;
        public unkBlock_XXXX _1103;
        public unkBlock_XXXX _0403;
        public Block_0E03 _0E03;
        public unkBlock_XXXX _1203;

        public Template(PakFile Pak, PakFile.PakTag Item, bool loadMesh)
        {
            var reader = Pak.Reader;
            reader.EndianType = EndianFormat.LittleEndian;
            reader.StreamOrigin = Item.Offset;
            reader.SeekTo(0);

            reader.ReadInt16(); //E402
            reader.ReadInt32(); //filesize (EOB offset?)

            #region Block E502
            reader.ReadInt16(); //E502
            reader.ReadInt32(); //EOB offset
            reader.ReadInt32(); //LPTA (probs part of the string)
            Name = reader.ReadNullTerminatedString();
            reader.ReadByte(); //00 
            #endregion

            #region Block 1603
            reader.ReadInt16(); //1603
            reader.ReadInt32(); //EOB offset
            reader.ReadBytes(3); //02 01 01 
            #endregion

            #region Block 5501
            reader.ReadInt16(); //5501
            reader.ReadInt32(); //address

            int count = reader.ReadInt32();
            Materials = new List<MatRefBlock_5601>();
            for (int i = 0; i < count; i++)
                Materials.Add(new MatRefBlock_5601(reader));
            #endregion

            reader.ReadInt16(); //0100
            reader.ReadInt32(); //address

            #region Block F000
            xF000 = reader.ReadInt16();
            reader.ReadInt32(); //EOB offset
            x2C01 = reader.ReadInt16();
            reader.ReadInt32(); //EOB offset

            count = reader.ReadInt32();
            Objects = new List<Node>();
            for (int i = 0; i < count; i++)
                Objects.Add(new Node(reader, loadMesh)); 

            foreach (var obj in Objects)
                if (obj.isInheritor)
                    Objects[obj._2901.InheritID].isInherited = true;
            #endregion

            reader.ReadInt16(); //0100
            reader.ReadInt32(); //address

            #region Block E802
            reader.ReadInt16(); //E802
            reader.ReadInt32(); //address

            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                Bones.Add(new BoneBlock_E902(reader)); 
            #endregion

            //havent mapped this block, assumed anim/sound related
            _E602 = new unkBlock_XXXX(reader, 0xE602);

            reader.ReadInt16(); //0100
            reader.ReadInt32(); //address

            if (reader.PeekUInt16() == 0xBA01)
                unkStrBlk = new StringBlock_BA01(reader);

            //contains data count, havent seen used
            _1D02 = new unkBlock_XXXX(reader, 0x1D02);

            //int16 count, [int16, int16] * count
            if (reader.PeekUInt16() == 0x1103)
                _1103 = new unkBlock_XXXX(reader, 0x1103);

            //contains null term string, used on IGA models
            _0403 = new unkBlock_XXXX(reader, 0x0403);

            if (reader.PeekUInt16() == 0x0503)
                _0503 = new Block_0503(reader);

            #region Block 0803
            reader.ReadInt16(); //0803
            reader.ReadInt32(); //address to end of bounds values
            reader.ReadInt32(); //bounds count?
            var min = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            var max = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            RenderBounds = new RealBoundingBox();
            RenderBounds.XBounds = new RealBounds(min.x, max.x);
            RenderBounds.YBounds = new RealBounds(min.y, max.y);
            RenderBounds.ZBounds = new RealBounds(min.z, max.z); 
            #endregion
            
            _0E03 = new Block_0E03(reader);

            //contains length prefixed string
            if (reader.PeekUInt16() == 0x1203)
                _1203 = new unkBlock_XXXX(reader, 0x1203);
                
            reader.ReadInt16(); //0100
            reader.ReadInt32(); //address to EOF

            reader.StreamOrigin = 0;
        }
    }
}
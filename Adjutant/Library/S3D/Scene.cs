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
    public class Scene : TemplateBase
    {
        public int xF000;
        public int x2C01;

        public unkBlock_XXXX _C003;
        public Block_2002 _2002;
        public Block_2102 _2102;
        public Block_2202 _2202;
        public unkBlock_XXXX _8404;

        public RealQuat unkCoords0;

        public List<StringBlock_BA01> Scripts;

        public Scene(PakFile Pak, PakFile.PakTag Item, bool loadMesh)
        {
            var reader = Pak.Reader;
            reader.EndianType = EndianFormat.LittleEndian;
            reader.StreamOrigin = Item.Offset;
            reader.SeekTo(0);

            Name = Item.Name;

            //contains 16bytes, maybe all uint16
            _C003 = new unkBlock_XXXX(reader, 0xC003);

            #region Block 5501
            reader.ReadInt16(); //5501
            reader.ReadInt32(); //address

            int count = reader.ReadInt32();
            Materials = new List<MatRefBlock_5601>();
            for (int i = 0; i < count; i++)
                Materials.Add(new MatRefBlock_5601(Pak.Reader)); 
            #endregion

            reader.ReadInt16(); //0100
            reader.ReadInt32(); //address

            #region Block 1F02
            reader.ReadInt16(); //1F02
            reader.ReadInt32(); //EOB offset

            _2002 = new Block_2002(reader);

            _2102 = new Block_2102(reader);

            _2202 = new Block_2202(reader, _2102.unk0); 
            #endregion

            reader.ReadInt16(); //0100
            reader.ReadInt32(); //address

            #region Block 8204
            reader.ReadInt16(); //8204
            reader.ReadInt32(); //address

            count = reader.ReadInt32();
            Scripts = new List<StringBlock_BA01>();
            for (int i = 0; i < count; i++)
                Scripts.Add(new StringBlock_BA01(Pak.Reader));
            #endregion

            _8404 = new unkBlock_XXXX(reader, 0x8404);

            #region Block F000
            xF000 = reader.ReadInt16();
            reader.ReadInt32();
            x2C01 = reader.ReadInt16();
            reader.ReadInt32(); //address to first object

            count = reader.ReadInt32();
            Objects = new List<Node>();
            for (int i = 0; i < count; i++)
                Objects.Add(new Node(Pak.Reader, loadMesh)); 
            #endregion

            foreach (var obj in Objects)
                if (obj.isInheritor)
                    Objects[obj._2901.InheritID].isInherited = true;

            reader.StreamOrigin = 0;
        }
    }
}

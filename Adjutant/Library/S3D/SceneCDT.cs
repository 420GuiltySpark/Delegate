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
    public class SceneCDT
    {
        public byte[] unmapped0;
        public List<struct0> unkS0 = new List<struct0>();

        public SceneCDT(PakFile Pak, PakFile.PakTag Item)
        {
            var reader = Pak.Reader;
            reader.EndianType = EndianFormat.LittleEndian;
            reader.SeekTo(Item.Offset);

            reader.ReadInt16(); //F000
            reader.ReadInt32(); //07000000
            reader.ReadByte(); //12

            unmapped0 = reader.ReadBytes(16); //???

            reader.ReadInt32(); //03000000

            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                unkS0.Add(new struct0(Pak, Item));

            //unmapped from here
        }

        public class struct0
        {
            public int unkIndex;
            public int unk0, unk1;
            public float unkf0;

            public struct0(PakFile Pak, PakFile.PakTag Item)
            {
                var reader = Pak.Reader;

                unkIndex = reader.ReadInt16(); //ID for bsp node

                unk0 = reader.ReadInt32(); //face count for bsp node
                unk1 = reader.ReadInt32(); //???
                
                //usually always large negative
                //may not actually be a float
                //(though always valid as one)
                unkf0 = reader.ReadSingle();
            }
        }
    }
}

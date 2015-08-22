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
    public class SceneData
    {
        public byte[] unmapped0;
        public List<int> indices;
        public List<struct0> unkS0;
        public byte[] unmapped1;

        public SceneData(PakFile Pak, PakFile.PakTag Item)
        {
            var reader = Pak.Reader;
            reader.EndianType = EndianFormat.LittleEndian;
            reader.SeekTo(Item.Offset);

            unmapped0 = reader.ReadBytes(44);

            var count = reader.ReadInt32();
            indices = new List<int>();
            for (int i = 0; i < count; i++)
                indices.Add(reader.ReadInt32());

            count = reader.ReadInt32();
            unkS0 = new List<struct0>();
            for (int i = 0; i < count; i++)
                unkS0.Add(new struct0(Pak, Item));

            unmapped1 = reader.ReadBytes(13);
        }

        public class struct0
        {
            public int[] unk0;

            public struct0(PakFile Pak, PakFile.PakTag Item)
            {
                unk0 = new int[12];

                for (int i = 0; i < 12; i++)
                    unk0[i] = Pak.Reader.ReadInt16();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.S3D;
using Adjutant.Library.Endian;
using Adjutant.Library.Controls;
using Adjutant.Library.DataTypes;

namespace Adjutant.Library.S3D
{
    public class SceneData
    {
        public byte[] unmapped0;
        public int x0700;
        public int xADDE;
        public RealBoundingBox unkBounds;
        public List<int> indices;
        public List<struct0> unkS0;
        public byte[] unmapped1;

        public SceneData(PakFile Pak, PakFile.PakTag Item)
        {
            var reader = Pak.Reader;
            reader.EndianType = EndianFormat.LittleEndian;
            reader.SeekTo(Item.Offset);

            unmapped0 = reader.ReadBytes(16);
            x0700 = reader.ReadInt16(); //0700
            xADDE = reader.ReadInt16(); //ADDE

            var min = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            var max = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            unkBounds = new RealBoundingBox()
            {
                XBounds = new RealBounds(min.x, max.x),
                YBounds = new RealBounds(min.y, max.y),
                ZBounds = new RealBounds(min.z, max.z),
            };

            var count = reader.ReadInt32(); //always bsp object count + 1
            indices = new List<int>();
            for (int i = 0; i < count; i++)
                indices.Add(reader.ReadInt32()); //last value is always struct0 count

            count = reader.ReadInt32();
            unkS0 = new List<struct0>();
            for (int i = 0; i < count; i++)
                unkS0.Add(new struct0(Pak, Item));

            unmapped1 = reader.ReadBytes(13); //always the same
        }

        public class struct0
        {
            public int[] unk0;

            public struct0(PakFile Pak, PakFile.PakTag Item)
            {
                unk0 = new int[12];

                //not sure if signed or not
                //appears to be compressed floats, might be
                //coordinates relative to the bounds values
                for (int i = 0; i < 12; i++)
                    unk0[i] = Pak.Reader.ReadInt16();
            }
        }
    }
}

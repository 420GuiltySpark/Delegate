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
    public class SceneCDT
    {
        public byte[] unmapped0;
        public List<DataSet> sets = new List<DataSet>();

        public SceneCDT(PakFile Pak, PakFile.PakTag Item)
        {
            var reader = Pak.Reader;
            reader.EndianType = EndianFormat.LittleEndian;
            reader.SeekTo(Item.Offset);

            reader.ReadInt16(); //F000
            reader.ReadInt32(); //07000000
            reader.ReadByte(); //12

            unmapped0 = reader.ReadBytes(16); //???

            var count = reader.ReadInt32(); //03000000 [data set count]
            for (int i = 0; i < count; i++)
                sets.Add(new DataSet(Pak, Item));
        }

        public class DataSet
        {
            public List<struct0> unkS0 = new List<struct0>();

            public int unk0;
            public RealQuat MinBound;
            public float unkf0;
            public int DataLength;


            public DataSet(PakFile Pak, PakFile.PakTag Item)
            {
                var reader = Pak.Reader;

                var count = reader.ReadInt32();
                if (count == 0) return;

                for (int i = 0; i < count; i++)
                    unkS0.Add(new struct0(Pak, Item));

                unk0 = reader.ReadInt32(); //total faces

                MinBound = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                unkf0 = reader.ReadSingle();
                DataLength = reader.ReadInt32();

                reader.ReadBytes(DataLength); //unmapped
            }

            public class struct0
            {
                public int NodeID;
                public int NodeFaces, unk1;
                public float unkf0;

                public struct0(PakFile Pak, PakFile.PakTag Item)
                {
                    var reader = Pak.Reader;

                    NodeID = reader.ReadInt16(); //ID for bsp node
                    NodeFaces = reader.ReadInt32(); //face count for bsp node

                    unk1 = reader.ReadInt32(); //???

                    //usually always large negative
                    //may not actually be a float
                    //(though always valid as one)
                    unkf0 = reader.ReadSingle();
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Endian;
using Adjutant.Library.Cache;
using zone = Adjutant.Library.Definitions.cache_file_resource_gestalt;

namespace Adjutant.Library.Definitions.Halo3Beta
{
    internal class cache_file_resource_gestalt : zone
    {
        internal cache_file_resource_gestalt(CacheFile Cache)
        {
            EndianReader Reader = Cache.Reader;

            Reader.BaseStream.Position += 36; //36

            RawEntries = new List<zone.RawEntry>();
            int rCount = Reader.ReadInt32();
            int rOffset = Reader.ReadInt32() - Cache.Magic;
            Reader.BaseStream.Position = rOffset;
            for (int i = 0; i < rCount; i++)
                RawEntries.Add(new RawEntry(Cache));
        }

        new internal class RawEntry : zone.RawEntry
        {
            internal RawEntry(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                Reader.BaseStream.Position += 12; //12
                TagID = Reader.ReadInt32();
                Reader.BaseStream.Position += 4; //20
                MapIndex = Reader.ReadInt32();
                Reader.BaseStream.Position += 16; //40
                Offset = Reader.ReadInt32();
                Size = Reader.ReadInt32();
                Reader.BaseStream.Position += 8; //56
                Offset2 = Reader.ReadInt32();
                Size2 = Reader.ReadInt32();
                Reader.BaseStream.Position += 32; //96
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Endian;
using Adjutant.Library.Cache;
using zone = Adjutant.Library.Definitions.cache_file_resource_gestalt;

namespace Adjutant.Library.Definitions.Halo3Retail
{
    internal class cache_file_resource_gestalt : zone
    {
        internal cache_file_resource_gestalt(CacheFile Cache)
        {
            EndianReader Reader = Cache.Reader;

            Reader.BaseStream.Position += 88; //88

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
                RawID = Reader.ReadInt32();
                Offset = Reader.ReadInt32();
                Size = Reader.ReadInt32();
                Reader.BaseStream.Position += 6; //34
                SegmentIndex = Reader.ReadInt16();
                Reader.BaseStream.Position += 28; //64
            }
        }
    }
}

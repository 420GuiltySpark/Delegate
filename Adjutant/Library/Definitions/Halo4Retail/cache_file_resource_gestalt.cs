using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Endian;
using Adjutant.Library.Cache;
using zone = Adjutant.Library.Definitions.cache_file_resource_gestalt;

namespace Adjutant.Library.Definitions.Halo4Retail
{
    public class cache_file_resource_gestalt : Halo4Beta.cache_file_resource_gestalt
    {
        protected cache_file_resource_gestalt() { }

        public cache_file_resource_gestalt(CacheBase Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            #region Raw Entries
            Reader.SeekTo(Address + 88);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            RawEntries = new List<zone.RawEntry>();
            for (int i = 0; i < iCount; i++)
                RawEntries.Add(new RawEntry(Cache, iOffset + 68 * i));
            #endregion

            #region Fixup Data
            Reader.SeekTo(Address + 340);
            iCount = Reader.ReadInt32();
            Reader.ReadInt32();
            Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Reader.SeekTo(iOffset);
            FixupData = Reader.ReadBytes(iCount);
            #endregion
        }
    }
}

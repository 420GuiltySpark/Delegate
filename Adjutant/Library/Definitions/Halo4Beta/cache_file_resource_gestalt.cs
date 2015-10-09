using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Endian;
using Adjutant.Library.Cache;
using zone = Adjutant.Library.Definitions.cache_file_resource_gestalt;

namespace Adjutant.Library.Definitions.Halo4Beta
{
    public class cache_file_resource_gestalt : Halo3Retail.cache_file_resource_gestalt
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
            Reader.SeekTo(Address + 316);
            iCount = Reader.ReadInt32();
            Reader.ReadInt32();
            Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Reader.SeekTo(iOffset);
            FixupData = Reader.ReadBytes(iCount);
            #endregion
        }

        new public class RawEntry : zone.RawEntry
        {
            public RawEntry(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.SeekTo(Address + 12);
                TagID = Reader.ReadInt32();
                Reader.ReadInt16();
                Reader.ReadByte();
                Reader.ReadByte();
                FixupSize = Reader.ReadInt32();
                Reader.ReadInt16();
                SegmentIndex = Reader.ReadInt16();
                Reader.ReadInt32();

                #region Resources
                Reader.SeekTo(Address + 32);
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                Fixups = new List<zone.RawEntry.ResourceFixup>();
                for (int i = 0; i < iCount; i++)
                    Fixups.Add(new ResourceFixup(Cache, iOffset + 8 * i));
                #endregion

                #region Resource Definitions
                Reader.SeekTo(Address + 44);
                iCount = Reader.ReadInt32();
                iOffset = Reader.ReadInt32() - Cache.Magic;
                DefinitionFixups = new List<zone.RawEntry.ResourceDefinitionFixup>();
                for (int i = 0; i < iCount; i++)
                    DefinitionFixups.Add(new ResourceDefinitionFixup(Cache, iOffset + 8 * i));
                #endregion

                #region Fixup Location
                Reader.SeekTo(Address + 56);
                iCount = Reader.ReadInt32();
                iOffset = Reader.ReadInt32() - Cache.Magic;
                if (iCount > 0)
                {
                    Reader.SeekTo(iOffset);
                    FixupOffset = Reader.ReadInt32();
                }
                #endregion
            }

            new public class ResourceFixup : zone.RawEntry.ResourceFixup
            {
                public ResourceFixup(CacheBase Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    Reader.ReadInt32();
                    int val = Reader.ReadInt32();
                    Unknown = val >> 28;
                    Offset = val & 0x0FFFFFFF;
                }
            }

            new public class ResourceDefinitionFixup : zone.RawEntry.ResourceDefinitionFixup
            {
                public ResourceDefinitionFixup(CacheBase Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    Offset = Reader.ReadInt32();
                    Type = Reader.ReadInt32();
                }
            }
        }
    }
}

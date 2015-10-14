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
    public class cache_file_resource_gestalt : zone
    {
        protected cache_file_resource_gestalt() { }

        public cache_file_resource_gestalt(CacheBase Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            #region Raw Entries
            Reader.SeekTo(Address + 36);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                RawEntries.Add(new RawEntry(Cache, iOffset + 96 * i));
            #endregion

            #region Fixup Data
            Reader.SeekTo(Address + 132);
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
                Reader.ReadInt16();
                Reader.ReadInt32();

                FixupOffset = Reader.ReadInt32();
                FixupSize = Reader.ReadInt32();

                Reader.ReadInt32();

                #region H3B only
                CacheIndex = Reader.ReadInt32();
                RequiredOffset = Reader.ReadInt32();
                RequiredSize = Reader.ReadInt32();

                Reader.ReadInt32();

                CacheIndex2 = Reader.ReadInt32();
                OptionalOffset = Reader.ReadInt32();
                OptionalSize = Reader.ReadInt32();

                Reader.ReadInt32();
                #endregion

                Reader.ReadInt32();

                #region Resource Fixups
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                for (int i = 0; i < iCount; i++)
                    Fixups.Add(new ResourceFixup(Cache, iOffset + 8 * i));
                #endregion

                #region Resource Definition Fixups
                Reader.SeekTo(Address + 84);
                iCount = Reader.ReadInt32();
                iOffset = Reader.ReadInt32() - Cache.Magic;
                for (int i = 0; i < iCount; i++)
                    DefinitionFixups.Add(new ResourceDefinitionFixup(Cache, iOffset + 8 * i));
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

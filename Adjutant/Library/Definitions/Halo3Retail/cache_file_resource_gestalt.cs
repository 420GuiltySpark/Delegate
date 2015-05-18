using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Endian;
using Adjutant.Library.Cache;
using Adjutant.Library.DataTypes;
using zone = Adjutant.Library.Definitions.cache_file_resource_gestalt;

namespace Adjutant.Library.Definitions.Halo3Retail
{
    internal class cache_file_resource_gestalt : zone
    {
        internal cache_file_resource_gestalt(CacheFile Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Reader.SeekTo(Address + 88);

            #region Raw Entries
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            RawEntries = new List<zone.RawEntry>();
            for (int i = 0; i < iCount; i++)
                RawEntries.Add(new RawEntry(Cache, iOffset + 64 * i));
            Reader.SeekTo(Address + 100);
            #endregion

            Reader.SeekTo(Address + 316);

            #region Fixup Data
            iCount = Reader.ReadInt32();
            int a = Reader.ReadInt32();
            int b = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Reader.SeekTo(iOffset);
            FixupData = Reader.ReadBytes(iCount);
            Reader.SeekTo(Address + 340);
            #endregion
        }

        new internal class RawEntry : zone.RawEntry
        {
            internal RawEntry(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.SeekTo(Address + 12);
                
                TagID = Reader.ReadInt32();
                RawID = Reader.ReadInt32();
                FixupOffset = Reader.ReadInt32();
                FixupSize = Reader.ReadInt32();
                Reader.ReadInt32();
                LocationType = Reader.ReadInt16();
                SegmentIndex = Reader.ReadInt16();
                Reader.ReadInt32();

                #region Resource Fixups
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                Fixups = new List<zone.RawEntry.ResourceFixup>();
                for (int i = 0; i < iCount; i++)
                    Fixups.Add(new ResourceFixup(Cache, iOffset + 8 * i));
                Reader.SeekTo(Address + 52);
                #endregion

                #region Resource Definition Fixups
                iCount = Reader.ReadInt32();
                iOffset = Reader.ReadInt32() - Cache.Magic;
                DefinitionFixups = new List<zone.RawEntry.ResourceDefinitionFixup>();
                for (int i = 0; i < iCount; i++)
                    DefinitionFixups.Add(new ResourceDefinitionFixup(Cache, iOffset + 8 * i));
                Reader.SeekTo(Address + 64);
                #endregion
            }

            new internal class ResourceFixup : zone.RawEntry.ResourceFixup
            {
                internal ResourceFixup(CacheFile Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    Reader.ReadInt32();
                    //value is masked, 4bit unknown, 28bit offset
                    int val = Reader.ReadInt32();
                    Unknown = val >> 28; //why does F0000000 come out as int64?
                    Offset = val & 0x0FFFFFFF;
                }
            }

            new internal class ResourceDefinitionFixup : zone.RawEntry.ResourceDefinitionFixup
            {
                internal ResourceDefinitionFixup(CacheFile Cache, int Address)
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

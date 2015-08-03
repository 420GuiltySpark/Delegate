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
    public class cache_file_resource_gestalt : zone
    {
        public cache_file_resource_gestalt(CacheFile Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Reader.SeekTo(Address + 88);

            #region Raw Entries
            long temp = Reader.BaseStream.Position;
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            Reader.BaseStream.Position = iOffset;
            RawEntries = new List<zone.RawEntry>();
            for (int i = 0; i < iCount; i++)
                RawEntries.Add(new RawEntry(Cache));
            Reader.SeekTo(Address + 100);
            #endregion

            Reader.SeekTo(Address + 316);

            if (Cache.Version == DefinitionSet.Halo4Retail)
                Reader.SeekTo(Address + 340);


            #region Fixup Data
            temp = Reader.BaseStream.Position;
            iCount = Reader.ReadInt32();
            int a = Reader.ReadInt32();
            int b = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Reader.BaseStream.Position = iOffset;
            FixupData = Reader.ReadBytes(iCount);
            Reader.BaseStream.Position = temp + 24;
            #endregion
        }

        new public class RawEntry : zone.RawEntry
        {
            public RawEntry(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                Reader.BaseStream.Position += 12; //12
                TagID = Reader.ReadInt32();
                Reader.ReadInt16();
                Reader.ReadByte();
                Reader.ReadByte();
                FixupSize = Reader.ReadInt32();
                Reader.ReadInt16();
                SegmentIndex = Reader.ReadInt16();
                Reader.ReadInt32();

                #region Resources
                long temp = Reader.BaseStream.Position;
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = iOffset;
                Fixups = new List<zone.RawEntry.ResourceFixup>();
                for (int i = 0; i < iCount; i++)
                    Fixups.Add(new ResourceFixup(Cache));
                Reader.BaseStream.Position = temp + 12;
                #endregion

                #region Resource Definitions
                temp = Reader.BaseStream.Position;
                iCount = Reader.ReadInt32();
                iOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = iOffset;
                DefinitionFixups = new List<zone.RawEntry.ResourceDefinitionFixup>();
                for (int i = 0; i < iCount; i++)
                    DefinitionFixups.Add(new ResourceDefinitionFixup(Cache));
                Reader.BaseStream.Position = temp + 12;
                #endregion

                #region Fixup Location
                temp = Reader.BaseStream.Position;
                iCount = Reader.ReadInt32();
                iOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = iOffset;
                if (iCount > 0)
                    FixupOffset = Reader.ReadInt32();
                //for (int i = 1; i < iCount; i++)
                //    if (Reader.ReadInt32() > 0) throw new Exception("check this");
                Reader.BaseStream.Position = temp + 12;
                #endregion
            }

            new public class ResourceFixup : zone.RawEntry.ResourceFixup
            {
                public ResourceFixup(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    Reader.ReadInt32();
                    int val = Reader.ReadInt32();
                    Unknown = val >> 28;
                    Offset = val & 0x0FFFFFFF;
                }
            }

            new public class ResourceDefinitionFixup : zone.RawEntry.ResourceDefinitionFixup
            {
                public ResourceDefinitionFixup(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    Offset = Reader.ReadInt32();
                    Type = Reader.ReadInt32();
                }
            }
        }
    }
}

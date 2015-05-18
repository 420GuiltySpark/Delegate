using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using sbnk = Adjutant.Library.Definitions.soundbank;

namespace Adjutant.Library.Definitions.Halo4Retail
{
    internal class soundbank : sbnk
    {
        internal soundbank(CacheFile Cache)
        {
            EndianReader Reader = Cache.Reader;

            unk0 = Reader.ReadInt32();
            unk1 = Reader.ReadInt32();
            unk2 = Reader.ReadInt32();

            #region Bank References
            long temp = Reader.BaseStream.Position;
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            BankRefs = new List<sbnk.BankReference>();
            for (int i = 0; i < iCount; i++)
                BankRefs.Add(new BankReference(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            unk3 = Reader.ReadInt32();
            Address = Reader.ReadUInt32();
        }

        new internal class BankReference : sbnk.BankReference
        {
            internal BankReference(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;
                Address = Reader.ReadUInt32();
            }
        }
    }
}

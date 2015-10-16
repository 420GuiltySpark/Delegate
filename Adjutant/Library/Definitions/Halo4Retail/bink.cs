using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using bik = Adjutant.Library.Definitions.bink;

namespace Adjutant.Library.Definitions.Halo4Retail
{
    public class bink : bik
    {
        public bink(CacheBase Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Reader.Skip(4);
            RawID = Reader.ReadInt32();
        }
    }
}

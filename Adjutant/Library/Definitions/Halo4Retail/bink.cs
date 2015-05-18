using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using bik = Adjutant.Library.Definitions.bink;

namespace Adjutant.Library.Definitions.Halo4Retail
{
    internal class bink : bik
    {
        internal bink(CacheFile Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Reader.Skip(4);
            RawID = Reader.ReadInt32();
        }
    }
}

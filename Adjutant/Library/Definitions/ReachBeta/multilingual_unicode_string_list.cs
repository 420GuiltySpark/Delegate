using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using unic = Adjutant.Library.Definitions.multilingual_unicode_string_list;

namespace Adjutant.Library.Definitions.ReachBeta
{
    public class multilingual_unicode_string_list : unic
    {
        public multilingual_unicode_string_list(CacheBase Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Reader.SeekTo(Address + 44);
            for (int i = 0; i < 12; i++)
            {
                Indices.Add(Reader.ReadUInt16());
                Lengths.Add(Reader.ReadUInt16());
            }
        }
    }
}

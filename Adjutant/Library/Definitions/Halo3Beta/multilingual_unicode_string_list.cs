﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Definitions;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using unic = Adjutant.Library.Definitions.multilingual_unicode_string_list;

namespace Adjutant.Library.Definitions.Halo3Beta
{
    internal class multilingual_unicode_string_list : unic
    {
        internal multilingual_unicode_string_list(CacheFile Cache)
        {
            EndianReader Reader = Cache.Reader;

            Reader.BaseStream.Position += 32; //32

            Indices = new List<int>();
            Lengths = new List<int>();
            for (int i = 0; i < 12; i++)
            {
                Indices.Add(Reader.ReadUInt16());
                Lengths.Add(Reader.ReadUInt16());
            }
        }
    }
}
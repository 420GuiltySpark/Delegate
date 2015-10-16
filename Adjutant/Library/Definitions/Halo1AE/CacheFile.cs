using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using CacheH1P = Adjutant.Library.Definitions.Halo1PC.CacheFile;

namespace Adjutant.Library.Definitions.Halo1AE
{
    public class CacheFile : CacheBase
    {
        public CacheFile(string Filename, string Build)
            : base(Filename, Build)
        {
            Version = DefinitionSet.Halo1AE;

            Header = new CacheH1P.CacheHeader(this);
            IndexHeader = new CacheH1P.CacheIndexHeader(this);
            IndexItems = new CacheH1P.IndexTable(this);
            Strings = new StringTable(this);

            LocaleTables = new List<LocaleTable>();
        }
    }
}

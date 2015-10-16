using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using CacheBase = Adjutant.Library.Definitions.CacheBase;

namespace Adjutant.Library.Definitions.Halo1CE
{
    public class CacheFile : Halo1PC.CacheFile
    {
        public CacheFile(string Filename, string Build)
            : base(Filename, Build)
        {
            Version = DefinitionSet.Halo1CE;
        }
    }
}

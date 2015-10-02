using System.Collections.Generic;
using Adjutant.Library.Definitions;
using Composer;

namespace Adjutant.Library.Definitions.Halo4Beta
{
    public class CacheFile : Halo3Retail.CacheFile
    {
        public CacheFile(string Filename, string Build)
            : base(Filename, Build)
        {
            Version = DefinitionSet.Halo4Beta;
        }
    }
}

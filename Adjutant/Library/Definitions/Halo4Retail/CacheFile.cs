using System.Collections.Generic;
using Adjutant.Library.Definitions;
using Composer;

namespace Adjutant.Library.Definitions.Halo4Retail
{
    public class CacheFile : Halo3Retail.CacheFile
    {
        public List<SoundPackInfo> SoundPacks;
        public Dictionary<uint, List<SoundFileInfo>> SoundFiles;

        public CacheFile(string Filename, string Build)
            : base(Filename, Build)
        {
            Version = DefinitionSet.Halo4Retail;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Cache;

namespace Adjutant.Library.Controls
{
    public delegate void TagExtractedEventHandler(object sender, CacheFile.IndexItem Tag);
    public delegate void ErrorExtractingEventHandler(object sender, CacheFile.IndexItem Tag, Exception Error);
    public delegate void FinishedRecursiveExtractEventHandler(object sender, CacheFile.IndexItem Tag);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Cache;

namespace Adjutant.Library.Controls
{
    public delegate void TagExtractedEventHandler(object sender, object Tag);
    public delegate void ErrorExtractingEventHandler(object sender, object Tag, Exception Error);
    public delegate void FinishedRecursiveExtractEventHandler(object sender, CacheBase.IndexItem Tag);
}

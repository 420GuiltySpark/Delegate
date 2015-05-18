using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adjutant.Library.Definitions
{
    public abstract class cache_file_resource_gestalt
    {
        public List<RawEntry> RawEntries;

        public abstract class RawEntry
        {
            public int TagID;
            public int RawID;
            public int Offset;
            public int Size;
            public int SegmentIndex;

            //H3B
            public int MapIndex;
            public int Offset2;
            public int Size2;
        }
    }
}

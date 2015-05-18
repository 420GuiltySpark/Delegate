using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.DataTypes;

namespace Adjutant.Library.Definitions
{
    public abstract class cache_file_resource_gestalt
    {
        public List<RawEntry> RawEntries;

        public abstract class RawEntry
        {
            public int TagID;
            public int RawID;
            public int FixupOffset;
            public int FixupSize;
            public int LocationType;
            public int SegmentIndex;

            //H3B
            public int CacheIndex;
            public int RequiredOffset;
            public int RequiredSize;
            public int CacheIndex2;
            public int OptionalOffset;
            public int OptionalSize;

            public List<ResourceFixup> Fixups;
            public List<ResourceDefinitionFixup> DefinitionFixups;

            public abstract class ResourceFixup
            {
                public int Unknown;
                public int Offset;
            }

            public abstract class ResourceDefinitionFixup
            {
                public int Offset;
                public int Type;
            }
        }

        public byte[] FixupData;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Endian;
using Adjutant.Library.Cache;

namespace Adjutant.Library.Definitions
{
    public static class DefinitionsManager
    {
        private static string errorMessage = "Supplied definition set does not support \"----\" tags.";

        //---, H3R, ODST, HRB, HRR, H4R
        public static cache_file_resource_layout_table play(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            Cache.Reader.BaseStream.Position = Tag.Offset;
            switch (Cache.Version)
            {
                case DefinitionSet.Halo3Retail:
                case DefinitionSet.Halo3ODST:
                case DefinitionSet.HaloReachBeta:
                case DefinitionSet.HaloReachRetail:
                    return new Halo3Retail.cache_file_resource_layout_table(Cache);

                case DefinitionSet.Halo4Retail:
                    return new Halo4Retail.cache_file_resource_layout_table(Cache);

                default:
                    return null; //this tag is required for map loading, so return null if not supported
            }
        }

        //H3B, H3R, ODST, HRB, HRR, H4R
        public static cache_file_resource_gestalt zone(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            Cache.Reader.BaseStream.Position = Tag.Offset;
            switch (Cache.Version)
            {
                case DefinitionSet.Halo3Beta:
                    return new Halo3Beta.cache_file_resource_gestalt(Cache);

                case DefinitionSet.Halo3Retail:
                case DefinitionSet.Halo3ODST:
                case DefinitionSet.HaloReachBeta:
                case DefinitionSet.HaloReachRetail:
                    return new Halo3Retail.cache_file_resource_gestalt(Cache);

                case DefinitionSet.Halo4Retail:
                    return new Halo4Retail.cache_file_resource_gestalt(Cache);

                default:
                    return null; //this tag is required for map loading, so return null if not supported
            }
        }

        //---, H3R, ODST, HRB, HRR, ---
        public static sound_cache_file_gestalt ugh_(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            Cache.Reader.BaseStream.Position = Tag.Offset;
            switch (Cache.Version)
            {
                case DefinitionSet.Halo3Retail:
                    return new Halo3Retail.sound_cache_file_gestalt(Cache);

                case DefinitionSet.Halo3ODST:
                    return new Halo3ODST.sound_cache_file_gestalt(Cache);

                case DefinitionSet.HaloReachBeta:
                    return new ReachBeta.sound_cache_file_gestalt(Cache);

                case DefinitionSet.HaloReachRetail:
                    return new ReachRetail.sound_cache_file_gestalt(Cache);

                default:
                    return null; //this tag is required for map loading, so return null if not supported
            }
        }

        //H3B, H3R, ODST, HRB, HRR, H4R
        public static bitmap bitm(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            Cache.Reader.BaseStream.Position = Tag.Offset;
            switch (Cache.Version)
            {
                case DefinitionSet.Halo3Beta:
                case DefinitionSet.Halo3Retail:
                case DefinitionSet.Halo3ODST:
                    return new Halo3Beta.bitmap(Cache);

                case DefinitionSet.HaloReachBeta:
                    return new ReachBeta.bitmap(Cache);

                case DefinitionSet.HaloReachRetail:
                case DefinitionSet.Halo4Retail:
                    return new ReachRetail.bitmap(Cache);

                default:
                    throw new NotSupportedException(errorMessage.Replace("----", "bitm"));
            }
        }

        //---, H3R, ODST, HRB, HRR, H4R
        public static render_model mode(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            Cache.Reader.BaseStream.Position = Tag.Offset;
            switch (Cache.Version)
            {
                case DefinitionSet.Halo3Retail:
                    return new Halo3Retail.render_model(Cache);

                case DefinitionSet.Halo3ODST:
                    return new Halo3ODST.render_model(Cache);

                case DefinitionSet.HaloReachBeta:
                    return new ReachBeta.render_model(Cache);

                case DefinitionSet.HaloReachRetail:
                    return new ReachRetail.render_model(Cache);

                case DefinitionSet.Halo4Retail:
                    return new Halo4Retail.render_model(Cache);

                default:
                    throw new NotSupportedException(errorMessage.Replace("----", "mode"));
            }
        }

        //---, H3R, ODST, HRB, HRR, H4R
        public static shader rmsh(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            Cache.Reader.BaseStream.Position = Tag.Offset;
            switch (Cache.Version)
            {
                case DefinitionSet.Halo3Retail:
                case DefinitionSet.Halo3ODST:
                    return new Halo3Retail.shader(Cache);

                case DefinitionSet.HaloReachBeta:
                case DefinitionSet.HaloReachRetail:
                    return new ReachBeta.shader(Cache);

                case DefinitionSet.Halo4Retail:
                    return new Halo4Retail.material(Cache);

                default:
                    throw new NotSupportedException(errorMessage.Replace("----", "mode"));
            }
        }

        //---, H3R, ODST, HRB, HRR, ---
        public static sound snd_(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            Cache.Reader.BaseStream.Position = Tag.Offset;
            switch (Cache.Version)
            {
                case DefinitionSet.Halo3Retail:
                case DefinitionSet.Halo3ODST:
                    return new Halo3Retail.sound(Cache);

                case DefinitionSet.HaloReachBeta:
                case DefinitionSet.HaloReachRetail:
                    return new ReachBeta.sound(Cache);

                default:
                    throw new NotSupportedException(errorMessage.Replace("----", "snd!"));
            }
        }

        //---, H3R, ODST, HRB, HRR, H4R
        public static multilingual_unicode_string_list unic(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            Cache.Reader.BaseStream.Position = Tag.Offset;
            switch (Cache.Version)
            {
                case DefinitionSet.Halo3Retail:
                case DefinitionSet.Halo3ODST:
                    return new Halo3Retail.multilingual_unicode_string_list(Cache);

                case DefinitionSet.HaloReachBeta:
                case DefinitionSet.HaloReachRetail:
                case DefinitionSet.Halo4Retail:
                    return new ReachBeta.multilingual_unicode_string_list(Cache);

                default:
                    throw new NotSupportedException(errorMessage.Replace("----", "unic"));
            }
        }
    }
}

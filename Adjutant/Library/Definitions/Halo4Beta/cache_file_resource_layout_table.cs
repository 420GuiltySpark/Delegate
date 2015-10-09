using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using play = Adjutant.Library.Definitions.cache_file_resource_layout_table;

namespace Adjutant.Library.Definitions.Halo4Beta
{
    public class cache_file_resource_layout_table : Halo3Retail.cache_file_resource_layout_table
    {
        protected cache_file_resource_layout_table() { }

        public cache_file_resource_layout_table(CacheBase Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            #region Shared Cache Block
            Reader.SeekTo(Address + 12);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            SharedCaches = new List<play.SharedCache>();
            for (int i = 0; i < iCount; i++)
                SharedCaches.Add(new SharedCache(Cache, iOffset + 264 * i));
            #endregion

            #region Page Block
            Reader.SeekTo(Address + 24);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Pages = new List<play.Page>();
            for (int i = 0; i < iCount; i++)
                Pages.Add(new Page(Cache, iOffset + 88 * i));
            #endregion

            SoundRawChunks = new List<play.SoundRawChunk>();
            //(sound blocks unused afaik)

            #region Segment Block
            Reader.SeekTo(Address + 48);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Segments = new List<play.Segment>();
            for (int i = 0; i < iCount; i++)
                Segments.Add(new Segment(Cache, iOffset + 24 * i));
            #endregion
        }

        new public class Page : play.Page
        {
            public Page(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.ReadInt32();
                CacheIndex = Reader.ReadInt16();
                Reader.ReadInt16();
                RawOffset = Reader.ReadInt32();
                CompressedSize = Reader.ReadInt32();
                DecompressedSize = Reader.ReadInt32();

                Reader.SeekTo(Address + 84);

                RawChunkCount = Reader.ReadInt16();
                Reader.ReadInt16();
            }
        }

        new public class Segment : play.Segment
        {
            public Segment(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                RequiredPageOffset = Reader.ReadInt32();
                OptionalPageOffset = Reader.ReadInt32();
                OptionalPageOffset2 = Reader.ReadInt32();
                RequiredPageIndex = Reader.ReadInt16();
                OptionalPageIndex = Reader.ReadInt16();
                OptionalPageIndex2 = Reader.ReadInt16();
                SoundNumber = -1;
                SoundRawIndex = -1;
                //probably sound stuff, but Halo4 doesn't store them in the maps anymore
                //so everything here is -1
                Reader.ReadInt16(); Reader.ReadInt16(); Reader.ReadInt16();
            }
        }
    }
}

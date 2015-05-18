using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using play = Adjutant.Library.Definitions.cache_file_resource_layout_table;

namespace Adjutant.Library.Definitions.Halo4Retail
{
    internal class cache_file_resource_layout_table : play
    {
        internal cache_file_resource_layout_table(CacheFile Cache)
        {
            EndianReader Reader = Cache.Reader;

            Reader.BaseStream.Position += 12; //12 (codecs block goes here)

            #region Shared Cache Block
            long temp = Reader.BaseStream.Position;
            int cCount = Reader.ReadInt32();
            int cOffset = Reader.ReadInt32() - Cache.Magic;
            SharedCaches = new List<play.SharedCache>();
            Reader.BaseStream.Position = cOffset;
            for (int i = 0; i < cCount; i++)
                SharedCaches.Add(new SharedCache(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            #region RawPool Block
            temp = Reader.BaseStream.Position;
            int pCount = Reader.ReadInt32();
            int pOffset = Reader.ReadInt32() - Cache.Magic;
            Pages = new List<play.Page>();
            Reader.BaseStream.Position = pOffset;
            for (int i = 0; i < pCount; i++)
                Pages.Add(new Page(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            SoundRawChunks = new List<SoundRawChunk>();
            Reader.BaseStream.Position += 12; //48 (sound blocks unused afaik)

            #region RawLocation Block
            temp = Reader.BaseStream.Position;
            int lCount = Reader.ReadInt32();
            int lOffset = Reader.ReadInt32() - Cache.Magic;
            Segments = new List<play.Segment>();
            Reader.BaseStream.Position = lOffset;
            for (int i = 0; i < lCount; i++)
                Segments.Add(new Segment(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion
        }

        new internal class SharedCache : play.SharedCache
        {
            internal SharedCache(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                FileName = Reader.ReadNullTerminatedString(32);

                Reader.BaseStream.Position += 232;
            }
        }

        new internal class Page : play.Page
        {
            internal Page(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                Reader.ReadInt32();
                CacheIndex = Reader.ReadInt16();
                Reader.ReadInt16();
                RawOffset = Reader.ReadInt32();
                CompressedSize = Reader.ReadInt32();
                DecompressedSize = Reader.ReadInt32();

                Reader.BaseStream.Position += 64; //84

                RawChunkCount = Reader.ReadInt16();
                Reader.ReadInt16();
            }
        }

        new internal class Segment : play.Segment
        {
            internal Segment(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

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

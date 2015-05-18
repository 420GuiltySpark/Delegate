using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using play = Adjutant.Library.Definitions.cache_file_resource_layout_table;

namespace Adjutant.Library.Definitions.Halo3Retail
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

            #region SoundRawChunk Block
            temp = Reader.BaseStream.Position;
            int sCount = Reader.ReadInt32();
            int sOffset = Reader.ReadInt32() - Cache.Magic;
            SoundRawChunks = new List<play.SoundRawChunk>();
            Reader.BaseStream.Position = sOffset;
            for (int i = 0; i < sCount; i++)
                SoundRawChunks.Add(new SoundRawChunk(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

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

        new internal class SoundRawChunk : play.SoundRawChunk
        {
            internal SoundRawChunk(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                RawSize = Reader.ReadInt32();

                #region Size Chunk
                long temp = Reader.BaseStream.Position;
                int sCount = Reader.ReadInt32();
                int sOffset = Reader.ReadInt32() - Cache.Magic;
                Sizes = new List<play.SoundRawChunk.Size>();
                Reader.BaseStream.Position = sOffset;
                for (int i = 0; i < sCount; i++)
                    Sizes.Add(new Size(Cache));
                Reader.BaseStream.Position = temp + 12;
                #endregion
            }

            new internal class Size : play.SoundRawChunk.Size
            {
                internal Size(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    Reader.ReadInt32();
                    PermutationSize = Reader.ReadInt32();

                    Reader.BaseStream.Position += 8; //16
                }
            }
        }

        new internal class Segment : play.Segment
        {
            internal Segment(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                RequiredPageIndex = Reader.ReadInt16();
                OptionalPageIndex = Reader.ReadInt16();
                OptionalPageIndex2 = -1; //doesn't exist in Halo3
                RequiredPageOffset = Reader.ReadInt32();
                OptionalPageOffset = Reader.ReadInt32();
                OptionalPageOffset2 = -1; //doesn't exist in Halo3
                SoundNumber = Reader.ReadInt16();
                SoundRawIndex = Reader.ReadInt16();
            }
        }
    }
}

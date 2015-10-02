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
    public class cache_file_resource_layout_table : play
    {
        public cache_file_resource_layout_table(CacheBase Cache, int Offset)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Offset);

            Reader.SeekTo(Offset + 12);

            #region Shared Cache Block
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            SharedCaches = new List<play.SharedCache>();
            for (int i = 0; i < iCount; i++)
                SharedCaches.Add(new SharedCache(Cache, iOffset + 264 * i));
            Reader.SeekTo(Offset + 24);
            #endregion

            #region RawPool Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Pages = new List<play.Page>();
            for (int i = 0; i < iCount; i++)
                Pages.Add(new Page(Cache, iOffset + 88 * i));
            Reader.SeekTo(Offset + 36);
            #endregion

            #region SoundRawChunk Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            SoundRawChunks = new List<play.SoundRawChunk>();
            for (int i = 0; i < iCount; i++)
                SoundRawChunks.Add(new SoundRawChunk(Cache, iOffset + 16 * i));
            Reader.SeekTo(Offset + 48);
            #endregion

            #region RawLocation Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Segments = new List<play.Segment>();
            for (int i = 0; i < iCount; i++)
                Segments.Add(new Segment(Cache, iOffset + 16 * i));
            Reader.SeekTo(Offset + 60);
            #endregion
        }

        new public class SharedCache : play.SharedCache
        {
            public SharedCache(CacheBase Cache, int Offset)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Offset);

                FileName = Reader.ReadNullTerminatedString(32);

                Reader.SeekTo(Offset + 264);
            }
        }

        new public class Page : play.Page
        {
            public Page(CacheBase Cache, int Offset)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Offset);

                Reader.ReadInt32();
                CacheIndex = Reader.ReadInt16();
                Reader.ReadInt16();
                RawOffset = Reader.ReadInt32();
                CompressedSize = Reader.ReadInt32();
                DecompressedSize = Reader.ReadInt32();

                Reader.SeekTo(Offset + 84);

                RawChunkCount = Reader.ReadInt16();
                Reader.ReadInt16();
            }
        }

        new public class SoundRawChunk : play.SoundRawChunk
        {
            public SoundRawChunk(CacheBase Cache, int Offset)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Offset);

                RawSize = Reader.ReadInt32();

                #region Size Chunk
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                Sizes = new List<play.SoundRawChunk.Size>();
                for (int i = 0; i < iCount; i++)
                    Sizes.Add(new Size(Cache, iOffset + 16 * i));
                Reader.SeekTo(Offset + 16);
                #endregion
            }

            new public class Size : play.SoundRawChunk.Size
            {
                public Size(CacheBase Cache, int Offset)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Offset);

                    Reader.ReadInt32();
                    PermutationSize = Reader.ReadInt32();

                    Reader.SeekTo(Offset + 16);
                }
            }
        }

        new public class Segment : play.Segment
        {
            public Segment(CacheBase Cache, int Offset)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Offset);

                RequiredPageIndex = Reader.ReadInt16();
                OptionalPageIndex = Reader.ReadInt16();
                OptionalPageIndex2 = -1; //doesn't exist till Halo4
                RequiredPageOffset = Reader.ReadInt32();
                OptionalPageOffset = Reader.ReadInt32();
                OptionalPageOffset2 = -1; //doesn't exist till Halo4
                SoundNumber = Reader.ReadInt16();
                SoundRawIndex = Reader.ReadInt16();
            }
        }
    }
}

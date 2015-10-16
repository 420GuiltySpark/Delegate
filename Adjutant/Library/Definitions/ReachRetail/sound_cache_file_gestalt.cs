using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using ugh_ = Adjutant.Library.Definitions.sound_cache_file_gestalt;

namespace Adjutant.Library.Definitions.ReachRetail
{
    public class sound_cache_file_gestalt : ReachBeta.sound_cache_file_gestalt
    {
        protected sound_cache_file_gestalt() { }

        public sound_cache_file_gestalt(CacheBase Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            #region Codec Chunk
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Codecs.Add(new Codec(Cache, iOffset + 3 * i));
            #endregion

            #region SoundName Chunk
            Reader.SeekTo(Address + 36);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                SoundNames.Add(new SoundName(Cache, iOffset + 4 * i));
            #endregion

            #region Playback Chunk
            Reader.SeekTo(Address + 72);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                PlayBacks.Add(new Playback(Cache, iOffset + 12 * i));
            #endregion

            #region SoundPermutation Chunk
            Reader.SeekTo(Address + 84);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                SoundPermutations.Add(new SoundPermutation(Cache, iOffset + 20 * i));
            #endregion

            #region Raw Chunks
            Reader.SeekTo(Address + 172);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                RawChunks.Add(new RawChunk(Cache, iOffset + 20 * i));
            #endregion
        }

        new public class SoundPermutation : ugh_.SoundPermutation
        {
            public SoundPermutation(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                NameIndex = Reader.ReadInt16();

                Reader.SeekTo(Address + 8);
                RawChunkIndex = Reader.ReadInt32();
                ChunkCount = Reader.ReadInt16();
            }
        }
    }
}

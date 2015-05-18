using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using ugh_ = Adjutant.Library.Definitions.sound_cache_file_gestalt;

namespace Adjutant.Library.Definitions.Halo3Retail
{
    internal class sound_cache_file_gestalt : ugh_
    {
        internal sound_cache_file_gestalt(CacheFile Cache)
        {
            EndianReader Reader = Cache.Reader;

            #region Codec Chunk
            long temp = Reader.BaseStream.Position;
            int count = Reader.ReadInt32();
            int offset = Reader.ReadInt32() - Cache.Magic;
            Codecs = new List<ugh_.Codec>();
            Reader.BaseStream.Position = offset;
            for (int i = 0; i < count; i++)
                Codecs.Add(new Codec(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.BaseStream.Position += 24; //36

            #region SoundName Chunk
            temp = Reader.BaseStream.Position;
            count = Reader.ReadInt32();
            offset = Reader.ReadInt32() - Cache.Magic;
            SoundNames = new List<ugh_.SoundName>();
            Reader.BaseStream.Position = offset;
            for (int i = 0; i < count; i++)
                SoundNames.Add(new SoundName(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.BaseStream.Position += 12; //60

            #region Playback Chunk
            temp = Reader.BaseStream.Position;
            count = Reader.ReadInt32();
            offset = Reader.ReadInt32() - Cache.Magic;
            PlayBacks = new List<ugh_.Playback>();
            Reader.BaseStream.Position = offset;
            for (int i = 0; i < count; i++)
                PlayBacks.Add(new Playback(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            #region SoundPermutation Chunk
            temp = Reader.BaseStream.Position;
            count = Reader.ReadInt32();
            offset = Reader.ReadInt32() - Cache.Magic;
            SoundPermutations = new List<ugh_.SoundPermutation>();
            Reader.BaseStream.Position = offset;
            for (int i = 0; i < count; i++)
                SoundPermutations.Add(new SoundPermutation(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.BaseStream.Position += 64; //148

            #region Raw Chunks
            temp = Reader.BaseStream.Position;
            count = Reader.ReadInt32();
            offset = Reader.ReadInt32() - Cache.Magic;
            RawChunks = new List<ugh_.RawChunk>();
            Reader.BaseStream.Position = offset;
            for (int i = 0; i < count; i++)
                RawChunks.Add(new RawChunk(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.BaseStream.Position += 20; //184
        }

        new internal class Codec : ugh_.Codec
        {
            internal Codec(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                Unknown = Reader.ReadByte();
                Type = (SoundType)Reader.ReadByte();
                Flags = new Bitmask(Reader.ReadByte());
            }
        }

        new internal class SoundName : ugh_.SoundName
        {
            internal SoundName(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());
            }
        }

        new internal class Playback : ugh_.Playback
        {
            internal Playback(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                NameIndex = Reader.ReadInt16();
                ParametersIndex = Reader.ReadInt16();
                Unknown = Reader.ReadInt16();
                FirstRuntimePermFlagIndex = Reader.ReadInt16();
                EncodedPermData = Reader.ReadInt16();
                FirstPermutation = Reader.ReadInt16();
            }
        }

        new internal class SoundPermutation : ugh_.SoundPermutation
        {
            internal SoundPermutation(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                NameIndex = Reader.ReadInt16();
                EncodedSkipFraction = Reader.ReadInt16();
                EncodedGain = Reader.ReadByte();
                PermInfoIndex = Reader.ReadByte();
                LanguageNeutralTime = Reader.ReadInt16();
                RawChunkIndex = Reader.ReadInt32();
                ChunkCount = Reader.ReadInt16();
                EncodedPermIndex = Reader.ReadInt16();
            }
        }

        new internal class RawChunk : ugh_.RawChunk
        {
            internal RawChunk(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                FileOffset = Reader.ReadInt32();
                Flags = new Bitmask(Reader.ReadInt16());
                Size = Reader.ReadUInt16();
                RuntimeIndex = Reader.ReadInt32();
                Unknown0 = Reader.ReadInt32();
                Unknown1 = Reader.ReadInt32();
            }
        }
    }
}

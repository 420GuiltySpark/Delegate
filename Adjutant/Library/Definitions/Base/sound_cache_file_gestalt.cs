﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.DataTypes;

namespace Adjutant.Library.Definitions
{
    public abstract class sound_cache_file_gestalt
    {
        public List<Codec> Codecs;
        public List<SoundName> SoundNames;
        public List<Playback> PlayBacks;
        public List<SoundPermutation> SoundPermutations;
        public List<RawChunk> RawChunks;

        public abstract class Codec
        {
            public int Unknown;
            public SoundType Type;
            public Bitmask Flags;
        }

        public abstract class SoundName
        {
            public string Name;

            public override string ToString()
            {
                return Name;
            }
        }

        public abstract class Playback
        {
            public int NameIndex;
            public int ParametersIndex;
            public int Unknown;
            public int FirstRuntimePermFlagIndex;
            public int EncodedPermData;
            public int FirstPermutation;
            public int PermutationCount
            {
                get { return (EncodedPermData >> 4) & 63; }
            }
        }

        public abstract class SoundPermutation
        {
            public int NameIndex;
            public int EncodedSkipFraction;
            public int EncodedGain;
            public int PermInfoIndex;
            public int LanguageNeutralTime;
            public int RawChunkIndex;
            public int ChunkCount;
            public int EncodedPermIndex;
        }

        public class RawChunk
        {
            public int FileOffset;
            public Bitmask Flags;
            public int Size;
            public int RuntimeIndex;
            public int Unknown0;
            public int Unknown1;
        }
    }
}

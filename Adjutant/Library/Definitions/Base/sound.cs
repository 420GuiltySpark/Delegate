using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.DataTypes;

namespace Adjutant.Library.Definitions
{
    public abstract class sound
    {
        public Bitmask Flags;
        public int SoundClass;
        public SampleRate SampleRate;
        public int Encoding;
        public int CodecIndex;
        public int PlaybackIndex;
        public int DialogueUnknown;
        public int Unknown0;
        public int PitchRangeIndex1;
        public int PitchRangeIndex2;
        public int ScaleIndex;
        public int PromotionIndex;
        public int CustomPlaybackIndex;
        public int ExtraInfoIndex;
        public int Unknown1;
        public int RawID;
        public int MaxPlaytime;

        //Halo4Retail
        public uint SoundAddress1;
        public uint SoundAddress2;
        public int SoundBankTagID;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using snd_ = Adjutant.Library.Definitions.sound;

namespace Adjutant.Library.Definitions.Halo3Retail
{
    public class sound : snd_
    {
        public sound(CacheBase Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Flags = new Bitmask(Reader.ReadInt16());
            SoundClass = Reader.ReadByte();
            SampleRate = (SampleRate)Reader.ReadByte();
            Encoding = Reader.ReadByte();
            CodecIndex = Reader.ReadByte();
            PlaybackIndex = Reader.ReadInt16();
            DialogueUnknown = Reader.ReadInt16();
            Unknown0 = Reader.ReadInt16();
            PitchRangeIndex1 = Reader.ReadInt16();
            PitchRangeIndex2 = Reader.ReadByte();
            ScaleIndex = Reader.ReadByte();
            PromotionIndex = Reader.ReadByte();
            CustomPlaybackIndex = Reader.ReadByte();
            ExtraInfoIndex = Reader.ReadInt16();
            Unknown1 = Reader.ReadInt32();
            RawID = Reader.ReadInt32();
            MaxPlaytime = Reader.ReadInt32();
        }
    }
}

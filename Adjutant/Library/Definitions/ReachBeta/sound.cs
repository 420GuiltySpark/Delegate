using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using snd_ = Adjutant.Library.Definitions.sound;

namespace Adjutant.Library.Definitions.ReachBeta
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

            Reader.SeekTo(Address + 28);
            RawID = Reader.ReadInt32();
        }
    }
}

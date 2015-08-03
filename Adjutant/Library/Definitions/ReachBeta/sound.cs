using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using Adjutant.Library.Definitions;
using Adjutant.Library.DataTypes;
using snd_ = Adjutant.Library.Definitions.sound;

namespace Adjutant.Library.Definitions.ReachBeta
{
    //TODO: finish this
    public class sound : snd_
    {
        public sound(CacheFile Cache)
        {
            EndianReader Reader = Cache.Reader;

            Flags = new Bitmask(Reader.ReadInt16());
            SoundClass = Reader.ReadByte();
            SampleRate = (SampleRate)Reader.ReadByte();
            Encoding = Reader.ReadByte();
            CodecIndex = Reader.ReadByte();
            PlaybackIndex = Reader.ReadInt16();
            DialogueUnknown = Reader.ReadInt16();

            Reader.BaseStream.Position += 18; //28

            RawID = Reader.ReadInt32();
            
            Reader.ReadInt32();
        }
    }
}

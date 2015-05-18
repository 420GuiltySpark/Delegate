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

namespace Adjutant.Library.Definitions.Halo4Retail
{
    internal class sound : snd_
    {
        internal sound(CacheFile Cache)
        {
            EndianReader Reader = Cache.Reader;

            Reader.BaseStream.Position += 12; //12
            
            SoundAddress1 = Reader.ReadUInt32();
            SoundAddress2 = Reader.ReadUInt32();

            Reader.BaseStream.Position += 20; //40

            Reader.BaseStream.Position += 12; //52
            SoundBankTagID = Reader.ReadInt32();

            Reader.BaseStream.Position += 20;
        }
    }
}

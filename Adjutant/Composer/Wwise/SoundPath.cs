using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Composer.IO;
using Adjutant.Library.Endian;

namespace Composer.Wwise
{
    public class SoundPathPoint
    {
        public SoundPathPoint(EndianReader reader)
        {
            X = reader.ReadSingle();
            reader.Skip(4); // Unknown
            Y = reader.ReadSingle();
            Duration = reader.ReadInt32();
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public int Duration { get; private set; }
    }

    public class SoundPathRandomRange
    {
        public SoundPathRandomRange(EndianReader reader)
        {
            HorizontalRange = reader.ReadSingle();
            VerticalRange = reader.ReadSingle();
        }

        public float HorizontalRange { get; private set; }
        public float VerticalRange { get; private set; }
    }

    public class SoundPath
    {
        public SoundPath(EndianReader reader)
        {
            FirstPointIndex = reader.ReadInt32();
            PointCount = reader.ReadInt32();
        }

        public int FirstPointIndex { get; private set; }
        public int PointCount { get; private set; }
    }
}

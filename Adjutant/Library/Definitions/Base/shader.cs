using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adjutant.Library.Definitions
{
    public abstract class shader
    {
        public int BaseShaderTagID;
        public List<PredictedBitmap> PredictedBitmaps;
        public List<ShaderProperties> Properties;

        public abstract class PredictedBitmap
        {
            public string Type;
            public int BitmapTagID;
        }

        public abstract class ShaderProperties
        {
            public int TemplateTagID;
            public List<ShaderMap> ShaderMaps;
            public List<Tiling> Tilings;

            public abstract class ShaderMap
            {
                public int BitmapTagID;
                public int Type;
                public int TilingIndex;
            }

            public abstract class Tiling
            {
                public float UTiling;
                public float VTiling;
                public float Unknown0;
                public float Unknown1;
            }
        }
    }
}

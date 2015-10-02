using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using rmsh = Adjutant.Library.Definitions.shader;

namespace Adjutant.Library.Definitions.ReachBeta
{
    public class shader : rmsh
    {
        public shader(CacheBase Cache)
        {
            EndianReader Reader = Cache.Reader;

            Reader.BaseStream.Position += 12; //12

            BaseShaderTagID = Reader.ReadInt32();

            Reader.BaseStream.Position += 40; //56 (reach doesn't use predicted bitmaps)

            #region ShaderProperties Chunk
            long temp = Reader.BaseStream.Position;
            int pCount = Reader.ReadInt32();
            int pOffset = Reader.ReadInt32() - Cache.Magic;
            Properties = new List<rmsh.ShaderProperties>();
            Reader.BaseStream.Position = pOffset;
            for (int i = 0; i < pCount; i++)
                Properties.Add(new ShaderProperties(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.BaseStream.Position += 52; //104
        }

        new public class ShaderProperties : rmsh.ShaderProperties
        {
            public ShaderProperties(CacheBase Cache)
            {
                EndianReader Reader = Cache.Reader;

                Reader.BaseStream.Position += 12; //12

                TemplateTagID = Reader.ReadInt32();

                #region ShaderProperties Chunk
                long temp = Reader.BaseStream.Position;
                int sCount = Reader.ReadInt32();
                int sOffset = Reader.ReadInt32() - Cache.Magic;
                ShaderMaps = new List<rmsh.ShaderProperties.ShaderMap>();
                Reader.BaseStream.Position = sOffset;
                for (int i = 0; i < sCount; i++)
                    ShaderMaps.Add(new ShaderMap(Cache));
                Reader.BaseStream.Position = temp + 12;
                #endregion
                
                #region Tiling Chunk
                temp = Reader.BaseStream.Position;
                int tCount = Reader.ReadInt32();
                int tOffset = Reader.ReadInt32() - Cache.Magic;
                Tilings = new List<rmsh.ShaderProperties.Tiling>();
                Reader.BaseStream.Position = tOffset;
                for (int i = 0; i < tCount; i++)
                    Tilings.Add(new Tiling(Cache));
                Reader.BaseStream.Position = temp + 12;
                #endregion

                Reader.BaseStream.Position += 132; //172
            }

            new public class ShaderMap : rmsh.ShaderProperties.ShaderMap
            {
                public ShaderMap(CacheBase Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    Reader.BaseStream.Position += 12; //12

                    BitmapTagID = Reader.ReadInt32();
                    Type = Reader.ReadInt32();
                    TilingIndex = Reader.ReadInt16();
                    Reader.ReadInt16();
                }
            }

            new public class Tiling : rmsh.ShaderProperties.Tiling
            {
                public Tiling(CacheBase Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    UTiling = Reader.ReadSingle();
                    VTiling = Reader.ReadSingle();
                    Unknown0 = Reader.ReadSingle();
                    Unknown1 = Reader.ReadSingle();
                }
            }
        }
    }
}

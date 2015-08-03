using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using rmsh = Adjutant.Library.Definitions.shader;

namespace Adjutant.Library.Definitions.Halo3Beta
{
    public class shader : rmsh
    {
        public shader(CacheFile Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Reader.SeekTo(Address + 12);

            BaseShaderTagID = Reader.ReadInt32();

            Reader.SeekTo(Address + 40);

            #region ShaderProperties Chunk
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            Properties = new List<rmsh.ShaderProperties>();
            for (int i = 0; i < iCount; i++)
                Properties.Add(new ShaderProperties(Cache, iOffset + 132 * i));
            #endregion

            Reader.SeekTo(Address + 68);
        }

        new public class ShaderProperties : rmsh.ShaderProperties
        {
            public ShaderProperties(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.SeekTo(Address + 12);

                TemplateTagID = Reader.ReadInt32();

                #region ShaderProperties Chunk
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                ShaderMaps = new List<rmsh.ShaderProperties.ShaderMap>();
                for (int i = 0; i < iCount; i++)
                    ShaderMaps.Add(new ShaderMap(Cache, iOffset + 24 * i));
                #endregion

                Reader.SeekTo(Address + 28);
                
                #region Tiling Chunk
                iCount = Reader.ReadInt32();
                iOffset = Reader.ReadInt32() - Cache.Magic;
                Tilings = new List<rmsh.ShaderProperties.Tiling>();
                for (int i = 0; i < iCount; i++)
                    Tilings.Add(new Tiling(Cache, iOffset + 16 * i));
                #endregion

                Reader.SeekTo(Address + 132);
            }

            new public class ShaderMap : rmsh.ShaderProperties.ShaderMap
            {
                public ShaderMap(CacheFile Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    Reader.SeekTo(Address + 12);

                    BitmapTagID = Reader.ReadInt32();
                    Type = Reader.ReadInt32();
                    TilingIndex = Reader.ReadInt16();
                    Reader.ReadInt16();
                }
            }

            new public class Tiling : rmsh.ShaderProperties.Tiling
            {
                public Tiling(CacheFile Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    UTiling = Reader.ReadSingle();
                    VTiling = Reader.ReadSingle();
                    Unknown0 = Reader.ReadSingle();
                    Unknown1 = Reader.ReadSingle();
                }
            }
        }
    }
}

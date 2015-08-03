using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using rmsh = Adjutant.Library.Definitions.shader;

namespace Adjutant.Library.Definitions.Halo4Retail
{
    public class material : rmsh
    {
        public material(CacheFile Cache)
        {
            EndianReader Reader = Cache.Reader;

            Reader.BaseStream.Position += 12; //12

            BaseShaderTagID = Reader.ReadInt32();

            Reader.BaseStream.Position += 12; //28

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

            Reader.BaseStream.Position += 28; //68
        }

        new public class ShaderProperties : rmsh.ShaderProperties
        {
            public ShaderProperties(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

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

                Reader.BaseStream.Position += 136; //140
            }

            new public class ShaderMap : rmsh.ShaderProperties.ShaderMap
            {
                public ShaderMap(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    Reader.BaseStream.Position += 12; //12

                    BitmapTagID = Reader.ReadInt32();
                    Type = Reader.ReadInt16();
                    Reader.ReadByte();
                    TilingIndex = Reader.ReadByte();

                    if (Cache.Version == DefinitionSet.Halo4Retail)
                    {
                        Reader.ReadInt16();
                        Reader.ReadInt16();
                    }
                }
            }

            new public class Tiling : rmsh.ShaderProperties.Tiling
            {
                public Tiling(CacheFile Cache)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using rmsh = Adjutant.Library.Definitions.shader;

namespace Adjutant.Library.Definitions.Halo3Retail
{
    internal class shader : rmsh
    {
        internal shader(CacheFile Cache)
        {
            EndianReader Reader = Cache.Reader;

            Reader.BaseStream.Position += 12; //12

            BaseShaderTagID = Reader.ReadInt32();

            Reader.BaseStream.Position += 12; //28

            #region PredictedBitmap Chunk
            long temp = Reader.BaseStream.Position;
            int bCount = Reader.ReadInt32();
            int bOffset = Reader.ReadInt32() - Cache.Magic;
            PredictedBitmaps = new List<rmsh.PredictedBitmap>();
            Reader.BaseStream.Position = bOffset;
            for (int i = 0; i < bCount; i++)
                PredictedBitmaps.Add(new PredictedBitmap(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            #region ShaderProperties Chunk
            temp = Reader.BaseStream.Position;
            int pCount = Reader.ReadInt32();
            int pOffset = Reader.ReadInt32() - Cache.Magic;
            Properties = new List<rmsh.ShaderProperties>();
            Reader.BaseStream.Position = pOffset;
            for (int i = 0; i < pCount; i++)
                Properties.Add(new ShaderProperties(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.BaseStream.Position += 16; //68
        }

        new internal class PredictedBitmap : rmsh.PredictedBitmap
        {
            internal PredictedBitmap(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                Type = Cache.Strings.GetItemByID(Reader.ReadInt32());

                Reader.BaseStream.Position += 16; //20

                BitmapTagID = Reader.ReadInt32();

                Reader.BaseStream.Position += 36; //60
            }
        }

        new internal class ShaderProperties : rmsh.ShaderProperties
        {
            internal ShaderProperties(CacheFile Cache)
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

                Reader.BaseStream.Position += 92; //132
            }

            new internal class ShaderMap : rmsh.ShaderProperties.ShaderMap
            {
                internal ShaderMap(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    Reader.BaseStream.Position += 12; //12

                    BitmapTagID = Reader.ReadInt32();
                    Type = Reader.ReadInt32();
                    TilingIndex = Reader.ReadInt32();
                }
            }

            new internal class Tiling : rmsh.ShaderProperties.Tiling
            {
                internal Tiling(CacheFile Cache)
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

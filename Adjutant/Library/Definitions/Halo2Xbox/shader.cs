﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using rmsh = Adjutant.Library.Definitions.shader;

namespace Adjutant.Library.Definitions.Halo2Xbox
{
    internal class shader : rmsh
    {
        public int Type;
        public int[] BitmIDs;

        internal shader(CacheFile Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            BaseShaderTagID = -1;

            Reader.SeekTo(Address + 12);

            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            var sMap = new ShaderProperties.ShaderMap(Cache, iOffset);

            Reader.SeekTo(Address + 20);
            Type = Reader.ReadInt16();

            Reader.SeekTo(Address + 32);

            #region ShaderProperties Chunk
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Properties = new List<rmsh.ShaderProperties>();
            for (int i = 0; i < iCount; i++)
                Properties.Add(new ShaderProperties(Cache, iOffset + 124 * i) { ShaderMaps = new List<rmsh.ShaderProperties.ShaderMap>() { sMap } });
            #endregion

            Reader.SeekTo(Address + 44);

            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Reader.SeekTo(iOffset);

            BitmIDs = new int[iCount];
            for (int i = 0; i < iCount; i++)
            {
                Reader.ReadInt32();
                BitmIDs[i] = Reader.ReadInt32();
            }

        }

        new internal class ShaderProperties : rmsh.ShaderProperties
        {
            internal ShaderProperties(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);
                TemplateTagID = -1;

                Reader.SeekTo(Address + 20);
                
                #region Tiling Chunk
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                Tilings = new List<rmsh.ShaderProperties.Tiling>();
                for (int i = 0; i < iCount; i++)
                    Tilings.Add(new Tiling(Cache, iOffset + 16 * i));
                #endregion
                //Tilings[0].UTiling = Tilings[0].VTiling = 1;
            }

            new internal class ShaderMap : rmsh.ShaderProperties.ShaderMap
            {
                internal ShaderMap(CacheFile Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    Reader.SeekTo(Address + 4);

                    BitmapTagID = Reader.ReadInt32();
                    if (BitmapTagID == -1) { Reader.ReadInt32(); BitmapTagID = Reader.ReadInt32(); }
                    if (BitmapTagID == -1) { Reader.SeekTo(Address + 56); BitmapTagID = Reader.ReadInt32(); }
                    TilingIndex = 1;
                }
            }

            new internal class Tiling : rmsh.ShaderProperties.Tiling
            {
                internal Tiling(CacheFile Cache, int Address)
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using rmsh = Adjutant.Library.Definitions.shader;

namespace Adjutant.Library.Definitions.Halo1PC
{
    public class shader_model : rmsh
    {
        public int baseMapID;

        public shader_model(CacheBase Cache, CacheBase.IndexItem Tag)
        {
            EndianReader Reader = Cache.Reader;
            int Address = Tag.Offset;
            Reader.SeekTo(Address);

            switch (Tag.ClassCode)
            {
                case "soso":
                    Reader.SeekTo(Address + 0xB0);
                    break;

                case "senv":
                    Reader.SeekTo(Address + 0x94);
                    break;

                case "sgla":
                    Reader.SeekTo(Address + 356);
                    break;

                case "schi":
                    Reader.SeekTo(Address + 228);
                    break;

                case "scex":
                    Reader.SeekTo(Address + 900);
                    break;

                case "swat":
                case "smet":
                    Reader.SeekTo(Address + 88);
                    break;

            }

            baseMapID = Reader.ReadInt32();

            Properties.Add(new ShaderProperties(this));
        }

        new public class ShaderProperties : rmsh.ShaderProperties
        {
            public ShaderProperties(shader_model soso)
            {
                ShaderMaps.Add(new ShaderMap()
                    {
                        BitmapTagID = soso.baseMapID
                    });

                Tilings.Add(new Tiling()
                    {
                        UTiling = 1,
                        VTiling = 1,
                    });
            }

            new public class ShaderMap : rmsh.ShaderProperties.ShaderMap { }

            new public class Tiling : rmsh.ShaderProperties.Tiling { }
        }
    }
}

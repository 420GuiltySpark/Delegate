using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using Adjutant.Library.Definitions;

namespace Adjutant.Library.S3D
{
    public class Texture
    {
        public bool isLittleEndian;

        public int Width;
        public int Height;
        public TextureType Type;
        public TextureFormat Format;
        public int DataAddress;

        public int VirtualWidth
        {
            get
            {
                int var;
                switch (Format)
                {
                    case TextureFormat.A8:
                    case TextureFormat.Y8:
                    case TextureFormat.AY8:
                    case TextureFormat.A8Y8:
                    case TextureFormat.A8R8G8B8:
                    case TextureFormat.A4R4G4B4:
                    case TextureFormat.R5G6B5:
                        var = 32;
                        break;

                    default:
                        var = 128;
                        break;
                }

                if (isLittleEndian) var = 4; //little endian used on mipmaps where blocksize is 4

                return (Width % var == 0) ? Width : Width + (var - (Width % var));
            }
        }
        public int VirtualHeight
        {
            get
            {
                int var;
                switch (Format)
                {
                    case TextureFormat.A8:
                    case TextureFormat.Y8:
                    case TextureFormat.AY8:
                    case TextureFormat.A8Y8:
                    case TextureFormat.A8R8G8B8:
                    case TextureFormat.A4R4G4B4:
                    case TextureFormat.R5G6B5:
                        var = 32;
                        break;
                    //return Height;

                    default:
                        var = 128;
                        break;
                }

                if (isLittleEndian) var = 4; //little endian used on mipmaps where blocksize is 4

                return (Height % var == 0) ? Height : Height + (var - (Height % var));
            }
        }
        public int RawSize
        {
            get
            {
                int size = 0;
                switch (Format)
                {
                    case TextureFormat.CTX1:
                    case TextureFormat.DXT1:
                    case TextureFormat.DXT3a_mono:
                    case TextureFormat.DXT3a_alpha:
                    case TextureFormat.DXT5a:
                    case TextureFormat.DXT5a_mono:
                    case TextureFormat.DXT5a_alpha:
                        size = VirtualWidth * VirtualHeight / 2;
                        break;
                    case TextureFormat.A8:
                    case TextureFormat.Y8:
                    case TextureFormat.AY8:
                    case TextureFormat.DXT3:
                    case TextureFormat.DXT5:
                    case TextureFormat.DXN:
                    case TextureFormat.DXN_mono_alpha:
                        size = VirtualWidth * VirtualHeight;
                        break;
                    case TextureFormat.A4R4G4B4:
                    case TextureFormat.A1R5G5B5:
                    case TextureFormat.A8Y8:
                    case TextureFormat.R5G6B5:
                        size = VirtualWidth * VirtualHeight * 2;
                        break;
                    case TextureFormat.A8R8G8B8:
                    case TextureFormat.X8R8G8B8:
                        size = VirtualWidth * VirtualHeight * 4;
                        break;
                    default:
                        return 0;
                }

                if (Type == TextureType.CubeMap)
                    size *= 6;

                return size;
            }
        }

        public Texture(PakFile Pak, PakFile.PakTag Item)
        {
            var reader = Pak.Reader;
            reader.EndianType = EndianFormat.LittleEndian;
            reader.SeekTo(Item.Offset + 6);

            isLittleEndian = reader.ReadInt32() == 1346978644; //PICT
            if (!isLittleEndian) reader.EndianType = Endian.EndianFormat.BigEndian;

            reader.SeekTo(Item.Offset + (isLittleEndian ? 16 : 12));
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();

            reader.SeekTo(Item.Offset + (isLittleEndian ? 38 : 32));
            Format = TextureFormat.DXT5;
            var intFormat = reader.ReadInt32();
            switch (intFormat)
            {
                case 0:
                    Format = TextureFormat.A8R8G8B8;
                    break;
                case 10:
                    Format = TextureFormat.A8Y8;
                    break;
                case 12:
                    Format = TextureFormat.DXT1;
                    break;
                case 13:
                    Format = TextureFormat.DXT1;
                    break;
                case 15:
                    Format = TextureFormat.DXT3;
                    break;
                case 17:
                    Format = TextureFormat.DXT5;
                    break;
                case 22:
                    Format = TextureFormat.X8R8G8B8;
                    break;
                case 36:
                    Format = TextureFormat.DXN;
                    break;
                case 37:
                    Format = TextureFormat.DXT5a;
                    break;
                default:
                    throw new Exception("CHECK THIS");
            }

            reader.SeekTo(Item.Offset + (isLittleEndian ? 28 : 24));
            int mapCount = reader.ReadInt32();
            if (mapCount == 6) Type = TextureType.CubeMap;
            else Type = TextureType.Texture2D;
            
            if (mapCount > 1 && mapCount != 6)
                throw new Exception("CHECK THIS");

            DataAddress = Item.Offset + (isLittleEndian ? 58 : 4096);
            reader.EndianType = Endian.EndianFormat.LittleEndian; //in case it was PICT
        }
    }
}

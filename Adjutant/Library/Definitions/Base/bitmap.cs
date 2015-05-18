using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.DataTypes;

namespace Adjutant.Library.Definitions
{
    public abstract class bitmap
    {
        public List<Sequence> Sequences;
        public List<BitmapData> Bitmaps;
        public List<RawChunkA> RawChunkAs;
        public List<RawChunkB> RawChunkBs;

        public abstract class Sequence
        {
            public string Name;
            public int FirstSubmapIndex;
            public int BitmapCount;
            public List<Sprite> Sprites;

            public abstract class Sprite
            {
                public int SubmapIndex;
                public float Left, Right, Top, Bottom;
                public RealQuat RegPoint;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public abstract class BitmapData
        {
            public string Class;
            public int Width, Height, Depth;
            public Bitmask Flags;
            public TextureType Type;
            public TextureFormat Format;
            public Bitmask MoreFlags;
            public int RegX, RegY;
            public int MipmapCount;
            public int InterleavedIndex;
            public int Index2;
            public int PixelsOffset;
            public int PixelsSize;

            public virtual int VirtualWidth
            {
                get
                {
                    //int i = 1;
                    //int vW = 0;
                    //while (vW < Width || vW < Height)
                    //    vW = (int)Math.Pow(2, i++);
                    //return vW;

                    //return Width;

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

                    return (Width % var == 0) ? Width : Width + (var - (Width % var));
                }
            }

            public virtual int VirtualHeight
            {
                get
                {
                    //int i = 1;
                    //int vH = 0;
                    //while (vH < Height || vH < Width)
                    //    vH = (int)Math.Pow(2, i++);
                    //return vH;

                    //return Height;

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

                    return (Height % var == 0) ? Height : Height + (var - (Height % var));
                }
            }

            public virtual int RawSize
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
                        case TextureFormat.P8_bump:
                            size = VirtualWidth * VirtualHeight;
                            break;
                        case TextureFormat.A4R4G4B4:
                        case TextureFormat.A1R5G5B5:
                        case TextureFormat.A8Y8:
                        //case TextureFormat.U8V8:
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

            public virtual int BlockSize
            {
                get
                {
                    switch (Format)
                    {
                        case TextureFormat.CTX1:
                        case TextureFormat.DXT1:
                        case TextureFormat.DXT3a_mono:
                        case TextureFormat.DXT3a_alpha:
                        case TextureFormat.DXT5a:
                        case TextureFormat.DXT5a_mono:
                        case TextureFormat.DXT5a_alpha:
                            return 2;
                        case TextureFormat.A8:
                        case TextureFormat.Y8:
                        case TextureFormat.AY8:
                        case TextureFormat.P8_bump:
                            return 1;
                        case TextureFormat.DXT3:
                        case TextureFormat.DXT5:
                        case TextureFormat.DXN:
                        case TextureFormat.DXN_mono_alpha:
                            return 2;
                        case TextureFormat.A4R4G4B4:
                        case TextureFormat.A1R5G5B5:
                        case TextureFormat.A8Y8:
                        case TextureFormat.R5G6B5:
                            return 2;
                        case TextureFormat.A8R8G8B8:
                        case TextureFormat.X8R8G8B8:
                            return 4;
                        default:
                            return 0;
                    }
                }
            }
        }

        public class RawChunkA
        {
            public int RawID;
        }

        public class RawChunkB
        {
            public int RawID;
        }
    }
}

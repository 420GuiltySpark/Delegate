using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using bitm = Adjutant.Library.Definitions.bitmap;

namespace Adjutant.Library.Definitions.Halo3Beta
{
    public class bitmap : bitm
    {
        public bitmap(CacheFile Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Reader.SeekTo(Address + 84);

            #region Sequence Chunk
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            Sequences = new List<bitm.Sequence>();
            for (int i = 0; i < iCount; i++)
                Sequences.Add(new Sequence(Cache, iOffset + 64 * i));
            Reader.SeekTo(Address + 96);
            #endregion

            #region BitmapData Chunk
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Bitmaps = new List<bitm.BitmapData>();
            for (int i = 0; i < iCount; i++)
                Bitmaps.Add(new BitmapData(Cache, iOffset + 48 * i));
            Reader.SeekTo(Address + 108);
            #endregion

            Reader.SeekTo(Address + 140);

            #region Raw Chunk A
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            RawChunkAs = new List<bitm.RawChunkA>();
            for (int i = 0; i < iCount; i++)
                RawChunkAs.Add(new RawChunkA(Cache, iOffset + 8 * i));
            Reader.SeekTo(Address + 152);
            #endregion

            #region Raw Chunk B
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            RawChunkBs = new List<bitm.RawChunkB>();
            for (int i = 0; i < iCount; i++)
                RawChunkBs.Add(new RawChunkB(Cache, iOffset + 8 * i));
            Reader.SeekTo(Address + 164);
            #endregion
        }

        new public class Sequence : bitm.Sequence
        {
            public Sequence(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Name = Reader.ReadNullTerminatedString(32);
                FirstSubmapIndex = Reader.ReadInt16();
                BitmapCount = Reader.ReadInt16();

                Reader.SeekTo(Address + 52);

                #region Sprite Chunk
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                Sprites = new List<bitm.Sequence.Sprite>();
                for (int i = 0; i < iCount; i++)
                    Sprites.Add(new Sprite(Cache, iOffset + 32 * i));
                Reader.SeekTo(Address + 64);
                #endregion
            }

            new public class Sprite : bitm.Sequence.Sprite
            {
                public Sprite(CacheFile Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    SubmapIndex = Reader.ReadInt32();
                    Reader.ReadInt32();
                    Left = Reader.ReadSingle();
                    Right = Reader.ReadSingle();
                    Top = Reader.ReadSingle();
                    Bottom = Reader.ReadSingle();
                    RegPoint = new RealQuat(
                        Reader.ReadSingle(),
                        Reader.ReadSingle());
                }
            }
        }

        new public class BitmapData : bitm.BitmapData
        {
            public BitmapData(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Class = Reader.ReadString(4);
                Width = Reader.ReadInt16();
                Height = Reader.ReadInt16();
                Depth = Reader.ReadByte();
                Flags = new Bitmask(Reader.ReadByte());
                Type = (TextureType)Reader.ReadInt16();
                Format = (TextureFormat)Reader.ReadInt16();
                MoreFlags = new Bitmask(Reader.ReadInt16());
                RegX = Reader.ReadInt16();
                RegY = Reader.ReadInt16();
                MipmapCount = Reader.ReadByte();
                Reader.ReadByte();
                InterleavedIndex = Reader.ReadByte();
                Index2 = Reader.ReadByte();
                PixelsOffset = Reader.ReadInt32();
                PixelsSize = Reader.ReadInt32();

                Reader.SeekTo(Address + 48);
            }
        }

        new public class RawChunkA : bitm.RawChunkA
        {
            public RawChunkA(CacheFile Cache, int Address)
            {
                Cache.Reader.SeekTo(Address);
                RawID = Cache.Reader.ReadInt32();
                Cache.Reader.ReadInt32();
            }
        }

        new public class RawChunkB : bitm.RawChunkB
        {
            public RawChunkB(CacheFile Cache, int Address)
            {
                Cache.Reader.SeekTo(Address);
                RawID = Cache.Reader.ReadInt32();
                Cache.Reader.ReadInt32();
            }
        }
    }
}

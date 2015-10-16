using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using bitm = Adjutant.Library.Definitions.bitmap;

namespace Adjutant.Library.Definitions.Halo3Beta
{
    public class bitmap : bitm
    {
        protected bitmap() { }

        public bitmap(CacheBase Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            #region Sequence Chunk
            Reader.SeekTo(Address + 84);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Sequences.Add(new Sequence(Cache, iOffset + 64 * i));
            #endregion

            #region BitmapData Chunk
            Reader.SeekTo(Address + 96);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Bitmaps.Add(new BitmapData(Cache, iOffset + 48 * i));
            #endregion

            #region Raw Chunk A
            Reader.SeekTo(Address + 140);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                RawChunkAs.Add(new RawChunkA(Cache, iOffset + 8 * i));
            #endregion

            #region Raw Chunk B
            Reader.SeekTo(Address + 152);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                RawChunkBs.Add(new RawChunkB(Cache, iOffset + 8 * i));
            Reader.SeekTo(Address + 164);
            #endregion
        }

        new public class Sequence : bitm.Sequence
        {
            public Sequence(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Name = Reader.ReadNullTerminatedString(32);
                FirstSubmapIndex = Reader.ReadInt16();
                BitmapCount = Reader.ReadInt16();

                #region Sprite Chunk
                Reader.SeekTo(Address + 52);
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                for (int i = 0; i < iCount; i++)
                    Sprites.Add(new Sprite(Cache, iOffset + 32 * i));
                Reader.SeekTo(Address + 64);
                #endregion
            }

            new public class Sprite : bitm.Sequence.Sprite
            {
                public Sprite(CacheBase Cache, int Address)
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
            public BitmapData(CacheBase Cache, int Address)
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
            }
        }

        new public class RawChunkA : bitm.RawChunkA
        {
            public RawChunkA(CacheBase Cache, int Address)
            {
                Cache.Reader.SeekTo(Address);
                RawID = Cache.Reader.ReadInt32();
                Cache.Reader.ReadInt32();
            }
        }

        new public class RawChunkB : bitm.RawChunkB
        {
            public RawChunkB(CacheBase Cache, int Address)
            {
                Cache.Reader.SeekTo(Address);
                RawID = Cache.Reader.ReadInt32();
                Cache.Reader.ReadInt32();
            }
        }
    }
}

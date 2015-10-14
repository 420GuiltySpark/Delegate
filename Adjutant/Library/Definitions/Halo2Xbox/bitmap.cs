using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using bitm = Adjutant.Library.Definitions.bitmap;

namespace Adjutant.Library.Definitions.Halo2Xbox
{
    public class bitmap : bitm
    {
        public bitmap(CacheBase Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            #region Sequence Chunk
            Reader.SeekTo(Address + 60);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Sequences.Add(new Sequence(Cache, iOffset + 60 * i));
            #endregion

            #region BitmapData Chunk
            Reader.SeekTo(Address + 68);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Bitmaps.Add(new BitmapData(Cache, iOffset + 116 * i));
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
            public int[] LODOffset;
            public int[] LODSize;

            public BitmapData(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Class = Reader.ReadString(4);
                Width = Reader.ReadUInt16();
                Height = Reader.ReadUInt16();
                Depth = Reader.ReadUInt16();
                //Flags = new Bitmask(Reader.ReadByte());
                Type = (TextureType)Reader.ReadUInt16();
                Format = (TextureFormat)Reader.ReadUInt16();
                Flags = new Bitmask(Reader.ReadUInt16());
                RegX = Reader.ReadUInt16();
                RegY = Reader.ReadUInt16();
                MipmapCount = Reader.ReadUInt16();
                Reader.ReadUInt16();
                Reader.ReadInt32();

                LODOffset = new int[3];
                LODSize = new int[3];

                LODOffset[0] = Reader.ReadInt32();
                LODOffset[1] = Reader.ReadInt32();
                LODOffset[2] = Reader.ReadInt32();
                Reader.ReadInt32();
                Reader.ReadInt32();
                Reader.ReadInt32();
                LODSize[0] = Reader.ReadInt32();
                LODSize[1] = Reader.ReadInt32();
                LODSize[2] = Reader.ReadInt32();

                PixelsOffset = LODOffset[0];
                PixelsSize = LODSize[0];

                Reader.SeekTo(Address + 116);
            }

            public override int VirtualWidth
            {
                get { return Width; }
            }

            public override int VirtualHeight
            {
                get { return Height; }
            }
        }
    }
}

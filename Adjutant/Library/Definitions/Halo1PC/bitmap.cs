using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using bitm = Adjutant.Library.Definitions.bitmap;

namespace Adjutant.Library.Definitions.Halo1PC
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

                Reader.SeekTo(Address + 52);

                #region Sprite Chunk
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

                    SubmapIndex = Reader.ReadInt16();
                    Reader.ReadInt16();
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
                Width = Reader.ReadUInt16();
                Height = Reader.ReadUInt16();
                Depth = Reader.ReadUInt16();
                //Flags = new Bitmask(Reader.ReadByte());
                Type = (TextureType)Reader.ReadUInt16();
                Format = (TextureFormat)Reader.ReadUInt16();
                Flags = new Bitmask(Reader.ReadUInt16());
                RegX = Reader.ReadUInt16();
                RegY = Reader.ReadUInt16();
                MipmapCount = Reader.ReadInt32();
                PixelsOffset = Reader.ReadInt32() | int.MinValue; //add 'extenal' flag: all PC bitmaps are external
                PixelsSize = Reader.ReadInt32();

                Reader.SeekTo(Address + 48);
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

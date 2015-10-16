using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using bitm = Adjutant.Library.Definitions.bitmap;

namespace Adjutant.Library.Definitions.ReachBeta
{
    public class bitmap : Halo3Beta.bitmap
    {
        protected bitmap() { }

        public bitmap(CacheBase Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            #region Sequence Chunk
            Reader.SeekTo(Address + 96);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Sequences.Add(new Sequence(Cache, iOffset + 64 * i));
            #endregion

            #region BitmapData Chunk
            Reader.SeekTo(Address + 108);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Bitmaps.Add(new BitmapData(Cache, iOffset + 48 * i));
            #endregion

            #region Raw Chunk A
            Reader.SeekTo(Address + 152);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                RawChunkAs.Add(new RawChunkA(Cache, iOffset + 8 * i));
            #endregion

            #region Raw Chunk B
            Reader.SeekTo(Address + 164);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                RawChunkBs.Add(new RawChunkB(Cache, iOffset + 8 * i));
            #endregion
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
                Type = (TextureType)Reader.ReadByte();
                Reader.ReadByte(); //dunno what this is
                Format = (TextureFormat)Reader.ReadInt16();

                if ((int)Format > 31) //change to match defined format list
                    Format -= 5;

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
    }
}

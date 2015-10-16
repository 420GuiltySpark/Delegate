using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using bitm = Adjutant.Library.Definitions.bitmap;

namespace Adjutant.Library.Definitions.Halo1CE
{
    public class bitmap : Halo1PC.bitmap
    {
        protected bitmap() { }

        public bitmap(CacheBase Cache, int Address)
        {
            int magic = Cache.Magic;
            EndianReader Reader;

            if (Address < 0) //external bitmap
            {
                var fs = new FileStream(Cache.FilePath + "\\bitmaps.map", FileMode.Open, FileAccess.Read);
                Reader = new EndianReader(fs, EndianFormat.LittleEndian);

                Reader.SeekTo(8);
                var indexOffset = Reader.ReadInt32();
                var index = Address + Cache.Magic;

                Reader.SeekTo(indexOffset + 12 * index + 8);
                Address = Reader.ReadInt32();
                magic = -Address;
            }
            else Reader = Cache.Reader;

            #region BitmapData Chunk
            Reader.SeekTo(Address + 96);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - magic;
            for (int i = 0; i < iCount; i++)
                Bitmaps.Add(new BitmapData(Reader, iOffset + 48 * i));
            #endregion

            if (Reader != Cache.Reader)
            {
                Reader.Close();
                Reader.Dispose();

                //add flag so the GetRawFromID function
                //can tell that it's an external bitmap
                foreach (var bData in Bitmaps)
                    bData.PixelsOffset |= int.MinValue; //0x80000000
            }
        }

        new public class BitmapData : bitm.BitmapData
        {
            public BitmapData(EndianReader Reader, int Address)
            {
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
                PixelsOffset = Reader.ReadInt32();
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

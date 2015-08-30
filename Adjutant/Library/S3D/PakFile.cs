using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Adjutant.Library.Endian;

namespace Adjutant.Library.S3D
{
    public class PakFile
    {
        #region Declarations
        public string Filename;
        public string FilePath
        {
            get { return Directory.GetParent(Filename).FullName; }
        }
        public EndianReader Reader;
        public PakTable PakItems;
        #endregion

        public PakFile(string Filename)
        {
            this.Filename = Filename;

            FileStream fs = new FileStream(Filename, FileMode.Open, FileAccess.Read);
            Reader = new EndianReader((Stream)fs, EndianFormat.LittleEndian);

            PakItems = new PakTable(this);
        }

        #region Classes
        public class PakTag
        {
            public int Offset;
            public int Size;
            public string Name;
            public TagType Class;
            public int unk1, unk2;

            public override string ToString()
            {
                return Name;
            }
        }

        public class PakTable : List<PakTag>
        {
            public PakTable(PakFile Pak)
            {
                var reader = Pak.Reader;

                reader.SeekTo(0);
                var fCount = reader.ReadInt32();

                for (int i = 0; i < fCount; i++)
                {
                    var item = new PakTag();

                    item.Offset = reader.ReadInt32();
                    item.Size = reader.ReadInt32();
                    var len = reader.ReadInt32();
                    item.Name = reader.ReadString(len);
                    item.Class = (TagType)reader.ReadInt32();
                    item.unk1 = reader.ReadInt32();
                    item.unk2 = reader.ReadInt32();

                    this.Add(item);
                }
            }
        }
        #endregion

        #region Methods
        public PakTag GetItemByName(string Name)
        {
            foreach (var item in PakItems)
                if (item.Name == Name) return item;

            return null;
        }

        public void Close()
        {
            Reader.Close();
            Reader.Dispose();
            PakItems.Clear();
        }
        #endregion
    }
}

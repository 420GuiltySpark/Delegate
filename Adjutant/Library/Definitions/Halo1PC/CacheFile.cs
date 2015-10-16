using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using CacheBase = Adjutant.Library.Definitions.CacheBase;

namespace Adjutant.Library.Definitions.Halo1PC
{
    public class CacheFile : CacheBase
    {
        public CacheFile(string Filename, string Build)
            : base(Filename, Build)
        {
            Reader.EndianType = EndianFormat.LittleEndian;
            Version = DefinitionSet.Halo1PC;

            Header = new CacheHeader(this);
            IndexHeader = new CacheIndexHeader(this);
            IndexItems = new IndexTable(this);
            Strings = new StringTable(this);

            LocaleTables = new List<LocaleTable>();
        }

        new public class CacheHeader : CacheBase.CacheHeader
        {
            public CacheHeader(CacheBase Cache)
            {
                cache = Cache;
                EndianReader Reader = cache.Reader;

                Reader.SeekTo(8);
                fileSize = Reader.ReadInt32();
                Reader.ReadInt32();
                indexOffset = Reader.ReadInt32();
            }
        }

        new public class CacheIndexHeader : CacheBase.CacheIndexHeader
        {
            public int vertDataCount;
            public int vertDataOffset;
            public int indexDataCount;
            public int indexDataOffset;

            public CacheIndexHeader(CacheBase Cache)
            {
                cache = Cache;
                var Reader = cache.Reader;
                var CH = cache.Header;

                Reader.SeekTo(CH.indexOffset);
                var c = Reader.ReadInt32();
                cache.Magic = CH.Magic = c - (CH.indexOffset + 40);

                Reader.Skip(8);
                tagCount = Reader.ReadInt32();

                vertDataCount = Reader.ReadInt32();
                vertDataOffset = Reader.ReadInt32();
                indexDataCount = Reader.ReadInt32();
                indexDataOffset = Reader.ReadInt32();

                tagInfoOffset = CH.indexOffset + 40;

                CH.fileTableOffset = tagInfoOffset + tagCount * 32;            
            }
        }

        new public class IndexTable : CacheBase.IndexTable
        {
            public IndexTable(CacheBase Cache)
            {
                cache = Cache;

                var IH = cache.IndexHeader;
                var CH = cache.Header;
                var Reader = cache.Reader;

                #region Read Tags' Info
                ClassList = new List<TagClass>();
                var classDic = new Dictionary<string, int>();
                int[] sbspOffset = new int[0];
                int[] sbspMagic = new int[0];
                int[] sbspID = new int[0];             
                int[] indices = new int[IH.tagCount];

                Reader.SeekTo(IH.tagInfoOffset);
                for (int i = 0; i < IH.tagCount; i++)
                {
                    var cname = Reader.ReadString(4);
                    if (Reader.EndianType == EndianFormat.LittleEndian)
                    {
                        var tname = cname.ToCharArray();
                        Array.Reverse(tname);
                        cname = new string(tname);
                    }

                    int index;
                    if (!classDic.TryGetValue(cname, out index))
                    {
                        index = classDic.Count;
                        classDic.Add(cname, classDic.Count);
                    }

                    Reader.Skip(8); //parent classes

                    IndexItem item = new IndexItem() { Cache = cache };
                    item.ClassIndex = index;
                    item.ID = Reader.ReadInt32();
                    indices[i] = Reader.ReadInt32() - CH.Magic - CH.fileTableOffset;
                    item.Offset = Reader.ReadInt32() - CH.Magic;
                    this.Add(item);

                    Reader.Skip(8); //empty?

                    if (cname == "scnr")
                    {
                        long tempOffset = Reader.Position;

                        Reader.SeekTo(item.Offset + 1444);
                        int jCount = Reader.ReadInt32();
                        int jOffset = Reader.ReadInt32() - cache.Magic;

                        sbspOffset = new int[jCount];
                        sbspMagic = new int[jCount];
                        sbspID = new int[jCount];

                        for (int j = 0; j < jCount; j++)
                        {
                            Reader.SeekTo(jOffset + j * 32);
                            sbspOffset[j] = Reader.ReadInt32();
                            Reader.ReadInt32();
                            sbspMagic[j] = Reader.ReadInt32() - sbspOffset[j];
                            Reader.SeekTo(jOffset + j * 32 + 28);
                            sbspID[j] = Reader.ReadInt32();
                        }
                        Reader.SeekTo(tempOffset);
                    }
                }

                for (int i = 0; i < sbspID.Length; i++)
                {
                    var tag = GetItemByID(sbspID[i]);
                    tag.Offset = sbspOffset[i];
                    tag.Magic = sbspMagic[i];
                }

                CH.fileTableSize = this[0].Offset - CH.fileTableOffset;
                foreach (var pair in classDic)
                    ClassList.Add(new TagClass() { ClassCode = pair.Key });
                #endregion

                #region Read Names
                Reader.StreamOrigin = CH.fileTableOffset;

                for (int i = 0; i < indices.Length; i++)
                {
                    if (indices[i] == -1)
                    {
                        this[i].Filename = "<null>";
                        continue;
                    }

                    Reader.SeekTo(indices[i]);

                    int length;
                    if (i == indices.Length - 1)
                        length = CH.fileTableSize - indices[i];
                    else
                    {
                        if (indices[i + 1] == -1)
                        {
                            int index = -1;

                            for (int j = i + 1; j < indices.Length; j++)
                            {
                                if (indices[j] != -1)
                                {
                                    index = j;
                                    break;
                                }
                            }

                            length = (index == -1) ? CH.fileTableSize - indices[i] : indices[index] - indices[i];
                        }
                        else
                            length = indices[i + 1] - indices[i];
                    }

                    if (length <= 1)
                    {
                        this[i].Filename = this[i].Offset.ToString("X8");
                        continue;
                    }

                    this[i].Filename = Reader.ReadNullTerminatedString(length);
                }

                Reader.StreamOrigin = 0;
                #endregion
            }
        }

        public override byte[] GetRawFromID(int ID, int DataLength)
        {
            EndianReader er;
            string fName = "";

            bool flag = ((ID & 0x80000000) >> 31 == 1);
            int offset = ID & 0x7FFFFFFF;

            if (flag)
            {
                fName = FilePath + "\\bitmaps.map";
                FileStream fs = new FileStream(fName, FileMode.Open, FileAccess.Read);
                er = new EndianReader(fs, EndianFormat.LittleEndian);
            }
            else er = Reader;

            er.SeekTo(offset);

            var data = er.ReadBytes(DataLength);

            if (er != Reader)
            {
                er.Close();
                er.Dispose();
            }

            return data;
        }
    }
}

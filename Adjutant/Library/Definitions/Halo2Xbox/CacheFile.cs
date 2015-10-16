using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Adjutant.Library.Endian;
using Adjutant.Library.Definitions;

namespace Adjutant.Library.Definitions.Halo2Xbox
{
    public class CacheFile : CacheBase
    {
        public CacheFile(string Filename, string Build)
            : base(Filename, Build)
        {
            Reader.EndianType = EndianFormat.LittleEndian;
            Version = DefinitionSet.Halo2Xbox;

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

                #region Read Values
                XmlNode headerNode = cache.versionNode.ChildNodes[0];
                XmlAttribute attr = headerNode.Attributes["fileSize"];
                int offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                fileSize = Reader.ReadInt32();

                attr = headerNode.Attributes["indexOffset"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                indexOffset = Reader.ReadInt32();

                attr = headerNode.Attributes["tagDataAddress"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                tagDataAddress = Reader.ReadInt32();

                attr = headerNode.Attributes["stringCount"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                stringCount = Reader.ReadInt32();

                attr = headerNode.Attributes["stringTableSize"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                stringTableSize = Reader.ReadInt32();

                attr = headerNode.Attributes["stringTableIndexOffset"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                stringTableIndexOffset = Reader.ReadInt32();

                attr = headerNode.Attributes["stringTableOffset"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                stringTableOffset = Reader.ReadInt32();

                attr = headerNode.Attributes["scenarioName"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                scenarioName = Reader.ReadString(256);

                attr = headerNode.Attributes["fileCount"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                fileCount = Reader.ReadInt32();

                attr = headerNode.Attributes["fileTableOffset"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                fileTableOffset = Reader.ReadInt32();

                attr = headerNode.Attributes["fileTableSize"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                fileTableSize = Reader.ReadInt32();

                attr = headerNode.Attributes["fileTableIndexOffset"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                fileTableIndexOffset = Reader.ReadInt32();

                #endregion
            }
        }

        new public class CacheIndexHeader : CacheBase.CacheIndexHeader
        {
            public CacheIndexHeader(CacheBase Cache)
            {
                cache = Cache;
                var Reader = cache.Reader;

                #region Read Values
                XmlNode indexHeaderNode = cache.versionNode.ChildNodes[1];

                XmlAttribute attr = indexHeaderNode.Attributes["tagClassCount"];
                int offset = int.Parse(attr.Value);
                Reader.SeekTo(offset + cache.Header.indexOffset);
                tagClassCount = Reader.ReadInt32();

                attr = indexHeaderNode.Attributes["tagInfoOffset"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset + cache.Header.indexOffset);
                tagInfoOffset = Reader.ReadInt32() - cache.Header.Magic;

                Reader.SeekTo(cache.Header.indexOffset);
                cache.Header.Magic = Reader.ReadInt32() - (cache.Header.indexOffset + 32);

                tagClassCount = Reader.ReadInt32();
                tagInfoOffset = Reader.ReadInt32() - cache.Header.Magic;

                Reader.SeekTo(tagInfoOffset + 8);
                cache.Magic = Reader.ReadInt32() - (cache.Header.indexOffset + cache.Header.tagDataAddress);

                Reader.SeekTo(cache.Header.indexOffset + 24);
                tagCount = Reader.ReadInt32();
                #endregion
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

                ClassList = new List<TagClass>();

                #region Read Tags' Info
                var classDic = new Dictionary<string, int>();
                int[] sbspOffset = new int[0];
                int[] sbspMagic = new int[0];
                int[] sbspID = new int[0];

                Reader.SeekTo(IH.tagInfoOffset);
                for (int i = 0; i < IH.tagCount; i++)
                {
                    var cname = Reader.ReadString(4);
                    var tname = cname.ToCharArray();
                    Array.Reverse(tname);
                    cname = new string(tname);

                    int index;
                    if (!classDic.TryGetValue(cname, out index))
                    {
                        index = classDic.Count;
                        classDic.Add(cname, classDic.Count);
                    }

                    IndexItem item = new IndexItem() { Cache = cache };
                    item.ClassIndex = index;
                    item.ID = Reader.ReadInt32();
                    item.Offset = Reader.ReadInt32() - cache.Magic;
                    Reader.ReadInt32(); //meta size
                    this.Add(item);

                    if (cname == "scnr")
                    {
                        long tempOffset = Reader.Position;

                        Reader.SeekTo(item.Offset + 528);
                        int jCount = Reader.ReadInt32();
                        int jOffset = Reader.ReadInt32() - cache.Magic;

                        sbspOffset = new int[jCount];
                        sbspMagic = new int[jCount];
                        sbspID = new int[jCount];

                        for (int j = 0; j < jCount; j++)
                        {
                            Reader.SeekTo(jOffset + j * 68);
                            sbspOffset[j] = Reader.ReadInt32();
                            Reader.ReadInt32();
                            sbspMagic[j] = Reader.ReadInt32() - sbspOffset[j];
                            Reader.SeekTo(jOffset + j * 68 + 20);
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

                foreach (var pair in classDic)
                    ClassList.Add(new TagClass() { ClassCode = pair.Key });
                #endregion

                #region Read Indices
                Reader.SeekTo(CH.fileTableIndexOffset);
                int[] indices = new int[IH.tagCount];
                for (int i = 0; i < IH.tagCount; i++)
                    indices[i] = Reader.ReadInt32();
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

                    if (length == 1)
                    {
                        this[i].Filename = "<blank>";
                        continue;
                    }

                    this[i].Filename = Reader.ReadString(length);
                }

                Reader.StreamOrigin = 0;
                #endregion
            }
        }

        public override byte[] GetRawFromID(int ID, int DataLength)
        {
            EndianReader er;
            string fName = "";

            long cIndex = (ID & 0xC0000000) >> 30;
            int offset = ID & 0x3FFFFFFF;

            if (cIndex != 0)
            {
                switch (cIndex)
                {
                    case 1:
                        fName = FilePath + "\\mainmenu.map";
                        break;
                    case 2:
                        fName = FilePath + "\\shared.map";
                        break;
                    case 3:
                        fName = FilePath + "\\single_player_shared.map";
                        break;
                }
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

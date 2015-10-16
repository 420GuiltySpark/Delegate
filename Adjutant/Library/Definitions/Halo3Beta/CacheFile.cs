using System.Collections.Generic;
using System.IO;
using System.Xml;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;

namespace Adjutant.Library.Definitions.Halo3Beta
{
    public class CacheFile : CacheBase
    {
        public CacheFile(string Filename, string Build)
            : base(Filename, Build)
        {
            Version = DefinitionSet.Halo3Beta;

            Header = new CacheHeader(this);
            IndexHeader = new CacheIndexHeader(this);
            IndexItems = new IndexTable(this);
            Strings = new StringTable(this);

            LocaleTables = new List<LocaleTable>();
            try
            {
                for (int i = 0; i < int.Parse(buildNode.Attributes["languageCount"].Value); i++)
                    LocaleTables.Add(new LocaleTable(this, (Language)i));
            }
            catch { LocaleTables.Clear(); }
        }

        new public class CacheHeader : CacheBase.CacheHeader
        {
            public CacheHeader(CacheBase Cache)
            {
                cache = Cache;
                var Reader = cache.Reader;

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

                attr = headerNode.Attributes["virtualBaseAddress"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                virtualBaseAddress = Reader.ReadInt32();
                #endregion

                indexOffset = indexOffset - virtualBaseAddress + tagDataAddress;
                cache.Magic = virtualBaseAddress - tagDataAddress;
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
                tagInfoOffset = Reader.ReadInt32() - cache.Magic;

                attr = indexHeaderNode.Attributes["tagClassIndexOffset"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset + cache.Header.indexOffset);
                tagClassIndexOffset = Reader.ReadInt32() - cache.Magic;

                attr = indexHeaderNode.Attributes["tagCount"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset + cache.Header.indexOffset);
                tagCount = Reader.ReadInt32();

                attr = indexHeaderNode.Attributes["tagInfoHeaderCount"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset + cache.Header.indexOffset);
                tagInfoHeaderCount = Reader.ReadInt32();

                attr = indexHeaderNode.Attributes["tagInfoHeaderOffset"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset + cache.Header.indexOffset);
                tagInfoHeaderOffset = Reader.ReadInt32() - cache.Magic;

                attr = indexHeaderNode.Attributes["tagInfoHeaderCount2"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset + cache.Header.indexOffset);
                tagInfoHeaderCount2 = Reader.ReadInt32();

                attr = indexHeaderNode.Attributes["tagInfoHeaderOffset2"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset + cache.Header.indexOffset);
                tagInfoHeaderOffset2 = Reader.ReadInt32() - cache.Magic;
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

                #region Read Class List

                Reader.SeekTo(IH.tagClassIndexOffset);
                for (int i = 0; i < IH.tagClassCount; i++)
                {
                    TagClass tc = new TagClass();
                    tc.ClassCode = Reader.ReadString(4);
                    tc.Parent = Reader.ReadString(4);
                    tc.Parent2 = Reader.ReadString(4);
                    tc.StringID = Reader.ReadInt32();
                    ClassList.Add(tc);
                }
                #endregion

                #region Read Tags' Info
                Reader.SeekTo(IH.tagInfoOffset);
                for (int i = 0; i < IH.tagCount; i++)
                {
                    IndexItem item = new IndexItem() { Cache = cache };
                    item.ClassIndex = Reader.ReadInt16();
                    item.ID = (Reader.ReadInt16() << 16) | i;
                    item.Offset = Reader.ReadInt32() - cache.Magic;
                    item.metaIndex = i;
                    this.Add(item);
                }
                #endregion

                #region Read Indices
                Reader.SeekTo(CH.fileTableIndexOffset);
                int[] indices = new int[IH.tagCount];
                for (int i = 0; i < IH.tagCount; i++)
                    indices[i] = Reader.ReadInt32();
                #endregion

                #region Read Names
                Reader.SeekTo(CH.fileTableOffset);
                EndianReader newReader = (cache.tagsKey == "" || cache.tagsKey == null)
                    ? new EndianReader(new MemoryStream(Reader.ReadBytes(CH.fileTableSize)), EndianFormat.BigEndian)
                    : AES.DecryptSegment(Reader, CH.fileTableOffset, CH.fileTableSize, cache.tagsKey);

                for (int i = 0; i < indices.Length; i++)
                {
                    if (indices[i] == -1)
                    {
                        this[i].Filename = "<null>";
                        continue;
                    }

                    newReader.SeekTo(indices[i]);

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

                    if (length < 0)
                    {
                        int i0 = indices[i];
                        int i1 = indices[i + 1];
                        int i2 = indices[i + 2];
                        int i3 = indices[i + 3];
                    }

                    this[i].Filename = newReader.ReadString(length);
                }

                newReader.Close();
                newReader.Dispose();
                #endregion
            }
        }

        public override byte[] GetRawFromID(int ID, int DataLength)
        {
            EndianReader er;
            string fName = "";

            var Entry = (Halo3Beta.cache_file_resource_gestalt.RawEntry)zone.RawEntries[ID & ushort.MaxValue];

            var offset = (Entry.OptionalOffset > 0) ? Entry.OptionalOffset : Entry.RequiredOffset;
            var size = (Entry.OptionalSize > 0) ? Entry.OptionalSize : Entry.RequiredSize;

            if (DataLength > size)
                size = DataLength;

            if (Entry.CacheIndex != -1)
            {
                fName = FilePath + "\\shared.map";
                FileStream fs = new FileStream(fName, FileMode.Open, FileAccess.Read);
                er = new EndianReader(fs, EndianFormat.BigEndian);
            }
            else
                er = Reader;

            er.SeekTo(offset);
            var data = er.ReadBytes(size);

            if (er != Reader)
            {
                er.Close();
                er.Dispose();
            }

            return data;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Endian;
using Adjutant.Library.Definitions;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Runtime.InteropServices;
using Composer;
using Composer.Wwise;

namespace Adjutant.Library.Cache
{
    public partial class CacheFile
    {
        #region Declarations
        public string Filename;
        public string FilePath
        {
            get { return Directory.GetParent(Filename).FullName; }
        }
        public EndianReader Reader;
        public CacheHeader Header;
        public CacheIndexHeader IndexHeader;
        public IndexTable IndexItems;
        public StringTable Strings;
        public List<LocaleTable> LocaleTables;
        public DefinitionSet Version;

        public string Build;
        public int HeaderSize;
        public string PluginDir;

        public int Magic;

        public cache_file_resource_gestalt zone;
        public cache_file_resource_layout_table play;
        public sound_cache_file_gestalt ugh_;

        public List<SoundPackInfo> H4SoundPacks;
        public Dictionary<uint, List<SoundFileInfo>> H4SoundFiles;

        private string localesKey, stringsKey, tagsKey, networkKey;
        private string stringMods;

        internal string version;
        internal XmlNode buildNode;
        internal XmlNode versionNode;
        internal XmlNode vertexNode;
        #endregion

        public CacheFile(string Filename)
        {
            this.Filename = Filename;
            FileStream fs = new FileStream(Filename, FileMode.Open, FileAccess.Read);
            Reader = new EndianReader((Stream)fs, EndianFormat.BigEndian);

            Reader.SeekTo(284);
            Build = Reader.ReadString(32);

            VerifyXML();

            #region Read Build Info
            version = buildNode.Attributes["version"].Value;
            HeaderSize = int.Parse(buildNode.Attributes["headerSize"].Value);
            stringMods = buildNode.Attributes["stringMods"].Value;
                
            tagsKey = buildNode.Attributes["tagsKey"].Value;
            stringsKey = buildNode.Attributes["stringsKey"].Value;
            localesKey = buildNode.Attributes["localesKey"].Value;
            networkKey = buildNode.Attributes["networkKey"].Value;

            switch (buildNode.Attributes["definitions"].Value)
            {
                case "Halo3Beta":
                    Version = DefinitionSet.Halo3Beta;
                    break;

                case "Halo3Retail":
                    Version = DefinitionSet.Halo3Retail;
                    break;

                case "Halo3ODST":
                    Version = DefinitionSet.Halo3ODST;
                    break;

                case "ReachBeta":
                    Version = DefinitionSet.HaloReachBeta;
                    break;

                case "ReachRetail":
                    Version = DefinitionSet.HaloReachRetail;
                    break;

                case "Halo4Beta":
                    Version = DefinitionSet.Halo4Beta;
                    break;

                case "Halo4Retail":
                    Version = DefinitionSet.Halo4Retail;
                    break;

                default:
                    Version = DefinitionSet.Unknown;
                    break;
            }
            #endregion

            #region Find Vertex Definitions
            var xml = new MemoryStream(Adjutant.Properties.Resources.VertexBuffer);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xml);
            XmlElement element = xmlDoc.DocumentElement;


            foreach (XmlNode node in element.ChildNodes)
            {
                if (node.Attributes["Game"].Value == buildNode.Attributes["vertDef"].Value)
                {
                    vertexNode = node;
                    break;
                }
            }

            xml.Close();
            xml.Dispose();
            #endregion

            Header = new CacheHeader(this);
            IndexHeader = new CacheIndexHeader(this);
            IndexItems = new IndexTable(this);
            Strings = new StringTable(this);

            LocaleTables = new List<LocaleTable>();
            for (int i = 0; i < int.Parse(buildNode.Attributes["languageCount"].Value); i++)
                LocaleTables.Add(new LocaleTable(this, (Language)i));
           
            LoadPlayZone();
        }

        #region Classes
        public class CacheHeader
        {
            #region Declarations
            public int Magic;

            public int fileSize;
            public int indexOffset;
            public short CacheSizeType;
            public short CacheType;

            public int stringCount;
            public int stringTableSize;
            public int stringTableIndexOffset;
            public int stringTableOffset;

            public string scenarioName;

            public int fileCount;
            public int fileTableOffset;
            public int fileTableSize;
            public int fileTableIndexOffset;

            public int virtualBaseAddress;
            public int rawTableOffset;
            public int localeModifier;
            public int rawTableSize;
            public int tagDataAddress;

            private CacheFile cache;
            #endregion

            public CacheHeader(CacheFile Cache)
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

                attr = headerNode.Attributes["MapSizeType"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                CacheSizeType = Reader.ReadInt16();

                attr = headerNode.Attributes["MapType"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset);
                CacheType = Reader.ReadInt16();

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

                if (cache.Version >= DefinitionSet.Halo3Retail)
                {
                    attr = headerNode.Attributes["rawTableOffset"];
                    offset = int.Parse(attr.Value);
                    Reader.SeekTo(offset);
                    rawTableOffset = Reader.ReadInt32();

                    attr = headerNode.Attributes["localeModifier"];
                    offset = int.Parse(attr.Value);
                    Reader.SeekTo(offset);
                    localeModifier = Reader.ReadInt32();

                    attr = headerNode.Attributes["rawTableSize"];
                    offset = int.Parse(attr.Value);
                    Reader.SeekTo(offset);
                    rawTableSize = Reader.ReadInt32();
                }
                else
                {
                    rawTableOffset = 0;
                    localeModifier = 0;
                    rawTableSize = 0;
                }
                if (cache.Version > DefinitionSet.HaloReachRetail)
                {
                    attr = headerNode.Attributes["tagDataAddress"];
                    offset = int.Parse(attr.Value);
                    Reader.SeekTo(offset);
                    tagDataAddress = Reader.ReadInt32();
                }
                #endregion

                #region Modify Offsets
                if (cache.Version == DefinitionSet.Halo3Beta)
                {
                    Reader.SeekTo(16);
                    var constant = Reader.ReadInt32();
                    var metaStart = Reader.ReadInt32();

                    indexOffset = constant - virtualBaseAddress + metaStart;
                    cache.Magic = constant - indexOffset;
                }
                else if (cache.Version >= DefinitionSet.Halo3Retail)
                {
                    if (rawTableOffset == 0)
                    {
                        cache.Magic = virtualBaseAddress - tagDataAddress;
                    }
                    else
                    {

                        this.Magic = stringTableIndexOffset - cache.HeaderSize;

                        fileTableOffset -= this.Magic;
                        fileTableIndexOffset -= this.Magic;
                        stringTableIndexOffset -= this.Magic;
                        stringTableOffset -= this.Magic;

                        cache.Magic = virtualBaseAddress - (rawTableOffset + rawTableSize);
                    }
                    indexOffset -= cache.Magic;
                }
                else throw new InvalidDataException("Invalid cache version");
                #endregion
            }
        }

        public class CacheIndexHeader
        {
            #region Declarations
            public int tagClassCount;
            public int tagClassIndexOffset;

            public int tagCount;

            public int tagInfoOffset;
            public int tagInfoHeaderCount;
            public int tagInfoHeaderOffset;
            public int tagInfoHeaderCount2;
            public int tagInfoHeaderOffset2;

            private CacheFile cache;
            #endregion

            public CacheIndexHeader(CacheFile Cache)
            {
                cache = Cache;
                EndianReader Reader = cache.Reader;

                #region Read Values
                XmlNode indexHeaderNode = cache.versionNode.ChildNodes[1];

                XmlAttribute attr = indexHeaderNode.Attributes["tagClassCount"];
                int offset = int.Parse(attr.Value);
                Reader.SeekTo(offset + cache.Header.indexOffset);
                tagClassCount = Reader.ReadInt32();

                attr = indexHeaderNode.Attributes["tagClassIndexOffset"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset + cache.Header.indexOffset);
                tagClassIndexOffset = Reader.ReadInt32() - cache.Magic;

                attr = indexHeaderNode.Attributes["tagCount"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset + cache.Header.indexOffset);
                tagCount = Reader.ReadInt32();

                attr = indexHeaderNode.Attributes["tagInfoOffset"];
                offset = int.Parse(attr.Value);
                Reader.SeekTo(offset + cache.Header.indexOffset);
                tagInfoOffset = Reader.ReadInt32() - cache.Magic;

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

        public class StringTable : List<string>
        {
            private CacheFile cache;

            public StringTable(CacheFile Cache)
            {
                cache = Cache;
                EndianReader Reader = cache.Reader;
                CacheHeader CH = cache.Header;

                #region Read Indices
                Reader.SeekTo(CH.stringTableIndexOffset);
                int[] indices = new int[CH.stringCount];
                for (int i = 0; i < CH.stringCount; i++)
                {
                    indices[i] = Reader.ReadInt32();
                    this.Add("");
                }
                #endregion

                #region Read Names
                Reader.SeekTo(CH.stringTableOffset);
                EndianReader newReader = (cache.stringsKey == "")
                    ? new EndianReader(new MemoryStream(Reader.ReadBytes(CH.stringTableSize)), EndianFormat.BigEndian)
                    : AES.DecryptSegment(Reader, CH.stringTableOffset, CH.stringTableSize, cache.stringsKey);

                for (int i = 0; i < indices.Length; i++)
                {
                    if (indices[i] == -1)
                    {
                        this[i] = "<null>";
                        continue;
                    }

                    newReader.SeekTo(indices[i]);

                    int length;
                    if (i == indices.Length - 1)
                        length = CH.stringTableSize - indices[i];
                    else
                        length = (indices[i + 1] != -1)
                            ? indices[i + 1] - indices[i]
                            : indices[i + 2] - indices[i];

                    if (length == 1)
                    {
                        this[i] = "<blank>";
                        continue;
                    }

                    this[i] = newReader.ReadString(length);
                }
                newReader.Close();
                newReader.Dispose();
                #endregion
            }

            public string GetItemByID(int ID)
            {
                //go through the modifiers, if the ID matches a modifer return the correct string
                string[] mods = cache.stringMods.Split(';');
                try
                {
                    foreach (string mod in mods)
                    {
                        string[] Params = mod.Split(','); //[0] - check, [1] - change
                        int check = int.Parse(Params[0]);
                        int change = int.Parse(Params[1]);

                        if (check < 0)
                        {
                            if (ID < check)
                            {
                                ID += change;
                                return this[ID];
                            }
                        }
                        else
                        {
                            if (ID > check)
                            {
                                ID += change;
                                return this[ID];
                            }
                        }
                    }
                }
                catch
                {
                    return "invalid";
                }

                //if no matching modifier, return the string at index of ID, or null if out of bounds
                try { return this[ID]; }
                catch { return ""; }
            }
        }

        public class LocaleTable : List<string>
        {
            private CacheFile cache;

            public LocaleTable(CacheFile Cache, Language Lang)
            {
                cache = Cache;
                EndianReader Reader = cache.Reader;
                CacheHeader CH = cache.Header;

                #region Get Info
                int matgOffset = -1;
                foreach(IndexItem item in cache.IndexItems)
                    if (item.ClassCode == "matg")
                    {
                        matgOffset = item.Offset;
                        break;
                    }

                if (matgOffset == -1) return;

                int localeStart = int.Parse(cache.buildNode.Attributes["localesStart"].Value);
                Reader.SeekTo(matgOffset + localeStart + (int)Lang * int.Parse(cache.buildNode.Attributes["languageSize"].Value));

                int localeCount = Reader.ReadInt32();
                int tableSize = Reader.ReadInt32();
                int indexOffset = Reader.ReadInt32() + CH.localeModifier;
                int tableOffset = Reader.ReadInt32() + CH.localeModifier;
                #endregion

                #region Read Indices
                Reader.SeekTo(indexOffset);
                int[] indices = new int[localeCount];
                for (int i = 0; i < localeCount; i++)
                {
                    this.Add("");
                    Reader.ReadInt32();
                    indices[i] = Reader.ReadInt32();
                }
                #endregion

                #region Read Names
                Reader.SeekTo(tableOffset);
                EndianReader newReader = (cache.localesKey == "")
                    ? new EndianReader(new MemoryStream(Reader.ReadBytes(tableSize)), EndianFormat.BigEndian)
                    : AES.DecryptSegment(Reader, tableOffset, tableSize, cache.localesKey);

                for (int i = 0; i < indices.Length; i++)
                {
                    if (indices[i] == -1)
                    {
                        this[i] = "<null>";
                        continue;
                    }

                    newReader.SeekTo(indices[i]);

                    int length;
                    if (i == indices.Length - 1)
                        length = tableSize - indices[i];
                    else
                        length = (indices[i + 1] != -1)
                            ? indices[i + 1] - indices[i]
                            : indices[i + 2] - indices[i];

                    if (length == 1)
                    {
                        this[i] = "<blank>";
                        continue;
                    }

                    this[i] = newReader.ReadString(length);
                }
                newReader.Close();
                newReader.Dispose();
                #endregion
            }
        }

        public class IndexItem
        {
            public CacheFile Cache;
            public string ClassCode
            {
                get { return (ClassIndex == -1) ? "____" : Cache.IndexItems.ClassList[ClassIndex].ClassCode; }
            }
            public string ClassName
            {
                get { return Cache.Strings.GetItemByID(Cache.IndexItems.ClassList[ClassIndex].StringID); }
            }
            public string ParentClass
            {
                get { return (ClassIndex == -1) ? "____" : Cache.IndexItems.ClassList[ClassIndex].Parent; }
            }
            public string ParentClass2
            {
                get { return (ClassIndex == -1) ? "____" : Cache.IndexItems.ClassList[ClassIndex].Parent2; }
            }
            public string Filename;
            public int ID;
            public int Offset;
            public short ClassIndex;
            public int metaIndex;

            public override string ToString()
            {
                return "[" + ClassCode + "] " + Filename;
            }
        }

        public class IndexTable : List<IndexItem>
        {
            private CacheFile cache;
            public List<TagClass> ClassList;

            public IndexTable(CacheFile Cache)
            {
                cache = Cache;
                CacheIndexHeader IH = cache.IndexHeader;
                CacheHeader CH = cache.Header;
                EndianReader Reader = cache.Reader;

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
                EndianReader newReader = (cache.tagsKey == "")
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

            public IndexItem GetItemByID(int ID)
            {
                if (ID == -1)  
                    return null;
                return this[ID & 0xFFFF];
            }
        }

        public class TagClass
        {
            public string ClassCode;
            public string Parent;
            public string Parent2;
            public int StringID;
        }

        #endregion

        #region Methods
        public void Close()
        {
            Reader.Close();
            Reader.Dispose();
            LocaleTables.Clear();
            Strings.Clear();
            IndexItems.Clear();
            play = null;
            zone = null;
            ugh_ = null;
            buildNode = null;
            versionNode = null;
            vertexNode = null;
            Header = null;
            IndexHeader = null;
        }

        private void VerifyXML()
        {
            /* Originally meant to be used to verify that external
             * XML files had no errors and contained all needed data.
             * But XMLs ended up internal only, and now this just
             * loads the required build node and version node. */
             
            #region Find Build
            var xml = new MemoryStream(Adjutant.Properties.Resources.Builds);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xml);
            XmlElement element = xmlDoc.DocumentElement;

            for (int i = 0; i < element.ChildNodes.Count; i++)
            {
                if (element.ChildNodes[i].Name.ToLower() != "build")
                    continue;

                if (element.ChildNodes[i].Attributes["string"].Value == Build)
                {
                    if (element.ChildNodes[i].Attributes["inherits"].Value != "")
                    {
                        Build = element.ChildNodes[i].Attributes["inherits"].Value;
                        VerifyXML();
                        return;
                    }

                    buildNode = element.ChildNodes[i];
                    version = buildNode.Attributes["version"].Value;
                    PluginDir = buildNode.Attributes["plugins"].Value;
                    break;
                }
            }

            if (buildNode == null)
                throw new Exception("Build " + "\"" + Build + "\"" + " was not found!");

            xml.Close();
            xml.Dispose();
            #endregion

            #region Find Version
            xml = new MemoryStream(Adjutant.Properties.Resources.Versions);
            xmlDoc = new XmlDocument();
            xmlDoc.Load(xml);
            element = xmlDoc.DocumentElement;

            for (int i = 0; i < element.ChildNodes.Count; i++)
            {
                if (element.ChildNodes[i].Name.ToLower() != "version")
                    continue;

                if (element.ChildNodes[i].Attributes["name"].Value == version)
                {
                    versionNode = element.ChildNodes[i];
                    break;
                }
            }

            if (versionNode == null)
                throw new Exception("Version " + "\"" + version + "\"" + " was not found!");

            xml.Close();
            xml.Dispose();
            #endregion

        }

        private void LoadPlayZone()
        {
            foreach(IndexItem item in IndexItems)
                if (item.ClassCode == "play")
                {
                    if (item.Offset > Reader.Length)
                    {
                        foreach (IndexItem item2 in IndexItems)
                            if (item2.ClassCode == "zone")
                            {
                                //fix for H4 prologue, play address is out of 
                                //bounds and data is held inside the zone tag 
                                //instead so make a fake play tag using zone data
                                item.Offset = item2.Offset + 28;
                                break;
                            }
                    }

                    play = DefinitionsManager.play(this, item);
                    break;
                }

            foreach (IndexItem item in IndexItems)
                if (item.ClassCode == "zone")
                {
                    zone = DefinitionsManager.zone(this, item);
                    break;
                }

            foreach (IndexItem item in IndexItems)
                if (item.ClassCode == "ugh!")
                {
                    ugh_ = DefinitionsManager.ugh_(this, item);
                    break;
                }
        }

        public byte[] GetRawFromID(int ID)
        {
            return GetRawFromID(ID, -1);
        }

        public byte[] GetRawFromID(int ID, int DataLength)
        {
            if (Version == DefinitionSet.Halo3Beta)
                return GetH3BRaw(ID, DataLength);

            EndianReader er;
            string fName = "";

            var Entry = zone.RawEntries[ID & ushort.MaxValue];

            if (Entry.SegmentIndex == -1) throw new InvalidDataException("Raw data not found.");

            var Loc = play.Segments[Entry.SegmentIndex];

            //if (Loc.SoundRawIndex != -1)
            //    return GetSoundRaw(ID);

            int index = (Loc.OptionalPageIndex2 != -1) ? Loc.OptionalPageIndex2 : (Loc.OptionalPageIndex != -1) ? Loc.OptionalPageIndex : Loc.RequiredPageIndex;
            int locOffset = (Loc.OptionalPageOffset2 != -1) ? Loc.OptionalPageOffset2 : (Loc.OptionalPageOffset != -1) ? Loc.OptionalPageOffset : Loc.RequiredPageOffset;

            if (index == -1 || locOffset == -1) throw new InvalidDataException("Raw data not found.");
            
            if (play.Pages[index].RawOffset == -1)
            {
                index = Loc.RequiredPageIndex;
                locOffset = Loc.RequiredPageOffset;
            }

            var Pool = play.Pages[index];

            if (Pool.CacheIndex != -1)
            {
                fName = play.SharedCaches[Pool.CacheIndex].FileName;
                fName = fName.Substring(fName.LastIndexOf('\\'));
                fName = FilePath + fName;

                if (fName == Filename)
                    er = Reader;
                else
                {
                    FileStream fs = new FileStream(fName, FileMode.Open, FileAccess.Read);
                    er = new EndianReader(fs, EndianFormat.BigEndian);
                }
            }
            else
                er = Reader;

            er.SeekTo(int.Parse(versionNode.ChildNodes[0].Attributes["rawTableOffset"].Value));
            int offset = Pool.RawOffset + er.ReadInt32();
            er.SeekTo(offset);
            byte[] compressed = er.ReadBytes(Pool.CompressedSize);
            byte[] decompressed = new byte[Pool.DecompressedSize];

            if (Version >= DefinitionSet.Halo4Retail)
            {
                int decompressionContext = 0;
                XMemCreateDecompressionContext(XMemCodecType.LZX, 0, 0, ref decompressionContext);
                XMemResetDecompressionContext(decompressionContext);
                XMemDecompressStream(decompressionContext, decompressed, ref Pool.DecompressedSize, compressed, ref Pool.CompressedSize);
                XMemDestroyDecompressionContext(decompressionContext);
            }
            else
            {
                BinaryReader BR = new BinaryReader(new DeflateStream(new MemoryStream(compressed), CompressionMode.Decompress));
                decompressed = BR.ReadBytes(Pool.DecompressedSize);
                BR.Close();
                BR.Dispose();
            }

            byte[] data = new byte[(DataLength != -1) ? DataLength : (Pool.DecompressedSize - locOffset)];
            int length = data.Length;
            if (length > decompressed.Length) length = decompressed.Length;
            Array.Copy(decompressed, locOffset, data, 0, length);

            if (er != Reader)
            {
                er.Close();
                er.Dispose();
            }

            return data;
        }

        private byte[] GetH3BRaw(int ID, int DataLength)
        {
            EndianReader er;
            string fName = "";

            var Entry = zone.RawEntries[ID & ushort.MaxValue];

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

        public byte[] GetSoundRaw(int ID, int size)
        {
            var Entry = zone.RawEntries[ID & ushort.MaxValue];

            if (Entry.SegmentIndex == -1) throw new InvalidDataException("Raw data not found.");

            var segment = play.Segments[Entry.SegmentIndex];
            var sRaw = play.SoundRawChunks[segment.SoundRawIndex];
            var reqPage = play.Pages[segment.RequiredPageIndex];
            var optPage = play.Pages[segment.OptionalPageIndex];

            if (size == 0)
            {
                //System.Windows.Forms.MessageBox.Show("0 size");
                size = (reqPage.CompressedSize != 0) ? reqPage.CompressedSize : optPage.CompressedSize;
            }

            var reqSize = size - sRaw.RawSize;
            var optSize = size - reqSize;

#if DEBUG
            if (reqPage.CompressedSize != reqPage.DecompressedSize || optPage.CompressedSize != optPage.DecompressedSize)
                throw new Exception("COMPRESSED DATA");

            if (sRaw.Sizes.Count > 1)
                throw new Exception("MULTIPLE SEGMENTS");
#endif

            byte[] buffer;
            byte[] data = new byte[size];
            int offset;
            EndianReader er;
            string fName = "";

            #region REQUIRED
            if (reqSize > 0)
            {
                if (reqPage.CacheIndex != -1)
                {
                    fName = play.SharedCaches[reqPage.CacheIndex].FileName;
                    fName = fName.Substring(fName.LastIndexOf('\\'));
                    fName = FilePath + fName;

                    if (fName == Filename)
                        er = Reader;
                    else
                        er = new EndianReader(new FileStream(fName, FileMode.Open, FileAccess.Read), EndianFormat.BigEndian);
                }
                else
                    er = Reader;

                er.SeekTo(1136);
                offset = reqPage.RawOffset + er.ReadInt32();

                er.SeekTo(offset);
                buffer = er.ReadBytes(reqPage.CompressedSize);

                Array.Copy(buffer, segment.RequiredPageOffset, data, 0, reqSize);

                if (er != Reader)
                {
                    er.Close();
                    er.Dispose();
                }
            }
            #endregion

            #region OPTIONAL
            if (segment.OptionalPageIndex != -1 && optSize > 0)
            {
                if (optPage.CacheIndex != -1)
                {
                    fName = play.SharedCaches[optPage.CacheIndex].FileName;
                    fName = fName.Substring(fName.LastIndexOf('\\'));
                    fName = FilePath + fName;

                    if (fName == Filename)
                        er = Reader;
                    else
                        er = new EndianReader(new FileStream(fName, FileMode.Open, FileAccess.Read), EndianFormat.BigEndian);
                }
                else
                    er = Reader;

                er.SeekTo(1136);
                offset = optPage.RawOffset + er.ReadInt32();

                er.SeekTo(offset);
                buffer = er.ReadBytes(optPage.CompressedSize);

                Array.Copy(buffer, segment.OptionalPageOffset, data, reqSize, optSize);

                if (er != Reader)
                {
                    er.Close();
                    er.Dispose();
                }
            }
            #endregion

            return data;
        }
        #endregion
    }
}

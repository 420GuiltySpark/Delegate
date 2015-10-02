using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using Adjutant.Properties;

namespace Adjutant.Library.Cache
{
    public abstract class CacheBase
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

        public string localesKey, stringsKey, tagsKey, networkKey;
        public string stringMods;

        public XmlNode buildNode;
        public XmlNode versionNode;
        public XmlNode vertexNode;
        #endregion

        public CacheBase(string Filename, string Build)
        {
            this.Filename = Filename;
            this.Build = Build;
            var fs = new FileStream(Filename, FileMode.Open, FileAccess.Read);
            Reader = new EndianReader((Stream)fs, EndianFormat.BigEndian);

            #region Read XML
            buildNode = GetBuildNode(Build);
            PluginDir = buildNode.Attributes["plugins"].Value;
            versionNode = GetVersionNode(buildNode.Attributes["version"].Value);
            HeaderSize = int.Parse(buildNode.Attributes["headerSize"].Value);
            stringMods = buildNode.Attributes["stringMods"].Value;

            tagsKey = buildNode.Attributes["tagsKey"].Value;
            stringsKey = buildNode.Attributes["stringsKey"].Value;
            localesKey = buildNode.Attributes["localesKey"].Value;
            networkKey = buildNode.Attributes["networkKey"].Value;

            vertexNode = GetVertexNode(buildNode.Attributes["vertDef"].Value);
            #endregion
        }

        #region Classes
        public class CacheHeader
        {
            #region Declarations
            public int Magic;

            public int fileSize;
            public int indexOffset;

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

            protected CacheBase cache;
            #endregion
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

            protected CacheBase cache;
            #endregion
        }

        public class StringTable : List<string>
        {
            protected CacheBase cache;

            public StringTable(CacheBase Cache)
            {
                cache = Cache;
                var Reader = cache.Reader;
                var CH = cache.Header;

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
                EndianReader newReader = (cache.stringsKey == "" || cache.stringsKey == null)
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
            protected CacheBase cache;

            public LocaleTable(CacheBase Cache, Language Lang)
            {
                cache = Cache;
                EndianReader Reader = cache.Reader;
                CacheHeader CH = cache.Header;

                #region Get Info
                int matgOffset = -1;
                foreach (IndexItem item in cache.IndexItems)
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
                EndianReader newReader = (cache.localesKey == "" || cache.localesKey == null)
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
            public CacheBase Cache;
            public string ClassCode
            {
                get { return (ClassIndex == -1) ? "____" : Cache.IndexItems.ClassList[ClassIndex].ClassCode; }
            }
            public string ClassName
            {
                get
                {
                    if (Cache.Version >= DefinitionSet.Halo3Beta) return Cache.Strings.GetItemByID(Cache.IndexItems.ClassList[ClassIndex].StringID);
                    else
                    {
                        var xml = new MemoryStream();

                        switch (Cache.Version)
                        {
                            case DefinitionSet.Halo2Xbox:
                            case DefinitionSet.Halo2Vista:
                                xml = new MemoryStream(Adjutant.Properties.Resources.Classes_H2);
                                break;

                            default: return ClassCode;
                        }

                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(xml);
                        XmlElement element = xmlDoc.DocumentElement;

                        foreach (XmlNode node in element.ChildNodes)
                        {
                            try
                            {
                                if (node.Attributes["code"].Value == ClassCode)
                                    return node.Attributes["name"].Value;
                            }
                            catch { }
                        }

                        return ClassCode;
                    }
                }
            }
            public string ParentClass
            {
                get
                {
                    try { return (ClassIndex == -1) ? "____" : Cache.IndexItems.ClassList[ClassIndex].Parent; }
                    catch { return ClassCode; }
                }
            }
            public string ParentClass2
            {
                get
                {
                    try { return (ClassIndex == -1) ? "____" : Cache.IndexItems.ClassList[ClassIndex].Parent2; }
                    catch { return ClassCode; }
                }
            }
            public string Filename;
            public int ID;
            public int Offset;
            public int ClassIndex;
            public int metaIndex;
            public int Magic;

            public override string ToString()
            {
                return "[" + ClassCode + "] " + Filename;
            }
        }

        public class IndexTable : List<IndexItem>
        {
            protected CacheBase cache;
            public List<TagClass> ClassList;

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

            public override string ToString()
            {
                return ClassCode;
            }
        }
        #endregion

        public virtual void Close()
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

        public void LoadPlayZone()
        {
            if (Version <= DefinitionSet.Halo2Vista) return;

            foreach (IndexItem item in IndexItems)
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

        public virtual byte[] GetRawFromID(int ID, int DataLength)
        {
            throw new NotImplementedException();
        }

        public virtual byte[] GetSoundRaw(int ID, int size)
        {
            throw new NotImplementedException();
        }

        public static XmlNode GetBuildNode(string build)
        {
            XmlNode retNode = null;
            using (var xml = new MemoryStream(Adjutant.Properties.Resources.Builds))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xml);
                var element = xmlDoc.DocumentElement;

                for (int i = 0; i < element.ChildNodes.Count; i++)
                {
                    if (element.ChildNodes[i].Name.ToLower() != "build") continue;

                    if (element.ChildNodes[i].Attributes["string"].Value == build)
                    {
                        if (element.ChildNodes[i].Attributes["inherits"].Value != "")
                            return GetBuildNode(element.ChildNodes[i].Attributes["inherits"].Value);

                        retNode = element.ChildNodes[i];
                        break;
                    }
                }
            }

            if (retNode == null)
                throw new Exception("Build " + "\"" + build + "\"" + " was not found!");

            return retNode;
        }

        public static XmlNode GetVersionNode(string ver)
        {
            XmlNode retNode = null;
            using (var xml = new MemoryStream(Adjutant.Properties.Resources.Versions))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xml);
                var element = xmlDoc.DocumentElement;

                for (int i = 0; i < element.ChildNodes.Count; i++)
                {
                    if (element.ChildNodes[i].Name.ToLower() != "version") continue;

                    if (element.ChildNodes[i].Attributes["name"].Value == ver)
                    {
                        retNode = element.ChildNodes[i];
                        break;
                    }
                }
            }

            if (retNode == null)
                throw new Exception("Version " + "\"" + ver + "\"" + " was not found!");

            return retNode;
        }

        public static XmlNode GetVertexNode(string ver)
        {
            XmlNode retNode = null;
            using (var xml = new MemoryStream(Adjutant.Properties.Resources.VertexBuffer))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xml);
                var element = xmlDoc.DocumentElement;

                foreach (XmlNode node in element.ChildNodes)
                {
                    if (node.Attributes["Game"].Value == ver)
                    {
                        retNode = node;
                        break;
                    }
                }
            }

            return retNode;
        }
    }
}

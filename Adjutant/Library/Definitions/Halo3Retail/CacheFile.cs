using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;

namespace Adjutant.Library.Definitions.Halo3Retail
{
    public class CacheFile : CacheBase
    {
        public CacheFile(string Filename, string Build)
            : base(Filename, Build)
        {
            Version = DefinitionSet.Halo3Retail;

            Header = new CacheHeader(this);
            IndexHeader = new Halo3Beta.CacheFile.CacheIndexHeader(this);
            IndexItems = new Halo3Beta.CacheFile.IndexTable(this);
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
                #endregion

                #region Modify Offsets
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
                #endregion
            }
        }

        public override byte[] GetRawFromID(int ID, int DataLength)
        {
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
                xcompress.XMemCreateDecompressionContext(xcompress.XMemCodecType.LZX, 0, 0, ref decompressionContext);
                xcompress.XMemResetDecompressionContext(decompressionContext);
                xcompress.XMemDecompressStream(decompressionContext, decompressed, ref Pool.DecompressedSize, compressed, ref Pool.CompressedSize);
                xcompress.XMemDestroyDecompressionContext(decompressionContext);
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

        public override byte[] GetSoundRaw(int ID, int size)
        {
            var Entry = zone.RawEntries[ID & ushort.MaxValue];

            if (Entry.SegmentIndex == -1) throw new InvalidDataException("Raw data not found.");

            var segment = play.Segments[Entry.SegmentIndex];
            var sRaw = play.SoundRawChunks[segment.SoundRawIndex];
            var reqPage = play.Pages[segment.RequiredPageIndex];
            var optPage = play.Pages[segment.OptionalPageIndex];

            if (size == 0) size = (reqPage.CompressedSize != 0) ? reqPage.CompressedSize : optPage.CompressedSize;

            var reqSize = size - sRaw.RawSize;
            var optSize = size - reqSize;

            //if (reqPage.CompressedSize != reqPage.DecompressedSize || optPage.CompressedSize != optPage.DecompressedSize)
            //    throw new Exception("COMPRESSED DATA");

            //if (sRaw.Sizes.Count > 1)
            //    throw new Exception("MULTIPLE SEGMENTS");

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

                if (buffer.Length > data.Length)
                    data = buffer;
                else
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
    }
}

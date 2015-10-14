using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using Adjutant.Library.Controls;
using Adjutant.Library.DataTypes;
using sbsp = Adjutant.Library.Definitions.scenario_structure_bsp;
using mode = Adjutant.Library.Definitions.render_model;

namespace Adjutant.Library.Definitions.Halo3Beta
{
    public class scenario_structure_bsp : sbsp
    {
        protected scenario_structure_bsp() { }

        public scenario_structure_bsp(CacheBase Cache, int Address)
        {
            cache = Cache;
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            #region sldt ID
            //sldt's sections address will be used instead of the one in sbsp
            int sectionAddress = 0;
            foreach (var item in Cache.IndexItems)
            {
                if (item.ClassCode == "scnr")
                {
                    Reader.SeekTo(item.Offset + 12);
                    int cnt = Reader.ReadInt32();
                    int ptr = Reader.ReadInt32() - Cache.Magic;

                    int bspIndex = 0;

                    for (int i = 0; i < cnt; i++)
                    {
                        Reader.SeekTo(ptr + 104 * i + 12);
                        if (Cache.IndexItems.GetItemByID(Reader.ReadInt32()).Offset == Address)
                        {
                            bspIndex = i;
                            break;
                        }
                    }

                    Reader.SeekTo(item.Offset + 1720 + 12);
                    int sldtID = Reader.ReadInt32();
                    int sldtAddress = Cache.IndexItems.GetItemByID(sldtID).Offset;

                    Reader.SeekTo(sldtAddress + 4);
                    cnt = Reader.ReadInt32();
                    ptr = Reader.ReadInt32() - Cache.Magic;

                    for (int i = 0; i < cnt; i++)
                    {
                        Reader.SeekTo(ptr + 436 * i + 2);

                        if (Reader.ReadInt16() != bspIndex) continue;

                        Reader.SeekTo(ptr + 436 * i + 312);
                        sectionAddress = Reader.ReadInt32() - Cache.Magic;

                        Reader.SeekTo(ptr + 436 * i + 428);
                        geomRawID = Reader.ReadInt32();
                    }

                    break;
                }
            }
            #endregion

            Reader.SeekTo(Address + 60);
            XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

            #region Clusters Block
            Reader.SeekTo(Address + 180);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Clusters.Add(new Cluster(Cache, iOffset + 236 * i));
            #endregion

            #region Shaders Block
            Reader.SeekTo(Address + 192);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new Halo3Beta.render_model.Shader(Cache, iOffset + 36 * i));
            #endregion

            #region GeometryInstances Block
            Reader.SeekTo(Address + 432);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                GeomInstances.Add(new InstancedGeometry(Cache, iOffset + 120 * i));
            #endregion

            Reader.SeekTo(Address + 580);
            RawID1 = Reader.ReadInt32();

            #region ModelSections Block
            Reader.SeekTo(Address + 740);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                ModelSections.Add(new Halo3Beta.render_model.ModelSection(Cache, sectionAddress + 76 * i));
            #endregion

            #region BoundingBox Block
            Reader.SeekTo(Address + 752);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                BoundingBoxes.Add(new Halo3Beta.render_model.BoundingBox(Cache, iOffset + 44 * i));
            #endregion

            Reader.SeekTo(Address + 860);
            RawID2 = Reader.ReadInt32();

            Reader.SeekTo(Address + 892);
            RawID3 = Reader.ReadInt32();
        }

        public override void LoadRaw()
        {
            var sbsp = this;
            var data = cache.GetRawFromID(sbsp.geomRawID);

            var ms = new MemoryStream(data);
            var reader = new EndianReader(ms, EndianFormat.BigEndian);

            var validParts = new Dictionary<int, mode.ModelSection>();

            LoadFixups();

            #region Read Vertices
            for (int i = 0; i < sbsp.ModelSections.Count; i++)
            {
                var section = sbsp.ModelSections[i];
                if (section.Submeshes.Count == 0) continue;

                if (section.VertsIndex >= 0 && section.VertsIndex < sbsp.VertInfoList.Count) reader.SeekTo(sbsp.VertInfoList[section.VertsIndex].Offset);

                if (cache.vertexNode == null) throw new NotSupportedException("No vertex definitions found for " + cache.Version.ToString());

                #region Get Vertex Definition
                XmlNode formatNode = null;
                foreach (XmlNode node in cache.vertexNode.ChildNodes)
                {
                    if (Convert.ToInt32(node.Attributes["type"].Value, 16) == section.VertexFormat)
                    {
                        formatNode = node;
                        break;
                    }
                }

                if (formatNode == null) throw new NotSupportedException("Format " + section.VertexFormat.ToString() + " not found in definition for " + cache.Version.ToString());
                #endregion

                mode.ModelSection validPart;
                if (validParts.TryGetValue(section.VertsIndex, out validPart))
                {
                    section.Vertices = validPart.Vertices;
                    continue;
                }
                else
                    validParts.Add(section.VertsIndex, section);

                section.Vertices = new Vertex[sbsp.VertInfoList[section.VertsIndex].VertexCount];

                #region Get Vertices
                for (int j = 0; j < sbsp.VertInfoList[section.VertsIndex].VertexCount; j++)
                {
                    mode.BoundingBox bb;
                    section.Vertices[j] = new Vertex(reader, formatNode);
                    if (i >= sbsp.BoundingBoxes.Count)
                    {
                        bb = new mode.BoundingBox();
                        bb.XBounds = bb.YBounds = bb.ZBounds =
                        bb.UBounds = bb.VBounds = new RealBounds(0, 0);
                    }
                    else
                        bb = sbsp.BoundingBoxes[i];

                    ModelFunctions.DecompressVertex(ref section.Vertices[j], bb);
                }
                #endregion
            }
            #endregion

            validParts.Clear();

            #region Read Indices
            for (int i = 0; i < sbsp.ModelSections.Count; i++)
            {
                var section = sbsp.ModelSections[i];
                if (section.Submeshes.Count == 0) continue;

                if (section.FacesIndex >= 0 && section.FacesIndex < sbsp.IndexInfoList.Count) reader.SeekTo(sbsp.IndexInfoList[section.FacesIndex].Offset);

                mode.ModelSection validPart;
                if (validParts.TryGetValue(section.FacesIndex, out validPart))
                {
                    section.Indices = validPart.Indices;
                    continue;
                }
                else
                    validParts.Add(section.FacesIndex, section);

                section.Indices = new int[section.TotalFaceCount];
                for (int j = 0; j < section.TotalFaceCount; j++)
                    section.Indices[j] = (sbsp.VertInfoList[section.VertsIndex].VertexCount > 0xFFFF) ? reader.ReadInt32() : reader.ReadUInt16();
            }
            #endregion

            sbsp.RawLoaded = true;
        }

        protected void LoadFixups()
        {
            var sbsp = this;
            sbsp.VertInfoList = new List<render_model.VertexBufferInfo>();
            sbsp.Unknown1List = new List<render_model.UnknownInfo1>();
            sbsp.IndexInfoList = new List<render_model.IndexBufferInfo>();
            sbsp.Unknown2List = new List<render_model.UnknownInfo2>();
            sbsp.Unknown3List = new List<render_model.UnknownInfo3>();

            var Entry = cache.zone.RawEntries[sbsp.geomRawID & ushort.MaxValue];
            var reader = new EndianReader(new MemoryStream(cache.zone.FixupData), EndianFormat.BigEndian);

            reader.SeekTo(Entry.FixupOffset + (Entry.FixupSize - 24));
            int vCount = reader.ReadInt32();
            reader.Skip(8);
            int iCount = reader.ReadInt32();

            reader.SeekTo(Entry.FixupOffset);

            for (int i = 0; i < vCount; i++)
            {
                sbsp.VertInfoList.Add(new mode.VertexBufferInfo()
                {
                    Offset = Entry.Fixups[i].Offset,
                    VertexCount = reader.ReadInt32(),
                    Unknown1 = reader.ReadInt32(),
                    DataLength = reader.ReadInt32(),
                    Unknown2 = reader.ReadInt32(), //blank from here so far
                    Unknown3 = reader.ReadInt32(),
                    Unknown4 = reader.ReadInt32(),
                    Unknown5 = reader.ReadInt32(),
                });
            }

            for (int i = 0; i < vCount; i++)
            {
                //assumed to be vertex related
                sbsp.Unknown1List.Add(new mode.UnknownInfo1()
                {
                    Unknown1 = reader.ReadInt32(), //always 0 so far
                    Unknown2 = reader.ReadInt32(), //always 0 so far
                    Unknown3 = reader.ReadInt32(), //1350707457
                });
            }

            for (int i = 0; i < iCount; i++)
            {
                var data = new mode.IndexBufferInfo();
                data.Offset = Entry.Fixups[vCount * 2 + i].Offset;
                data.FaceFormat = reader.ReadInt32();

                //value exists only in reach beta and newer
                if (cache.Version >= DefinitionSet.HaloReachBeta) data.UnknownX = reader.ReadInt32();
                else data.UnknownX = -1;

                data.DataLength = reader.ReadInt32();
                data.Unknown0 = reader.ReadInt32(); //blank from here so far
                data.Unknown1 = reader.ReadInt32();
                data.Unknown2 = reader.ReadInt32();
                data.Unknown3 = reader.ReadInt32();

                sbsp.IndexInfoList.Add(data);
            }

            for (int i = 0; i < iCount; i++)
            {
                //assumed to be index related
                sbsp.Unknown2List.Add(new mode.UnknownInfo2()
                {
                    Unknown1 = reader.ReadInt32(), //always 0 so far
                    Unknown2 = reader.ReadInt32(), //always 0 so far
                    Unknown3 = reader.ReadInt32(), //1753688321
                });
            }

            for (int i = 0; i < 4; i++)
            {
                sbsp.Unknown3List.Add(new mode.UnknownInfo3()
                {
                    Unknown1 = reader.ReadInt32(), //vCount in 3rd, iCount in 4th
                    Unknown2 = reader.ReadInt32(), //always 0 so far
                    Unknown3 = reader.ReadInt32(),
                });
            }

            reader.Close();
            reader.Dispose();
        }

        new public class Cluster : sbsp.Cluster
        {
            public Cluster(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

                Reader.SeekTo(Address + 172);
                SectionIndex = Reader.ReadInt16();
            }
        }

        new public class InstancedGeometry : sbsp.InstancedGeometry
        {
            public InstancedGeometry(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                TransformScale = Reader.ReadSingle();

                TransformMatrix = new Matrix();

                TransformMatrix.m11 = Reader.ReadSingle();
                TransformMatrix.m12 = Reader.ReadSingle();
                TransformMatrix.m13 = Reader.ReadSingle();

                TransformMatrix.m21 = Reader.ReadSingle();
                TransformMatrix.m22 = Reader.ReadSingle();
                TransformMatrix.m23 = Reader.ReadSingle();

                TransformMatrix.m31 = Reader.ReadSingle();
                TransformMatrix.m32 = Reader.ReadSingle();
                TransformMatrix.m33 = Reader.ReadSingle();

                TransformMatrix.m41 = Reader.ReadSingle();
                TransformMatrix.m42 = Reader.ReadSingle();
                TransformMatrix.m43 = Reader.ReadSingle();

                SectionIndex = Reader.ReadInt16();

                Reader.SeekTo(Address + 84);
                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());
            }
        }
    }
}

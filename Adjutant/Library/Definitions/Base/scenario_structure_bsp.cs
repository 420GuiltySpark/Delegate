using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using Adjutant.Library.Controls;
using Adjutant.Library.DataTypes;

namespace Adjutant.Library.Definitions
{
    public abstract class scenario_structure_bsp
    {
        public CacheBase cache;
        public int geomRawID;
        public string BSPName;

        public RealBounds XBounds, YBounds, ZBounds;

        public List<Cluster> Clusters;
        public List<Shader> Shaders;
        public List<InstancedGeometry> GeomInstances;
        public List<render_model.ModelSection> ModelSections;
        public List<render_model.BoundingBox> BoundingBoxes;
        public List<Prefab> Prefabs;

        public int RawID1; //decorator vertex buffers
        public int RawID2; //???
        public int RawID3; //bsp raw

        public List<render_model.VertexBufferInfo> VertInfoList;
        public List<render_model.UnknownInfo1> Unknown1List;
        public List<render_model.IndexBufferInfo> IndexInfoList;
        public List<render_model.UnknownInfo2> Unknown2List;
        public List<render_model.UnknownInfo3> Unknown3List;

        public bool RawLoaded = false;

        public virtual void LoadRaw()
        {
            var sbsp = this;
            var data = cache.GetRawFromID(sbsp.geomRawID);

            var ms = new MemoryStream(data);
            var reader = new EndianReader(ms, EndianFormat.BigEndian);

            var validParts = new Dictionary<int, render_model.ModelSection>();

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

                render_model.ModelSection validPart;
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
                    render_model.BoundingBox bb;
                    section.Vertices[j] = new Vertex(reader, formatNode);
                    if (i >= sbsp.BoundingBoxes.Count)
                    {
                        bb = new render_model.BoundingBox();
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

                render_model.ModelSection validPart;
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

        private void LoadFixups()
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
                sbsp.VertInfoList.Add(new render_model.VertexBufferInfo()
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
                sbsp.Unknown1List.Add(new render_model.UnknownInfo1()
                {
                    Unknown1 = reader.ReadInt32(), //always 0 so far
                    Unknown2 = reader.ReadInt32(), //always 0 so far
                    Unknown3 = reader.ReadInt32(), //1350707457
                });
            }

            for (int i = 0; i < iCount; i++)
            {
                var data = new render_model.IndexBufferInfo();
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
                sbsp.Unknown2List.Add(new render_model.UnknownInfo2()
                {
                    Unknown1 = reader.ReadInt32(), //always 0 so far
                    Unknown2 = reader.ReadInt32(), //always 0 so far
                    Unknown3 = reader.ReadInt32(), //1753688321
                });
            }

            for (int i = 0; i < 4; i++)
            {
                sbsp.Unknown3List.Add(new render_model.UnknownInfo3()
                {
                    Unknown1 = reader.ReadInt32(), //vCount in 3rd, iCount in 4th
                    Unknown2 = reader.ReadInt32(), //always 0 so far
                    Unknown3 = reader.ReadInt32(),
                });
            }

            reader.Close();
            reader.Dispose();
        }

        public abstract class Cluster
        {
            public RealBounds XBounds, YBounds, ZBounds;

            public int SectionIndex;
        }

        public abstract class Shader : render_model.Shader
        {
        }

        public abstract class InstancedGeometry
        {
            public float TransformScale;
            public Matrix TransformMatrix;
            public int SectionIndex;
            public string Name;

            public override string ToString()
            {
                return Name;
            }
        }

        public abstract class Prefab
        {
            public string Name;
            public float TransformScale;
            public Matrix TransformMatrix;
            public int InstanceCount;
            public int InstanceIndex;

            public override string ToString()
            {
                return Name;
            }
        }
    }
}

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
using sbsp = Adjutant.Library.Definitions.scenario_structure_bsp;
using mode = Adjutant.Library.Definitions.render_model;

namespace Adjutant.Library.Definitions.Halo2Xbox
{
    internal class scenario_structure_bsp : sbsp
    {
        internal scenario_structure_bsp(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            cache = Cache;
            int Address = Tag.Offset;
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Reader.SeekTo(Address + 68);

            XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

            Reader.SeekTo(Address + 172);

            #region Clusters Block
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Tag.Magic;
            ModelSections = new List<mode.ModelSection>();
            BoundingBoxes = new List<mode.BoundingBox>();
            Clusters = new List<sbsp.Cluster>();
            for (int i = 0; i < iCount; i++)
            {
                ModelSections.Add(new ModelSection(Cache, Tag, iOffset + 176 * i, null) { FacesIndex = i, VertsIndex = i, NodeIndex = 255 });
                Clusters.Add(new Cluster() { SectionIndex = i });
            }
            #endregion

            Reader.SeekTo(Address + 180);

            #region Shaders Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Tag.Magic;
            Shaders = new List<sbsp.Shader>();
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new Shader(Cache, iOffset + 32 * i));
            #endregion

            Reader.SeekTo(Address + 328);

            #region ModelParts Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Tag.Magic;
            for (int i = 0; i < iCount; i++)
                ModelSections.Add(new ModelSection(Cache, Tag, iOffset + 200 * i, BoundingBoxes) { FacesIndex = i + Clusters.Count, VertsIndex = i + Clusters.Count, NodeIndex = 255 });
            #endregion

            Reader.SeekTo(Address + 336);

            #region GeometryInstances Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Tag.Magic;
            GeomInstances = new List<sbsp.InstancedGeometry>();
            for (int i = 0; i < iCount; i++)
                GeomInstances.Add(new InstancedGeometry(Cache, iOffset + 88 * i, Clusters.Count));
            #endregion
        }

        public override void LoadRaw()
        {
            var sbsp = this;
            sbsp.IndexInfoList = new List<mode.IndexBufferInfo>();

            #region Clusters
            for (int i = 0; i < sbsp.Clusters.Count; i++)
            {
                var section = (Halo2Xbox.scenario_structure_bsp.ModelSection)sbsp.ModelSections[i];

                if (section.rSize.Length == 0 || section.vertcount == 0)
                {
                    sbsp.IndexInfoList.Add(new mode.IndexBufferInfo());
                    continue;
                }

                var data = cache.GetRawFromID(section.rawOffset, section.rawSize);
                var ms = new MemoryStream(data);
                var reader = new EndianReader(ms, Endian.EndianFormat.LittleEndian);

                if (cache.vertexNode == null) throw new NotSupportedException("No vertex definitions found for " + cache.Version.ToString());

                #region Get Vertex Definition
                XmlNode formatNode = null;
                foreach (XmlNode node in cache.vertexNode.ChildNodes)
                {
                    if (Convert.ToInt32(node.Attributes["type"].Value, 16) == 0)
                    {
                        formatNode = node;
                        break;
                    }
                }

                if (formatNode == null) throw new NotSupportedException("Format " + section.VertexFormat.ToString() + " not found in definition for " + cache.Version.ToString());
                #endregion

                #region Read Submeshes
                for (int j = 0; j < section.rSize[0] / 72; j++)
                {
                    var mesh = new mode.ModelSection.Submesh();
                    reader.SeekTo(section.hSize + section.rOffset[0] + j * 72 + 4);
                    mesh.ShaderIndex = reader.ReadUInt16();
                    mesh.FaceIndex = reader.ReadUInt16();
                    mesh.FaceCount = reader.ReadUInt16();
                    section.Submeshes.Add(mesh);
                }
                #endregion

                #region Get Resource Indices
                int iIndex = 0, vIndex = 0, uIndex = 0, nIndex = 0, bIndex = 0;

                for (int j = 0; j < section.rType.Length; j++)
                {
                    switch (section.rType[j] & 0x0000FFFF)
                    {
                        case 32: iIndex = j;
                            break;

                        case 56:
                            switch ((section.rType[j] & 0xFFFF0000) >> 16)
                            {
                                case 0: vIndex = j;
                                    break;
                                case 1: uIndex = j;
                                    break;
                                case 2: nIndex = j;
                                    break;
                            }
                            break;

                        case 100: bIndex = j;
                            break;
                    }
                }
                #endregion

                section.Vertices = new Vertex[section.vertcount];
                for (int j = 0; j < section.vertcount; j++)
                {
                    reader.SeekTo(section.hSize + section.rOffset[vIndex] + ((section.rSize[vIndex] / section.vertcount) * j));
                    var v = new Vertex(reader, formatNode);

                    //v.FormatName = "Halo2X format " + section.VertexFormat.ToString();
                    section.Vertices[j] = v;
                }

                #region Read UVs and Normals
                for (int j = 0; j < section.vertcount; j++)
                {
                    reader.SeekTo(section.hSize + section.rOffset[uIndex] + (8 * j));
                    var v = section.Vertices[j];
                    var uv = new RealQuat(reader.ReadSingle(), 1 - reader.ReadSingle());
                    v.Values.Add(new VertexValue(uv, 0, "texcoords", 0));
                    //DecompressVertex(ref v, sbsp.BoundingBoxes[0]);
                }

                for (int j = 0; j < section.vertcount; j++)
                {
                    reader.SeekTo(section.hSize + section.rOffset[uIndex + 1] + (12 * j));
                    var v = section.Vertices[j];
                    var n = RealQuat.FromHenDN3(reader.ReadUInt32());
                    v.Values.Add(new VertexValue(n, 0, "normal", 0));
                }
                #endregion

                reader.SeekTo(40);
                section.Indices = new int[reader.ReadUInt16()];
                reader.SeekTo(section.hSize + section.rOffset[iIndex]);
                for (int j = 0; j < section.Indices.Length; j++)
                    section.Indices[j] = reader.ReadUInt16();

                var facetype = 5;
                if (section.facecount * 3 == section.Indices.Length) facetype = 3;
                sbsp.IndexInfoList.Add(new mode.IndexBufferInfo() { FaceFormat = facetype });
            }
            #endregion

            #region Instances
            if (sbsp.GeomInstances.Count == 0) { sbsp.RawLoaded = true; return; }
            for (int i = sbsp.GeomInstances[0].SectionIndex; i < sbsp.ModelSections.Count; i++)
            {
                var section = (Halo2Xbox.scenario_structure_bsp.ModelSection)sbsp.ModelSections[i];
                var geomIndex = i - sbsp.Clusters.Count;

                if (section.rSize.Length == 0 || section.vertcount == 0)
                {
                    sbsp.IndexInfoList.Add(new mode.IndexBufferInfo());
                    continue;
                }

                var data = cache.GetRawFromID(section.rawOffset, section.rawSize);
                var ms = new MemoryStream(data);
                var reader = new EndianReader(ms, Endian.EndianFormat.LittleEndian);

                if (cache.vertexNode == null) throw new NotSupportedException("No vertex definitions found for " + cache.Version.ToString());

                #region Get Vertex Definition
                XmlNode formatNode = null;
                foreach (XmlNode node in cache.vertexNode.ChildNodes)
                {
                    if (Convert.ToInt32(node.Attributes["type"].Value, 16) == 0)
                    {
                        formatNode = node;
                        break;
                    }
                }

                if (formatNode == null) throw new NotSupportedException("Format " + section.VertexFormat.ToString() + " not found in definition for " + cache.Version.ToString());
                #endregion

                #region Read Submeshes
                for (int j = 0; j < section.rSize[0] / 72; j++)
                {
                    var mesh = new mode.ModelSection.Submesh();
                    reader.SeekTo(section.hSize + section.rOffset[0] + j * 72 + 4);
                    mesh.ShaderIndex = reader.ReadUInt16();
                    mesh.FaceIndex = reader.ReadUInt16();
                    mesh.FaceCount = reader.ReadUInt16();
                    section.Submeshes.Add(mesh);
                }
                #endregion

                #region Get Resource Indices
                int iIndex = 0, vIndex = 0, uIndex = 0, nIndex = 0, bIndex = 0;

                for (int j = 0; j < section.rType.Length; j++)
                {
                    switch (section.rType[j] & 0x0000FFFF)
                    {
                        case 32: iIndex = j;
                            break;

                        case 56:
                            switch ((section.rType[j] & 0xFFFF0000) >> 16)
                            {
                                case 0: vIndex = j;
                                    break;
                                case 1: uIndex = j;
                                    break;
                                case 2: nIndex = j;
                                    break;
                            }
                            break;

                        case 100: bIndex = j;
                            break;
                    }
                }
                #endregion

                section.Vertices = new Vertex[section.vertcount];
                for (int j = 0; j < section.vertcount; j++)
                {
                    reader.SeekTo(section.hSize + section.rOffset[vIndex] + ((section.rSize[vIndex] / section.vertcount) * j));
                    var v = new Vertex(reader, formatNode);

                    //v.FormatName = "Halo2X format " + section.VertexFormat.ToString();
                    section.Vertices[j] = v;
                }

                #region Read UVs and Normals
                for (int j = 0; j < section.vertcount; j++)
                {
                    reader.SeekTo(section.hSize + section.rOffset[uIndex] + (8 * j));
                    var v = section.Vertices[j];
                    var uv = new RealQuat(reader.ReadSingle(), 1 - reader.ReadSingle());
                    v.Values.Add(new VertexValue(uv, 0, "texcoords", 0));
                    //DecompressVertex(ref v, sbsp.BoundingBoxes[geomIndex]);
                }

                for (int j = 0; j < section.vertcount; j++)
                {
                    reader.SeekTo(section.hSize + section.rOffset[nIndex] + (12 * j));
                    var v = section.Vertices[j];
                    var n = RealQuat.FromHenDN3(reader.ReadUInt32());
                    v.Values.Add(new VertexValue(n, 0, "normal", 0));
                }
                #endregion

                reader.SeekTo(40);
                section.Indices = new int[reader.ReadUInt16()];
                reader.SeekTo(section.hSize + section.rOffset[iIndex]);
                for (int j = 0; j < section.Indices.Length; j++)
                    section.Indices[j] = reader.ReadUInt16();

                var facetype = 5;
                if (section.facecount * 3 == section.Indices.Length) facetype = 3;
                sbsp.IndexInfoList.Add(new mode.IndexBufferInfo() { FaceFormat = facetype });
            }
            #endregion

            sbsp.RawLoaded = true;
        }

        new internal class Cluster : sbsp.Cluster
        {
            internal Cluster()
            {
                XBounds = new RealBounds();
                YBounds = new RealBounds();
                ZBounds = new RealBounds();
            }
        }

        new internal class Shader : sbsp.Shader
        {
            internal Shader(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.Skip(12);

                tagID = Reader.ReadInt32();
            }
        }

        internal class ModelSection : mode.ModelSection
        {
            public int vertcount;
            public int facecount;

            public int rawOffset;
            public int rawSize;
            public int hSize;

            public int[] rSize;
            public int[] rOffset;
            public int[] rType;

            internal ModelSection(CacheFile Cache, CacheFile.IndexItem Tag, int Address, List<mode.BoundingBox> bounds)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Submeshes = new List<mode.ModelSection.Submesh>();
                Subsets = new List<mode.ModelSection.Subset>();

                VertexFormat = 0;
                vertcount = Reader.ReadUInt16();
                if (vertcount == 0)
                {
                    rSize = rOffset = rType = new int[0];
                    rawOffset = -1;
                    return;
                }
                facecount = Reader.ReadUInt16();

                int iCount, iOffset;

                if (bounds != null)
                {
                    Reader.SeekTo(Address + 24);
                    iCount = Reader.ReadInt32();
                    iOffset = Reader.ReadInt32() - Tag.Magic;
                    bounds.Add(new BoundingBox(Cache, iOffset));
                }

                Reader.SeekTo(Address + 40);

                rawOffset = Reader.ReadInt32();
                rawSize = Reader.ReadInt32();
                hSize = Reader.ReadInt32() + 8;
                Reader.ReadInt32();
                
                iCount = Reader.ReadInt32();
                iOffset = Reader.ReadInt32() - Tag.Magic;

                rSize = new int[iCount];
                rOffset = new int[iCount];
                rType = new int[iCount];

                for (int i = 0; i < iCount; i++)
                {
                    Reader.SeekTo(iOffset + 16 * i + 4);
                    rType[i] = Reader.ReadInt32();
                    rSize[i] = Reader.ReadInt32();
                    rOffset[i] = Reader.ReadInt32();
                }
            }
        }

        internal class BoundingBox : mode.BoundingBox
        {
            internal BoundingBox(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                UBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                VBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            }
        }

        new internal class InstancedGeometry : sbsp.InstancedGeometry
        {
            internal InstancedGeometry(CacheFile Cache, int Address, int modifier)
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

                SectionIndex = Reader.ReadInt16() + modifier;

                Reader.SeekTo(Address + 80);

                Name = Cache.Strings.GetItemByID(Reader.ReadInt16());
            }
        }
    }
}

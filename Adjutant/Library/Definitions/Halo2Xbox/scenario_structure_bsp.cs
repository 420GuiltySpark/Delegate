﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Adjutant.Library;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using Adjutant.Library.Controls;
using Adjutant.Library.DataTypes;
using sbsp = Adjutant.Library.Definitions.scenario_structure_bsp;
using mode = Adjutant.Library.Definitions.render_model;

namespace Adjutant.Library.Definitions.Halo2Xbox
{
    public class scenario_structure_bsp : sbsp
    {
        public scenario_structure_bsp(CacheBase Cache, CacheBase.IndexItem Tag)
        {
            cache = Cache;
            int Address = Tag.Offset;
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Reader.SeekTo(Address + 68);
            XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

            #region Clusters Block
            Reader.SeekTo(Address + 172);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Tag.Magic;
            for (int i = 0; i < iCount; i++)
            {
                ModelSections.Add(new ModelSection(Cache, Tag, iOffset + 176 * i, null) { FacesIndex = i, VertsIndex = i, NodeIndex = 255 });
                Clusters.Add(new Cluster(i));
            }
            #endregion

            #region Shaders Block
            Reader.SeekTo(Address + 180);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Tag.Magic;
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new Halo2Xbox.render_model.Shader(Cache, iOffset + 32 * i));
            #endregion

            #region ModelParts Block
            Reader.SeekTo(Address + 328);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Tag.Magic;
            for (int i = 0; i < iCount; i++)
                ModelSections.Add(new ModelSection(Cache, Tag, iOffset + 200 * i, BoundingBoxes) { FacesIndex = i + Clusters.Count, VertsIndex = i + Clusters.Count, NodeIndex = 255 });
            #endregion

            #region GeometryInstances Block
            Reader.SeekTo(Address + 336);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Tag.Magic;
            for (int i = 0; i < iCount; i++)
                GeomInstances.Add(new InstancedGeometry(Cache, iOffset + 88 * i, Clusters.Count));
            #endregion
        }

        public override void LoadRaw()
        {
            if (RawLoaded) return;

            #region Clusters
            for (int i = 0; i < Clusters.Count; i++)
            {
                var section = (Halo2Xbox.scenario_structure_bsp.ModelSection)ModelSections[i];

                if (section.rSize.Length == 0 || section.vertcount == 0)
                {
                    IndexInfoList.Add(new mode.IndexBufferInfo());
                    continue;
                }

                var data = cache.GetRawFromID(section.rawOffset, section.rawSize);
                var ms = new MemoryStream(data);
                var reader = new EndianReader(ms, Endian.EndianFormat.LittleEndian);

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
                    var v = new Vertex();
                    var p = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    v.Values.Add(new VertexValue(p, 0, "position", 0));
                    section.Vertices[j] = v;
                }

                #region Read UVs and Normals
                for (int j = 0; j < section.vertcount; j++)
                {
                    reader.SeekTo(section.hSize + section.rOffset[uIndex] + (8 * j));
                    var v = section.Vertices[j];
                    var uv = new RealQuat(reader.ReadSingle(), 1 - reader.ReadSingle());
                    v.Values.Add(new VertexValue(uv, 0, "texcoords", 0));
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
                IndexInfoList.Add(new mode.IndexBufferInfo() { FaceFormat = facetype });
            }
            #endregion

            #region Instances
            if (GeomInstances.Count == 0) { RawLoaded = true; return; }
            for (int i = GeomInstances[0].SectionIndex; i < ModelSections.Count; i++)
            {
                var section = (Halo2Xbox.scenario_structure_bsp.ModelSection)ModelSections[i];
                var geomIndex = i - Clusters.Count;

                if (section.rSize.Length == 0 || section.vertcount == 0)
                {
                    IndexInfoList.Add(new mode.IndexBufferInfo());
                    continue;
                }

                var data = cache.GetRawFromID(section.rawOffset, section.rawSize);
                var ms = new MemoryStream(data);
                var reader = new EndianReader(ms, Endian.EndianFormat.LittleEndian);

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
                    var v = new Vertex();
                    var p = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                    v.Values.Add(new VertexValue(p, 0, "position", 0));
                    section.Vertices[j] = v;
                }

                #region Read UVs and Normals
                for (int j = 0; j < section.vertcount; j++)
                {
                    reader.SeekTo(section.hSize + section.rOffset[uIndex] + (8 * j));
                    var v = section.Vertices[j];
                    var uv = new RealQuat(reader.ReadSingle(), 1 - reader.ReadSingle());
                    v.Values.Add(new VertexValue(uv, 0, "texcoords", 0));
                    //DecompressVertex(ref v, BoundingBoxes[geomIndex]);
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
                IndexInfoList.Add(new mode.IndexBufferInfo() { FaceFormat = facetype });
            }
            #endregion

            RawLoaded = true;
        }

        new public class Cluster : sbsp.Cluster
        {
            public Cluster(int Index)
            {
                XBounds = new RealBounds();
                YBounds = new RealBounds();
                ZBounds = new RealBounds();

                SectionIndex = Index;
            }
        }

        public class ModelSection : mode.ModelSection
        {
            public int vertcount;
            public int facecount;

            public int rawOffset;
            public int rawSize;
            public int hSize;

            public int[] rSize;
            public int[] rOffset;
            public int[] rType;

            public ModelSection(CacheBase Cache, CacheBase.IndexItem Tag, int Address, List<mode.BoundingBox> bounds)
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

        public class BoundingBox : mode.BoundingBox
        {
            public BoundingBox(CacheBase Cache, int Address)
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

        new public class InstancedGeometry : sbsp.InstancedGeometry
        {
            public InstancedGeometry(CacheBase Cache, int Address, int modifier)
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

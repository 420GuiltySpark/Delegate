using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using Adjutant.Library.Controls;
using Adjutant.Library.DataTypes;
using mode = Adjutant.Library.Definitions.render_model;

namespace Adjutant.Library.Definitions.Halo3Beta
{
    public class render_model : mode
    {
        protected render_model() { }

        public render_model(CacheBase Cache, int Address)
        {
            cache = Cache;
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Name = Cache.Strings.GetItemByID(Reader.ReadInt32());
            Flags = new Bitmask(Reader.ReadInt32());

            #region Regions Block
            Reader.SeekTo(Address + 12);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Regions.Add(new Region(Cache, iOffset + 16 * i));
            #endregion

            Reader.SeekTo(Address + 28);
            InstancedGeometryIndex = Reader.ReadInt32();

            #region Instanced Geometry Block
            Reader.SeekTo(Address + 32);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                GeomInstances.Add(new InstancedGeometry(Cache, iOffset + 60 * i));
            #endregion

            #region Nodes Block
            Reader.SeekTo(Address + 48);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Nodes.Add(new Node(Cache, iOffset + 96 * i));
            #endregion

            #region MarkerGroups Block
            Reader.SeekTo(Address + 60);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                MarkerGroups.Add(new MarkerGroup(Cache, iOffset + 16 * i));
            #endregion

            #region Shaders Block
            Reader.SeekTo(Address + 72);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new Shader(Cache, iOffset + 36 * i));
            #endregion

            #region ModelSections Block
            Reader.SeekTo(Address + 104);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                ModelSections.Add(new ModelSection(Cache, iOffset + 76 * i));
            #endregion

            #region BoundingBox Block
            Reader.SeekTo(Address + 116);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                BoundingBoxes.Add(new BoundingBox(Cache, iOffset + 56 * i));
            Reader.SeekTo(Address + 128);
            #endregion

            #region NodeMapGroup Block
            Reader.SeekTo(Address + 176);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                NodeIndexGroups.Add(new NodeIndexGroup(Cache, iOffset + 12 * i));
            Reader.SeekTo(Address + 188);
            #endregion

            Reader.SeekTo(Address + 224);           
            RawID = Reader.ReadInt32();
        }

        public override void LoadRaw()
        {
            if (RawLoaded) return;

            var mode = this;
            var data = cache.GetRawFromID(mode.RawID);
            var ms = new MemoryStream(data);
            var reader = new EndianReader(ms, Endian.EndianFormat.BigEndian);

            var validParts = new Dictionary<int, mode.ModelSection>();

            LoadFixups();

            if (mode.IndexInfoList.Count == 0) throw new Exception("Geometry contains no faces");

            #region Read Vertices
            for (int i = 0; i < mode.ModelSections.Count; i++)
            {
                var section = mode.ModelSections[i];
                if (section.Submeshes.Count == 0) continue;

                if (section.VertsIndex >= 0 && section.VertsIndex < mode.VertInfoList.Count) reader.SeekTo(mode.VertInfoList[section.VertsIndex].Offset);

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

                section.Vertices = new Vertex[mode.VertInfoList[section.VertsIndex].VertexCount];

                #region Get Vertices
                for (int j = 0; j < mode.VertInfoList[section.VertsIndex].VertexCount; j++)
                {
                    section.Vertices[j] = new Vertex(reader, formatNode);
                    ModelFunctions.DecompressVertex(ref section.Vertices[j], mode.BoundingBoxes[0]);

                    #region fixups
                    var vert = section.Vertices[j];
                    VertexValue v;

                    #region rigid fix
                    if (section.NodeIndex != 255 && !mode.Flags.Values[18])
                    {
                        vert.Values.Add(new VertexValue(new RealQuat(section.NodeIndex, 0, 0, 0), VertexValue.ValueType.Int8_N4, "blendindices", 0));
                        vert.Values.Add(new VertexValue(new RealQuat(1, 0, 0, 0), VertexValue.ValueType.Int8_N4, "blendweight", 0));
                    }
                    #endregion

                    #region flag 18 fix
                    if (mode.Flags.Values[18])
                    {
                        VertexValue w;
                        var hasWeights = vert.TryGetValue("blendweight", 0, out w);

                        if (!hasWeights) w = new VertexValue(new RealQuat(1, 0, 0, 0), VertexValue.ValueType.Int8_N4, "blendweight", 0);

                        if (vert.TryGetValue("blendindices", 0, out v))
                        {
                            v.Data.a = w.Data.a == 0 ? 0 : mode.NodeIndexGroups[i].NodeIndices[(int)v.Data.a].Index;
                            v.Data.b = w.Data.b == 0 ? 0 : mode.NodeIndexGroups[i].NodeIndices[(int)v.Data.b].Index;
                            v.Data.c = w.Data.c == 0 ? 0 : mode.NodeIndexGroups[i].NodeIndices[(int)v.Data.c].Index;
                            v.Data.d = w.Data.d == 0 ? 0 : mode.NodeIndexGroups[i].NodeIndices[(int)v.Data.d].Index;
                        }
                        else
                        {
                            v = new VertexValue(new RealQuat(0, 0, 0, 0), VertexValue.ValueType.Int8_N4, "blendindices", 0);
                            v.Data.a = mode.NodeIndexGroups[i].NodeIndices[0].Index;
                            vert.Values.Add(v);
                            vert.Values.Add(w);
                        }
                    }
                    #endregion

                    #region rigid_boned fix
                    if (!vert.TryGetValue("blendweight", 0, out v) && vert.TryGetValue("blendindices", 0, out v))
                    {
                        var q = new RealQuat(
                            v.Data.a == 0 ? 0 : 1,
                            v.Data.b == 0 ? 0 : 1,
                            v.Data.c == 0 ? 0 : 1,
                            v.Data.d == 0 ? 0 : 1);
                        vert.Values.Add(new VertexValue(q, VertexValue.ValueType.Int8_N4, "blendweight", 0));
                    }
                    #endregion

                    #endregion
                }
                #endregion
            }
            #endregion

            validParts.Clear();

            #region Read Indices
            for (int i = 0; i < mode.ModelSections.Count; i++)
            {
                var section = mode.ModelSections[i];
                if (section.Submeshes.Count == 0) continue;

                if (section.FacesIndex >= 0 && section.FacesIndex < mode.IndexInfoList.Count) reader.SeekTo(mode.IndexInfoList[section.FacesIndex].Offset);

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
                    section.Indices[j] = (mode.VertInfoList[section.VertsIndex].VertexCount > 0xFFFF) ? reader.ReadInt32() : reader.ReadUInt16();
            }
            #endregion

            LoadModelExtras();

            mode.RawLoaded = true;
        }

        protected void LoadFixups()
        {
            var Entry = cache.zone.RawEntries[RawID & ushort.MaxValue];
            var reader = new EndianReader(new MemoryStream(cache.zone.FixupData), EndianFormat.BigEndian);

            reader.SeekTo(Entry.FixupOffset + (Entry.FixupSize - 24));
            int vCount = reader.ReadInt32();
            reader.Skip(8);
            int iCount = reader.ReadInt32();

            reader.SeekTo(Entry.FixupOffset);

            for (int i = 0; i < vCount; i++)
            {
                VertInfoList.Add(new mode.VertexBufferInfo()
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
                Unknown1List.Add(new mode.UnknownInfo1()
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

                IndexInfoList.Add(data);
            }

            for (int i = 0; i < iCount; i++)
            {
                //assumed to be index related
                Unknown2List.Add(new mode.UnknownInfo2()
                {
                    Unknown1 = reader.ReadInt32(), //always 0 so far
                    Unknown2 = reader.ReadInt32(), //always 0 so far
                    Unknown3 = reader.ReadInt32(), //1753688321
                });
            }

            for (int i = 0; i < 4; i++)
            {
                Unknown3List.Add(new mode.UnknownInfo3()
                {
                    Unknown1 = reader.ReadInt32(), //vCount in 3rd, iCount in 4th
                    Unknown2 = reader.ReadInt32(), //always 0 so far
                    Unknown3 = reader.ReadInt32(),
                });
            }

            reader.Close();
            reader.Dispose();
        }

        protected void LoadModelExtras()
        {
            var mode = this;
            #region Mesh Merging
            foreach (var reg in mode.Regions)
            {
                foreach (var perm in reg.Permutations)
                {
                    if (perm.PieceCount < 2) continue;

                    var firstPart = mode.ModelSections[perm.PieceIndex];

                    if (firstPart.Submeshes.Count == 0) continue;

                    var verts = firstPart.Vertices.ToList();
                    var indices = firstPart.Indices.ToList();

                    for (int i = 1; i < perm.PieceCount; i++)
                    {
                        var nextPart = mode.ModelSections[perm.PieceIndex + i];

                        foreach (var mesh in nextPart.Submeshes)
                        {
                            firstPart.Submeshes.Add(mesh);
                            mesh.FaceIndex += indices.Count;
                        }

                        for (int j = 0; j < nextPart.Indices.Length; j++) nextPart.Indices[j] += verts.Count;

                        verts.AddRange(nextPart.Vertices);
                        indices.AddRange(nextPart.Indices);
                        nextPart.UnloadRaw(); //save memory seeing as it wont be used now
                    }

                    firstPart.Vertices = verts.ToArray();
                    firstPart.Indices = indices.ToArray();
                }
            }
            #endregion

            #region Mesh Splitting
            if (mode.InstancedGeometryIndex == -1) return;

            var part = mode.ModelSections[mode.InstancedGeometryIndex];
            var list = new List<mode.ModelSection>();

            for (int i = 0; i < part.Submeshes.Count; i++)
            {
                var submesh = part.Submeshes[i];

                for (int j = submesh.SubsetIndex; j < (submesh.SubsetIndex + submesh.SubsetCount); j++)
                {
                    var set = part.Subsets[j];
                    var vList = ModelFunctions.GetTriangleList(part.Indices, set.FaceIndex, set.FaceCount, mode.IndexInfoList[part.FacesIndex].FaceFormat);

                    var newStrip = vList.ToArray();

                    var min = vList.Min();
                    var max = vList.Max();

                    //adjust faces to start at 0, seeing as
                    //we're going to use a new set of vertices
                    for (int k = 0; k < newStrip.Length; k++)
                        newStrip[k] -= min;

                    var verts = new Vertex[(max - min) + 1];
                    for (int k = 0; k < verts.Length; k++)                    //need to deep clone in case the vertices need to be
                        verts[k] = (Vertex)ModelFunctions.DeepClone(part.Vertices[k + min]); //transformed, so it doesnt transform all instances

                    #region Make new instances
                    var newPart = new mode.ModelSection()
                    {
                        Vertices = verts,
                        Indices = newStrip,
                        Submeshes = new List<mode.ModelSection.Submesh>(),
                        Subsets = new List<mode.ModelSection.Subset>(),
                        VertexFormat = 1,
                        OpaqueNodesPerVertex = part.OpaqueNodesPerVertex,
                        NodeIndex = mode.GeomInstances[j].NodeIndex,
                        VertsIndex = mode.VertInfoList.Count,
                        FacesIndex = mode.IndexInfoList.Count
                    };

                    mode.VertInfoList.Add(new mode.VertexBufferInfo()
                    {
                        VertexCount = verts.Length //dont need the rest
                    });

                    mode.IndexInfoList.Add(new mode.IndexBufferInfo()
                    {
                        FaceFormat = 3 //dont need the rest
                    });

                    var newMesh = new mode.ModelSection.Submesh()
                    {
                        SubsetCount = 1,
                        SubsetIndex = 0,
                        FaceIndex = 0,
                        FaceCount = newStrip.Length,
                        ShaderIndex = submesh.ShaderIndex,
                        VertexCount = verts.Length
                    };

                    var newSet = new mode.ModelSection.Subset()
                    {
                        SubmeshIndex = 0,
                        FaceIndex = 0,
                        FaceCount = newStrip.Length,
                        VertexCount = verts.Length
                    };
                    #endregion

                    newPart.Submeshes.Add(newMesh);
                    newPart.Subsets.Add(newSet);

                    list.Add(newPart);
                }
            }

            //clear raw to save memory seeing as it wont be used anymore
            mode.ModelSections[mode.InstancedGeometryIndex].UnloadRaw();

            mode.ModelSections.AddRange(list.ToArray());

            var newRegion = new mode.Region()
            {
                Name = "Instances",
                Permutations = new List<mode.Region.Permutation>()
            };

            for (int i = 0; i < list.Count; i++)
            {
                var newPerm = new mode.Region.Permutation()
                {
                    Name = mode.GeomInstances[i].Name,
                    PieceIndex = mode.InstancedGeometryIndex + i + 1,
                    PieceCount = 1
                };

                newRegion.Permutations.Add(newPerm);
            }

            for (int i = 0; i < newRegion.Permutations.Count; i++)
            {
                var modelPart = mode.ModelSections[newRegion.Permutations[i].PieceIndex];
                var instance = mode.GeomInstances[i];

                //negative scale flips the faces after transform, fix it
                if (instance.TransformScale < 0) Array.Reverse(modelPart.Indices);

                for (int j = 0; j < modelPart.Vertices.Length; j++)
                {
                    var vert = modelPart.Vertices[j];
                    VertexValue p, n, v, w;

                    vert.TryGetValue("position", 0, out p);
                    vert.TryGetValue("normal", 0, out n);

                    p.Data *= instance.TransformScale;
                    p.Data.Point3DTransform(instance.TransformMatrix);

                    n.Data *= instance.TransformScale;
                    n.Data.Vector3DTransform(instance.TransformMatrix);

                    if (vert.TryGetValue("blendindices", 0, out v))
                        v.Data = new RealQuat(instance.NodeIndex, 0, 0, 0);
                    else
                        vert.Values.Add(new VertexValue(new RealQuat(instance.NodeIndex, 0, 0, 0), VertexValue.ValueType.Int8_N4, "blendindices", 0));

                    if (vert.TryGetValue("blendweight", 0, out w))
                        w.Data = new RealQuat(instance.NodeIndex, 0, 0, 0);
                    else
                        vert.Values.Add(new VertexValue(new RealQuat(1, 0, 0, 0), VertexValue.ValueType.Int8_N4, "blendweight", 0));
                }
            }

            mode.Regions.Add(newRegion);
            #endregion
        }

        new public class Region : mode.Region
        {
            public Region(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());

                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                for (int i = 0; i < iCount; i++)
                    Permutations.Add(new Permutation(Cache, iOffset + 16 * i));
            }

            new public class Permutation : mode.Region.Permutation
            {
                public Permutation(CacheBase Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    Name = Cache.Strings.GetItemByID(Reader.ReadInt32());
                    PieceIndex = Reader.ReadInt16();
                    PieceCount = Reader.ReadInt16();
                }
            }
        }

        new public class InstancedGeometry : mode.InstancedGeometry
        {
            public InstancedGeometry(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());
                NodeIndex = Reader.ReadInt32();

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
            }
        }

        new public class Node : mode.Node
        {
            public Node(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());
                ParentIndex = Reader.ReadInt16();
                FirstChildIndex = Reader.ReadInt16();
                NextSiblingIndex = Reader.ReadInt16();
                Reader.ReadInt16();
                Position = new RealQuat(
                    Reader.ReadSingle(), 
                    Reader.ReadSingle(),
                    Reader.ReadSingle());
                Rotation = new RealQuat(
                    Reader.ReadSingle(),
                    Reader.ReadSingle(),
                    Reader.ReadSingle(),
                    Reader.ReadSingle());

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

                DistanceFromParent = Reader.ReadSingle();
            }
        }

        new public class MarkerGroup : mode.MarkerGroup
        {
            public MarkerGroup(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());

                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                for (int i = 0; i < iCount; i++)
                    Markers.Add(new Marker(Cache, iOffset + 36 * i));
            }

            new public class Marker : mode.MarkerGroup.Marker
            {
                public Marker(CacheBase Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    RegionIndex = Reader.ReadByte();
                    PermutationIndex = Reader.ReadByte();
                    NodeIndex = Reader.ReadByte();
                    Reader.ReadByte();
                    Position = new RealQuat(
                        Reader.ReadSingle(),
                        Reader.ReadSingle(),
                        Reader.ReadSingle());
                    Rotation = new RealQuat(
                        Reader.ReadSingle(),
                        Reader.ReadSingle(),
                        Reader.ReadSingle(),
                        Reader.ReadSingle());
                    Scale = Reader.ReadSingle();
                }
            }
        }

        new public class Shader : mode.Shader
        {
            public Shader(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.SeekTo(Address + 12);
                tagID = Reader.ReadInt32();
            }
        }

        new public class ModelSection : mode.ModelSection
        {
            public ModelSection(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);
                    
                #region Submesh Block
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                for (int i = 0; i < iCount; i++)
                    Submeshes.Add(new Submesh(Cache, iOffset + 16 * i));
                #endregion

                #region Subset Block
                Reader.SeekTo(Address + 12);
                iCount = Reader.ReadInt32();
                iOffset = Reader.ReadInt32() - Cache.Magic;
                for (int i = 0; i < iCount; i++)
                    Subsets.Add(new Subset(Cache, iOffset + 8 * i));
                #endregion

                #region Other
                Reader.SeekTo(Address + 24);
                VertsIndex = Reader.ReadInt16();
                Reader.ReadInt32();
                UnknownIndex = Reader.ReadInt16();

                Reader.SeekTo(Address + 40);
                FacesIndex = Reader.ReadInt16();

                Reader.SeekTo(Address + 44);
                TransparentNodesPerVertex = Reader.ReadByte();
                NodeIndex = Reader.ReadByte();
                VertexFormat = Reader.ReadByte();
                OpaqueNodesPerVertex = Reader.ReadByte();
                #endregion
            }

            new public class Submesh : mode.ModelSection.Submesh
            {
                public Submesh(CacheBase Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    ShaderIndex = Reader.ReadInt16();
                    Reader.ReadInt16();
                    FaceIndex = Reader.ReadUInt16();
                    FaceCount = Reader.ReadUInt16();
                    SubsetIndex = Reader.ReadUInt16();
                    SubsetCount = Reader.ReadUInt16();
                    Reader.ReadInt16();
                    VertexCount = Reader.ReadUInt16();
                }
            }

            new public class Subset : mode.ModelSection.Subset
            {
                public Subset(CacheBase Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    FaceIndex = Reader.ReadUInt16();
                    FaceCount = Reader.ReadUInt16();
                    SubmeshIndex = Reader.ReadUInt16();
                    VertexCount = Reader.ReadUInt16();
                }
            }
        }

        new public class BoundingBox : mode.BoundingBox
        {
            public BoundingBox(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.ReadInt32();
                XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                UBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                VBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            }
        }

        new public class NodeIndexGroup : mode.NodeIndexGroup
        {
            public NodeIndexGroup(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                for (int i = 0; i < iCount; i++)
                    NodeIndices.Add(new NodeIndex(Cache, iOffset + 1 * i));
            }

            new public class NodeIndex : mode.NodeIndexGroup.NodeIndex
            {
                public NodeIndex(CacheBase Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    Index = Reader.ReadByte();
                }
            }
        }
    }
}
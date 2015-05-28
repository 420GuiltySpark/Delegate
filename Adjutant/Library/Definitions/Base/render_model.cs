using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using Adjutant.Library.Controls;
using Adjutant.Library.DataTypes;

namespace Adjutant.Library.Definitions
{
    public abstract class render_model
    {
        public CacheFile cache;

        public string Name;
        public Bitmask Flags;
        public List<Region> Regions;
        public List<InstancedGeometry> GeomInstances;
        public int InstancedGeometryIndex;
        public List<Node> Nodes;
        public List<MarkerGroup> MarkerGroups;
        public List<Shader> Shaders;
        public List<ModelSection> ModelSections;
        public List<BoundingBox> BoundingBoxes;
        public List<NodeIndexGroup> NodeIndexGroups;
        public int RawID;

        public List<VertexBufferInfo> VertInfoList;
        public List<UnknownInfo1> Unknown1List;
        public List<IndexBufferInfo> IndexInfoList;
        public List<UnknownInfo2> Unknown2List;
        public List<UnknownInfo3> Unknown3List;

        public virtual void LoadRaw()
        {
            var mode = this;
            var data = cache.GetRawFromID(mode.RawID);
            var ms = new MemoryStream(data);
            var reader = new EndianReader(ms, Endian.EndianFormat.BigEndian);

            var validParts = new Dictionary<int, render_model.ModelSection>();

            LoadFixups();

            if (mode.IndexInfoList.Count == 0) throw new Exception("Geometry contains no faces");

            #region Read Vertices
            for (int i = 0; i < mode.ModelSections.Count; i++)
            {
                var section = mode.ModelSections[i];
                if (section.Submeshes.Count == 0) continue;

                if (section.VertsIndex >= 0 && section.VertsIndex < mode.VertInfoList.Count) reader.BaseStream.Position = mode.VertInfoList[section.VertsIndex].Offset;

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

                if (section.FacesIndex >= 0 && section.FacesIndex < mode.IndexInfoList.Count) reader.BaseStream.Position = mode.IndexInfoList[section.FacesIndex].Offset;

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
                    section.Indices[j] = (mode.VertInfoList[section.VertsIndex].VertexCount > 0xFFFF) ? reader.ReadInt32() : reader.ReadUInt16();
            }
            #endregion

            LoadModelExtras();

            mode.RawLoaded = true;
        }

        private void LoadFixups()
        {
            VertInfoList = new List<render_model.VertexBufferInfo>();
            Unknown1List = new List<render_model.UnknownInfo1>();
            IndexInfoList = new List<render_model.IndexBufferInfo>();
            Unknown2List = new List<render_model.UnknownInfo2>();
            Unknown3List = new List<render_model.UnknownInfo3>();

            var Entry = cache.zone.RawEntries[RawID & ushort.MaxValue];
            var reader = new EndianReader(new MemoryStream(cache.zone.FixupData), EndianFormat.BigEndian);

            reader.SeekTo(Entry.FixupOffset + (Entry.FixupSize - 24));
            int vCount = reader.ReadInt32();
            reader.Skip(8);
            int iCount = reader.ReadInt32();

            reader.SeekTo(Entry.FixupOffset);

            for (int i = 0; i < vCount; i++)
            {
                VertInfoList.Add(new render_model.VertexBufferInfo()
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
                Unknown1List.Add(new render_model.UnknownInfo1()
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

                IndexInfoList.Add(data);
            }

            for (int i = 0; i < iCount; i++)
            {
                //assumed to be index related
                Unknown2List.Add(new render_model.UnknownInfo2()
                {
                    Unknown1 = reader.ReadInt32(), //always 0 so far
                    Unknown2 = reader.ReadInt32(), //always 0 so far
                    Unknown3 = reader.ReadInt32(), //1753688321
                });
            }

            for (int i = 0; i < 4; i++)
            {
                Unknown3List.Add(new render_model.UnknownInfo3()
                {
                    Unknown1 = reader.ReadInt32(), //vCount in 3rd, iCount in 4th
                    Unknown2 = reader.ReadInt32(), //always 0 so far
                    Unknown3 = reader.ReadInt32(),
                });
            }

            reader.Close();
            reader.Dispose();
        }

        private void LoadModelExtras()
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
            var list = new List<ModelSection>();

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
                    var newPart = new ModelSection()
                    {
                        Vertices = verts,
                        Indices = newStrip,
                        Submeshes = new List<render_model.ModelSection.Submesh>(),
                        Subsets = new List<render_model.ModelSection.Subset>(),
                        VertexFormat = 1,
                        OpaqueNodesPerVertex = part.OpaqueNodesPerVertex,
                        NodeIndex = mode.GeomInstances[j].NodeIndex,
                        VertsIndex = mode.VertInfoList.Count,
                        FacesIndex = mode.IndexInfoList.Count
                    };

                    mode.VertInfoList.Add(new render_model.VertexBufferInfo()
                    {
                        VertexCount = verts.Length //dont need the rest
                    });

                    mode.IndexInfoList.Add(new render_model.IndexBufferInfo()
                    {
                        FaceFormat = 3 //dont need the rest
                    });

                    var newMesh = new ModelSection.Submesh()
                    {
                        SubsetCount = 1,
                        SubsetIndex = 0,
                        FaceIndex = 0,
                        FaceCount = newStrip.Length,
                        ShaderIndex = submesh.ShaderIndex,
                        VertexCount = verts.Length
                    };

                    var newSet = new ModelSection.Subset()
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

            var newRegion = new Region()
            {
                Name = "Instances",
                Permutations = new List<render_model.Region.Permutation>()
            };

            for (int i = 0; i < list.Count; i++)
            {
                var newPerm = new Region.Permutation()
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

        public bool RawLoaded = false;

        public class Region
        {
            public string Name;
            public List<Permutation> Permutations;

            public class Permutation
            {
                public string Name;
                public int PieceIndex;
                public int PieceCount;

                public override string ToString()
                {
                    return Name;
                }
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public abstract class InstancedGeometry
        {
            public string Name;
            public int NodeIndex;
            public float TransformScale;
            public Matrix TransformMatrix;

            public override string ToString()
            {
                return Name;
            }
        }

        public abstract class Node
        {
            public string Name;
            public int ParentIndex;
            public int FirstChildIndex;
            public int NextSiblingIndex;
            public RealQuat Position;
            public RealQuat Rotation;
            public float TransformScale;
            public Matrix TransformMatrix;
            public float DistanceFromParent;

            public override string ToString()
            {
                return Name;
            }
        }

        public abstract class MarkerGroup
        {
            public string Name;
            public List<Marker> Markers;

            public abstract class Marker
            {
                public int RegionIndex;
                public int PermutationIndex;
                public int NodeIndex;
                public RealQuat Position;
                public RealQuat Rotation;
                public float Scale;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public abstract class Shader
        {
            public int tagID;
        }

        public class ModelSection
        {
            public List<Submesh> Submeshes;
            public Vertex[] Vertices;
            public int[] Indices;
            
            public class Submesh
            {
                public int ShaderIndex;
                public int FaceIndex;
                public int FaceCount;
                public int SubsetIndex;
                public int SubsetCount;
                public int VertexCount;
            }

            public List<Subset> Subsets;

            public class Subset
            {
                public int FaceIndex;
                public int FaceCount;
                public int SubmeshIndex;
                public int VertexCount;
            }

            public int VertsIndex;
            public int UnknownIndex;
            public int FacesIndex;
            public int TransparentNodesPerVertex;
            public int NodeIndex;
            public int VertexFormat;
            public int OpaqueNodesPerVertex;

            public int TotalVertexCount
            {
                get
                {
                    int total = 0;
                    foreach (Submesh submesh in this.Submeshes)
                        total += submesh.VertexCount;

                    return total;
                }
            }
            public int TotalFaceCount
            {
                get
                {
                    int total = 0;
                    foreach (Submesh submesh in this.Submeshes)
                        total += submesh.FaceCount;

                    return total;
                }
            }

            public void UnloadRaw()
            {
                Vertices = null;
                Indices = null;
            }
        }

        public class BoundingBox
        {
            public RealBounds XBounds, YBounds, ZBounds;
            public RealBounds UBounds, VBounds;

            public float Length
            {
                get
                {
                    return (float)Math.Sqrt(
                    Math.Pow(XBounds.Length, 2) +
                    Math.Pow(YBounds.Length, 2) +
                    Math.Pow(ZBounds.Length, 2));
                }
            }
        }

        public abstract class NodeIndexGroup
        {
            public List<NodeIndex> NodeIndices;

            public abstract class NodeIndex
            {
                public int Index;
            }
        }

        public class VertexBufferInfo
        {
            public int Offset;
            public int VertexCount;
            public int Unknown1;
            public int DataLength;
            public int Unknown2;
            public int Unknown3;
            public int Unknown4;
            public int Unknown5;

            public int BlockSize
            {
                get { return DataLength / VertexCount; }
            }
        }

        //assumed to be vertex related
        public class UnknownInfo1
        {
            public int Unknown1;
            public int Unknown2;
            public int Unknown3;
        }

        public class IndexBufferInfo
        {
            public int Offset;
            public int FaceFormat;
            public int UnknownX; //reach beta and up
            public int DataLength;
            public int Unknown0;
            public int Unknown1;
            public int Unknown2;
            public int Unknown3;
        }

        //assumed to be index related
        public class UnknownInfo2
        {
            public int Unknown1;
            public int Unknown2;
            public int Unknown3;
        }

        //like a footer sort of thing
        public class UnknownInfo3
        {
            public int Unknown1;
            public int Unknown2;
            public int Unknown3;
        }
    }
}

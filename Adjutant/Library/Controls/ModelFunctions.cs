using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.DataTypes;
using Adjutant.Library.DataTypes.Space;
using Adjutant.Library.Endian;

namespace Adjutant.Library.Controls
{
    public static class ModelFunctions
    {
        public static void LoadModelRaw(CacheFile Cache, ref render_model mode)
        {
            var data = Cache.GetRawFromID(mode.RawID);
            var ms = new MemoryStream(data);
            var reader = new EndianReader(ms, Endian.EndianFormat.BigEndian);

            var validParts = new Dictionary<int, render_model.ModelPart>();

            int totalVerts = 0;
            for (int i = 0; i < mode.ModelParts.Count; i++)
            {
                totalVerts += mode.ModelParts[i].TotalVertexCount;
            }

            for(int i = 0; i < mode.ModelParts.Count; i++)
            {
                var ModelPart = mode.ModelParts[i];

                if (ModelPart.ValidPartIndex == 255) continue;

                render_model.ModelPart validPart;
                if (validParts.TryGetValue(ModelPart.ValidPartIndex, out validPart))
                {
                    ModelPart.Vertices = validPart.Vertices;
                    continue;
                }
                else
                    validParts.Add(ModelPart.ValidPartIndex, ModelPart);

                ModelPart.Vertices = new Vertex[ModelPart.TotalVertexCount];
                #region Get Padding Size
                int totalPad = 0;
                switch (Cache.Version)
                {
                    case DefinitionSet.Halo3Retail:
                    case DefinitionSet.Halo3ODST:
                        switch (ModelPart.TransparentNodesPerVertex)
                        {
                            case 3:
                                totalPad += 8 * ModelPart.TotalVertexCount;
                                break;
                        }
                        switch (ModelPart.OpaqueNodesPerVertex)
                        {
                            case 0:
                            case 2:
                                totalPad += 4 * ModelPart.TotalVertexCount;
                                break;
                            case 1:
                                totalPad += ModelPart.TotalVertexCount;
                                break;
                            case 3:
                                totalPad += 12 * ModelPart.TotalVertexCount;
                                break;
                        }
                        break;

                    case DefinitionSet.HaloReachBeta:
                    case DefinitionSet.HaloReachRetail:
                    case DefinitionSet.Halo4Retail:
                        switch (ModelPart.TransparentNodesPerVertex)
                        {
                            case 3:
                                totalPad += 11 * ModelPart.TotalVertexCount;
                                break;
                        }
                        switch (ModelPart.OpaqueNodesPerVertex)
                        {
                            case 0:
                            case 1:
                                totalPad += ModelPart.OpaqueNodesPerVertex * ModelPart.TotalVertexCount;
                                break;
                            default:
                                throw new Exception("CHECK THIS");
                        }
                        break;

                    default:
                        throw new NotSupportedException("Supplied definition set does not support vertex formats.");
                }
                #endregion

                #region Get Vertices
                for (int j = 0; j < ModelPart.TotalVertexCount; j++)
                    ModelPart.Vertices[j] = DecompressVertex(GetVertex(reader, ModelPart, Cache.Version), mode.BoundingBoxs[0]);

                reader.ReadBytes(totalPad);
                reader.BaseStream.Position += (reader.BaseStream.Position % 4 != 0) ? 4 - reader.BaseStream.Position % 4 : 0;
                #endregion
            }

            validParts.Clear();

            for (int i = 0; i < mode.ModelParts.Count; i++)
            {
                var ModelPart = mode.ModelParts[i];

                if (ModelPart.ValidPartIndex == 255) continue;

                render_model.ModelPart validPart;
                if (validParts.TryGetValue(ModelPart.ValidPartIndex, out validPart))
                {
                    ModelPart.Indices = validPart.Indices;
                    continue;
                }
                else
                    validParts.Add(ModelPart.ValidPartIndex, ModelPart);

                ModelPart.Indices = new int[ModelPart.TotalFaceCount];
                for (int j = 0; j < ModelPart.TotalFaceCount; j++)
                    ModelPart.Indices[j] = (ModelPart.TotalVertexCount > 0xFFFF) ? reader.ReadInt32() : reader.ReadUInt16();

                reader.BaseStream.Position += (reader.BaseStream.Position % 4 != 0) ? 4 - reader.BaseStream.Position % 4 : 0;
            }

            mode.RawLoaded = true;
        }

        public static List<int> GetTriangleList(int[] Indices, int Start, int Length, render_model.ModelPart Part)
        {
            if (Part.Subsets[0] is Definitions.Halo4Retail.render_model.ModelPart.Subset)
            {
                if (((Definitions.Halo4Retail.render_model.ModelPart.Subset)Part.Subsets[0]).FaceType == 0)
                {
                    var arr = new int[Length];
                    Array.Copy(Indices, Start, arr, 0, Length);
                    return new List<int>(arr);
                }
            }

            var list = new List<int>();
            bool flag = false;

            for (int n = 0; n < (Length - 2); n++)
            {
                int val1 = Indices[Start + n + 0];
                int val2 = Indices[Start + n + 1];
                int val3 = Indices[Start + n + 2];

                if ((val1 != val2) && (val1 != val3) && (val2 != val3))
                {
                    list.Add(val1);
                    if (flag)
                    {
                        list.Add(val3);
                        list.Add(val2);
                    }
                    else
                    {
                        list.Add(val2);
                        list.Add(val3);
                    }
                }
                flag = !flag;
            }
            return list;
        }

        private static Vertex GetVertex(EndianReader reader, render_model.ModelPart Part, DefinitionSet version)
        {
            Vertex v = new Vertex() { Format = Part.VertexFormat, Positions = new List<RealPoint4D>(), TexPos = new List<RealPoint2D>(), Nodes = new List<int>(), Weights = new List<float>() };

            switch (v.Format)
            {
                #region Rigid
                case VertexFormat.Rigid:
                    v.Positions.Add(new RealPoint4D(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()));
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    v.Tangent = new RealVector3D(reader.ReadUInt32());

                    if (version <= DefinitionSet.Halo3ODST)
                        v.Binormal = new RealVector3D(reader.ReadUInt32());
                    break;
                #endregion

                #region Skinned
                case VertexFormat.Skinned:
                    v.Positions.Add(new RealPoint4D(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()));
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    v.Tangent = new RealVector3D(reader.ReadUInt32());

                    if (version <= DefinitionSet.Halo3ODST)
                        v.Binormal = new RealVector3D(reader.ReadUInt32());

                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());

                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    break;
                #endregion

                #region Decorator
#if DEBUG
                case VertexFormat.Decorator:
                    v.Positions.Add(new RealPoint4D(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()));
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    break;
#endif
                #endregion

                #region TinyPosition
                case VertexFormat.TinyPosition:
                    v.Positions.Add(new RealPoint4D(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()));
                    v.TexPos.Add(new RealPoint2D());
                    break;
                #endregion

                #region RigidCompressed
                case VertexFormat.RigidCompressed:
                    v.Positions.Add(new RealPoint4D(reader.ReadUInt32()));
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    v.Tangent = new RealVector3D(reader.ReadUInt32());
                    break;
                #endregion

                #region SkinnedCompressed
                case VertexFormat.SkinnedCompressed:
                    v.Positions.Add(new RealPoint4D(reader.ReadUInt32()));
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    v.Tangent = new RealVector3D(reader.ReadUInt32());

                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());

                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    break;
                #endregion

                #region Halo4
                #region World
                case VertexFormat.H4_World:
                    v.Positions.Add(new RealPoint4D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                    v.TexPos.Add(new RealPoint2D(Half.ToHalf(reader.ReadUInt16()), Half.ToHalf(reader.ReadUInt16())));
                    
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    v.Tangent = new RealVector3D(reader.ReadUInt32());
                    break;
                #endregion

                #region Rigid
                case VertexFormat.H4_Rigid:
                    v.Positions.Add(new RealPoint4D(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()));
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    v.Tangent = new RealVector3D(reader.ReadUInt32());
                    break;
                #endregion

                #region Skinned
                case VertexFormat.H4_Skinned:
                    v.Positions.Add(new RealPoint4D(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()));
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    v.Tangent = new RealVector3D(reader.ReadUInt32());

                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());

                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    break;
                #endregion

                #region H4_Contrail
                case VertexFormat.H4_Contrail:
                    v.Positions.Add(new RealPoint4D(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()));
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    v.Tangent = new RealVector3D(reader.ReadUInt32());
                    
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    break;
                #endregion

                #region H4_Beam
                case VertexFormat.H4_Beam:
                    v.Positions.Add(new RealPoint4D(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()));
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    v.Tangent = new RealVector3D(reader.ReadUInt32());

                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());

                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);

                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    break;
                #endregion

                #region H4_RigidCompressed
                case VertexFormat.H4_RigidCompressed:
                    v.Positions.Add(new RealPoint4D(reader.ReadUInt32()));
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    v.Tangent = new RealVector3D(reader.ReadUInt32());
                    break;
                #endregion

                #region H4_SkinnedCompressed
                case VertexFormat.H4_SkinnedCompressed:
                    v.Positions.Add(new RealPoint4D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0));
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    v.Tangent = new RealVector3D(reader.ReadUInt32());

                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());

                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    v.Weights.Add((float)reader.ReadByte() / 255);
                    break;
                #endregion

                #region H4_RigidBoned
                case VertexFormat.H4_RigidBoned:
                    v.Positions.Add(new RealPoint4D(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()));
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    v.Tangent = new RealVector3D(reader.ReadUInt32());
                    
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());

                    v.Weights.Add(v.Nodes[0] == 0 ? 0 : 1);
                    v.Weights.Add(v.Nodes[1] == 0 ? 0 : 1);
                    v.Weights.Add(v.Nodes[2] == 0 ? 0 : 1);
                    v.Weights.Add(v.Nodes[3] == 0 ? 0 : 1);
                    break;
                #endregion

                #region H4_RigidBoned2UV
                case VertexFormat.H4_RigidBoned2UV:
                    v.Positions.Add(new RealPoint4D(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()));
                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    
                    v.Normal = new RealVector3D(reader.ReadUInt32());
                    v.Tangent = new RealVector3D(reader.ReadUInt32());
                    
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());
                    v.Nodes.Add(reader.ReadByte());

                    v.Weights.Add(v.Nodes[0] == 0 ? 0 : 1);
                    v.Weights.Add(v.Nodes[1] == 0 ? 0 : 1);
                    v.Weights.Add(v.Nodes[2] == 0 ? 0 : 1);
                    v.Weights.Add(v.Nodes[3] == 0 ? 0 : 1);

                    v.TexPos.Add(new RealPoint2D(reader.ReadUInt16(), reader.ReadUInt16()));
                    break;
                #endregion
                #endregion

                default:
                    throw new NotSupportedException("Unsupported vertex format.");
            }

            if (Part.NodeIndex != 255)
            {
                v.Nodes.Add(0);
                v.Nodes.Add(0);
                v.Nodes.Add(0);
                v.Nodes.Add(Part.NodeIndex);

                v.Weights.Add(0);
                v.Weights.Add(0);
                v.Weights.Add(0);
                v.Weights.Add(1);
            }

            return v;
        }

        private static Vertex DecompressVertex(Vertex v, render_model.BoundingBox bb)
        {
            float posScalar = 1;
            float texScalar = 1;

            switch (v.Format)
            {
                case VertexFormat.H4_World:
                    texScalar = 0.5f;
                    break;

                case VertexFormat.Rigid:
                case VertexFormat.Skinned:
                case VertexFormat.TinyPosition:
                case VertexFormat.H4_Rigid:
                case VertexFormat.H4_Skinned:
                case VertexFormat.H4_Contrail:
                case VertexFormat.H4_Beam:
                case VertexFormat.H4_RigidBoned:
                case VertexFormat.H4_RigidBoned2UV:
                    posScalar = texScalar = 0xFFFF;
                    break;

                case VertexFormat.RigidCompressed:
                case VertexFormat.SkinnedCompressed:
                case VertexFormat.H4_RigidCompressed:
                    posScalar = 0x03FF;
                    texScalar = 0xFFFF;
                    break;

                case VertexFormat.H4_SkinnedCompressed:
                    texScalar = 0xFFFF;
                    break;
            }

            var pos = v.Positions[0];
            var tex = v.TexPos[0];

            pos.x = (float)(pos.x / posScalar * bb.XBounds.Length + bb.XBounds.Min);
            pos.y = (float)(pos.y / posScalar * bb.YBounds.Length + bb.YBounds.Min);
            pos.z = (float)(pos.z / posScalar * bb.ZBounds.Length + bb.ZBounds.Min);

            tex.x =      (float)(tex.x / texScalar * bb.UBounds.Length + bb.UBounds.Min);
            tex.y = 1f - (float)(tex.y / texScalar * bb.VBounds.Length + bb.VBounds.Min);

            v.Positions[0] = pos;
            v.TexPos[0] = tex;

            return v;
        }

        #region Geometry Recovery
        internal static void LoadModelExtras(ref render_model mode)
        {
            if (mode.ExtrasIndex == -1) return;

            var part = mode.ModelParts[mode.ExtrasIndex];
            var list = new List<NewModelPart>();

            for (int i = 0; i < part.Submeshes.Count; i++)
            {
                var mesh = part.Submeshes[i];

                //for (int j = submesh.SubsetIndex; j < (submesh.SubsetIndex + submesh.SubsetCount); j++)
                //{
                //    var set = part.Subsets[j];
                var array = GetTriangleList(part.Indices, mesh.FaceIndex, mesh.FaceCount, part);

                var newStrip = new int[mesh.FaceCount];
                Array.Copy(part.Indices, mesh.FaceIndex, newStrip, 0, mesh.FaceCount);

                var min = array.Min();
                var max = array.Max();

                for (int k = 0; k < newStrip.Length; k++)
                    newStrip[k] -= min;

                var verts = new Vertex[(max - min) + 1];
                Array.Copy(part.Vertices, min, verts, 0, (max - min) + 1);

                #region Make new instances
                var newPart = new NewModelPart()
                {
                    Indices = newStrip,
                    Submeshes = new List<render_model.ModelPart.Submesh>(),
                    Subsets = new List<render_model.ModelPart.Subset>(),
                    VertexFormat = part.VertexFormat,
                    OpaqueNodesPerVertex = part.OpaqueNodesPerVertex,
                    RawID = -1,
                    Vertices = verts
                };

                var newMesh = new NewModelPart.NewSubmesh()
                {
                    SubsetCount = 1,
                    SubsetIndex = 0,
                    FaceIndex = 0,
                    FaceCount = mesh.FaceCount,
                    ShaderIndex = mesh.ShaderIndex,
                    VertexCount = verts.Length
                };

                var newSet = new NewModelPart.NewSubset()
                {
                    SubmeshIndex = 0,
                    FaceIndex = 0,
                    FaceCount = mesh.FaceCount,
                    VertexCount = 0
                };
                #endregion

                newPart.Submeshes.Add(newMesh);
                newPart.Subsets.Add(newSet);

                list.Add(newPart);
                //}
            }

            mode.ModelParts.AddRange(list.ToArray());
            
            var newRegion = new NewRegion()
            {
                Name = "Unknown",
                Permutations = new List<render_model.Region.Permutation>()
            };

            for (int i = 0; i < list.Count; i++)
            {
                var newPerm = new NewRegion.NewPermutation()
                {
                    Name = "Unknown" + i.ToString(),
                    PieceIndex = mode.ExtrasIndex + i + 1
                };
                newRegion.Permutations.Add(newPerm);
            }
            
            mode.Regions.Add(newRegion);
        }

        private class NewRegion : render_model.Region
        {
            public class NewPermutation : render_model.Region.Permutation
            {
            }
        }

        private class NewModelPart : render_model.ModelPart
        {
            public class NewSubmesh : render_model.ModelPart.Submesh
            {
            }

            public class NewSubset : render_model.ModelPart.Subset
            {
            }
        }
        #endregion

        #region Write to file
        public static void WriteEMF3(string Filename, CacheFile Cache, render_model Model, bool SplitMeshes, List<int> PartIndices)
        {
            if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();
            if (!Filename.EndsWith(".emf")) Filename += ".emf";

            if (!Model.RawLoaded) LoadModelRaw(Cache, ref Model);

            var fs = new FileStream(Filename, FileMode.Create, FileAccess.Write);
            var bw = new BinaryWriter(fs);

            bw.Write(560360805);
            bw.Write(3);
            bw.Write((Model.Name + "\0").ToCharArray());

            #region Nodes
            bw.Write(Model.Nodes.Count);
            foreach (render_model.Node node in Model.Nodes)
            {
                bw.Write((node.Name + "\0").ToCharArray());
                bw.Write((short)node.ParentIndex);
                bw.Write(node.Position.x);
                bw.Write(node.Position.y);
                bw.Write(node.Position.z);
                bw.Write(node.Rotation.i);
                bw.Write(node.Rotation.j);
                bw.Write(node.Rotation.k);
                bw.Write(node.Rotation.w);
            }
            #endregion
            #region Markers
            bw.Write(Model.MarkerGroups.Count);
            foreach (render_model.MarkerGroup group in Model.MarkerGroups)
            {
                bw.Write((group.Name + "\0").ToCharArray());
                bw.Write(group.Markers.Count);
                foreach (render_model.MarkerGroup.Marker marker in group.Markers)
                {
                    bw.Write((byte)marker.RegionIndex);
                    bw.Write((byte)marker.PermutationIndex);
                    bw.Write((byte)marker.NodeIndex);
                    bw.Write(marker.Position.x);
                    bw.Write(marker.Position.y);
                    bw.Write(marker.Position.z);
                    bw.Write(marker.Rotation.i);
                    bw.Write(marker.Rotation.j);
                    bw.Write(marker.Rotation.k);
                    bw.Write(marker.Rotation.w);
                    bw.Write(marker.Scale);
                }
            }
            #endregion
            #region Regions
            var regions = new List<render_model.Region>();
            foreach (render_model.Region region in Model.Regions)
            {
                foreach (render_model.Region.Permutation perm in region.Permutations)
                {
                    if (PartIndices.Contains(perm.PieceIndex) && !regions.Contains(region))
                        regions.Add(region);
                }
            }
            bw.Write(regions.Count);

            foreach (render_model.Region region in regions)
            {
                bw.Write((region.Name + "\0").ToCharArray());
                var perms = new List<render_model.Region.Permutation>();

                foreach (render_model.Region.Permutation perm in region.Permutations)
                {
                    if (PartIndices.Contains(perm.PieceIndex))
                        perms.Add(perm);
                }

                bw.Write(perms.Count);
                foreach (render_model.Region.Permutation perm in perms)
                {
                    var part = Model.ModelParts[perm.PieceIndex];

                    bw.Write((perm.Name + "\0").ToCharArray());

                    if (part.Vertices[0].Nodes.Count > 0)
                        bw.Write((byte)2);
                    else
                        bw.Write((byte)1);

                    bw.Write(part.TotalVertexCount);
                    foreach (Vertex vert in part.Vertices)
                    {
                        bw.Write(vert.Positions[0].x);
                        bw.Write(vert.Positions[0].y);
                        bw.Write(vert.Positions[0].z);
                        bw.Write(vert.Normal.i);
                        bw.Write(vert.Normal.j);
                        bw.Write(vert.Normal.k);
                        bw.Write(vert.TexPos[0].x);
                        bw.Write(vert.TexPos[0].y);
                        if (vert.Nodes.Count > 0)
                        {
                            bw.Write((byte)vert.Nodes[0]);
                            bw.Write((byte)vert.Nodes[1]);
                            bw.Write((byte)vert.Nodes[2]);
                            bw.Write((byte)vert.Nodes[3]);
                            bw.Write(vert.Weights[0]);
                            bw.Write(vert.Weights[1]);
                            bw.Write(vert.Weights[2]);
                            bw.Write(vert.Weights[3]);
                        }
                    }

                    if (SplitMeshes)
                    {
                        bw.Write(part.Submeshes.Count);
                        foreach (render_model.ModelPart.Submesh submesh in part.Submeshes)
                        {
                            var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, part);
                            bw.Write(indices.Count / 3);
                            for (int i = 0; i < indices.Count; i += 3)
                            {
                                if (part.TotalVertexCount > 0xFFFF)
                                {
                                    bw.Write(indices[i + 0]);
                                    bw.Write(indices[i + 1]);
                                    bw.Write(indices[i + 2]);
                                }
                                else
                                {
                                    bw.Write((ushort)indices[i + 0]);
                                    bw.Write((ushort)indices[i + 1]);
                                    bw.Write((ushort)indices[i + 2]);
                                }
                                bw.Write((short)submesh.ShaderIndex);
                            }
                        }
                    }
                    else
                    {
                        bw.Write(1);

                        int count = 0;
                        foreach (render_model.ModelPart.Submesh submesh in part.Submeshes)
                            count += GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, part).Count;

                        bw.Write(count / 3);
                        foreach (render_model.ModelPart.Submesh submesh in part.Submeshes)
                        {
                            var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, part);
                            for (int i = 0; i < indices.Count; i += 3)
                            {
                                if (part.TotalVertexCount > 0xFFFF)
                                {
                                    bw.Write(indices[i + 0]);
                                    bw.Write(indices[i + 1]);
                                    bw.Write(indices[i + 2]);
                                }
                                else
                                {
                                    bw.Write((ushort)indices[i + 0]);
                                    bw.Write((ushort)indices[i + 1]);
                                    bw.Write((ushort)indices[i + 2]);
                                }
                                bw.Write((short)submesh.ShaderIndex);
                            }
                        }
                    }
                }
            }
            #endregion
            #region Shaders
            bw.Write(Model.Shaders.Count);
            foreach (render_model.Shader shad3r in Model.Shaders)
            {
                var rmshTag = Cache.IndexItems.GetItemByID(shad3r.tagID);
                var rmsh = DefinitionsManager.rmsh(Cache, rmshTag);
                string shaderName = rmshTag.Filename.Substring(rmshTag.Filename.LastIndexOf("\\") + 1) + "\0";
                string[] paths = new string[8] { "null\0", "null\0", "null\0", "null\0", "null\0", "null\0", "null\0", "null\0" };
                float[] uTiles = new float[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
                float[] vTiles = new float[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
                bool isTransparent = false;
                bool ccOnly = false;

                //ODST and Reach don't use predicted bitmaps, so this only applies to Halo3Retail
                if (Cache.Version == DefinitionSet.Halo3Retail)
                {
                    foreach (shader.PredictedBitmap bitmap in rmsh.PredictedBitmaps)
                    {
                        var s = bitmap.Type;
                        var bitmTag = Cache.IndexItems.GetItemByID(bitmap.BitmapTagID);

                        switch (s)
                        {
                            case "base_map":
                                paths[0] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                                break;
                            case "detail_map":
                            case "detail_map_overlay":
                                paths[1] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                                break;
                            case "change_color_map":
                                paths[2] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                                break;
                            case "bump_map":
                                paths[3] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                                break;
                            case "bump_detail_map":
                                paths[4] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                                break;
                            case "self_illum_map":
                                paths[5] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                                break;
                            case "specular_map":
                                paths[6] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                                break;
                        }
                    }

                    short[] tiles = new short[8] { -1, -1, -1, -1, -1, -1, -1, -1 };

                    foreach (shader.ShaderProperties.ShaderMap map in rmsh.Properties[0].ShaderMaps)
                    {
                        var bitmTag = Cache.IndexItems.GetItemByID(map.BitmapTagID);

                        for (int i = 0; i < 8; i++)
                        {
                            if (bitmTag.Filename + "\0" != paths[i]) continue;

                            tiles[i] = (short)map.TilingIndex;
                        }
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        try
                        {
                            uTiles[i] = rmsh.Properties[0].Tilings[tiles[i]].UTiling;
                            vTiles[i] = rmsh.Properties[0].Tilings[tiles[i]].VTiling;
                        }
                        catch { }
                    }
                }
                else
                    try { paths[0] = Cache.IndexItems.GetItemByID(rmsh.Properties[0].ShaderMaps[0].BitmapTagID).Filename + "\0"; }
                    catch { }

                if (rmshTag.ClassCode != "rmsh")
                {
                    isTransparent = true;
                    if (paths[0] == "null\0" && paths[2] != "null\0")
                        ccOnly = true;
                }

                bw.Write(shaderName.ToCharArray());
                for (int i = 0; i < 8; i++)
                {
                    bw.Write(paths[i].ToCharArray());
                    bw.Write(uTiles[i]);
                    bw.Write(vTiles[i]);
                }

                bw.Write(Convert.ToByte(isTransparent));
                bw.Write(Convert.ToByte(ccOnly));
            }
            #endregion

            bw.Close();
            bw.Dispose();
        }

        public static void WriteJMSX(string Filename, CacheFile Cache, render_model Model, List<int> PartIndices)
        {
            var safeName = Filename.Substring(Filename.LastIndexOf("\\") + 1);
            if (safeName.EndsWith(".jms")) safeName = safeName.Remove(safeName.Length - 4);

            Filename = Directory.GetParent(Filename).FullName + "\\models\\" + safeName;
            if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();

            if (!Model.RawLoaded) ModelFunctions.LoadModelRaw(Cache, ref Model);

            foreach (var region in Model.Regions)
            {
                foreach (var permutation in region.Permutations)
                {
                    if (!PartIndices.Contains(permutation.PieceIndex)) continue;

                    var part = Model.ModelParts[permutation.PieceIndex];
                    StreamWriter sw = new StreamWriter(Filename + "-" + region.Name + "_" + permutation.Name + ".jms");

                    sw.WriteLine("8200");
                    sw.WriteLine("14689795");

                    #region Nodes
                    sw.WriteLine(Model.Nodes.Count);
                    foreach (var node in Model.Nodes)
                    {
                        sw.WriteLine(node.Name);
                        sw.WriteLine(node.FirstChildIndex);
                        sw.WriteLine(node.NextSiblingIndex);
                        sw.WriteLine(node.Rotation.ToString());
                        sw.WriteLine((node.Position * 100).ToString());
                    }
                    #endregion

                    #region Shaders
                    sw.WriteLine(Model.Shaders.Count);
                    foreach (render_model.Shader shade in Model.Shaders)
                    {
                        var tag = Cache.IndexItems.GetItemByID(shade.tagID);
                        var path = tag.Filename.Substring(tag.Filename.LastIndexOf('\\') + 1);
                        sw.WriteLine(path);
                        sw.WriteLine("<none>"); //unknown
                    }
                    #endregion

                    #region Markers
                    int mCount = 0;
                    foreach (var group in Model.MarkerGroups)
                        foreach (var marker in group.Markers)
                            mCount++;

                    sw.WriteLine(mCount);
                    foreach (var group in Model.MarkerGroups)
                    {
                        foreach (var marker in group.Markers)
                        {
                            sw.WriteLine(group.Name);
                            sw.WriteLine("-1"); //unknown
                            sw.WriteLine((int)marker.NodeIndex);
                            sw.WriteLine(marker.Rotation.ToString());
                            sw.WriteLine((marker.Position * 100).ToString());
                            sw.WriteLine("1"); //radius
                        }
                    }
                    #endregion

                    #region Vertices
                    sw.WriteLine("1"); //region count
                    sw.WriteLine(region.Name);

                    sw.WriteLine(part.TotalVertexCount);
                    foreach (Vertex vertex in part.Vertices)
                    {
                        if (vertex.Nodes.Count > 0) sw.WriteLine((vertex.Nodes[0]).ToString());
                        else sw.WriteLine("0");

                        sw.WriteLine((((RealPoint3D)vertex.Positions[0]) * 100).ToString());
                        sw.WriteLine(vertex.Normal.ToString());

                        if (vertex.Nodes.Count > 1) sw.WriteLine((vertex.Nodes[1]).ToString());
                        else sw.WriteLine("0");

                        if (vertex.Weights.Count > 1) sw.WriteLine(vertex.Weights[1].ToString("F6"));
                        else sw.WriteLine("0.000000");

                        sw.WriteLine(vertex.TexPos[0].x.ToString("F6"));
                        sw.WriteLine(vertex.TexPos[0].y.ToString("F6"));
                        sw.WriteLine("0.000000"); //unknown
                    }
                    #endregion

                    #region Faces
                    int count = 0;
                    foreach (var submesh in part.Submeshes)
                        count += GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, part).Count;

                    sw.WriteLine(count / 3);
                    foreach (var submesh in part.Submeshes)
                    {
                        var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, part);

                        for (int i = 0; i < indices.Count; i += 3)
                        {
                            sw.WriteLine("0"); //region index
                            sw.WriteLine(submesh.ShaderIndex.ToString());
                            sw.WriteLine(indices[i + 0].ToString() + "\t" + indices[i + 1].ToString() + "\t" + indices[i + 2].ToString());
                        }
                    }
                    #endregion

                    sw.Close();
                    sw.Dispose();
                }
            }
        }

        public static void WriteOBJ(string Filename, CacheFile Cache, render_model Model, List<int> PartIndices)
        {
            if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();

            if (!Model.RawLoaded) ModelFunctions.LoadModelRaw(Cache, ref Model);

            StreamWriter sw = new StreamWriter(Filename);
                    
            sw.WriteLine("# -----------------------------------------");
            sw.WriteLine("# Halo x360 Model - Extracted with Adjutant");
            sw.WriteLine("# -----------------------------------------");

            foreach (var region in Model.Regions)
            {
                foreach (var permutation in region.Permutations)
                {
                    if (!PartIndices.Contains(permutation.PieceIndex)) continue;

                    var part = Model.ModelParts[permutation.PieceIndex];

                    foreach (var v in part.Vertices)
                        sw.WriteLine("v  {0} {1} {2}", v.Positions[0].x, v.Positions[0].y, v.Positions[0].z);

                    foreach (var v in part.Vertices)
                        sw.WriteLine("vt {0} {1}", v.TexPos[0].x, v.TexPos[0].y);

                    foreach (var v in part.Vertices)
                        sw.WriteLine("vn {0} {1} {2}", v.Normal.i, v.Normal.j, v.Normal.k);
                }
            }

            int position = 1;

            foreach (var region in Model.Regions)
            {
                foreach (var permutation in region.Permutations)
                {
                    if (!PartIndices.Contains(permutation.PieceIndex)) continue;

                    var part = Model.ModelParts[permutation.PieceIndex];

                    sw.WriteLine("g " + region.Name + "-" + permutation.Name);

                    foreach (var submesh in part.Submeshes)
                    {
                        for (int i = submesh.SubsetIndex; i < (submesh.SubsetIndex + submesh.SubsetCount); i++)
                        {
                            var indices = GetTriangleList(part.Indices, part.Subsets[i].FaceIndex, part.Subsets[i].FaceCount, part);
                            for (int j = 0; j < indices.Count; j += 3)
                            {
                                var line = string.Concat(new object[] {
                                    "f ", indices[j + 0] + position, "/", indices[j + 0] + position, "/", indices[j + 0] + position,
                                     " ", indices[j + 1] + position, "/", indices[j + 1] + position, "/", indices[j + 1] + position,
                                     " ", indices[j + 2] + position, "/", indices[j + 2] + position, "/", indices[j + 2] + position
                                });
                                sw.WriteLine(line);
                            }
                        }
                    }
                    position += part.TotalVertexCount;
                }
            }

            sw.Close();
            sw.Dispose();
        }

        public static void WriteJMS(string Filename, CacheFile Cache, render_model Model, List<int> PartIndices)
        {
            var safeName = Filename.Substring(Filename.LastIndexOf("\\") + 1);
            if (safeName.EndsWith(".jms")) safeName = safeName.Remove(safeName.Length - 4);

            Filename = Directory.GetParent(Filename).FullName + "\\models\\";
            if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();

            if (!Model.RawLoaded) ModelFunctions.LoadModelRaw(Cache, ref Model);

            var dic = new Dictionary<string, List<render_model.Region>>();
            List<string> permList = new List<string>();

            foreach (var region in Model.Regions)
            {
                foreach (var permutation in region.Permutations)
                {
                    if (!PartIndices.Contains(permutation.PieceIndex)) continue;

                    List<render_model.Region> rList;
                    
                    if(dic.TryGetValue(permutation.Name, out rList))
                        rList.Add(region);
                    else
                    {
                        rList = new List<render_model.Region>();
                        rList.Add(region);
                        dic.Add(permutation.Name, rList);
                        permList.Add(permutation.Name);
                    }
                }
            }

            foreach (var perm in permList)
            {
                var rList = new List<render_model.Region>();
                if (!dic.TryGetValue(perm, out rList)) throw new Exception("wtf?");

                StreamWriter sw = new StreamWriter(Filename + perm + ".jms");

                sw.WriteLine("8200");
                sw.WriteLine("14689795");

                #region Nodes
                sw.WriteLine(Model.Nodes.Count);
                foreach (var node in Model.Nodes)
                {
                    sw.WriteLine(node.Name);
                    sw.WriteLine(node.FirstChildIndex);
                    sw.WriteLine(node.NextSiblingIndex);
                    sw.WriteLine(node.Rotation.ToString());
                    sw.WriteLine((node.Position * 100).ToString());
                }
                #endregion

                #region Shaders
                sw.WriteLine(Model.Shaders.Count);
                foreach (render_model.Shader shade in Model.Shaders)
                {
                    var tag = Cache.IndexItems.GetItemByID(shade.tagID);
                    var path = tag.Filename.Substring(tag.Filename.LastIndexOf('\\') + 1);
                    sw.WriteLine(path);
                    sw.WriteLine("<none>"); //unknown
                }
                #endregion

                #region Markers
                int mCount = 0;
                foreach (var group in Model.MarkerGroups)
                    foreach (var marker in group.Markers)
                        mCount++;

                sw.WriteLine(mCount);
                foreach (var group in Model.MarkerGroups)
                {
                    foreach (var marker in group.Markers)
                    {
                        sw.WriteLine(group.Name);
                        sw.WriteLine("-1"); //unknown
                        sw.WriteLine((int)marker.NodeIndex);
                        sw.WriteLine(marker.Rotation.ToString());
                        sw.WriteLine((marker.Position * 100).ToString());
                        sw.WriteLine("1"); //radius
                    }
                }
                #endregion

                #region Vertices
                sw.WriteLine(rList.Count.ToString()); //region count

                foreach (var region in rList)
                {
                    foreach (var permutation in region.Permutations)
                    {
                        if (!PartIndices.Contains(permutation.PieceIndex) && permutation.Name == perm ) continue;
                        var part = Model.ModelParts[permutation.PieceIndex];

                        sw.WriteLine(region.Name);
                        sw.WriteLine(part.TotalVertexCount);
                        foreach (Vertex vertex in part.Vertices)
                        {
                            if (vertex.Nodes.Count > 0) sw.WriteLine((vertex.Nodes[0]).ToString());
                            else sw.WriteLine("0");

                            sw.WriteLine((((RealPoint3D)vertex.Positions[0]) * 100).ToString());
                            sw.WriteLine(vertex.Normal.ToString());

                            if (vertex.Nodes.Count > 1) sw.WriteLine((vertex.Nodes[1]).ToString());
                            else sw.WriteLine("0");

                            if (vertex.Weights.Count > 1) sw.WriteLine(vertex.Weights[1].ToString("F6"));
                            else sw.WriteLine("0.000000");

                            sw.WriteLine(vertex.TexPos[0].x.ToString("F6"));
                            sw.WriteLine(vertex.TexPos[0].y.ToString("F6"));
                            sw.WriteLine("0.000000"); //unknown
                        }
                    }
                }
                #endregion

                #region Faces
                int count = 0;
                foreach (var region in rList)
                {
                    foreach (var permutation in region.Permutations)
                    {
                        if (!PartIndices.Contains(permutation.PieceIndex) && permutation.Name == perm) continue;
                        var part = Model.ModelParts[permutation.PieceIndex];

                        foreach (var submesh in part.Submeshes)
                            count += GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, part).Count;
                    }
                }

                sw.WriteLine(count / 3);
                
                foreach (var region in rList)
                {
                    foreach (var permutation in region.Permutations)
                    {
                        if (!PartIndices.Contains(permutation.PieceIndex) && permutation.Name == perm) continue;
                        var part = Model.ModelParts[permutation.PieceIndex];
                        foreach (var submesh in part.Submeshes)
                        {
                            var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, part);
                            for (int i = 0; i < indices.Count; i += 3)
                            {
                                sw.WriteLine(rList.IndexOf(region).ToString()); //region index
                                sw.WriteLine(submesh.ShaderIndex.ToString());
                                sw.WriteLine(indices[i + 0].ToString() + "\t" + indices[i + 1].ToString() + "\t" + indices[i + 2].ToString());
                            }
                        }
                    }
                }
                #endregion

                sw.Close();
                sw.Dispose();
            }
        }
        #endregion
    }
}
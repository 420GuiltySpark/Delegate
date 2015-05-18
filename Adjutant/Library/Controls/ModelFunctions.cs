using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Adjutant.Library.S3D;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.DataTypes;
using Adjutant.Library.Endian;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;

namespace Adjutant.Library.Controls
{
    public static class ModelFunctions
    {
        /*public static void LoadModelRaw(CacheFile Cache, ref render_model mode)
        {
            var data = Cache.GetRawFromID(mode.RawID);
            var ms = new MemoryStream(data);
            var reader = new EndianReader(ms, Endian.EndianFormat.BigEndian);

            var validParts = new Dictionary<int, render_model.ModelSection>();

            //int totalVerts = 0;
            //for (int i = 0; i < mode.ModelParts.Count; i++)
            //{
            //    totalVerts += mode.ModelParts[i].TotalVertexCount;
            //}

            LoadModelFixups(Cache, ref mode);

            if (mode.IndexInfoList.Count == 0) throw new Exception("Geometry contains no faces");

            #region Read Vertices
            for (int i = 0; i < mode.ModelSections.Count; i++)
            {
                var section = mode.ModelSections[i];
                if (section.Submeshes.Count == 0) continue;

                if (section.VertsIndex >= 0 && section.VertsIndex < mode.VertInfoList.Count) reader.BaseStream.Position = mode.VertInfoList[section.VertsIndex].Offset;

                if (Cache.vertexNode == null) throw new NotSupportedException("No vertex definitions found for " + Cache.Version.ToString());
               
                #region Get Vertex Definition
                XmlNode formatNode = null;
                foreach (XmlNode node in Cache.vertexNode.ChildNodes)
                {
                    if (Convert.ToInt32(node.Attributes["type"].Value, 16) == section.VertexFormat)
                    {
                        formatNode = node;
                        break;
                    }
                }

                if (formatNode == null) throw new NotSupportedException("Format " + section.VertexFormat.ToString() + " not found in definition for " + Cache.Version.ToString());
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
                    DecompressVertex(ref section.Vertices[j], mode.BoundingBoxes[0]);

                    #region fixups
                    var vert = section.Vertices[j];
                    VertexValue v;

                    #region rigid fix
                    if (section.NodeIndex != 255 && !mode.Flags.Values[18])
                    {
                        vert.Values.Add(new VertexValue(new RealQuat(section.NodeIndex, 0, 0, 0), 0, "blendindices", 0));
                        vert.Values.Add(new VertexValue(new RealQuat(1, 0, 0, 0), 0, "blendweight", 0));
                    }
                    #endregion

                    #region flag 18 fix
                    if (mode.Flags.Values[18])
                    {
                        VertexValue w;
                        var hasWeights = vert.TryGetValue("blendweight", 0, out w);

                        if(!hasWeights) w = new VertexValue(new RealQuat(1, 0, 0, 0), 0, "blendweight", 0);

                        if (vert.TryGetValue("blendindices", 0, out v))
                        {
                            v.Data.a = w.Data.a == 0 ? 0 : mode.NodeIndexGroups[i].NodeIndices[(int)v.Data.a].Index;
                            v.Data.b = w.Data.b == 0 ? 0 : mode.NodeIndexGroups[i].NodeIndices[(int)v.Data.b].Index;
                            v.Data.c = w.Data.c == 0 ? 0 : mode.NodeIndexGroups[i].NodeIndices[(int)v.Data.c].Index;
                            v.Data.d = w.Data.d == 0 ? 0 : mode.NodeIndexGroups[i].NodeIndices[(int)v.Data.d].Index;
                        }
                        else
                        {
                            v = new VertexValue(new RealQuat(0, 0, 0, 0), 0, "blendindices", 0);
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
                        vert.Values.Add(new VertexValue(q, 0, "blendweight", 0));
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

            LoadModelExtras(ref mode);

            mode.RawLoaded = true;
        }
        */

        /*public static void LoadBSPRaw(CacheFile Cache, ref scenario_structure_bsp sbsp)
        {
            var data = Cache.GetRawFromID(sbsp.geomRawID);
            
            var ms = new MemoryStream(data);
            var reader = new EndianReader(ms, EndianFormat.BigEndian);

            var validParts = new Dictionary<int, render_model.ModelSection>();

            LoadBSPFixups(Cache, ref sbsp);

            #region Read Vertices
            for (int i = 0; i < sbsp.ModelSections.Count; i++)
            {
                var section = sbsp.ModelSections[i];
                if (section.Submeshes.Count == 0) continue;

                if (section.VertsIndex >= 0 && section.VertsIndex < sbsp.VertInfoList.Count) reader.SeekTo(sbsp.VertInfoList[section.VertsIndex].Offset);

                if (Cache.vertexNode == null) throw new NotSupportedException("No vertex definitions found for " + Cache.Version.ToString());

                #region Get Vertex Definition
                XmlNode formatNode = null;
                foreach (XmlNode node in Cache.vertexNode.ChildNodes)
                {
                    if (Convert.ToInt32(node.Attributes["type"].Value, 16) == section.VertexFormat)
                    {
                        formatNode = node;
                        break;
                    }
                }

                if (formatNode == null) throw new NotSupportedException("Format " + section.VertexFormat.ToString() + " not found in definition for " + Cache.Version.ToString());
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
                        bb = new NewBoundingBox();
                        bb.XBounds = bb.YBounds = bb.ZBounds = 
                        bb.UBounds = bb.VBounds = new RealBounds(0, 0);
                    }
                    else
                        bb = sbsp.BoundingBoxes[i];

                    DecompressVertex(ref section.Vertices[j], bb);
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
        */

        public static List<int> GetTriangleList(int[] Indices, int Start, int Length, int FaceFormat)
        {
            if (FaceFormat == 3)
            {
                var arr = new int[Length];
                Array.Copy(Indices, Start, arr, 0, Length);
                return new List<int>(arr);
            }

            var list = new List<int>();

            for (int n = 0; n < (Length - 2); n++)
            {
                int indx1 = Indices[Start + n + 0];
                int indx2 = Indices[Start + n + 1];
                int indx3 = Indices[Start + n + 2];

                if ((indx1 != indx2) && (indx1 != indx3) && (indx2 != indx3))
                {
                    list.Add(indx1);
                    if (n % 2 == 0)
                    {
                        list.Add(indx2);
                        list.Add(indx3);
                    }
                    else
                    {
                        list.Add(indx3);
                        list.Add(indx2);
                    }
                }
            }
            return list;
        }

        public static void DecompressVertex(ref Vertex v, render_model.BoundingBox bb)
        {
            VertexValue vv;
            if (v.TryGetValue("position", 0, out vv))
            {
                if (bb.XBounds.Length != 0) vv.Data.x = (float)(vv.Data.x * bb.XBounds.Length + bb.XBounds.Min);
                if (bb.YBounds.Length != 0) vv.Data.y = (float)(vv.Data.y * bb.YBounds.Length + bb.YBounds.Min);
                if (bb.ZBounds.Length != 0) vv.Data.z = (float)(vv.Data.z * bb.ZBounds.Length + bb.ZBounds.Min);
            }

            if (v.TryGetValue("texcoords", 0, out vv))
            {
                if (bb.UBounds.Length != 0) vv.Data.x = (float)(vv.Data.x * bb.UBounds.Length + bb.UBounds.Min);
                vv.Data.y = 1f - ((bb.VBounds.Length == 0) ? vv.Data.y : (float)(vv.Data.y * bb.VBounds.Length + bb.VBounds.Min));
                //vv.Data.y = 1f - vv.Data.y;
            }
        }

        public static void DecompressVertex(ref Vertex[] vArray, render_model.BoundingBox bb)
        {
            for (int i = 0; i < vArray.Length; i++)
            {
                VertexValue vv;
                if (vArray[i].TryGetValue("position", 0, out vv))
                {
                    if (bb.XBounds.Length != 0) vv.Data.x = (float)(vv.Data.x * bb.XBounds.Length + bb.XBounds.Min);
                    if (bb.YBounds.Length != 0) vv.Data.y = (float)(vv.Data.y * bb.YBounds.Length + bb.YBounds.Min);
                    if (bb.ZBounds.Length != 0) vv.Data.z = (float)(vv.Data.z * bb.ZBounds.Length + bb.ZBounds.Min);
                }

                if (vArray[i].TryGetValue("texcoords", 0, out vv))
                {
                    if (bb.UBounds.Length != 0) vv.Data.x = (float)(vv.Data.x * bb.UBounds.Length + bb.UBounds.Min);
                    vv.Data.y = 1f - ((bb.VBounds.Length == 0) ? vv.Data.y : (float)(vv.Data.y * bb.VBounds.Length + bb.VBounds.Min));
                    //vv.Data.y = 1f - vv.Data.y;
                }
            }
        }

        public static Matrix MatrixFromBounds(render_model.BoundingBox bb)
        {
            Matrix m = Matrix.Identity;

            if (bb.XBounds.Length != 0) m.m11 = bb.XBounds.Length;
            if (bb.YBounds.Length != 0) m.m22 = bb.YBounds.Length;
            if (bb.ZBounds.Length != 0) m.m33 = bb.ZBounds.Length;

            m.m41 = bb.XBounds.Min;
            m.m42 = bb.YBounds.Min;
            m.m43 = bb.ZBounds.Min;

            return m;
        }

        public static Matrix MatrixFromBounds(RealQuat Min, RealQuat Max)
        {
            Matrix m = Matrix.Identity;

            if (Max.x - Min.x != 0) m.m11 = Max.x - Min.x;
            if (Max.y - Min.y != 0) m.m22 = Max.y - Min.y;
            if (Max.z - Min.z != 0) m.m33 = Max.z - Min.z;

            m.m41 = Min.x;
            m.m42 = Min.y;
            m.m43 = Min.z;

            return m;
        }

        /*private static void LoadModelFixups(CacheFile cache, ref render_model mode)
        {
            mode.VertInfoList = new List<render_model.VertexBufferInfo>();
            mode.Unknown1List = new List<render_model.UnknownInfo1>();
            mode.IndexInfoList = new List<render_model.IndexBufferInfo>();
            mode.Unknown2List = new List<render_model.UnknownInfo2>();
            mode.Unknown3List = new List<render_model.UnknownInfo3>();
            
            var Entry = cache.zone.RawEntries[mode.RawID & ushort.MaxValue];
            var reader = new EndianReader(new MemoryStream(cache.zone.FixupData), EndianFormat.BigEndian);

            reader.SeekTo(Entry.FixupOffset + (Entry.FixupSize - 24));
            int vCount = reader.ReadInt32();
            reader.Skip(8);
            int iCount = reader.ReadInt32();

            reader.SeekTo(Entry.FixupOffset);

            for (int i = 0; i < vCount; i++)
            {
                mode.VertInfoList.Add(new render_model.VertexBufferInfo()
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
                mode.Unknown1List.Add(new render_model.UnknownInfo1()
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

                mode.IndexInfoList.Add(data);
            }

            for (int i = 0; i < iCount; i++)
            {
                //assumed to be index related
                mode.Unknown2List.Add(new render_model.UnknownInfo2()
                {
                    Unknown1 = reader.ReadInt32(), //always 0 so far
                    Unknown2 = reader.ReadInt32(), //always 0 so far
                    Unknown3 = reader.ReadInt32(), //1753688321
                });
            }

            for (int i = 0; i < 4; i++)
            {
                mode.Unknown3List.Add(new render_model.UnknownInfo3()
                {
                    Unknown1 = reader.ReadInt32(), //vCount in 3rd, iCount in 4th
                    Unknown2 = reader.ReadInt32(), //always 0 so far
                    Unknown3 = reader.ReadInt32(),
                });
            }

            reader.Close();
            reader.Dispose();
        }
        */

        /*private static void LoadBSPFixups(CacheFile cache, ref scenario_structure_bsp sbsp)
        {
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
        */

        #region Geometry Recovery
        /*private static void LoadModelExtras(ref render_model mode)
        {
            #region Mesh Merging
            foreach (var reg in mode.Regions)
            {
                foreach (var perm in reg.Permutations)
                {
                    if(perm.PieceCount < 2) continue;

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
            var list = new List<NewModelPart>();

            for (int i = 0; i < part.Submeshes.Count; i++)
            {
                var submesh = part.Submeshes[i];

                for (int j = submesh.SubsetIndex; j < (submesh.SubsetIndex + submesh.SubsetCount); j++)
                {
                    var set = part.Subsets[j];
                    var vList = GetTriangleList(part.Indices, set.FaceIndex, set.FaceCount, mode.IndexInfoList[part.FacesIndex].FaceFormat);

                    var newStrip = vList.ToArray();

                    var min = vList.Min();
                    var max = vList.Max();

                    //adjust faces to start at 0, seeing as
                    //we're going to use a new set of vertices
                    for (int k = 0; k < newStrip.Length; k++)
                        newStrip[k] -= min;

                    var verts = new Vertex[(max - min) + 1];
                    for (int k = 0; k < verts.Length; k++)                    //need to deep clone in case the vertices need to be
                        verts[k] = (Vertex)DeepClone(part.Vertices[k + min]); //transformed, so it doesnt transform all instances

                    #region Make new instances
                    var newPart = new NewModelPart()
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

                    var newMesh = new NewModelPart.NewSubmesh()
                    {
                        SubsetCount = 1,
                        SubsetIndex = 0,
                        FaceIndex = 0,
                        FaceCount = newStrip.Length,
                        ShaderIndex = submesh.ShaderIndex,
                        VertexCount = verts.Length
                    };

                    var newSet = new NewModelPart.NewSubset()
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
            
            var newRegion = new NewRegion()
            {
                Name = "Instances",
                Permutations = new List<render_model.Region.Permutation>()
            };

            for (int i = 0; i < list.Count; i++)
            {
                var newPerm = new NewRegion.NewPermutation()
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
                        vert.Values.Add(new VertexValue(new RealQuat(instance.NodeIndex, 0, 0, 0), 0, "blendindices", 0));

                    if (vert.TryGetValue("blendweight", 0, out w))
                        w.Data = new RealQuat(instance.NodeIndex, 0, 0, 0);
                    else
                        vert.Values.Add(new VertexValue(new RealQuat(1, 0, 0, 0), 0, "blendweight", 0));
                }
            }
            
            mode.Regions.Add(newRegion);
            #endregion
        }
        */

        #region Dummy Class Overrides
        /*private class NewRegion : render_model.Region
        {
            public class NewPermutation : render_model.Region.Permutation
            {
            }
        }*/

        /*private class NewModelPart : render_model.ModelSection
        {
            public class NewSubmesh : render_model.ModelSection.Submesh
            {
            }

            public class NewSubset : render_model.ModelSection.Subset
            {
            }
        }*/

        /*private class NewBoundingBox : render_model.BoundingBox
        {
        }*/
        #endregion
        #endregion

        #region Write to file
        public static void WriteEMF3(string Filename, CacheFile Cache, render_model Model, bool SplitMeshes, List<int> PartIndices)
        {
            if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();
            if (!Filename.EndsWith(".emf")) Filename += ".emf";

            if (!Model.RawLoaded) Model.LoadRaw();

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
                    var part = Model.ModelSections[perm.PieceIndex];

                    bw.Write((perm.Name + "\0").ToCharArray());

                    VertexValue v;
                    bool hasNodes = part.Vertices[0].TryGetValue("blendindices", 0, out v);
                    if (hasNodes)
                        bw.Write((byte)2);
                    else
                        bw.Write((byte)1);

                    bw.Write(part.Vertices.Length);
                    foreach (Vertex vert in part.Vertices)
                    {
                        vert.TryGetValue("position", 0, out v);
                        bw.Write(v.Data.x);
                        bw.Write(v.Data.y);
                        bw.Write(v.Data.z);

                        vert.TryGetValue("normal", 0, out v);
                        bw.Write(v.Data.i);
                        bw.Write(v.Data.j);
                        bw.Write(v.Data.k);

                        vert.TryGetValue("texcoords", 0, out v);
                        bw.Write(v.Data.x);
                        bw.Write(v.Data.y);

                        if (hasNodes)
                        {
                            vert.TryGetValue("blendindices", 0, out v);
                            bw.Write((byte)v.Data.a);
                            bw.Write((byte)v.Data.b);
                            bw.Write((byte)v.Data.c);
                            bw.Write((byte)v.Data.d);

                            vert.TryGetValue("blendweight", 0, out v);
                            bw.Write(v.Data.a);
                            bw.Write(v.Data.b);
                            bw.Write(v.Data.c);
                            bw.Write(v.Data.d);
                        }
                    }

                    if (SplitMeshes)
                    {
                        bw.Write(part.Submeshes.Count);
                        foreach (render_model.ModelSection.Submesh submesh in part.Submeshes)
                        {
                            var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, Model.IndexInfoList[part.FacesIndex].FaceFormat);
                            bw.Write(indices.Count / 3);
                            for (int i = 0; i < indices.Count; i += 3)
                            {
                                if (part.Vertices.Length > 0xFFFF)
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
                        foreach (render_model.ModelSection.Submesh submesh in part.Submeshes)
                            count += GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, Model.IndexInfoList[part.FacesIndex].FaceFormat).Count;

                        bw.Write(count / 3);
                        foreach (render_model.ModelSection.Submesh submesh in part.Submeshes)
                        {
                            var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, Model.IndexInfoList[part.FacesIndex].FaceFormat);
                            for (int i = 0; i < indices.Count; i += 3)
                            {
                                if (part.Vertices.Length > 0xFFFF)
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
            foreach (render_model.Shader shaderBlock in Model.Shaders)
            {
                //skip null shaders
                if (shaderBlock.tagID == -1)
                {
                    bw.Write("null\0".ToCharArray());
                    for (int i = 0; i < 8; i++)
                        bw.Write("null\0".ToCharArray());

                    bw.Write(Convert.ToByte(false));
                    bw.Write(Convert.ToByte(false));

                    continue;
                }

                var rmshTag = Cache.IndexItems.GetItemByID(shaderBlock.tagID);
                var rmsh = DefinitionsManager.rmsh(Cache, rmshTag);
                string shaderName = rmshTag.Filename.Substring(rmshTag.Filename.LastIndexOf("\\") + 1) + "\0";
                string[] paths = new string[8] { "null\0", "null\0", "null\0", "null\0", "null\0", "null\0", "null\0", "null\0" };
                float[] uTiles = new float[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
                float[] vTiles = new float[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
                bool isTransparent = false;
                bool ccOnly = false;

                //Halo4 fucked this up
                if (Cache.Version >= DefinitionSet.Halo3Beta && Cache.Version <= DefinitionSet.HaloReachRetail)
                {
                    var rmt2Tag = Cache.IndexItems.GetItemByID(rmsh.Properties[0].TemplateTagID);
                    var rmt2 = DefinitionsManager.rmt2(Cache, rmt2Tag);

                    for (int i = 0; i < rmt2.UsageBlocks.Count; i++)
                    {
                        var s = rmt2.UsageBlocks[i].Usage;
                        var bitmTag = Cache.IndexItems.GetItemByID(rmsh.Properties[0].ShaderMaps[i].BitmapTagID);

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

                    foreach (var map in rmsh.Properties[0].ShaderMaps)
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

                if (rmshTag.ClassCode != "rmsh" && rmshTag.ClassCode != "mat")
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

        public static void WriteOBJ(string Filename, CacheFile Cache, render_model Model, List<int> PartIndices)
        {
            if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();

            if (!Model.RawLoaded) Model.LoadRaw();

            StreamWriter sw = new StreamWriter(Filename);

            sw.WriteLine("# -----------------------------------------");
            sw.WriteLine("# Halo x360 Model - Extracted with Adjutant");
            sw.WriteLine("# -----------------------------------------");

            foreach (var region in Model.Regions)
            {
                foreach (var permutation in region.Permutations)
                {
                    if (!PartIndices.Contains(permutation.PieceIndex)) continue;

                    var part = Model.ModelSections[permutation.PieceIndex];

                    if (part.Submeshes.Count == 0) continue;

                    VertexValue v;

                    foreach (var vert in part.Vertices)
                    {
                        vert.TryGetValue("position", 0, out v);
                        sw.WriteLine("v  {0} {1} {2}", v.Data.x * 100, v.Data.y * 100, v.Data.z * 100);
                    }

                    foreach (var vert in part.Vertices)
                    {
                        vert.TryGetValue("texcoords", 0, out v);
                        sw.WriteLine("vt {0} {1}", v.Data.x, v.Data.y);
                    }

                    foreach (var vert in part.Vertices)
                    {
                        vert.TryGetValue("normal", 0, out v);
                        sw.WriteLine("vn {0} {1} {2}", v.Data.i, v.Data.j, v.Data.k);
                    }
                }
            }

            int position = 1;

            foreach (var region in Model.Regions)
            {
                foreach (var permutation in region.Permutations)
                {
                    if (!PartIndices.Contains(permutation.PieceIndex)) continue;

                    var part = Model.ModelSections[permutation.PieceIndex];

                    if (part.Submeshes.Count == 0) continue;

                    sw.WriteLine("g " + region.Name + "-" + permutation.Name);

                    foreach (var submesh in part.Submeshes)
                    {
                        //for (int i = submesh.SubsetIndex; i < (submesh.SubsetIndex + submesh.SubsetCount); i++)
                        //{
                        var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, Model.IndexInfoList[part.FacesIndex].FaceFormat);
                        for (int j = 0; j < indices.Count; j += 3)
                        {
                            var line = string.Concat(new object[] {
                                    "f ", indices[j + 0] + position, "/", indices[j + 0] + position, "/", indices[j + 0] + position,
                                     " ", indices[j + 1] + position, "/", indices[j + 1] + position, "/", indices[j + 1] + position,
                                     " ", indices[j + 2] + position, "/", indices[j + 2] + position, "/", indices[j + 2] + position
                                });
                            sw.WriteLine(line);
                        }
                        //}
                    }
                    position += Model.VertInfoList[part.VertsIndex].VertexCount;
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

            if (!Model.RawLoaded) Model.LoadRaw();

            var dic = new Dictionary<string, List<render_model.Region>>();
            List<string> permList = new List<string>();

            foreach (var region in Model.Regions)
            {
                foreach (var permutation in region.Permutations)
                {
                    if (!PartIndices.Contains(permutation.PieceIndex)) continue;

                    List<render_model.Region> rList;

                    if (dic.TryGetValue(permutation.Name, out rList))
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
                    sw.WriteLine(node.Rotation.ToString(4, "\t"));
                    sw.WriteLine((node.Position * 100).ToString(3, "\t"));
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
                        //if (Model.Regions[marker.RegionIndex].Permutations[marker.PermutationIndex].Name == perm)
                        mCount++;

                sw.WriteLine(mCount);
                foreach (var group in Model.MarkerGroups)
                {
                    foreach (var marker in group.Markers)
                    {
                        sw.WriteLine(group.Name);
                        sw.WriteLine("-1"); //unknown
                        sw.WriteLine(marker.NodeIndex);
                        sw.WriteLine(marker.Rotation.ToString(4, "\t"));
                        sw.WriteLine((marker.Position * 100).ToString(3, "\t"));
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
                        if (!PartIndices.Contains(permutation.PieceIndex) && permutation.Name == perm) continue;
                        var part = Model.ModelSections[permutation.PieceIndex];

                        sw.WriteLine(region.Name);
                        sw.WriteLine(Model.VertInfoList[part.VertsIndex].VertexCount);
                        foreach (Vertex vert in part.Vertices)
                        {
                            VertexValue vNodes, vWeights, vPos, vNorm, vTex;

                            bool hasNodes = vert.TryGetValue("blendindices", 0, out vNodes);
                            bool hasWeights = vert.TryGetValue("blendweight", 0, out vWeights);

                            vert.TryGetValue("position", 0, out vPos);
                            vert.TryGetValue("normal", 0, out vNorm);
                            vert.TryGetValue("texcoords", 0, out vTex);

                            if (hasNodes) sw.WriteLine(vNodes.Data.a.ToString());
                            else sw.WriteLine("0");

                            sw.WriteLine((vPos.Data * 100).ToString(3, "\t"));
                            sw.WriteLine(vNorm.Data.ToString(3, "\t"));

                            if (hasNodes) sw.WriteLine(vNodes.Data.b.ToString());
                            else sw.WriteLine("0");

                            if (hasWeights) sw.WriteLine(vWeights.Data.b.ToString("F6"));
                            else sw.WriteLine("0.000000");

                            sw.WriteLine(vTex.Data.x.ToString("F6"));
                            sw.WriteLine(vTex.Data.y.ToString("F6"));
                            sw.WriteLine("0.000000"); //unknown, W coord?
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
                        var part = Model.ModelSections[permutation.PieceIndex];

                        foreach (var submesh in part.Submeshes)
                            count += GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, Model.IndexInfoList[part.FacesIndex].FaceFormat).Count;
                    }
                }

                sw.WriteLine(count / 3);

                foreach (var region in rList)
                {
                    foreach (var permutation in region.Permutations)
                    {
                        if (!PartIndices.Contains(permutation.PieceIndex) && permutation.Name == perm) continue;
                        var part = Model.ModelSections[permutation.PieceIndex];
                        foreach (var submesh in part.Submeshes)
                        {
                            var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, Model.IndexInfoList[part.FacesIndex].FaceFormat);
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

        public static void WriteAMF(string Filename, CacheFile Cache, render_model Model, List<int> PartIndices)
        {
            if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();
            if (!Filename.EndsWith(".amf")) Filename += ".amf";

            if (!Model.RawLoaded) Model.LoadRaw();

            var fs = new FileStream(Filename, FileMode.Create, FileAccess.Write);
            var bw = new BinaryWriter(fs);

            Dictionary<int, int> dupeDic = new Dictionary<int, int>();

            #region Address Lists
            var headerAddressList = new List<int>();
            var headerValueList = new List<int>();

            var markerAddressList = new List<int>();
            var markerValueList = new List<int>();

            var permAddressList = new List<int>();
            var permValueList = new List<int>();

            var vertAddressList = new List<int>();
            var vertValueList = new List<int>();

            var indxAddressList = new List<int>();
            var indxValueList = new List<int>();

            var meshAddressList = new List<int>();
            var meshValueList = new List<int>();
            #endregion

            var regions = new List<render_model.Region>();
            var perms = new List<render_model.Region.Permutation>();
            foreach (var region in Model.Regions)
            {
                foreach (render_model.Region.Permutation perm in region.Permutations)
                {
                    if (PartIndices.Contains(perm.PieceIndex))
                    {
                        if (!regions.Contains(region)) regions.Add(region);
                        perms.Add(perm);
                    }
                }
            }

            #region Header
            bw.Write("AMF!".ToCharArray());
            bw.Write(2.0f); //format version
            bw.Write((Model.Name + "\0").ToCharArray());

            bw.Write(Model.Nodes.Count);
            headerAddressList.Add((int)bw.BaseStream.Position);
            bw.Write(0);

            bw.Write(Model.MarkerGroups.Count);
            headerAddressList.Add((int)bw.BaseStream.Position);
            bw.Write(0);

            bw.Write(regions.Count);
            headerAddressList.Add((int)bw.BaseStream.Position);
            bw.Write(0);

            bw.Write(Model.Shaders.Count);
            headerAddressList.Add((int)bw.BaseStream.Position);
            bw.Write(0);
            #endregion
            #region Nodes
            headerValueList.Add((int)bw.BaseStream.Position);
            foreach (var node in Model.Nodes)
            {
                bw.Write((node.Name + "\0").ToCharArray());
                bw.Write((short)node.ParentIndex);
                bw.Write((short)node.FirstChildIndex);
                bw.Write((short)node.NextSiblingIndex);
                bw.Write(node.Position.x * 100);
                bw.Write(node.Position.y * 100);
                bw.Write(node.Position.z * 100);
                bw.Write(node.Rotation.i);
                bw.Write(node.Rotation.j);
                bw.Write(node.Rotation.k);
                bw.Write(node.Rotation.w);
                //bw.Write(node.TransformScale);
                //bw.Write(node.SkewX.x);
                //bw.Write(node.SkewX.y);
                //bw.Write(node.SkewX.z);
                //bw.Write(node.SkewY.x);
                //bw.Write(node.SkewY.y);
                //bw.Write(node.SkewY.z);
                //bw.Write(node.SkewZ.x);
                //bw.Write(node.SkewZ.y);
                //bw.Write(node.SkewZ.z);
                //bw.Write(node.Center.x);
                //bw.Write(node.Center.y);
                //bw.Write(node.Center.z);
                //bw.Write(node.DistanceFromParent);
            }
            #endregion
            #region Marker Groups
            headerValueList.Add((int)bw.BaseStream.Position);
            foreach (var group in Model.MarkerGroups)
            {
                bw.Write((group.Name + "\0").ToCharArray());
                bw.Write(group.Markers.Count);
                markerAddressList.Add((int)bw.BaseStream.Position);
                bw.Write(0);
            }
            #endregion
            #region Markers
            foreach (var group in Model.MarkerGroups)
            {
                markerValueList.Add((int)bw.BaseStream.Position);
                foreach (var marker in group.Markers)
                {
                    bw.Write((byte)marker.RegionIndex);
                    bw.Write((byte)marker.PermutationIndex);
                    bw.Write((short)marker.NodeIndex);
                    bw.Write(marker.Position.x * 100);
                    bw.Write(marker.Position.y * 100);
                    bw.Write(marker.Position.z * 100);
                    bw.Write(marker.Rotation.i);
                    bw.Write(marker.Rotation.j);
                    bw.Write(marker.Rotation.k);
                    bw.Write(marker.Rotation.w);
                }
            }
            #endregion
            #region Regions
            headerValueList.Add((int)bw.BaseStream.Position);
            foreach (var region in regions)
            {
                bw.Write((region.Name + "\0").ToCharArray());

                int count = 0;
                foreach (var perm in region.Permutations)
                    if (PartIndices.Contains(perm.PieceIndex)) count++;

                bw.Write(count);
                permAddressList.Add((int)bw.BaseStream.Position);
                bw.Write(0);
            }
            #endregion
            #region Permutations
            foreach (var region in regions)
            {
                permValueList.Add((int)bw.BaseStream.Position);
                foreach (var perm in region.Permutations)
                {
                    if (!PartIndices.Contains(perm.PieceIndex)) continue;

                    var part = Model.ModelSections[perm.PieceIndex];
                    VertexValue v;
                    bool hasNodes = part.Vertices[0].TryGetValue("blendindices", 0, out v) && part.NodeIndex == 255;
                    bool isBoned = part.Vertices[0].FormatName.Contains("rigid_boned");

                    bw.Write((perm.Name + "\0").ToCharArray());
                    if (isBoned) bw.Write((byte)2);
                    else bw.Write(hasNodes ? (byte)1 : (byte)0);
                    bw.Write((byte)part.NodeIndex);

                    bw.Write(part.Vertices.Length);
                    vertAddressList.Add((int)bw.BaseStream.Position);
                    bw.Write(0);

                    int count = 0;
                    foreach (var submesh in part.Submeshes)
                        count += GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, Model.IndexInfoList[part.FacesIndex].FaceFormat).Count / 3;

                    bw.Write(count);
                    indxAddressList.Add((int)bw.BaseStream.Position);
                    bw.Write(0);

                    bw.Write(part.Submeshes.Count);
                    meshAddressList.Add((int)bw.BaseStream.Position);
                    bw.Write(0);

                    bw.Write(float.NaN); //no transforms (render_models are pre-transformed)
                }
            }
            #endregion
            #region Vertices
            foreach (var perm in perms)
            {
                var part = Model.ModelSections[perm.PieceIndex];

                int address;
                if (dupeDic.TryGetValue(part.VertsIndex, out address))
                {
                    vertValueList.Add(address);
                    continue;
                }
                else
                    dupeDic.Add(part.VertsIndex, (int)bw.BaseStream.Position);

                VertexValue v;
                bool hasNodes = part.Vertices[0].TryGetValue("blendindices", 0, out v) && part.NodeIndex == 255;
                bool isBoned = part.Vertices[0].FormatName.Contains("rigid_boned");

                vertValueList.Add((int)bw.BaseStream.Position);

                foreach (Vertex vert in part.Vertices)
                {
                    vert.TryGetValue("position", 0, out v);
                    bw.Write(v.Data.x * 100);
                    bw.Write(v.Data.y * 100);
                    bw.Write(v.Data.z * 100);

                    vert.TryGetValue("normal", 0, out v);
                    bw.Write(v.Data.i);
                    bw.Write(v.Data.j);
                    bw.Write(v.Data.k);

                    vert.TryGetValue("texcoords", 0, out v);
                    bw.Write(v.Data.x);
                    bw.Write(v.Data.y);

                    if (isBoned)
                    {
                        VertexValue i;
                        var indices = new List<int>();
                        vert.TryGetValue("blendindices", 0, out i);

                        if (!indices.Contains((int)i.Data.a) && i.Data.a != 0) indices.Add((int)i.Data.a);
                        if (!indices.Contains((int)i.Data.b) && i.Data.a != 0) indices.Add((int)i.Data.b);
                        if (!indices.Contains((int)i.Data.c) && i.Data.a != 0) indices.Add((int)i.Data.c);
                        if (!indices.Contains((int)i.Data.d) && i.Data.a != 0) indices.Add((int)i.Data.d);

                        if (indices.Count == 0) indices.Add(0);

                        foreach (int index in indices) bw.Write((byte)index);

                        if (indices.Count < 4) bw.Write((byte)255);

                        continue;
                    }

                    if (hasNodes)
                    {
                        VertexValue i, w;
                        vert.TryGetValue("blendindices", 0, out i);
                        vert.TryGetValue("blendweight", 0, out w);
                        int count = 0;
                        if (w.Data.a > 0)
                        {
                            bw.Write((byte)i.Data.a);
                            count++;
                        }
                        if (w.Data.b > 0)
                        {
                            bw.Write((byte)i.Data.b);
                            count++;
                        }
                        if (w.Data.c > 0)
                        {
                            bw.Write((byte)i.Data.c);
                            count++;
                        }
                        if (w.Data.d > 0)
                        {
                            bw.Write((byte)i.Data.d);
                            count++;
                        }

                        if (count == 0) throw new Exception("no weights on a weighted node. report this.");

                        if (count != 4) bw.Write((byte)255);

                        if (w.Data.a > 0) bw.Write(w.Data.a);
                        if (w.Data.b > 0) bw.Write(w.Data.b);
                        if (w.Data.c > 0) bw.Write(w.Data.c);
                        if (w.Data.d > 0) bw.Write(w.Data.d);
                    }
                }
            }
            #endregion

            dupeDic.Clear();

            #region Indices
            foreach (var perm in perms)
            {
                var part = Model.ModelSections[perm.PieceIndex];

                int address;
                if (dupeDic.TryGetValue(part.FacesIndex, out address))
                {
                    indxValueList.Add(address);
                    continue;
                }
                else
                    dupeDic.Add(part.FacesIndex, (int)bw.BaseStream.Position);

                indxValueList.Add((int)bw.BaseStream.Position);

                foreach (var submesh in part.Submeshes)
                {
                    var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, Model.IndexInfoList[part.FacesIndex].FaceFormat);
                    foreach (var index in indices)
                    {
                        if (part.Vertices.Length > 0xFFFF) bw.Write(index);
                        else bw.Write((ushort)index);
                    }
                }

            }
            #endregion
            #region Submeshes
            foreach (var perm in perms)
            {
                var part = Model.ModelSections[perm.PieceIndex];
                meshValueList.Add((int)bw.BaseStream.Position);
                int tCount = 0;
                foreach (var mesh in part.Submeshes)
                {

                    int sCount = GetTriangleList(part.Indices, mesh.FaceIndex, mesh.FaceCount, Model.IndexInfoList[part.FacesIndex].FaceFormat).Count / 3;

                    bw.Write((short)mesh.ShaderIndex);
                    bw.Write(tCount);
                    bw.Write(sCount);

                    tCount += sCount;
                }
            }
            #endregion
            #region Shaders
            headerValueList.Add((int)bw.BaseStream.Position);
            foreach (var shaderBlock in Model.Shaders)
            {
                //skip null shaders
                if (shaderBlock.tagID == -1)
                {
                    bw.Write("null\0".ToCharArray());
                    for (int i = 0; i < 8; i++)
                        bw.Write("null\0".ToCharArray());

                    for (int i = 0; i < 4; i++)
                        bw.Write(0);

                    bw.Write(Convert.ToByte(false));
                    bw.Write(Convert.ToByte(false));

                    continue;
                }

                var rmshTag = Cache.IndexItems.GetItemByID(shaderBlock.tagID);
                var rmsh = DefinitionsManager.rmsh(Cache, rmshTag);
                string shaderName = rmshTag.Filename.Substring(rmshTag.Filename.LastIndexOf("\\") + 1) + "\0";
                string[] paths = new string[8] { "null\0", "null\0", "null\0", "null\0", "null\0", "null\0", "null\0", "null\0" };
                float[] uTiles = new float[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
                float[] vTiles = new float[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
                int[] tints = new int[4] { -1, -1, -1, -1 };
                bool isTransparent = false;
                bool ccOnly = false;

                //Halo4 fucked this up
                if (Cache.Version >= DefinitionSet.Halo3Beta && Cache.Version <= DefinitionSet.HaloReachRetail)
                {
                    var rmt2Tag = Cache.IndexItems.GetItemByID(rmsh.Properties[0].TemplateTagID);
                    var rmt2 = DefinitionsManager.rmt2(Cache, rmt2Tag);

                    for (int i = 0; i < rmt2.UsageBlocks.Count; i++)
                    {
                        var s = rmt2.UsageBlocks[i].Usage;
                        var bitmTag = Cache.IndexItems.GetItemByID(rmsh.Properties[0].ShaderMaps[i].BitmapTagID);

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

                    for (int i = 0; i < rmt2.ArgumentBlocks.Count; i++)
                    {
                        var s = rmt2.ArgumentBlocks[i].Argument;

                        switch (s)
                        {
                            //case "env_tint_color":
                            //case "fresnel_color":
                            case "albedo_color":
                                tints[0] = i;
                                break;

                            case "self_illum_color":
                                tints[1] = i;
                                break;

                            case "specular_tint":
                                tints[2] = i;
                                break;
                        }
                    }

                    short[] tiles = new short[8] { -1, -1, -1, -1, -1, -1, -1, -1 };

                    foreach (var map in rmsh.Properties[0].ShaderMaps)
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
                    try 
                    { 
                        paths[0] = Cache.IndexItems.GetItemByID(rmsh.Properties[0].ShaderMaps[0].BitmapTagID).Filename + "\0";
                        uTiles[0] = rmsh.Properties[0].Tilings[rmsh.Properties[0].ShaderMaps[0].TilingIndex].UTiling;
                        vTiles[0] = rmsh.Properties[0].Tilings[rmsh.Properties[0].ShaderMaps[0].TilingIndex].VTiling;
                    }
                    catch { }

                if (rmshTag.ClassCode != "rmsh" && rmshTag.ClassCode != "mat")
                {
                    isTransparent = true;
                    if (paths[0] == "null\0" && paths[2] != "null\0")
                        ccOnly = true;
                }

                bw.Write(shaderName.ToCharArray());
                for (int i = 0; i < 8; i++)
                {
                    bw.Write(paths[i].ToCharArray());
                    if (paths[i] != "null\0")
                    {
                        bw.Write(uTiles[i]);
                        bw.Write(vTiles[i]);
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    if (tints[i] == -1)
                    {
                        bw.Write(0);
                        continue;
                    }

                    bw.Write((byte)(255f * rmsh.Properties[0].Tilings[tints[i]].UTiling));
                    bw.Write((byte)(255f * rmsh.Properties[0].Tilings[tints[i]].VTiling));
                    bw.Write((byte)(255f * rmsh.Properties[0].Tilings[tints[i]].Unknown0));
                    bw.Write((byte)(255f * rmsh.Properties[0].Tilings[tints[i]].Unknown1));
                }

                bw.Write(Convert.ToByte(isTransparent));
                bw.Write(Convert.ToByte(ccOnly));
            }
            #endregion
            #region Write Addresses
            for (int i = 0; i < headerAddressList.Count; i++)
            {
                bw.BaseStream.Position = headerAddressList[i];
                bw.Write(headerValueList[i]);
            }

            for (int i = 0; i < markerAddressList.Count; i++)
            {
                bw.BaseStream.Position = markerAddressList[i];
                bw.Write(markerValueList[i]);
            }

            for (int i = 0; i < permAddressList.Count; i++)
            {
                bw.BaseStream.Position = permAddressList[i];
                bw.Write(permValueList[i]);
            }

            for (int i = 0; i < vertAddressList.Count; i++)
            {
                bw.BaseStream.Position = vertAddressList[i];
                bw.Write(vertValueList[i]);
            }

            for (int i = 0; i < indxAddressList.Count; i++)
            {
                bw.BaseStream.Position = indxAddressList[i];
                bw.Write(indxValueList[i]);
            }

            for (int i = 0; i < meshAddressList.Count; i++)
            {
                bw.BaseStream.Position = meshAddressList[i];
                bw.Write(meshValueList[i]);
            }
            #endregion

            bw.Close();
            bw.Dispose();
        }

        public static void WriteEMF3(string Filename, CacheFile Cache, scenario_structure_bsp BSP, List<int> ClusterIndices, List<int> InstanceIndices)
        {
            if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();
            if (!Filename.EndsWith(".emf")) Filename += ".emf";

            if (!BSP.RawLoaded) BSP.LoadRaw();

            var fs = new FileStream(Filename, FileMode.Create, FileAccess.Write);
            var bw = new BinaryWriter(fs);

            bw.Write(560360805);
            bw.Write(3);
            bw.Write((BSP.BSPName + "\0").ToCharArray());

            #region Nodes
            bw.Write(0);
            #endregion
            #region Markers
            bw.Write(0);
            #endregion
            #region Lists
            var clusters = new List<scenario_structure_bsp.Cluster>();
            var igs = new List<scenario_structure_bsp.InstancedGeometry>();

            foreach (var cluster in BSP.Clusters)
            {
                if (ClusterIndices.Contains(BSP.Clusters.IndexOf(cluster)) && BSP.ModelSections[cluster.SectionIndex].Submeshes.Count > 0)
                    clusters.Add(cluster);
            }

            foreach (var geom in BSP.GeomInstances)
            {
                if (InstanceIndices.Contains(BSP.GeomInstances.IndexOf(geom)) && BSP.ModelSections[geom.SectionIndex].Submeshes.Count > 0)
                    igs.Add(geom);
            }

            int rCount = 0;
            if (clusters.Count > 0) rCount++;
            if (igs.Count > 0) rCount++;
            #endregion
            #region Regions
            bw.Write(rCount);

            bw.Write("Clusters\0".ToCharArray());
            bw.Write(clusters.Count);
            foreach (scenario_structure_bsp.Cluster cluster in clusters)
            {
                var part = BSP.ModelSections[cluster.SectionIndex];

                bw.Write((BSP.Clusters.IndexOf(cluster).ToString("D3") + "\0").ToCharArray());

                VertexValue v;
                bw.Write((byte)1); //no nodes

                bw.Write(part.Vertices.Length);
                foreach (Vertex vert in part.Vertices)
                {
                    vert.TryGetValue("position", 0, out v);
                    bw.Write(v.Data.x);
                    bw.Write(v.Data.y);
                    bw.Write(v.Data.z);

                    vert.TryGetValue("normal", 0, out v);
                    bw.Write(v.Data.i);
                    bw.Write(v.Data.j);
                    bw.Write(v.Data.k);

                    vert.TryGetValue("texcoords", 0, out v);
                    bw.Write(v.Data.x);
                    bw.Write(v.Data.y);
                }

                bw.Write(1);

                int count = 0;
                foreach (var submesh in part.Submeshes)
                    count += GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, BSP.IndexInfoList[part.FacesIndex].FaceFormat).Count;

                bw.Write(count / 3);
                foreach (var submesh in part.Submeshes)
                {
                    var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, BSP.IndexInfoList[part.FacesIndex].FaceFormat);
                    for (int i = 0; i < indices.Count; i += 3)
                    {
                        if (part.Vertices.Length > 0xFFFF)
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

            bw.Write("Instances\0".ToCharArray());
            bw.Write(igs.Count);
            foreach (scenario_structure_bsp.InstancedGeometry geom in igs)
            {
                var part = BSP.ModelSections[geom.SectionIndex];

                bw.Write((geom.Name + "\0").ToCharArray());

                VertexValue v;
                bw.Write((byte)1); //no nodes

                bw.Write(part.Vertices.Length);
                foreach (Vertex vert in DeepClone(part.Vertices))
                {
                    vert.TryGetValue("position", 0, out v);
                    v.Data *= geom.TransformScale;
                    v.Data.Point3DTransform(geom.TransformMatrix);
                    bw.Write(v.Data.x);
                    bw.Write(v.Data.y);
                    bw.Write(v.Data.z);

                    vert.TryGetValue("normal", 0, out v);
                    v.Data *= geom.TransformScale;
                    v.Data.Vector3DTransform(geom.TransformMatrix);
                    bw.Write(v.Data.i);
                    bw.Write(v.Data.j);
                    bw.Write(v.Data.k);

                    vert.TryGetValue("texcoords", 0, out v);
                    bw.Write(v.Data.x);
                    bw.Write(v.Data.y);
                }

                bw.Write(1);

                int count = 0;
                foreach (var submesh in part.Submeshes)
                    count += GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, BSP.IndexInfoList[part.FacesIndex].FaceFormat).Count;

                bw.Write(count / 3);
                foreach (var submesh in part.Submeshes)
                {
                    var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, BSP.IndexInfoList[part.FacesIndex].FaceFormat);
                    for (int i = 0; i < indices.Count; i += 3)
                    {
                        if (part.Vertices.Length > 0xFFFF)
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
            #endregion
            #region Shaders
            bw.Write(BSP.Shaders.Count);
            foreach (render_model.Shader shaderBlock in BSP.Shaders)
            {
                //skip null shaders
                if (shaderBlock.tagID == -1)
                {
                    bw.Write("null\0".ToCharArray());
                    for (int i = 0; i < 8; i++)
                        bw.Write("null\0".ToCharArray());

                    bw.Write(Convert.ToByte(false));
                    bw.Write(Convert.ToByte(false));

                    continue;
                }

                var rmshTag = Cache.IndexItems.GetItemByID(shaderBlock.tagID);
                var rmsh = DefinitionsManager.rmsh(Cache, rmshTag);
                string shaderName = rmshTag.Filename.Substring(rmshTag.Filename.LastIndexOf("\\") + 1) + "\0";
                string[] paths = new string[8] { "null\0", "null\0", "null\0", "null\0", "null\0", "null\0", "null\0", "null\0" };
                float[] uTiles = new float[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
                float[] vTiles = new float[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
                bool isTransparent = false;
                bool ccOnly = false;

                //Halo4 fucked this up
                if (Cache.Version >= DefinitionSet.Halo3Beta && Cache.Version <= DefinitionSet.HaloReachRetail)
                {
                    var rmt2Tag = Cache.IndexItems.GetItemByID(rmsh.Properties[0].TemplateTagID);
                    var rmt2 = DefinitionsManager.rmt2(Cache, rmt2Tag);

                    for (int i = 0; i < rmt2.UsageBlocks.Count; i++)
                    {
                        var s = rmt2.UsageBlocks[i].Usage;
                        var bitmTag = Cache.IndexItems.GetItemByID(rmsh.Properties[0].ShaderMaps[i].BitmapTagID);

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

                    foreach (var map in rmsh.Properties[0].ShaderMaps)
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

                if (rmshTag.ClassCode != "rmsh" && rmshTag.ClassCode != "mat")
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

        public static void WriteOBJ(string Filename, CacheFile Cache, scenario_structure_bsp BSP, List<int> ClusterIndices, List<int> InstanceIndices)
        {
            if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();

            if (!BSP.RawLoaded) BSP.LoadRaw();

            StreamWriter sw = new StreamWriter(Filename);

            sw.WriteLine("# ---------------------------------------");
            sw.WriteLine("# Halo x360 BSP - Extracted with Adjutant");
            sw.WriteLine("# ---------------------------------------");

            foreach (var cluster in BSP.Clusters)
            {
                if (!ClusterIndices.Contains(BSP.Clusters.IndexOf(cluster))) continue;

                var part = BSP.ModelSections[cluster.SectionIndex];

                if (part.Submeshes.Count == 0) continue;

                VertexValue v;

                foreach (var vert in part.Vertices)
                {
                    vert.TryGetValue("position", 0, out v);
                    sw.WriteLine("v  {0} {1} {2}", v.Data.x * 100, v.Data.y * 100, v.Data.z * 100);
                }

                foreach (var vert in part.Vertices)
                {
                    vert.TryGetValue("texcoords", 0, out v);
                    sw.WriteLine("vt {0} {1}", v.Data.x, v.Data.y);
                }

                foreach (var vert in part.Vertices)
                {
                    vert.TryGetValue("normal", 0, out v);
                    sw.WriteLine("vn {0} {1} {2}", v.Data.i, v.Data.j, v.Data.k);
                }
            }

            foreach (var geom in BSP.GeomInstances)
            {
                if (!InstanceIndices.Contains(BSP.GeomInstances.IndexOf(geom))) continue;

                var part = BSP.ModelSections[geom.SectionIndex];

                if (part.Submeshes.Count == 0) continue;

                VertexValue v;
                var verts = DeepClone(part.Vertices);

                foreach (var vert in verts)
                {
                    vert.TryGetValue("position", 0, out v);
                    v.Data *= geom.TransformScale;
                    v.Data.Point3DTransform(geom.TransformMatrix);
                    sw.WriteLine("v  {0} {1} {2}", v.Data.x * 100, v.Data.y * 100, v.Data.z * 100);
                }

                foreach (var vert in verts)
                {
                    vert.TryGetValue("texcoords", 0, out v);
                    sw.WriteLine("vt {0} {1}", v.Data.x, v.Data.y);
                }

                foreach (var vert in verts)
                {
                    vert.TryGetValue("normal", 0, out v);
                    v.Data *= geom.TransformScale;
                    v.Data.Vector3DTransform(geom.TransformMatrix);
                    sw.WriteLine("vn {0} {1} {2}", v.Data.i, v.Data.j, v.Data.k);
                }
            }

            int position = 1;

            foreach (var cluster in BSP.Clusters)
            {
                if (!ClusterIndices.Contains(BSP.Clusters.IndexOf(cluster))) continue;

                var part = BSP.ModelSections[cluster.SectionIndex];

                if (part.Submeshes.Count == 0) continue;

                sw.WriteLine("g Clusters-" + BSP.Clusters.IndexOf(cluster).ToString("D3"));

                foreach (var submesh in part.Submeshes)
                {
                    var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, BSP.IndexInfoList[part.FacesIndex].FaceFormat);
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
                position += BSP.VertInfoList[part.VertsIndex].VertexCount;
            }

            foreach (var geom in BSP.GeomInstances)
            {
                if (!InstanceIndices.Contains(BSP.GeomInstances.IndexOf(geom))) continue;

                var part = BSP.ModelSections[geom.SectionIndex];

                if (part.Submeshes.Count == 0) continue;

                sw.WriteLine("g Instances-" + geom.Name);

                foreach (var submesh in part.Submeshes)
                {
                    var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, BSP.IndexInfoList[part.FacesIndex].FaceFormat);
                    if (geom.TransformScale < 0) indices.Reverse();
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
                position += BSP.VertInfoList[part.VertsIndex].VertexCount;

            }

            sw.Close();
            sw.Dispose();
        }

        public static void WriteAMF(string Filename, CacheFile Cache, scenario_structure_bsp BSP, List<int> ClusterIndices, List<int> InstanceIndices)
        {
            if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();
            if (!Filename.EndsWith(".amf")) Filename += ".amf";

            if (!BSP.RawLoaded) BSP.LoadRaw();

            var fs = new FileStream(Filename, FileMode.Create, FileAccess.Write);
            var bw = new BinaryWriter(fs);

            var dupeDic = new Dictionary<int, int>();

            #region Address Lists
            var headerAddressList = new List<int>();
            var headerValueList = new List<int>();

            var markerAddressList = new List<int>();
            var markerValueList = new List<int>();

            var permAddressList = new List<int>();
            var permValueList = new List<int>();

            var vertAddressList = new List<int>();
            var vertValueList = new List<int>();

            var indxAddressList = new List<int>();
            var indxValueList = new List<int>();

            var meshAddressList = new List<int>();
            var meshValueList = new List<int>();
            #endregion

            var clusters = new List<scenario_structure_bsp.Cluster>();
            var igs = new List<scenario_structure_bsp.InstancedGeometry>();

            foreach (var cluster in BSP.Clusters)
            {
                if (ClusterIndices.Contains(BSP.Clusters.IndexOf(cluster)) && BSP.ModelSections[cluster.SectionIndex].Submeshes.Count > 0)
                    clusters.Add(cluster);
            }

            foreach (var geom in BSP.GeomInstances)
            {
                if (InstanceIndices.Contains(BSP.GeomInstances.IndexOf(geom)) && BSP.ModelSections[geom.SectionIndex].Submeshes.Count > 0)
                    igs.Add(geom);
            }

            int rCount = 0;
            if (clusters.Count > 0) rCount++;
            if (igs.Count > 0) rCount++;

            #region Header
            bw.Write("AMF!".ToCharArray());
            bw.Write(2.0f); //format version
            bw.Write((BSP.BSPName + "\0").ToCharArray());

            bw.Write(0); //nodes
            //headerAddressList.Add((int)bw.BaseStream.Position);
            bw.Write(0);

            bw.Write(0); //marker groups
            //headerAddressList.Add((int)bw.BaseStream.Position);
            bw.Write(0);

            bw.Write(rCount);
            headerAddressList.Add((int)bw.BaseStream.Position);
            bw.Write(0);

            bw.Write(BSP.Shaders.Count);
            headerAddressList.Add((int)bw.BaseStream.Position);
            bw.Write(0);
            #endregion
            #region Regions
            headerValueList.Add((int)bw.BaseStream.Position);
            if (clusters.Count > 0)
            {
                bw.Write(("Clusters" + "\0").ToCharArray());
                bw.Write(clusters.Count);
                permAddressList.Add((int)bw.BaseStream.Position);
                bw.Write(0);
            }

            if (igs.Count > 0)
            {
                bw.Write(("Instances" + "\0").ToCharArray());
                bw.Write(igs.Count);
                permAddressList.Add((int)bw.BaseStream.Position);
                bw.Write(0);
            }
            #endregion
            #region Permutations
            permValueList.Add((int)bw.BaseStream.Position);
            foreach (var cluster in clusters)
            {
                var part = BSP.ModelSections[cluster.SectionIndex];

                bw.Write((BSP.Clusters.IndexOf(cluster).ToString("D3") + "\0").ToCharArray());
                bw.Write((byte)0);   //not weighted
                bw.Write((byte)255); //no node index

                bw.Write(part.Vertices.Length);
                vertAddressList.Add((int)bw.BaseStream.Position);
                bw.Write(0);

                int count = 0;
                foreach (var submesh in part.Submeshes)
                    count += GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, BSP.IndexInfoList[part.FacesIndex].FaceFormat).Count / 3;

                bw.Write(count);
                indxAddressList.Add((int)bw.BaseStream.Position);
                bw.Write(0);

                bw.Write(part.Submeshes.Count);
                meshAddressList.Add((int)bw.BaseStream.Position);
                bw.Write(0);

                bw.Write(float.NaN); //no transform
            }

            permValueList.Add((int)bw.BaseStream.Position);
            foreach (var geom in igs)
            {
                var part = BSP.ModelSections[geom.SectionIndex];

                bw.Write((geom.Name + "\0").ToCharArray());

                byte vType = 0;      //not weighted
                //if (part.Vertices[0].FormatName == "s_rigid_vertex") vType += 32;
                bw.Write(vType);
                bw.Write((byte)255); //no node index

                bw.Write(part.Vertices.Length);
                vertAddressList.Add((int)bw.BaseStream.Position);
                bw.Write(0);

                int count = 0;
                foreach (var submesh in part.Submeshes)
                    count += GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, BSP.IndexInfoList[part.FacesIndex].FaceFormat).Count / 3;

                bw.Write(count);
                indxAddressList.Add((int)bw.BaseStream.Position);
                bw.Write(0);

                bw.Write(part.Submeshes.Count);
                meshAddressList.Add((int)bw.BaseStream.Position);
                bw.Write(0);

                bw.Write(geom.TransformScale);
                bw.Write(geom.TransformMatrix.m11);
                bw.Write(geom.TransformMatrix.m12);
                bw.Write(geom.TransformMatrix.m13);
                bw.Write(geom.TransformMatrix.m21);
                bw.Write(geom.TransformMatrix.m22);
                bw.Write(geom.TransformMatrix.m23);
                bw.Write(geom.TransformMatrix.m31);
                bw.Write(geom.TransformMatrix.m32);
                bw.Write(geom.TransformMatrix.m33);
                bw.Write(geom.TransformMatrix.m41);
                bw.Write(geom.TransformMatrix.m42);
                bw.Write(geom.TransformMatrix.m43);
            }
            #endregion
            #region Vertices

            #region Clusters
            foreach (var clust in clusters)
            {
                var part = BSP.ModelSections[clust.SectionIndex];

                vertValueList.Add((int)bw.BaseStream.Position);

                VertexValue v;
                foreach (Vertex vert in part.Vertices)
                {
                    vert.TryGetValue("position", 0, out v);
                    bw.Write(v.Data.x * 100);
                    bw.Write(v.Data.y * 100);
                    bw.Write(v.Data.z * 100);

                    vert.TryGetValue("normal", 0, out v);
                    bw.Write(v.Data.i);
                    bw.Write(v.Data.j);
                    bw.Write(v.Data.k);

                    vert.TryGetValue("texcoords", 0, out v);
                    bw.Write(v.Data.x);
                    bw.Write(v.Data.y);
                }
            }
            #endregion

            #region Instances
            foreach (var geom in igs)
            {
                var part = BSP.ModelSections[geom.SectionIndex];

                int address;
                if (dupeDic.TryGetValue(part.VertsIndex, out address))
                {
                    vertValueList.Add(address);
                    continue;
                }
                else
                    dupeDic.Add(part.VertsIndex, (int)bw.BaseStream.Position);

                vertValueList.Add((int)bw.BaseStream.Position);

                //part.Vertices[0].FormatName == "s_rigid_vertex"
                //var bb = new render_model.BoundingBox();
                //bb.XBounds = bb.YBounds = bb.ZBounds = new RealBounds(0, 1);
                //bb.UBounds = bb.VBounds = new RealBounds(-1, 1);
                //if (geom.SectionIndex < BSP.BoundingBoxes.Count) bb = BSP.BoundingBoxes[geom.SectionIndex];

                //bool compress = part.Vertices[0].FormatName == "s_rigid_vertex";
                //compress = false;
                //if (compress)
                //{
                //    bw.Write(bb.XBounds.Min);
                //    bw.Write(bb.XBounds.Max);
                //    bw.Write(bb.YBounds.Min);
                //    bw.Write(bb.YBounds.Max);
                //    bw.Write(bb.ZBounds.Min);
                //    bw.Write(bb.ZBounds.Max);
                //    bw.Write(bb.UBounds.Min);
                //    bw.Write(bb.UBounds.Max);
                //    bw.Write(bb.VBounds.Min);
                //    bw.Write(bb.VBounds.Max);
                //}

                VertexValue v;
                foreach (Vertex vert in part.Vertices)
                {
                    //if (compress)
                    //{
                    //    vert.TryGetValue("position", 0, out v);
                    //    bw.Write((ushort)Math.Round((((v.Data.x - bb.XBounds.Min) / bb.XBounds.Length) * 0xFFFF), 0));
                    //    bw.Write((ushort)Math.Round((((v.Data.y - bb.YBounds.Min) / bb.YBounds.Length) * 0xFFFF), 0));
                    //    bw.Write((ushort)Math.Round((((v.Data.z - bb.ZBounds.Min) / bb.ZBounds.Length) * 0xFFFF), 0));

                    //    vert.TryGetValue("normal", 0, out v);
                    //    bw.Write(0);
                    //    //bw.Write(v.Data.i);
                    //    //bw.Write(v.Data.k);
                    //    //bw.Write(v.Data.j);

                    //    vert.TryGetValue("texcoords", 0, out v);
                    //    bw.Write((ushort)Math.Round((((v.Data.x - bb.UBounds.Min) / bb.UBounds.Length) * 0xFFFF), 0));
                    //    bw.Write((ushort)Math.Round((((v.Data.y - bb.VBounds.Min) / bb.VBounds.Length) * 0xFFFF), 0));
                    //}
                    //else
                    {
                        vert.TryGetValue("position", 0, out v);
                        bw.Write(v.Data.x);
                        bw.Write(v.Data.y);
                        bw.Write(v.Data.z);

                        vert.TryGetValue("normal", 0, out v);
                        bw.Write(v.Data.i);
                        bw.Write(v.Data.j);
                        bw.Write(v.Data.k);

                        vert.TryGetValue("texcoords", 0, out v);
                        bw.Write(v.Data.x);
                        bw.Write(v.Data.y);
                    }
                }
            }
            #endregion

            #endregion

            dupeDic.Clear();

            #region Indices

            #region Clusters
            foreach (var clust in clusters)
            {
                var part = BSP.ModelSections[clust.SectionIndex];

                indxValueList.Add((int)bw.BaseStream.Position);

                foreach (var submesh in part.Submeshes)
                {
                    var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, BSP.IndexInfoList[part.FacesIndex].FaceFormat);
                    foreach (var index in indices)
                    {
                        if (part.Vertices.Length > 0xFFFF) bw.Write(index);
                        else bw.Write((ushort)index);
                    }
                }

            }
            #endregion

            #region Instances
            foreach (var geom in igs)
            {
                var part = BSP.ModelSections[geom.SectionIndex];

                int address;
                if (dupeDic.TryGetValue(part.FacesIndex, out address))
                {
                    indxValueList.Add(address);
                    continue;
                }
                else
                    dupeDic.Add(part.FacesIndex, (int)bw.BaseStream.Position);

                indxValueList.Add((int)bw.BaseStream.Position);

                foreach (var submesh in part.Submeshes)
                {
                    var indices = GetTriangleList(part.Indices, submesh.FaceIndex, submesh.FaceCount, BSP.IndexInfoList[part.FacesIndex].FaceFormat);
                    if (geom.TransformScale < 0) indices.Reverse();

                    foreach (var index in indices)
                    {
                        if (part.Vertices.Length > 0xFFFF) bw.Write(index);
                        else bw.Write((ushort)index);
                    }
                }

            }
            #endregion

            #endregion
            #region Submeshes
            foreach (var clust in clusters)
            {
                var part = BSP.ModelSections[clust.SectionIndex];
                meshValueList.Add((int)bw.BaseStream.Position);
                int tCount = 0;
                foreach (var mesh in part.Submeshes)
                {

                    int sCount = GetTriangleList(part.Indices, mesh.FaceIndex, mesh.FaceCount, BSP.IndexInfoList[part.FacesIndex].FaceFormat).Count / 3;

                    bw.Write((short)mesh.ShaderIndex);
                    bw.Write(tCount);
                    bw.Write(sCount);

                    tCount += sCount;
                }
            }

            foreach (var geom in igs)
            {
                var part = BSP.ModelSections[geom.SectionIndex];
                meshValueList.Add((int)bw.BaseStream.Position);
                int tCount = 0;
                foreach (var mesh in part.Submeshes)
                {

                    int sCount = GetTriangleList(part.Indices, mesh.FaceIndex, mesh.FaceCount, BSP.IndexInfoList[part.FacesIndex].FaceFormat).Count / 3;

                    bw.Write((short)mesh.ShaderIndex);
                    bw.Write(tCount);
                    bw.Write(sCount);

                    tCount += sCount;
                }
            }
            #endregion
            #region Shaders
            headerValueList.Add((int)bw.BaseStream.Position);
            foreach (var shaderBlock in BSP.Shaders)
            {
                //skip null shaders
                if (shaderBlock.tagID == -1)
                {
                    bw.Write("null\0".ToCharArray());
                    for (int i = 0; i < 8; i++)
                        bw.Write("null\0".ToCharArray());

                    for (int i = 0; i < 4; i++)
                        bw.Write(0);

                    bw.Write(Convert.ToByte(false));
                    bw.Write(Convert.ToByte(false));

                    continue;
                }

                var rmshTag = Cache.IndexItems.GetItemByID(shaderBlock.tagID);
                var rmsh = DefinitionsManager.rmsh(Cache, rmshTag);
                string shaderName = rmshTag.Filename.Substring(rmshTag.Filename.LastIndexOf("\\") + 1) + "\0";
                string[] paths = new string[8] { "null\0", "null\0", "null\0", "null\0", "null\0", "null\0", "null\0", "null\0" };
                float[] uTiles = new float[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
                float[] vTiles = new float[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
                int[] tints = new int[4] { -1, -1, -1, -1 };
                bool isTransparent = false;
                bool ccOnly = false;

                var baseMaps = new List<KeyValuePair<string, RealQuat>>();
                var bumpMaps = new List<KeyValuePair<string, RealQuat>>();
                var detailMaps = new List<KeyValuePair<string, RealQuat>>();
                var blendmap = new KeyValuePair<string, RealQuat>();

                //Halo4 fucked this up
                if (Cache.Version >= DefinitionSet.Halo3Beta && Cache.Version <= DefinitionSet.HaloReachRetail)
                {
                    var rmt2Tag = Cache.IndexItems.GetItemByID(rmsh.Properties[0].TemplateTagID);
                    var rmt2 = DefinitionsManager.rmt2(Cache, rmt2Tag);

                    if (rmshTag.ClassCode == "rmtr")
                    {
                        shaderName = "*" + shaderName;
                        for (int i = 0; i < rmt2.UsageBlocks.Count; i++)
                        {
                            var s = rmt2.UsageBlocks[i].Usage;
                            var map = rmsh.Properties[0].ShaderMaps[i];
                            var tileInfo = (map.TilingIndex != 255) ? rmsh.Properties[0].Tilings[map.TilingIndex] : null;
                            var bitmTag = Cache.IndexItems.GetItemByID(map.BitmapTagID);

                            if (s == "blend_map" && bitmTag != null)
                                blendmap = new KeyValuePair<string, RealQuat>(bitmTag.Filename + "\0", new RealQuat(tileInfo.UTiling, tileInfo.VTiling));

                            if (s.Contains("base_map_m_") && bitmTag != null)
                                baseMaps.Add(new KeyValuePair<string, RealQuat>(bitmTag.Filename + "\0", new RealQuat(tileInfo.UTiling, tileInfo.VTiling)));

                            if (s.Contains("bump_map_m_") && bitmTag != null)
                                bumpMaps.Add(new KeyValuePair<string, RealQuat>(bitmTag.Filename + "\0", new RealQuat(tileInfo.UTiling, tileInfo.VTiling)));

                            if (s.Contains("detail_map_m_") && bitmTag != null)
                                detailMaps.Add(new KeyValuePair<string, RealQuat>(bitmTag.Filename + "\0", new RealQuat(tileInfo.UTiling, tileInfo.VTiling)));
                        }
                    }
                    else // default shaders
                    {
                        for (int i = 0; i < rmt2.UsageBlocks.Count; i++)
                        {
                            var s = rmt2.UsageBlocks[i].Usage;
                            var bitmTag = Cache.IndexItems.GetItemByID(rmsh.Properties[0].ShaderMaps[i].BitmapTagID);

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

                        for (int i = 0; i < rmt2.ArgumentBlocks.Count; i++)
                        {
                            var s = rmt2.ArgumentBlocks[i].Argument;

                            switch (s)
                            {
                                //case "env_tint_color":
                                //case "fresnel_color":
                                case "albedo_color":
                                    tints[0] = i;
                                    break;

                                case "self_illum_color":
                                    tints[1] = i;
                                    break;

                                case "specular_tint":
                                    tints[2] = i;
                                    break;
                            }
                        }

                        short[] tiles = new short[8] { -1, -1, -1, -1, -1, -1, -1, -1 };

                        foreach (var map in rmsh.Properties[0].ShaderMaps)
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
                }
                else
                    try { paths[0] = Cache.IndexItems.GetItemByID(rmsh.Properties[0].ShaderMaps[0].BitmapTagID).Filename + "\0"; }
                    catch { }

                bw.Write(shaderName.ToCharArray());
                if (rmshTag.ClassCode == "rmtr")
                {
                    if (blendmap.Key == null)
                        bw.Write("null\0".ToCharArray());
                    else
                    {
                        bw.Write(blendmap.Key.ToCharArray());
                        bw.Write(blendmap.Value.a);
                        bw.Write(blendmap.Value.b);
                    }

                    bw.Write((byte)baseMaps.Count);
                    bw.Write((byte)bumpMaps.Count);
                    bw.Write((byte)detailMaps.Count);

                    foreach (var map in baseMaps)
                    {
                        bw.Write(map.Key.ToCharArray());
                        bw.Write(map.Value.a);
                        bw.Write(map.Value.b);
                    }

                    foreach (var map in bumpMaps)
                    {
                        bw.Write(map.Key.ToCharArray());
                        bw.Write(map.Value.a);
                        bw.Write(map.Value.b);
                    }

                    foreach (var map in detailMaps)
                    {
                        bw.Write(map.Key.ToCharArray());
                        bw.Write(map.Value.a);
                        bw.Write(map.Value.b);
                    }
                }
                else
                {
                    if (rmshTag.ClassCode != "rmsh" && rmshTag.ClassCode != "mat")
                    {
                        isTransparent = true;
                        if (paths[0] == "null\0" && paths[2] != "null\0")
                            ccOnly = true;
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        bw.Write(paths[i].ToCharArray());
                        if (paths[i] != "null\0")
                        {
                            bw.Write(uTiles[i]);
                            bw.Write(vTiles[i]);
                        }
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        if (tints[i] == -1)
                        {
                            bw.Write(0);
                            continue;
                        }

                        bw.Write((byte)(255f * rmsh.Properties[0].Tilings[tints[i]].UTiling));
                        bw.Write((byte)(255f * rmsh.Properties[0].Tilings[tints[i]].VTiling));
                        bw.Write((byte)(255f * rmsh.Properties[0].Tilings[tints[i]].Unknown0));
                        bw.Write((byte)(255f * rmsh.Properties[0].Tilings[tints[i]].Unknown1));
                    }

                    bw.Write(Convert.ToByte(isTransparent));
                    bw.Write(Convert.ToByte(ccOnly));
                }
            }
            #endregion
            #region Write Addresses
            for (int i = 0; i < headerAddressList.Count; i++)
            {
                bw.BaseStream.Position = headerAddressList[i];
                bw.Write(headerValueList[i]);
            }

            //for (int i = 0; i < markerAddressList.Count; i++)
            //{
            //    bw.BaseStream.Position = markerAddressList[i];
            //    bw.Write(markerValueList[i]);
            //}

            for (int i = 0; i < permAddressList.Count; i++)
            {
                bw.BaseStream.Position = permAddressList[i];
                bw.Write(permValueList[i]);
            }

            for (int i = 0; i < vertAddressList.Count; i++)
            {
                bw.BaseStream.Position = vertAddressList[i];
                bw.Write(vertValueList[i]);
            }

            for (int i = 0; i < indxAddressList.Count; i++)
            {
                bw.BaseStream.Position = indxAddressList[i];
                bw.Write(indxValueList[i]);
            }

            for (int i = 0; i < meshAddressList.Count; i++)
            {
                bw.BaseStream.Position = meshAddressList[i];
                bw.Write(meshValueList[i]);
            }
            #endregion

            bw.Close();
            bw.Dispose();
        }

        public static void WriteAMF(string Filename, S3DPak Pak, S3DModelBase ATPL, List<int> PartIndices)
        {
            if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();
            if (!Filename.EndsWith(".amf")) Filename += ".amf";

            var fs = new FileStream(Filename, FileMode.Create, FileAccess.Write);
            var bw = new BinaryWriter(fs);

            #region Address Lists
            var headerAddressList = new List<int>();
            var headerValueList = new List<int>();

            //var markerAddressList = new List<int>();
            //var markerValueList = new List<int>();

            var permAddressList = new List<int>();
            var permValueList = new List<int>();

            var vertAddressList = new List<int>();
            var vertValueList = new List<int>();

            var indxAddressList = new List<int>();
            var indxValueList = new List<int>();

            var meshAddressList = new List<int>();
            var meshValueList = new List<int>();
            #endregion

            var objDic = new Dictionary<string, List<int>>();

            #region Populate Dictionary
            foreach (var obj in ATPL.Objects)
            {
                if (!PartIndices.Contains(ATPL.Objects.IndexOf(obj))) continue;
                if (obj.Vertices == null || obj.Submeshes == null) continue;
                if (obj.VertCount == 0 || obj.Submeshes.Count == 0) continue;

                List<int> iList;
                if (obj.isInheritor)
                {
                    var pObj = ATPL.ObjectByID(obj.inheritIndex);

                    if (objDic.TryGetValue(pObj.Name, out iList))
                        iList.Add(ATPL.Objects.IndexOf(obj));
                    else
                    {
                        iList = new List<int>();
                        iList.Add(ATPL.Objects.IndexOf(obj));
                        objDic.Add(pObj.Name, iList);
                    }
                }
                else
                {
                    if (objDic.TryGetValue(ATPL.Name, out iList))
                        iList.Add(ATPL.Objects.IndexOf(obj));
                    else
                    {
                        iList = new List<int>();
                        iList.Add(ATPL.Objects.IndexOf(obj));
                        objDic.Add(ATPL.Name, iList);
                    }
                }
            }
            #endregion

            #region Header
            bw.Write("AMF!".ToCharArray());
            bw.Write(2.0f); //format version
            bw.Write((ATPL.Name + "\0").ToCharArray());

            bw.Write(0);
            headerAddressList.Add((int)bw.BaseStream.Position);
            bw.Write(0);

            bw.Write(0);
            headerAddressList.Add((int)bw.BaseStream.Position);
            bw.Write(0);

            bw.Write(objDic.Count);
            headerAddressList.Add((int)bw.BaseStream.Position);
            bw.Write(0);

            bw.Write(ATPL.Materials.Count);
            headerAddressList.Add((int)bw.BaseStream.Position);
            bw.Write(0);
            #endregion
            #region Nodes
            headerValueList.Add((int)bw.BaseStream.Position);
            //foreach (var node in Model.Nodes)
            //{
            //    bw.Write((node.Name + "\0").ToCharArray());
            //    bw.Write((short)node.ParentIndex);
            //    bw.Write((short)node.FirstChildIndex);
            //    bw.Write((short)node.NextSiblingIndex);
            //    bw.Write(node.Position.x * 100);
            //    bw.Write(node.Position.y * 100);
            //    bw.Write(node.Position.z * 100);
            //    bw.Write(node.Rotation.i);
            //    bw.Write(node.Rotation.j);
            //    bw.Write(node.Rotation.k);
            //    bw.Write(node.Rotation.w);
            //    //bw.Write(node.TransformScale);
            //    //bw.Write(node.SkewX.x);
            //    //bw.Write(node.SkewX.y);
            //    //bw.Write(node.SkewX.z);
            //    //bw.Write(node.SkewY.x);
            //    //bw.Write(node.SkewY.y);
            //    //bw.Write(node.SkewY.z);
            //    //bw.Write(node.SkewZ.x);
            //    //bw.Write(node.SkewZ.y);
            //    //bw.Write(node.SkewZ.z);
            //    //bw.Write(node.Center.x);
            //    //bw.Write(node.Center.y);
            //    //bw.Write(node.Center.z);
            //    //bw.Write(node.DistanceFromParent);
            //}
            #endregion
            #region Marker Groups
            headerValueList.Add((int)bw.BaseStream.Position);
            //foreach (var group in Model.MarkerGroups)
            //{
            //    bw.Write((group.Name + "\0").ToCharArray());
            //    bw.Write(group.Markers.Count);
            //    markerAddressList.Add((int)bw.BaseStream.Position);
            //    bw.Write(0);
            //}
            #endregion
            #region Markers
            //foreach (var group in Model.MarkerGroups)
            //{
            //    markerValueList.Add((int)bw.BaseStream.Position);
            //    foreach (var marker in group.Markers)
            //    {
            //        bw.Write((byte)marker.RegionIndex);
            //        bw.Write((byte)marker.PermutationIndex);
            //        bw.Write((short)marker.NodeIndex);
            //        bw.Write(marker.Position.x * 100);
            //        bw.Write(marker.Position.y * 100);
            //        bw.Write(marker.Position.z * 100);
            //        bw.Write(marker.Rotation.i);
            //        bw.Write(marker.Rotation.j);
            //        bw.Write(marker.Rotation.k);
            //        bw.Write(marker.Rotation.w);
            //    }
            //}
            #endregion

            #region Regions
            headerValueList.Add((int)bw.BaseStream.Position);
            foreach (var pair in objDic)
            {
                bw.Write((pair.Key + "\0").ToCharArray());

                //int count = 0;
                //foreach (var obj in ATPL.Objects)
                //    if (PartIndices.Contains(ATPL.Objects.IndexOf(obj))) count++;

                bw.Write(pair.Value.Count);
                permAddressList.Add((int)bw.BaseStream.Position);
                bw.Write(0);

                //foreach (var region in regions)
                //{
                //    bw.Write((region.Name + "\0").ToCharArray());

                //    int count = 0;
                //    foreach (var perm in region.Permutations)
                //        if (PartIndices.Contains(perm.PieceIndex)) count++;

                //    bw.Write(count);
                //    permAddressList.Add((int)bw.BaseStream.Position);
                //    bw.Write(0);
                //}
            }
            #endregion
            #region Permutations
            foreach (var pair in objDic)
            {
                permValueList.Add((int)bw.BaseStream.Position);
                foreach (int i in pair.Value)
                {
                    var obj = ATPL.Objects[i];
                    //if (!PartIndices.Contains(perm.PieceIndex)) continue;

                    //var part = Model.ModelSections[perm.PieceIndex];
                    //VertexValue v;
                    //bool hasNodes = part.Vertices[0].TryGetValue("blendindices", 0, out v) && part.NodeIndex == 255;
                    //bool isBoned = part.Vertices[0].FormatName.Contains("rigid_boned");
                    bool hasNodes = false;
                    bool isBoned = false;

                    bw.Write((obj.Name + "\0").ToCharArray());

                    // 0 - no nodes
                    // 1 - nodes
                    // 2 - rigid with single bone (bone ID after or 255 if none)
                    byte vType;
                    if (isBoned) vType = (byte)2;
                    else vType = hasNodes ? (byte)1 : (byte)0;
                    vType += 16;
                    bw.Write(vType);
                    bw.Write((byte)255); //bone ID if applicable


                    bw.Write(obj.Vertices.Length);
                    vertAddressList.Add((int)bw.BaseStream.Position);
                    bw.Write(0);

                    int count = 0;
                    foreach (var submesh in obj.Submeshes)
                        count += GetTriangleList(obj.Indices, submesh.FaceStart * 3, submesh.FaceLength * 3, 3).Count / 3;

                    bw.Write(count);
                    indxAddressList.Add((int)bw.BaseStream.Position);
                    bw.Write(0);

                    bw.Write(obj.Submeshes.Count);
                    meshAddressList.Add((int)bw.BaseStream.Position);
                    bw.Write(0);

                    //var mat = MatrixFromBounds(obj.BoundingBox);
                    var mat = new Matrix(1, 0, 0, 0, 0, 1, 0, -1, 0, 0, 0, 0);

                    //if (obj.Name.Contains("ound_1"))
                    //{
                    //    S3DModelBase.S3DObject obj1 = null;
                    //    foreach (var ob in ATPL.Objects)
                    //        if (ob.ID == obj.Submeshes[0].MeshInheritID) obj1 = ob;
                    //    mat = MatrixFromBounds(obj1.BoundingBox);
                    //}

                    //bw.Write(float.NaN); //no transforms (render_models are pre-transformed)
                    bw.Write(1f);
                    bw.Write(mat.m11 * 0.01f);
                    bw.Write(mat.m12 * 0.01f);
                    bw.Write(mat.m13 * 0.01f);
                    bw.Write(mat.m21 * 0.01f);
                    bw.Write(mat.m22 * 0.01f);
                    bw.Write(mat.m23 * 0.01f);
                    bw.Write(mat.m31 * 0.01f);
                    bw.Write(mat.m32 * 0.01f);
                    bw.Write(mat.m33 * 0.01f);
                    bw.Write(mat.m41 * 0.01f);
                    bw.Write(mat.m42 * 0.01f);
                    bw.Write(mat.m43 * 0.01f);
                }
            }
            #endregion
            #region Vertices
            foreach (var pair in objDic)
            {
                foreach (int z in pair.Value)
                {
                    var obj = ATPL.Objects[z];
                    //var part = Model.ModelSections[perm.PieceIndex];

                    //int address;
                    //if (dupeDic.TryGetValue(part.VertsIndex, out address))
                    //{
                    //    vertValueList.Add(address);
                    //    continue;
                    //}
                    //else
                    //    dupeDic.Add(part.VertsIndex, (int)bw.BaseStream.Position);

                    VertexValue v;
                    //bool hasNodes = part.Vertices[0].TryGetValue("blendindices", 0, out v) && part.NodeIndex == 255;
                    //bool isBoned = part.Vertices[0].FormatName.Contains("rigid_boned");
                    bool hasNodes = false;
                    bool isBoned = false;

                    vertValueList.Add((int)bw.BaseStream.Position);

                    var bb = new render_model.BoundingBox();
                    bb.XBounds = bb.YBounds = bb.ZBounds = new RealBounds(0, 1);
                    bb.UBounds = bb.VBounds = new RealBounds(-1, 1);
                    if (obj.BoundingBox.Length > 0 && !obj.isInheritor) bb = obj.BoundingBox;

                    bool compress = obj.Vertices[0].FormatName == "S3D";
                    if (compress)
                    {
                        bw.Write(bb.XBounds.Min);
                        bw.Write(bb.XBounds.Max);
                        bw.Write(bb.YBounds.Min);
                        bw.Write(bb.YBounds.Max);
                        bw.Write(bb.ZBounds.Min);
                        bw.Write(bb.ZBounds.Max);
                        bw.Write(bb.UBounds.Min);
                        bw.Write(bb.UBounds.Max);
                        bw.Write(bb.VBounds.Min);
                        bw.Write(bb.VBounds.Max);
                    }

                    foreach (Vertex vert in obj.Vertices)
                    {
                        if (compress)
                        {
                            vert.TryGetValue("position", 0, out v);
                            bw.Write((short)Math.Round(((((v.Data.x - bb.XBounds.Min) / bb.XBounds.Length) * 0xFFFF) - 0x7FFF), 0));
                            bw.Write((short)Math.Round(((((v.Data.y - bb.YBounds.Min) / bb.YBounds.Length) * 0xFFFF) - 0x7FFF), 0));
                            bw.Write((short)Math.Round(((((v.Data.z - bb.ZBounds.Min) / bb.ZBounds.Length) * 0xFFFF) - 0x7FFF), 0));

                            vert.TryGetValue("normal", 0, out v);
                            bw.Write(0);
                            //bw.Write(v.Data.i);
                            //bw.Write(v.Data.k);
                            //bw.Write(v.Data.j);

                            vert.TryGetValue("texcoords", 0, out v);
                            bw.Write((short)Math.Round(((((v.Data.x - bb.UBounds.Min) / bb.UBounds.Length) * 0xFFFF) - 0x7FFF), 0));
                            bw.Write((short)Math.Round(((((v.Data.y - bb.VBounds.Min) / bb.VBounds.Length) * 0xFFFF) - 0x7FFF), 0));
                        }
                        else
                        {
                            vert.TryGetValue("position", 0, out v);
                            bw.Write(v.Data.x * 1);
                            bw.Write(v.Data.y * 1);
                            bw.Write(v.Data.z * 1);

                            vert.TryGetValue("normal", 0, out v);
                            bw.Write(v.Data.i);
                            bw.Write(v.Data.k);
                            bw.Write(v.Data.j);

                            vert.TryGetValue("texcoords", 0, out v);
                            bw.Write(v.Data.x);
                            bw.Write(v.Data.y);
                        }

                        if (isBoned)
                        {
                            VertexValue i;
                            var indices = new List<int>();
                            vert.TryGetValue("blendindices", 0, out i);

                            if (!indices.Contains((int)i.Data.a) && i.Data.a != 0) indices.Add((int)i.Data.a);
                            if (!indices.Contains((int)i.Data.b) && i.Data.a != 0) indices.Add((int)i.Data.b);
                            if (!indices.Contains((int)i.Data.c) && i.Data.a != 0) indices.Add((int)i.Data.c);
                            if (!indices.Contains((int)i.Data.d) && i.Data.a != 0) indices.Add((int)i.Data.d);

                            if (indices.Count == 0) indices.Add(0);

                            foreach (int index in indices) bw.Write((byte)index);

                            if (indices.Count < 4) bw.Write((byte)255);

                            continue;
                        }

                        if (hasNodes)
                        {
                            VertexValue i, w;
                            vert.TryGetValue("blendindices", 0, out i);
                            vert.TryGetValue("blendweight", 0, out w);
                            int count = 0;
                            if (w.Data.a > 0)
                            {
                                bw.Write((byte)i.Data.a);
                                count++;
                            }
                            if (w.Data.b > 0)
                            {
                                bw.Write((byte)i.Data.b);
                                count++;
                            }
                            if (w.Data.c > 0)
                            {
                                bw.Write((byte)i.Data.c);
                                count++;
                            }
                            if (w.Data.d > 0)
                            {
                                bw.Write((byte)i.Data.d);
                                count++;
                            }

                            if (count == 0) throw new Exception("no weights on a weighted node. report this.");

                            if (count != 4) bw.Write((byte)255);

                            if (w.Data.a > 0) bw.Write(w.Data.a);
                            if (w.Data.b > 0) bw.Write(w.Data.b);
                            if (w.Data.c > 0) bw.Write(w.Data.c);
                            if (w.Data.d > 0) bw.Write(w.Data.d);
                        }
                    }
                }
            }
            #endregion

            //dupeDic.Clear();

            #region Indices
            foreach (var pair in objDic)
            {
                foreach (int z in pair.Value)
                {
                    var obj = ATPL.Objects[z];
                    //var part = Model.ModelSections[perm.PieceIndex];

                    //int address;
                    //if (dupeDic.TryGetValue(part.FacesIndex, out address))
                    //{
                    //    indxValueList.Add(address);
                    //    continue;
                    //}
                    //else
                    //    dupeDic.Add(part.FacesIndex, (int)bw.BaseStream.Position);

                    indxValueList.Add((int)bw.BaseStream.Position);

                    foreach (var submesh in obj.Submeshes)
                    {
                        var indices = GetTriangleList(obj.Indices, submesh.FaceStart * 3, submesh.FaceLength * 3, 3).ToArray();
                        //for (int i = 0; i < indices.Length; i += 3)
                        //    Array.Reverse(indices, i, 3);
                        foreach (var index in indices)
                        {
                            if (obj.Vertices.Length > 0xFFFF) bw.Write(index);
                            else /*if (obj.Vertices.Length > 0xFF)*/ bw.Write((ushort)index);
                            //else bw.Write((byte)index);
                        }
                    }
                }
            }
            #endregion
            #region Submeshes
            foreach (var pair in objDic)
            {
                foreach (int z in pair.Value)
                {
                    var obj = ATPL.Objects[z];
                    //var part = Model.ModelSections[perm.PieceIndex];
                    meshValueList.Add((int)bw.BaseStream.Position);
                    int tCount = 0;
                    foreach (var mesh in obj.Submeshes)
                    {

                        int sCount = GetTriangleList(obj.Indices, mesh.FaceStart * 3, mesh.FaceLength * 3, 3).Count / 3;

                        bw.Write((short)mesh.MaterialIndex);
                        bw.Write(tCount);
                        bw.Write(sCount);

                        tCount += sCount;
                    }
                }
            }
            #endregion
            #region Shaders
            headerValueList.Add((int)bw.BaseStream.Position);
            foreach (var mat in ATPL.Materials)
            {
            //    //skip null shaders
            //    if (shaderBlock.tagID == -1)
            //    {
            //        bw.Write("null\0".ToCharArray());
            //        for (int i = 0; i < 8; i++)
            //            bw.Write("null\0".ToCharArray());

            //        bw.Write(Convert.ToByte(false));
            //        bw.Write(Convert.ToByte(false));

            //        continue;
            //    }

            //    var rmshTag = Cache.IndexItems.GetItemByID(shaderBlock.tagID);
            //    var rmsh = DefinitionsManager.rmsh(Cache, rmshTag);
                string shaderName = mat.Name + "\0";
                string[] paths = new string[8] { "null\0", "null\0", "null\0", "null\0", "null\0", "null\0", "null\0", "null\0" };
                float[] uTiles = new float[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
                float[] vTiles = new float[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
                bool isTransparent = false;
                bool ccOnly = false;

                paths[0] = mat.Name + "\0";
                if (Pak.GetItemByName(mat.Name + "_nm") != null) paths[3] = mat.Name + "_nm\0";
                if (Pak.GetItemByName(mat.Name + "_spec") != null) paths[6] = mat.Name + "_spec\0";

                #region OLD
                //    //Halo4 fucked this up
                //    if (Cache.Version >= DefinitionSet.Halo3Beta && Cache.Version <= DefinitionSet.HaloReachRetail)
                //    {
                //        var rmt2Tag = Cache.IndexItems.GetItemByID(rmsh.Properties[0].TemplateTagID);
                //        var rmt2 = DefinitionsManager.rmt2(Cache, rmt2Tag);

                //        for (int i = 0; i < rmt2.UsageBlocks.Count; i++)
                //        {
                //            var s = rmt2.UsageBlocks[i].Usage;
                //            var bitmTag = Cache.IndexItems.GetItemByID(rmsh.Properties[0].ShaderMaps[i].BitmapTagID);

                //            switch (s)
                //            {
                //                case "base_map":
                //                    paths[0] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                //                    break;
                //                case "detail_map":
                //                case "detail_map_overlay":
                //                    paths[1] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                //                    break;
                //                case "change_color_map":
                //                    paths[2] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                //                    break;
                //                case "bump_map":
                //                    paths[3] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                //                    break;
                //                case "bump_detail_map":
                //                    paths[4] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                //                    break;
                //                case "self_illum_map":
                //                    paths[5] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                //                    break;
                //                case "specular_map":
                //                    paths[6] = (bitmTag != null) ? bitmTag.Filename + "\0" : "null\0";
                //                    break;
                //            }
                //        }

                //        short[] tiles = new short[8] { -1, -1, -1, -1, -1, -1, -1, -1 };

                //        foreach (var map in rmsh.Properties[0].ShaderMaps)
                //        {
                //            var bitmTag = Cache.IndexItems.GetItemByID(map.BitmapTagID);

                //            for (int i = 0; i < 8; i++)
                //            {
                //                if (bitmTag.Filename + "\0" != paths[i]) continue;

                //                tiles[i] = (short)map.TilingIndex;
                //            }
                //        }

                //        for (int i = 0; i < 8; i++)
                //        {
                //            try
                //            {
                //                uTiles[i] = rmsh.Properties[0].Tilings[tiles[i]].UTiling;
                //                vTiles[i] = rmsh.Properties[0].Tilings[tiles[i]].VTiling;
                //            }
                //            catch { }
                //        }
                //    }
                //    else
                //        try { paths[0] = Cache.IndexItems.GetItemByID(rmsh.Properties[0].ShaderMaps[0].BitmapTagID).Filename + "\0"; }
                //        catch { }

                //    if (rmshTag.ClassCode != "rmsh" && rmshTag.ClassCode != "mat")
                //    {
                //        isTransparent = true;
                //        if (paths[0] == "null\0" && paths[2] != "null\0")
                //            ccOnly = true;
                //    }
                #endregion

                bw.Write(shaderName.ToCharArray());
                for (int i = 0; i < 8; i++)
                {
                    bw.Write(paths[i].ToCharArray());
                    if (paths[i] != "null\0")
                    {
                        bw.Write(uTiles[i]);
                        bw.Write(vTiles[i]);
                    }
                }

                bw.Write(Convert.ToByte(isTransparent));
                bw.Write(Convert.ToByte(ccOnly));
            }
            #endregion
            #region Write Addresses
            for (int i = 0; i < headerAddressList.Count; i++)
            {
                bw.BaseStream.Position = headerAddressList[i];
                bw.Write(headerValueList[i]);
            }

            //for (int i = 0; i < markerAddressList.Count; i++)
            //{
            //    bw.BaseStream.Position = markerAddressList[i];
            //    bw.Write(markerValueList[i]);
            //}

            for (int i = 0; i < permAddressList.Count; i++)
            {
                bw.BaseStream.Position = permAddressList[i];
                bw.Write(permValueList[i]);
            }

            for (int i = 0; i < vertAddressList.Count; i++)
            {
                bw.BaseStream.Position = vertAddressList[i];
                bw.Write(vertValueList[i]);
            }

            for (int i = 0; i < indxAddressList.Count; i++)
            {
                bw.BaseStream.Position = indxAddressList[i];
                bw.Write(indxValueList[i]);
            }

            for (int i = 0; i < meshAddressList.Count; i++)
            {
                bw.BaseStream.Position = meshAddressList[i];
                bw.Write(meshValueList[i]);
            }
            #endregion

            bw.Close();
            bw.Dispose();
        }
        #endregion

        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
    }
}
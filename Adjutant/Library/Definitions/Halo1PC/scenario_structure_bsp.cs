using System;
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

namespace Adjutant.Library.Definitions.Halo1PC
{
    public class scenario_structure_bsp : sbsp
    {
        public int indexCount, indexOffset;

        public scenario_structure_bsp(CacheBase Cache, CacheBase.IndexItem Tag)
        {
            cache = Cache;
            int Address = Tag.Offset;
            EndianReader Reader = Cache.Reader;

            Reader.SeekTo(Address + 0xE0);
            XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

            Reader.SeekTo(Address + 0x110);
            indexCount = Reader.ReadInt32() * 3;
            indexOffset = Reader.ReadInt32() - Tag.Magic;

            #region Lightmaps Block
            Reader.SeekTo(Address + 0x11C);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Tag.Magic;
            for (int i = 0; i < iCount; i++)
                ModelSections.Add(new Lightmap(cache, iOffset + 32 * i, Tag.Magic) { VertsIndex = i, FacesIndex = i, NodeIndex = 255 });
            #endregion

            #region Create Shader List
            var sIDs = new List<int>();
            for (int i = 0; i < ModelSections.Count; i++)
            {
                var section = ModelSections[i];
                for (int j = 0; j < section.Submeshes.Count; j++)
                {
                    var mesh = (Lightmap.Material)section.Submeshes[j];

                    if (!sIDs.Contains(mesh.shaderID)) sIDs.Add(mesh.shaderID);
                    mesh.ShaderIndex = sIDs.IndexOf(mesh.shaderID);
                }
            }
            foreach (int ID in sIDs) Shaders.Add(new Shader(ID)); 
            for (int i = 0; i < ModelSections.Count; i++) Clusters.Add(new Cluster(i));
            #endregion
        }

        public override void LoadRaw()
        {
            if (RawLoaded) return;

            var reader = cache.Reader;

            #region Read Indices
            int[] indices = new int[indexCount];
            reader.SeekTo(indexOffset);
            for (int i = 0; i < indexCount; i++)
                indices[i] = reader.ReadUInt16(); 
            #endregion

            for (int i = 0; i < ModelSections.Count; i++)
            {
                var section = ModelSections[i];
                var tempVerts = new List<Vertex>();

                if (section.Submeshes.Count == 0) continue;

                #region Read Vertices
                for (int j = 0; j < section.Submeshes.Count; j++)
                {
                    var mesh = (Lightmap.Material)section.Submeshes[j];

                    reader.SeekTo(mesh.vertsOffset);
                    for (int k = 0; k < mesh.VertexCount; k++)
                    {
                        var v = new Vertex() { FormatName = "Halo1PC_World" };
                        var position = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        var normal = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        var binormal = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        var tangent = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        var texcoord = new RealQuat(reader.ReadSingle(), 1f - reader.ReadSingle());

                        v.Values.Add(new VertexValue(position, VertexValue.ValueType.Float32_3, "position", 0));
                        v.Values.Add(new VertexValue(normal, VertexValue.ValueType.Float32_3, "normal", 0));
                        v.Values.Add(new VertexValue(binormal, VertexValue.ValueType.Float32_3, "binormal", 0));
                        v.Values.Add(new VertexValue(tangent, VertexValue.ValueType.Float32_3, "tangent", 0));
                        v.Values.Add(new VertexValue(texcoord, VertexValue.ValueType.Float32_2, "texcoords", 0));

                        tempVerts.Add(v);
                    }
                } 
                #endregion

                #region Copy & Translate Indices
                int offset = section.Submeshes[0].FaceIndex;
                section.Indices = new int[section.TotalFaceCount];
                Array.Copy(indices, offset, section.Indices, 0, section.TotalFaceCount);
                section.Vertices = tempVerts.ToArray();

                int pos = 0;
                for (int j = 0; j < section.Submeshes.Count; j++)
                {
                    var mesh = section.Submeshes[j];
                    mesh.FaceIndex -= offset;

                    for (int k = 0; k < mesh.FaceCount; k++)
                        section.Indices[k + mesh.FaceIndex] += pos;

                    Array.Reverse(section.Indices, mesh.FaceIndex, mesh.FaceCount);
                    pos += mesh.VertexCount;
                } 
                #endregion

                IndexInfoList.Add(new mode.IndexBufferInfo() { FaceFormat = 3 });
                VertInfoList.Add(new mode.VertexBufferInfo() { VertexCount = section.TotalVertexCount });
            }

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

        public class Lightmap : mode.ModelSection
        {
            public Lightmap(CacheBase Cache, int Address, int Magic)
            {
                var Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.SeekTo(Address + 0x14);
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Magic;
                Submeshes = new List<Submesh>();
                for (int i = 0; i < iCount; i++)
                    Submeshes.Add(new Material(Cache, iOffset + 256 * i, Magic));
            }

            public class Material : mode.ModelSection.Submesh
            {
                public int shaderID, vertsOffset;

                public Material(CacheBase Cache, int Address, int Magic) 
                {
                    var Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    Reader.SeekTo(Address + 12);
                    shaderID = Reader.ReadInt32();

                    Reader.SeekTo(Address + 20);
                    FaceIndex = Reader.ReadInt32() * 3;
                    FaceCount = Reader.ReadInt32() * 3;

                    Reader.SeekTo(Address + 180);
                    VertexCount = Reader.ReadInt32();
                    Reader.SeekTo(Address + 228);
                    vertsOffset = Reader.ReadInt32() - Magic;
                }
            }
        }

        public class Shader : mode.Shader
        {
            public Shader(int ID)
            {
                tagID = ID;
            }
        }

        public class BoundingBox : mode.BoundingBox
        {
            public BoundingBox(CacheBase Cache, int Address)
            {
            }
        }
    }
}

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
using mode = Adjutant.Library.Definitions.render_model;

namespace Adjutant.Library.Definitions.Halo1PC
{
    public class gbxmodel : mode
    {
        public float uScale;
        public float vScale;

        public gbxmodel(CacheBase Cache, int Address)
        {
            cache = Cache;
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Name = "gbxmodel";
            Flags = new Bitmask(Reader.ReadInt16());

            Reader.SeekTo(Address + 0x30);
            uScale = Reader.ReadSingle();
            vScale = Reader.ReadSingle();


            #region MarkerGroups Block
            Reader.SeekTo(Address + 0xAC);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                MarkerGroups.Add(new MarkerGroup(Cache, iOffset + 64 * i));
            #endregion

            #region Nodes Block
            Reader.SeekTo(Address + 0xB8);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Nodes.Add(new Node(Cache, iOffset + 156 * i));
            #endregion

            #region Regions Block
            Reader.SeekTo(Address + 0xC4);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Regions.Add(new Region(Cache, iOffset + 76 * i));
            #endregion

            #region ModelParts Block
            Reader.SeekTo(Address + 0xD0);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                ModelSections.Add(new ModelSection(Cache, iOffset + 48 * i) { FacesIndex = i, VertsIndex = i, NodeIndex = 255 });
            #endregion

            #region Shaders Block
            Reader.SeekTo(Address + 0xDC);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new Shader(Cache, iOffset + 32 * i));
            #endregion

            #region BoundingBox Block
            BoundingBoxes.Add(new BoundingBox());
            #endregion
        }

        public override void LoadRaw()
        {
            if (RawLoaded) return;

            var bb = BoundingBoxes[0];
            var IH = (Halo1PC.CacheFile.CacheIndexHeader)cache.IndexHeader;
            var reader = cache.Reader;

            for (int i = 0; i < ModelSections.Count; i++)
            {
                var section = ModelSections[i];
                
                List<int> tIndices = new List<int>();
                List<Vertex> tVertices = new List<Vertex>();

                for (int j = 0; j < section.Submeshes.Count; j++)
                {
                    var submesh = (Halo1PC.gbxmodel.ModelSection.Submesh)section.Submeshes[j];

                    #region Read Indices
                    submesh.FaceIndex = tIndices.Count;
                    var strip = new List<int>();

                    reader.SeekTo(submesh.FaceOffset + IH.vertDataOffset + IH.indexDataOffset);
                    for (int k = 0; k < submesh.FaceCount; k++)
                        strip.Add(reader.ReadUInt16() + tVertices.Count);

                    strip = ModelFunctions.GetTriangleList(strip.ToArray(), 0, strip.Count, 5);
                    strip.Reverse();
                    submesh.FaceCount = strip.Count;
                    tIndices.AddRange(strip); 
                    #endregion

                    #region Read Vertices
                    reader.SeekTo(submesh.VertOffset + IH.vertDataOffset);
                    for (int k = 0; k < submesh.VertexCount; k++)
                    {
                        var v = new Vertex() { FormatName = "Halo1PC_Skinned" };
                        var position = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        var normal = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        var binormal = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        var tangent = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        var texcoord = new RealQuat(reader.ReadSingle() * uScale, 1f - reader.ReadSingle() * vScale);
                        var nodes = (Flags.Values[1]) ? 
                            new RealQuat(submesh.LocalNodes[reader.ReadInt16()], submesh.LocalNodes[reader.ReadInt16()], 0, 0) : 
                            new RealQuat(reader.ReadInt16(), reader.ReadInt16(), 0, 0);
                        var weights = new RealQuat(reader.ReadSingle(), reader.ReadSingle(), 0, 0);

                        v.Values.Add(new VertexValue(position, VertexValue.ValueType.Float32_3, "position", 0));
                        v.Values.Add(new VertexValue(normal, VertexValue.ValueType.Float32_3, "normal", 0));
                        v.Values.Add(new VertexValue(binormal, VertexValue.ValueType.Float32_3, "binormal", 0));
                        v.Values.Add(new VertexValue(tangent, VertexValue.ValueType.Float32_3, "tangent", 0));
                        v.Values.Add(new VertexValue(texcoord, VertexValue.ValueType.Float32_2, "texcoords", 0));
                        v.Values.Add(new VertexValue(nodes, VertexValue.ValueType.Int16_N2, "blendindices", 0));
                        v.Values.Add(new VertexValue(weights, VertexValue.ValueType.Float32_2, "blendweight", 0));

                        tVertices.Add(v);

                        bb.XBounds.Min = Math.Min(bb.XBounds.Min, position.x);
                        bb.XBounds.Max = Math.Max(bb.XBounds.Max, position.x);
                        bb.YBounds.Min = Math.Min(bb.YBounds.Min, position.y);
                        bb.YBounds.Max = Math.Max(bb.YBounds.Max, position.y);
                        bb.ZBounds.Min = Math.Min(bb.ZBounds.Min, position.z);
                        bb.ZBounds.Max = Math.Max(bb.ZBounds.Max, position.z);
                    } 
                    #endregion
                }

                section.Indices = tIndices.ToArray();
                section.Vertices = tVertices.ToArray();

                IndexInfoList.Add(new IndexBufferInfo() { FaceFormat = 3 });
                VertInfoList.Add(new VertexBufferInfo() { VertexCount = section.TotalVertexCount });
            }

            RawLoaded = true;
        }

        new public class Region : mode.Region
        {
            public Region(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Name = Reader.ReadNullTerminatedString(32);
                Reader.ReadInt16();
                Reader.ReadInt32();

                Reader.SeekTo(Address + 0x40);

                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                for (int i = 0; i < iCount; i++)
                    Permutations.Add(new Permutation(Cache, iOffset + 88 * i));
            }

            new public class Permutation : mode.Region.Permutation
            {
                public Permutation(CacheBase Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    Name = Reader.ReadNullTerminatedString(32);
                    Reader.ReadInt16();

                    Reader.SeekTo(Address + 0x40);
                    Reader.ReadInt16(); //super low
                    Reader.ReadInt16(); //low
                    Reader.ReadInt16(); //medium
                    Reader.ReadInt16(); //high
                    PieceIndex = Reader.ReadInt16(); //super high
                    PieceCount = 1;

                    //more markers here
                }
            }
        }

        new public class Node : mode.Node
        {
            public Node(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Name = Reader.ReadNullTerminatedString(32);
                NextSiblingIndex = Reader.ReadInt16();
                FirstChildIndex = Reader.ReadInt16();
                ParentIndex = Reader.ReadInt16();
                Reader.ReadInt16();
                Position = new RealQuat(
                    Reader.ReadSingle(), 
                    Reader.ReadSingle(),
                    Reader.ReadSingle());
                Rotation = new RealQuat(
                    -Reader.ReadSingle(),
                    -Reader.ReadSingle(),
                    -Reader.ReadSingle(),
                    Reader.ReadSingle());

                DistanceFromParent = Reader.ReadSingle();
            }
        }

        new public class MarkerGroup : mode.MarkerGroup
        {
            public MarkerGroup(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Name = Reader.ReadNullTerminatedString(32);
                Reader.ReadInt16();

                Reader.SeekTo(Address + 0x34);

                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                for (int i = 0; i < iCount; i++)
                    Markers.Add(new Marker(Cache, iOffset + 32 * i));
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
                        -Reader.ReadSingle(),
                        -Reader.ReadSingle(),
                        -Reader.ReadSingle(),
                        Reader.ReadSingle());
                }
            }
        }

        new public class Shader : mode.Shader
        {
            public Shader(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.Skip(12);

                tagID = Reader.ReadInt32();
            }
        }

        new public class ModelSection : mode.ModelSection
        {
            public ModelSection(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                NodeIndex = 255;

                Reader.SeekTo(Address + 0x24);
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;                   
                for (int i = 0; i < iCount; i++)
                    Submeshes.Add(new Submesh(Cache, iOffset + 132 * i));
            }

            new public class Submesh : mode.ModelSection.Submesh
            {
                public int FaceOffset;
                public int VertOffset;
                public List<byte> LocalNodes = new List<byte>();

                public Submesh(CacheBase Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    Reader.ReadInt32();
                    ShaderIndex = Reader.ReadInt16();

                    Reader.SeekTo(Address + 0x48);
                    FaceCount = Reader.ReadInt32() + 2;
                    FaceOffset = Reader.ReadInt32();

                    Reader.SeekTo(Address + 0x58);
                    VertexCount = Reader.ReadInt32();
                    Reader.ReadInt32();
                    Reader.ReadInt32();
                    VertOffset = Reader.ReadInt32();

                    Reader.Skip(3);
                    int iCount = Reader.ReadByte();
                    for (int i = 0; i < iCount; i++)
                        LocalNodes.Add(Reader.ReadByte());
                }
            }
        }

        new public class BoundingBox : mode.BoundingBox
        {
            public BoundingBox()
            {
                XBounds = YBounds = ZBounds = new RealBounds(float.PositiveInfinity, float.NegativeInfinity);
                UBounds = VBounds = new RealBounds(0, 1);
            }
        }
    }
}

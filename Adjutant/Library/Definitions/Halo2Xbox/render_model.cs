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
using mode = Adjutant.Library.Definitions.render_model;

namespace Adjutant.Library.Definitions.Halo2Xbox
{
    internal class render_model : mode
    {
        internal render_model(CacheFile Cache, int Address)
        {
            cache = Cache;
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Name = Cache.Strings.GetItemByID(Reader.ReadInt16());
            //Flags = new Bitmask(Reader.ReadInt32());

            Reader.SeekTo(Address + 20);

            #region BoundingBox Block
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            BoundingBoxes = new List<mode.BoundingBox>();
            for (int i = 0; i < iCount; i++)
                BoundingBoxes.Add(new BoundingBox(Cache, iOffset + 56 * i));
            Reader.SeekTo(Address + 28);
            #endregion

            #region Regions Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Regions = new List<mode.Region>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++)
                Regions.Add(new Region(Cache));
            Reader.SeekTo(Address + 36);
            #endregion

            #region ModelParts Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            ModelSections = new List<mode.ModelSection>();
            for (int i = 0; i < iCount; i++)
                ModelSections.Add(new ModelSection(Cache, iOffset + 92 * i) { FacesIndex = i, VertsIndex = i, NodeIndex = 255 });
            Reader.SeekTo(Address + 72);
            #endregion

            #region Nodes Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Nodes = new List<mode.Node>();
            try { Reader.BaseStream.Position = iOffset; }
            catch { }
            for (int i = 0; i < iCount; i++)
                Nodes.Add(new Node(Cache));
            Reader.SeekTo(Address + 88);
            #endregion

            #region MarkerGroups Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            MarkerGroups = new List<mode.MarkerGroup>();
            try { Reader.BaseStream.Position = iOffset; }
            catch { }
            for (int i = 0; i < iCount; i++)
                MarkerGroups.Add(new MarkerGroup(Cache));
            Reader.SeekTo(Address + 96);
            #endregion

            #region Shaders Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Shaders = new List<mode.Shader>();
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new Shader(Cache, iOffset + 32 * i));
            Reader.SeekTo(Address + 84);
            #endregion
        }

        public override void LoadRaw()
        {
            var mode = this;
            mode.IndexInfoList = new List<IndexBufferInfo>();

            for (int i = 0; i < mode.ModelSections.Count; i++)
            {
                var section = (Halo2Xbox.render_model.ModelSection)mode.ModelSections[i];
                var data = cache.GetRawFromID(section.rawOffset, section.rawSize);
                var ms = new MemoryStream(data);
                var reader = new EndianReader(ms, Endian.EndianFormat.LittleEndian);

                if (cache.vertexNode == null) throw new NotSupportedException("No vertex definitions found for " + cache.Version.ToString());

                #region Get Vertex Definition
                XmlNode formatNode = null;
                foreach (XmlNode node in cache.vertexNode.ChildNodes)
                {
                    if (Convert.ToInt32(node.Attributes["type"].Value, 16) == 1)
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

                reader.SeekTo(40);
                section.Indices = new int[reader.ReadUInt16()];
                section.Vertices = new Vertex[section.vertcount];

                var facetype = 5;
                if (section.facecount * 3 == section.Indices.Length) facetype = 3;
                mode.IndexInfoList.Add(new IndexBufferInfo() { FaceFormat = facetype });

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

                reader.SeekTo(108);
                int bCount = reader.ReadUInt16();
                int[] bArr = new int[bCount];
                if (bCount > 0)
                {
                    reader.SeekTo(section.hSize + section.rOffset[bIndex]);
                    for (int j = 0; j < bCount; j++)
                        bArr[j] = reader.ReadByte();
                }

                #region Read Vertices
                for (int j = 0; j < section.vertcount; j++)
                {
                    reader.SeekTo(section.hSize + section.rOffset[vIndex] + ((section.rSize[vIndex] / section.vertcount) * j));
                    var v = new Vertex(reader, formatNode);
                    var b = new RealQuat();
                    var w = new RealQuat();

                    switch (section.type)
                    {
                        case 1:
                            switch (section.bones)
                            {
                                case 0:
                                    section.NodeIndex = 0;
                                    break;
                                case 1:
                                    section.NodeIndex = (bCount > 0) ? bArr[0] : 0;
                                    break;
                            }
                            section.Vertices[j] = v;
                            continue;
                        case 2:
                            switch (section.bones)
                            {
                                case 1:
                                    b = new RealQuat(reader.ReadByte(), reader.ReadByte(), 0, 0);
                                    w = new RealQuat(1, 0, 0, 0);
                                    break;
                            }
                            break;
                        case 3:
                            switch (section.bones)
                            {
                                case 2:
                                    reader.ReadInt16();
                                    b = new RealQuat(reader.ReadByte(), reader.ReadByte(), 0, 0);
                                    w = new RealQuat((float)reader.ReadByte() / 255f, (float)reader.ReadByte() / 255f, 0, 0);
                                    break;
                                case 3:
                                    b = new RealQuat(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), 0);
                                    w = new RealQuat((float)reader.ReadByte() / 255f, (float)reader.ReadByte() / 255f, (float)reader.ReadByte() / 255f, 0);
                                    break;
                                case 4:
                                    reader.ReadInt16();
                                    b = new RealQuat(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                                    w = new RealQuat((float)reader.ReadByte() / 255f, (float)reader.ReadByte() / 255f, (float)reader.ReadByte() / 255f, (float)reader.ReadByte() / 255f);
                                    break;
                            }
                            break;
                    }

                    if (bCount > 0)
                    {
                        b.a = (w.a == 0) ? 0 : bArr[(int)b.a];
                        b.b = (w.b == 0) ? 0 : bArr[(int)b.b];
                        b.c = (w.c == 0) ? 0 : bArr[(int)b.c];
                        b.d = (w.d == 0) ? 0 : bArr[(int)b.d];
                    }

                    v.Values.Add(new VertexValue(b, 0, "blendindices", 0));
                    v.Values.Add(new VertexValue(w, 0, "blendweight", 0));

                    //v.FormatName = "Halo2X format " + section.VertexFormat.ToString();
                    section.Vertices[j] = v;
                }
                #endregion

                #region Read UVs and Normals
                for (int j = 0; j < section.vertcount; j++)
                {
                    reader.SeekTo(section.hSize + section.rOffset[uIndex] + (4 * j));
                    var v = section.Vertices[j];
                    var uv = new RealQuat(((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF, ((float)reader.ReadInt16() + (float)0x7FFF) / (float)0xFFFF);
                    v.Values.Add(new VertexValue(uv, 0, "texcoords", 0));
                }
                
                for (int j = 0; j < section.vertcount; j++)
                {
                    reader.SeekTo(section.hSize + section.rOffset[nIndex] + (12 * j));
                    var v = section.Vertices[j];
                    var n = RealQuat.FromHenDN3(reader.ReadUInt32());
                    v.Values.Add(new VertexValue(n, 0, "normal", 0));
                }
                #endregion

                ModelFunctions.DecompressVertex(ref section.Vertices, mode.BoundingBoxes[0]);

                reader.SeekTo(section.hSize + section.rOffset[iIndex]);
                for (int j = 0; j < section.Indices.Length; j++)
                    section.Indices[j] = reader.ReadUInt16();
            }

            mode.RawLoaded = true;
        }

        new internal class Region : mode.Region
        {
            internal Region(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;
                long temp = Reader.BaseStream.Position;

                Name = Cache.Strings.GetItemByID(Reader.ReadInt16());
                Reader.ReadInt16();
                Reader.ReadInt32();

                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = iOffset;
                Permutations = new List<mode.Region.Permutation>();
                for (int i = 0; i < iCount; i++)
                    Permutations.Add(new Permutation(Cache));
                Reader.BaseStream.Position = temp + 16;
            }

            new internal class Permutation : mode.Region.Permutation
            {
                internal Permutation(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    Name = Cache.Strings.GetItemByID(Reader.ReadInt16());
                    Reader.ReadInt16();
                    
                    Reader.BaseStream.Position += 4;
                    Reader.BaseStream.Position += 6;
                    PieceIndex = Reader.ReadInt16();
                    PieceCount = 1;
                }
            }
        }

        new internal class Node : mode.Node
        {
            internal Node(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                Name = Cache.Strings.GetItemByID(Reader.ReadInt16());
                Reader.ReadInt16();
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

        new internal class MarkerGroup : mode.MarkerGroup
        {
            internal MarkerGroup(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;
                long temp = Reader.BaseStream.Position;

                Name = Cache.Strings.GetItemByID(Reader.ReadInt16());
                Reader.ReadInt16();

                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = iOffset;
                Markers = new List<mode.MarkerGroup.Marker>();
                for (int i = 0; i < iCount; i++)
                    Markers.Add(new Marker(Cache));
                Reader.BaseStream.Position = temp + 12;
            }

            new internal class Marker : mode.MarkerGroup.Marker
            {
                internal Marker(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

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

        new internal class Shader : mode.Shader
        {
            internal Shader(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.Skip(12);

                tagID = Reader.ReadInt32();
            }
        }

        new internal class ModelSection : mode.ModelSection
        {
            public int vertcount;
            public int facecount;

            public int rawOffset;
            public int rawSize;
            public int hSize;

            public int[] rSize;
            public int[] rOffset;
            public int[] rType;

            public int type;
            public int bones;

            internal ModelSection(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);
                    
                Submeshes = new List<mode.ModelSection.Submesh>();
                Subsets = new List<mode.ModelSection.Subset>();

                type = Reader.ReadInt16();
                Reader.ReadUInt16();
                vertcount = Reader.ReadUInt16();
                facecount = Reader.ReadUInt16();
                Reader.SeekTo(Address + 20);
                bones = Reader.ReadByte();
                Reader.SeekTo(Address + 56);
                rawOffset = Reader.ReadInt32();
                rawSize = Reader.ReadInt32();
                Reader.ReadUInt32();
                hSize = rawSize - Reader.ReadInt32() - 4;

                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                
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

        new internal class BoundingBox : mode.BoundingBox
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

                Reader.ReadInt32();

                Reader.BaseStream.Position += 12; //56
            }
        }
    }
}

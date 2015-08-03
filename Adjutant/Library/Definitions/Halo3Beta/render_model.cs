using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using mode = Adjutant.Library.Definitions.render_model;

namespace Adjutant.Library.Definitions.Halo3Beta
{
    public class render_model : mode
    {
        public render_model(CacheFile Cache, int Address)
        {
            cache = Cache;
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Name = Cache.Strings.GetItemByID(Reader.ReadInt32());
            Flags = new Bitmask(Reader.ReadInt32());

            Reader.SeekTo(Address + 12);

            #region Regions Block
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            Regions = new List<mode.Region>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++)
                Regions.Add(new Region(Cache));
            Reader.SeekTo(Address + 24);
            #endregion

            Reader.ReadInt32();
            InstancedGeometryIndex = Reader.ReadInt32();

            #region Instanced Geometry Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            GeomInstances = new List<mode.InstancedGeometry>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++)
                GeomInstances.Add(new InstancedGeometry(Cache));
            Reader.SeekTo(Address + 44);
            #endregion

            Reader.SeekTo(Address + 48);

            #region Nodes Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Nodes = new List<mode.Node>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++)
                Nodes.Add(new Node(Cache));
            Reader.SeekTo(Address + 60);
            #endregion

            #region MarkerGroups Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            MarkerGroups = new List<mode.MarkerGroup>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++)
                MarkerGroups.Add(new MarkerGroup(Cache));
            Reader.SeekTo(Address + 72);
            #endregion

            #region Shaders Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Shaders = new List<mode.Shader>();
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new Shader(Cache, iOffset + 36 * i));
            Reader.SeekTo(Address + 84);
            #endregion

            Reader.SeekTo(Address + 104);

            #region ModelParts Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            ModelSections = new List<mode.ModelSection>();
            for (int i = 0; i < iCount; i++)
                ModelSections.Add(new ModelSection(Cache, iOffset + 76 * i));
            Reader.SeekTo(Address + 116);
            #endregion

            #region BoundingBox Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            BoundingBoxes = new List<mode.BoundingBox>();
            for (int i = 0; i < iCount; i++)
                BoundingBoxes.Add(new BoundingBox(Cache, iOffset + 56 * i));
            Reader.SeekTo(Address + 128);
            #endregion

            Reader.SeekTo(Address + 176);

            #region NodeMapGroup Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            NodeIndexGroups = new List<mode.NodeIndexGroup>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++)
                NodeIndexGroups.Add(new NodeIndexGroup(Cache));
            Reader.SeekTo(Address + 188);
            #endregion

            Reader.SeekTo(Address + 224);
            
            RawID = Reader.ReadInt32();

            Reader.SeekTo(Address + 460);
        }

        new public class Region : mode.Region
        {
            public Region(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());

                long temp = Reader.BaseStream.Position;
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = iOffset;
                Permutations = new List<mode.Region.Permutation>();
                for (int i = 0; i < iCount; i++)
                    Permutations.Add(new Permutation(Cache));
                Reader.BaseStream.Position = temp + 12;
            }

            new public class Permutation : mode.Region.Permutation
            {
                public Permutation(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    Name = Cache.Strings.GetItemByID(Reader.ReadInt32());
                    PieceIndex = Reader.ReadInt16();
                    PieceCount = Reader.ReadInt16();
                    
                    Reader.BaseStream.Position += 8; //16
                }
            }
        }

        new public class InstancedGeometry : mode.InstancedGeometry
        {
            public InstancedGeometry(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

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
            public Node(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

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
            public MarkerGroup(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());

                long temp = Reader.BaseStream.Position;
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = iOffset;
                Markers = new List<mode.MarkerGroup.Marker>();
                for (int i = 0; i < iCount; i++)
                    Markers.Add(new Marker(Cache));
                Reader.BaseStream.Position = temp + 12;
            }

            new public class Marker : mode.MarkerGroup.Marker
            {
                public Marker(CacheFile Cache)
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

        new public class Shader : mode.Shader
        {
            public Shader(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.Skip(12);

                tagID = Reader.ReadInt32();

                Reader.SeekTo(Address + 36);
            }
        }

        new public class ModelSection : mode.ModelSection
        {
            public ModelSection(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);
                    
                #region Submesh Block
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                Submeshes = new List<mode.ModelSection.Submesh>();
                for (int i = 0; i < iCount; i++)
                    Submeshes.Add(new Submesh(Cache, iOffset + 16 * i));
                Reader.SeekTo(Address + 12);
                #endregion

                #region Subset Block
                iCount = Reader.ReadInt32();
                iOffset = Reader.ReadInt32() - Cache.Magic;
                Subsets = new List<mode.ModelSection.Subset>();
                for (int i = 0; i < iCount; i++)
                    Subsets.Add(new Subset(Cache, iOffset + 8 * i));
                Reader.SeekTo(Address + 24);
                #endregion

                #region Other
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

                Reader.SeekTo(Address + 76);
                #endregion
            }

            new public class Submesh : mode.ModelSection.Submesh
            {
                public Submesh(CacheFile Cache, int Address)
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
                public Subset(CacheFile Cache, int Address)
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
            public BoundingBox(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.ReadInt32();
                XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                UBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                VBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

                Reader.BaseStream.Position += 12; //56
            }
        }

        new public class NodeIndexGroup : mode.NodeIndexGroup
        {
            public NodeIndexGroup(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                long temp = Reader.BaseStream.Position;
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = iOffset;
                NodeIndices = new List<mode.NodeIndexGroup.NodeIndex>();
                for (int i = 0; i < iCount; i++)
                    NodeIndices.Add(new NodeIndex(Cache));
                Reader.BaseStream.Position = temp + 12;
            }

            new public class NodeIndex : mode.NodeIndexGroup.NodeIndex
            {
                public NodeIndex(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    Index = Reader.ReadByte();
                }
            }
        }
    }
}

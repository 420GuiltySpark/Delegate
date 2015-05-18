using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using mode = Adjutant.Library.Definitions.render_model;

namespace Adjutant.Library.Definitions.ReachRetail
{
    internal class render_model : mode
    {
        internal render_model(CacheFile Cache, int Offset)
        {
            cache = Cache;
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Offset);

            Name = Cache.Strings.GetItemByID(Reader.ReadInt32());
            Flags = new Bitmask(Reader.ReadInt32());

            Reader.BaseStream.Position += 4; //12

            #region Regions Block
            long temp = Reader.BaseStream.Position;
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            Regions = new List<mode.Region>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++)
                Regions.Add(new Region(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.ReadInt32();
            InstancedGeometryIndex = Reader.ReadInt32();

            #region Instanced Geometry Block
            temp = Reader.BaseStream.Position;
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            GeomInstances = new List<mode.InstancedGeometry>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++)
                GeomInstances.Add(new InstancedGeometry(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.BaseStream.Position += 4; //48

            #region Nodes Block
            temp = Reader.BaseStream.Position;
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Nodes = new List<mode.Node>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++)
                Nodes.Add(new Node(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            #region MarkerGroups Block
            temp = Reader.BaseStream.Position;
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            MarkerGroups = new List<mode.MarkerGroup>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++)
                MarkerGroups.Add(new MarkerGroup(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            #region Shaders Block
            temp = Reader.BaseStream.Position;
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Shaders = new List<mode.Shader>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new Shader(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.BaseStream.Position += 20; //104

            #region ModelParts Block
            temp = Reader.BaseStream.Position;
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            ModelSections = new List<mode.ModelSection>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++)
                ModelSections.Add(new ModelSection(Cache, iOffset + 92 * i));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            #region BoundingBox Block
            temp = Reader.BaseStream.Position;
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            BoundingBoxes = new List<mode.BoundingBox>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++)
                BoundingBoxes.Add(new BoundingBox(Cache, iOffset + 52 * i));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.BaseStream.Position += 48; //176

            #region NodeMapGroup Block
            temp = Reader.BaseStream.Position;
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            NodeIndexGroups = new List<mode.NodeIndexGroup>();
            Reader.BaseStream.Position = iOffset;
            for (int i = 0; i < iCount; i++) 
                NodeIndexGroups.Add(new NodeIndexGroup(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.BaseStream.Position += 60; //248
            
            RawID = Reader.ReadInt32();
            
            Reader.BaseStream.Position += 348; //600
        }

        new internal class Region : mode.Region
        {
            internal Region(CacheFile Cache)
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

            new internal class Permutation : mode.Region.Permutation
            {
                internal Permutation(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    Name = Cache.Strings.GetItemByID(Reader.ReadInt32());
                    PieceIndex = Reader.ReadInt16();
                    PieceCount = Reader.ReadInt16();
                    
                    Reader.BaseStream.Position += 16; //24
                }
            }
        }

        new internal class InstancedGeometry : mode.InstancedGeometry
        {
            internal InstancedGeometry(CacheFile Cache)
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

        new internal class Node : mode.Node
        {
            internal Node(CacheFile Cache)
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

                //var vp = new Microsoft.DirectX.Vector3(Position.x, Position.y, Position.z);
                //var vr = new Microsoft.DirectX.Vector4(0, 0, 0, 1);

                //vp.TransformCoordinate(TransformMatrix);
                //vr.Transform(TransformMatrix);

                //Position = new RealQuat(vp.X, vp.Y, vp.Z);
                //Rotation = new RealQuat(vr.X, vr.Y, vr.Z, vr.W);

                DistanceFromParent = Reader.ReadSingle();
            }
        }

        new internal class MarkerGroup : mode.MarkerGroup
        {
            internal MarkerGroup(CacheFile Cache)
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

                    Reader.BaseStream.Position += 12; //48
                }
            }
        }

        new internal class Shader : mode.Shader
        {
            internal Shader(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                Reader.BaseStream.Position += 12; 

                tagID = Reader.ReadInt32();

                Reader.BaseStream.Position += 28; //44
            }
        }

        new internal class ModelSection : mode.ModelSection
        {
            internal ModelSection(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                #region Submesh Block
                long temp = Reader.BaseStream.Position;
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = iOffset;
                Submeshes = new List<mode.ModelSection.Submesh>();
                for (int i = 0; i < iCount; i++)
                    Submeshes.Add(new Submesh(Cache));
                Reader.BaseStream.Position = temp + 12;
                #endregion

                #region Subset Block
                temp = Reader.BaseStream.Position;
                iCount = Reader.ReadInt32();
                iOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = iOffset;
                Subsets = new List<mode.ModelSection.Subset>();
                for (int i = 0; i < iCount; i++)
                    Subsets.Add(new Subset(Cache));
                Reader.BaseStream.Position = temp + 12;
                #endregion

                #region Other
                VertsIndex = Reader.ReadInt16();
                Reader.ReadInt32();
                UnknownIndex = Reader.ReadInt16();

                Reader.BaseStream.Position += 8; //40

                FacesIndex = Reader.ReadInt16();

                Reader.BaseStream.Position += 3; //45

                TransparentNodesPerVertex = Reader.ReadByte();
                NodeIndex = Reader.ReadByte();
                VertexFormat = Reader.ReadByte();
                OpaqueNodesPerVertex = Reader.ReadByte();

                Reader.BaseStream.Position += 43; //92
                #endregion
            }

            new internal class Submesh : mode.ModelSection.Submesh
            {
                internal Submesh(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    ShaderIndex = Reader.ReadInt16();
                    Reader.ReadInt16();
                    FaceIndex = Reader.ReadInt32();
                    FaceCount = Reader.ReadInt32();
                    SubsetIndex = Reader.ReadUInt16();
                    SubsetCount = Reader.ReadUInt16();
                    Reader.ReadInt16();
                    Reader.ReadInt16();
                    VertexCount = Reader.ReadUInt16();
                    Reader.ReadInt16();
                }
            }

            new internal class Subset : mode.ModelSection.Subset
            {
                internal Subset(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    FaceIndex = Reader.ReadInt32();
                    FaceCount = Reader.ReadInt32();
                    SubmeshIndex = Reader.ReadUInt16();
                    VertexCount = Reader.ReadUInt16();
                    Reader.ReadInt32();
                }
            }
        }

        new internal class BoundingBox : mode.BoundingBox
        {
            internal BoundingBox(CacheFile Cache, int Address)
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

        new internal class NodeIndexGroup : mode.NodeIndexGroup
        {
            internal NodeIndexGroup(CacheFile Cache)
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

            new internal class NodeIndex : mode.NodeIndexGroup.NodeIndex
            {
                internal NodeIndex(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    Index = Reader.ReadByte();
                }
            }
        }
    }
}

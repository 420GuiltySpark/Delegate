using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using Adjutant.Library.DataTypes.Space;
using mode = Adjutant.Library.Definitions.render_model;

namespace Adjutant.Library.Definitions.Halo3Retail
{
    internal class render_model : mode
    {
        internal render_model(CacheFile Cache)
        {
            EndianReader Reader = Cache.Reader;

            Name = Cache.Strings.GetItemByID(Reader.ReadInt32());

            Reader.BaseStream.Position += 8; //12

            #region Regions Block
            long temp = Reader.BaseStream.Position;
            int rCount = Reader.ReadInt32();
            int rOffset = Reader.ReadInt32() - Cache.Magic;
            Regions = new List<mode.Region>();
            Reader.BaseStream.Position = rOffset;
            for (int i = 0; i < rCount; i++)
                Regions.Add(new Region(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.ReadInt32();
            ExtrasIndex = Reader.ReadInt32();
            
            Reader.BaseStream.Position += 16; //48

            #region Nodes Block
            temp = Reader.BaseStream.Position;
            int nCount = Reader.ReadInt32();
            int nOffset = Reader.ReadInt32() - Cache.Magic;
            Nodes = new List<mode.Node>();
            Reader.BaseStream.Position = nOffset;
            for (int i = 0; i < nCount; i++)
                Nodes.Add(new Node(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            #region MarkerGroups Block
            temp = Reader.BaseStream.Position;
            int mCount = Reader.ReadInt32();
            int mOffset = Reader.ReadInt32() - Cache.Magic;
            MarkerGroups = new List<mode.MarkerGroup>();
            Reader.BaseStream.Position = mOffset;
            for (int i = 0; i < mCount; i++)
                MarkerGroups.Add(new MarkerGroup(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            #region Shaders Block
            temp = Reader.BaseStream.Position;
            int sCount = Reader.ReadInt32();
            int sOffset = Reader.ReadInt32() - Cache.Magic;
            Shaders = new List<mode.Shader>();
            Reader.BaseStream.Position = sOffset;
            for (int i = 0; i < sCount; i++)
                Shaders.Add(new Shader(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.BaseStream.Position += 20; //104

            #region ModelParts Block
            temp = Reader.BaseStream.Position;
            int pCount = Reader.ReadInt32();
            int pOffset = Reader.ReadInt32() - Cache.Magic;
            ModelParts = new List<mode.ModelPart>();
            Reader.BaseStream.Position = pOffset;
            for (int i = 0; i < pCount; i++)
                ModelParts.Add(new ModelPart(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            #region BoundingBox Block
            temp = Reader.BaseStream.Position;
            int bCount = Reader.ReadInt32();
            int bOffset = Reader.ReadInt32() - Cache.Magic;
            BoundingBoxs = new List<mode.BoundingBox>();
            Reader.BaseStream.Position = bOffset;
            for (int i = 0; i < bCount; i++)
                BoundingBoxs.Add(new BoundingBox(Cache));
            Reader.BaseStream.Position = temp + 12;
            #endregion

            Reader.BaseStream.Position += 96; //224
            
            RawID = Reader.ReadInt32();
            
            Reader.BaseStream.Position += 232; //460
        }

        new internal class Region : mode.Region
        {
            internal Region(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());

                long temp = Reader.BaseStream.Position;
                int pCount = Reader.ReadInt32();
                int pOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = pOffset;
                Permutations = new List<mode.Region.Permutation>();
                for (int i = 0; i < pCount; i++)
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
                    
                    Reader.BaseStream.Position += 10; //16
                }
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
                Position = new RealPoint3D(
                    Reader.ReadSingle(), 
                    Reader.ReadSingle(),
                    Reader.ReadSingle());
                Rotation = new RealVector4D(
                    Reader.ReadSingle(),
                    Reader.ReadSingle(),
                    Reader.ReadSingle(),
                    Reader.ReadSingle());
                Scale = Reader.ReadSingle();
                SkewX = new RealVector3D(
                    Reader.ReadSingle(),
                    Reader.ReadSingle(),
                    Reader.ReadSingle());
                SkewY = new RealVector3D(
                    Reader.ReadSingle(),
                    Reader.ReadSingle(),
                    Reader.ReadSingle());
                SkewZ = new RealVector3D(
                    Reader.ReadSingle(),
                    Reader.ReadSingle(),
                    Reader.ReadSingle());
                Center = new RealPoint3D(
                    Reader.ReadSingle(),
                    Reader.ReadSingle(),
                    Reader.ReadSingle());
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
                int mCount = Reader.ReadInt32();
                int mOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = mOffset;
                Markers = new List<mode.MarkerGroup.Marker>();
                for (int i = 0; i < mCount; i++)
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
                    Position = new RealPoint3D(
                        Reader.ReadSingle(),
                        Reader.ReadSingle(),
                        Reader.ReadSingle());
                    Rotation = new RealVector4D(
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
            internal Shader(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                Reader.BaseStream.Position += 12; 

                tagID = Reader.ReadInt32();

                Reader.BaseStream.Position += 20; //36
            }
        }

        new internal class ModelPart : mode.ModelPart
        {
            internal ModelPart(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                #region Submesh Block
                long temp = Reader.BaseStream.Position;
                int sCount = Reader.ReadInt32();
                int sOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = sOffset;
                Submeshes = new List<mode.ModelPart.Submesh>();
                for (int i = 0; i < sCount; i++)
                    Submeshes.Add(new Submesh(Cache));
                Reader.BaseStream.Position = temp + 12;
                #endregion

                #region Subset Block
                temp = Reader.BaseStream.Position;
                sCount = Reader.ReadInt32();
                sOffset = Reader.ReadInt32() - Cache.Magic;
                Reader.BaseStream.Position = sOffset;
                Subsets = new List<mode.ModelPart.Subset>();
                for (int i = 0; i < sCount; i++)
                    Subsets.Add(new Subset(Cache));
                Reader.BaseStream.Position = temp + 12;
                #endregion

                #region Other
                Reader.BaseStream.Position += 8; //32

                RawID = Reader.ReadInt32();

                Reader.BaseStream.Position += 4; //40

                ValidPartIndex = Reader.ReadInt16();

                Reader.BaseStream.Position += 2; //44

                TransparentNodesPerVertex = Reader.ReadByte();
                NodeIndex = Reader.ReadByte();
                VertexFormat = (VertexFormat)Reader.ReadByte();
                OpaqueNodesPerVertex = Reader.ReadByte();

                Reader.BaseStream.Position += 28; //76
                #endregion
            }

            new internal class Submesh : mode.ModelPart.Submesh
            {
                internal Submesh(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

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

            new internal class Subset : mode.ModelPart.Subset
            {
                internal Subset(CacheFile Cache)
                {
                    EndianReader Reader = Cache.Reader;

                    FaceIndex = Reader.ReadUInt16();
                    FaceCount = Reader.ReadUInt16();
                    SubmeshIndex = Reader.ReadUInt16();
                    VertexCount = Reader.ReadUInt16();
                }
            }
        }

        new internal class BoundingBox : mode.BoundingBox
        {
            internal BoundingBox(CacheFile Cache)
            {
                EndianReader Reader = Cache.Reader;

                Reader.ReadInt32();
                XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                UBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                VBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

                Reader.BaseStream.Position += 12; //56
            }
        }
    }
}

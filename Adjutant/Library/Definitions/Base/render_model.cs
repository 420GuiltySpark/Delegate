using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using Adjutant.Library.DataTypes.Space;

namespace Adjutant.Library.Definitions
{
    public abstract class render_model
    {
        public string Name;
        public List<Region> Regions;
        public int ExtrasIndex;
        public List<Node> Nodes;
        public List<MarkerGroup> MarkerGroups;
        public List<Shader> Shaders;
        public List<ModelPart> ModelParts;
        public List<BoundingBox> BoundingBoxs;
        public int RawID;

        public bool RawLoaded = false;

        public abstract class Region
        {
            public string Name;
            public List<Permutation> Permutations;

            public abstract class Permutation
            {
                public string Name;
                public int PieceIndex;

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

        public abstract class Node
        {
            public string Name;
            public int ParentIndex;
            public int FirstChildIndex;
            public int NextSiblingIndex;
            public RealPoint3D Position;
            public RealVector4D Rotation;
            public float Scale;
            public RealVector3D SkewX;
            public RealVector3D SkewY;
            public RealVector3D SkewZ;
            public RealPoint3D Center;
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
                public RealPoint3D Position;
                public RealVector4D Rotation;
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

        public abstract class ModelPart
        {
            public List<Submesh> Submeshes;
            public Vertex[] Vertices;
            public int[] Indices;
            
            public abstract class Submesh
            {
                public int ShaderIndex;
                public int FaceIndex;
                public int FaceCount;
                public int SubsetIndex;
                public int SubsetCount;
                public int VertexCount;
            }

            public List<Subset> Subsets;

            public abstract class Subset
            {
                public int FaceIndex;
                public int FaceCount;
                public int SubmeshIndex;
                public int VertexCount;
            }

            public int RawID;
            public int ValidPartIndex;
            public int TransparentNodesPerVertex;
            public int NodeIndex;
            public VertexFormat VertexFormat;
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
        }

        public abstract class BoundingBox
        {
            public RealBounds XBounds, YBounds, ZBounds;
            public RealBounds UBounds, VBounds;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;

namespace Adjutant.Library.Definitions
{
    public abstract class render_model
    {
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

        public bool RawLoaded = false;

        public abstract class Region
        {
            public string Name;
            public List<Permutation> Permutations;

            public abstract class Permutation
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

        public abstract class ModelSection
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

        public abstract class BoundingBox
        {
            public RealBounds XBounds, YBounds, ZBounds;
            public RealBounds UBounds, VBounds;
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

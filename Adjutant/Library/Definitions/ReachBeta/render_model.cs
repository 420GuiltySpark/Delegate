using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using mode = Adjutant.Library.Definitions.render_model;

namespace Adjutant.Library.Definitions.ReachBeta
{
    public class render_model : Halo3ODST.render_model
    {
        protected render_model() { }

        public render_model(CacheBase Cache, int Address)
        {
            cache = Cache;
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Name = Cache.Strings.GetItemByID(Reader.ReadInt32());
            Flags = new Bitmask(Reader.ReadInt32());

            #region Regions Block
            Reader.SeekTo(Address + 12);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Regions.Add(new Region(Cache, iOffset + 16 * i));
            #endregion

            Reader.SeekTo(Address + 28);
            InstancedGeometryIndex = Reader.ReadInt32();

            #region Instanced Geometry Block
            Reader.SeekTo(Address + 32);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                GeomInstances.Add(new InstancedGeometry(Cache, iOffset + 60 * i));
            #endregion

            #region Nodes Block
            Reader.SeekTo(Address + 48);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Nodes.Add(new Node(Cache, iOffset + 96 * i));
            #endregion

            #region MarkerGroups Block
            Reader.SeekTo(Address + 60);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                MarkerGroups.Add(new MarkerGroup(Cache, iOffset + 16 * i));
            #endregion

            #region Shaders Block
            Reader.SeekTo(Address + 72);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new Shader(Cache, iOffset + 44 * i));
            #endregion

            #region ModelSections Block
            Reader.SeekTo(Address + 104);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                ModelSections.Add(new ModelSection(Cache, iOffset + 92 * i));
            #endregion

            #region BoundingBox Block
            Reader.SeekTo(Address + 116);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                BoundingBoxes.Add(new BoundingBox(Cache, iOffset + 52 * i));
            #endregion

            #region NodeMapGroup Block
            Reader.SeekTo(Address + 176);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                NodeIndexGroups.Add(new NodeIndexGroup(Cache, iOffset + 12 * i));
            #endregion

            Reader.SeekTo(Address + 236);
            RawID = Reader.ReadInt32();
        }

        new public class MarkerGroup : mode.MarkerGroup
        {
            public MarkerGroup(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());

                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                for (int i = 0; i < iCount; i++)
                    Markers.Add(new Marker(Cache, iOffset + 48 * i));
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
            public Shader(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.SeekTo(Address + 12);
                tagID = Reader.ReadInt32();
            }
        }

        new public class ModelSection : mode.ModelSection
        {
            public ModelSection(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                #region Submesh Block
                int iCount = Reader.ReadInt32();
                int iOffset = Reader.ReadInt32() - Cache.Magic;
                for (int i = 0; i < iCount; i++)
                    Submeshes.Add(new Submesh(Cache, iOffset + 24 * i));
                #endregion

                #region Subset Block
                Reader.SeekTo(Address + 12);
                iCount = Reader.ReadInt32();
                iOffset = Reader.ReadInt32() - Cache.Magic;
                for (int i = 0; i < iCount; i++)
                    Subsets.Add(new Subset(Cache, iOffset + 16 * i));
                #endregion

                #region Other
                Reader.SeekTo(Address + 24);

                VertsIndex = Reader.ReadInt16();
                Reader.ReadInt32();
                UnknownIndex = Reader.ReadInt16();

                Reader.SeekTo(Address + 40);
                FacesIndex = Reader.ReadInt16();

                Reader.SeekTo(Address + 45);
                TransparentNodesPerVertex = Reader.ReadByte();
                NodeIndex = Reader.ReadByte();
                VertexFormat = Reader.ReadByte();
                OpaqueNodesPerVertex = Reader.ReadByte();
                #endregion
            }

            new public class Submesh : mode.ModelSection.Submesh
            {
                public Submesh(CacheBase Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

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

            new public class Subset : mode.ModelSection.Subset
            {
                public Subset(CacheBase Cache, int Address)
                {
                    EndianReader Reader = Cache.Reader;
                    Reader.SeekTo(Address);

                    FaceIndex = Reader.ReadInt32();
                    FaceCount = Reader.ReadInt32();
                    SubmeshIndex = Reader.ReadUInt16();
                    VertexCount = Reader.ReadUInt16();
                    Reader.ReadInt32();
                }
            }
        }

        new public class BoundingBox : mode.BoundingBox
        {
            public BoundingBox(CacheBase Cache, int Address)
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
    }
}
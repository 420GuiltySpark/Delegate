using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using sbsp = Adjutant.Library.Definitions.scenario_structure_bsp;
using mode = Adjutant.Library.Definitions.render_model;

namespace Adjutant.Library.Definitions.Halo3Beta
{
    public class scenario_structure_bsp : sbsp
    {
        public scenario_structure_bsp(CacheFile Cache, int Address)
        {
            cache = Cache;
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            #region sldt ID
            //sldt's sections address will be used instead of the one in sbsp
            int sectionAddress = 0;
            foreach (var item in Cache.IndexItems)
            {
                if (item.ClassCode == "scnr")
                {
                    Reader.SeekTo(item.Offset + 12);
                    int cnt = Reader.ReadInt32();
                    int ptr = Reader.ReadInt32() - Cache.Magic;

                    int bspIndex = 0;

                    for (int i = 0; i < cnt; i++)
                    {
                        Reader.SeekTo(ptr + 104 * i + 12);
                        if (Cache.IndexItems.GetItemByID(Reader.ReadInt32()).Offset == Address)
                        {
                            bspIndex = i;
                            break;
                        }
                    }

                    Reader.SeekTo(item.Offset + 1720 + 12);
                    int sldtID = Reader.ReadInt32();
                    int sldtAddress = Cache.IndexItems.GetItemByID(sldtID).Offset;

                    Reader.SeekTo(sldtAddress + 4);
                    cnt = Reader.ReadInt32();
                    ptr = Reader.ReadInt32() - Cache.Magic;

                    for (int i = 0; i < cnt; i++)
                    {
                        Reader.SeekTo(ptr + 436 * i + 2);

                        if (Reader.ReadInt16() != bspIndex) continue;

                        Reader.SeekTo(ptr + 436 * i + 312);
                        sectionAddress = Reader.ReadInt32() - Cache.Magic;

                        Reader.SeekTo(ptr + 436 * i + 428);
                        geomRawID = Reader.ReadInt32();
                    }

                    break;
                }
            }
            #endregion

            Reader.SeekTo(Address + 60);

            XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

            Reader.SeekTo(Address + 180);

            #region Clusters Block
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            Clusters = new List<sbsp.Cluster>();
            for (int i = 0; i < iCount; i++)
                Clusters.Add(new Cluster(Cache, iOffset + 236 * i));
            #endregion

            Reader.SeekTo(Address + 192);

            #region Shaders Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Shaders = new List<sbsp.Shader>();
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new Shader(Cache, iOffset + 36 * i));
            #endregion

            Reader.SeekTo(Address + 432);

            #region GeometryInstances Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            GeomInstances = new List<sbsp.InstancedGeometry>();
            for (int i = 0; i < iCount; i++)
                GeomInstances.Add(new InstancedGeometry(Cache, iOffset + 120 * i));
            #endregion

            Reader.SeekTo(Address + 580);
            RawID1 = Reader.ReadInt32();

            Reader.SeekTo(Address + 740);

            #region ModelParts Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            ModelSections = new List<mode.ModelSection>();
            for (int i = 0; i < iCount; i++)
                ModelSections.Add(new Halo3Beta.render_model.ModelSection(Cache, sectionAddress + 76 * i));
            #endregion

            Reader.SeekTo(Address + 752);

            #region Bounding Boxes Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            BoundingBoxes = new List<mode.BoundingBox>();
            for (int i = 0; i < iCount; i++)
                BoundingBoxes.Add(new Halo3Beta.render_model.BoundingBox(Cache, iOffset + 44 * i));
            #endregion

            Reader.SeekTo(Address + 860);
            RawID2 = Reader.ReadInt32();

            Reader.SeekTo(Address + 892);
            RawID3 = Reader.ReadInt32();
        }

        new public class Cluster : sbsp.Cluster
        {
            public Cluster(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

                Reader.SeekTo(Address + 172);
                SectionIndex = Reader.ReadInt16();
            }
        }

        new public class Shader : sbsp.Shader
        {
            public Shader(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.SeekTo(Address + 12);

                tagID = Reader.ReadInt32();

                Reader.SeekTo(Address + 36);
            }
        }

        new public class InstancedGeometry : sbsp.InstancedGeometry
        {
            public InstancedGeometry(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

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

                SectionIndex = Reader.ReadInt16();

                Reader.SeekTo(Address + 84);
                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());

                Reader.SeekTo(Address + 120);
            }
        }
    }
}

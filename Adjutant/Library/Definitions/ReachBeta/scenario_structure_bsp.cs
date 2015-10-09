using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using System.IO;
using sbsp = Adjutant.Library.Definitions.scenario_structure_bsp;
using mode = Adjutant.Library.Definitions.render_model;

namespace Adjutant.Library.Definitions.ReachBeta
{
    public class scenario_structure_bsp : Halo3ODST.scenario_structure_bsp
    {
        protected scenario_structure_bsp() { }

        public scenario_structure_bsp(CacheBase Cache, int Address)
        {
            cache = Cache;
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            #region sldt/lbsp ID
            //lbsp's sections address will be used instead of the one in sbsp
            int sectionAddress = 0;
            foreach (var item in Cache.IndexItems)
            {
                if (item.ClassCode == "scnr")
                {
                    Reader.SeekTo(item.Offset + 68);
                    int cnt = Reader.ReadInt32();
                    int ptr = Reader.ReadInt32() - Cache.Magic;

                    int bspIndex = 0;

                    for (int i = 0; i < cnt; i++)
                    {
                        Reader.SeekTo(ptr + 168 * i + 12);
                        if (Cache.IndexItems.GetItemByID(Reader.ReadInt32()).Offset == Address)
                        {
                            bspIndex = i;
                            break;
                        }
                    }

                    Reader.SeekTo(item.Offset + 1828 + 12);
                    int sldtID = Reader.ReadInt32();
                    var sldt = Cache.IndexItems.GetItemByID(sldtID);
                    int sldtAddress = Cache.IndexItems.GetItemByID(sldtID).Offset;

                    Reader.SeekTo(sldtAddress + 4);
                    cnt = Reader.ReadInt32();
                    ptr = Reader.ReadInt32() - Cache.Magic;

                    Reader.SeekTo(ptr + 32 * bspIndex + 12);
                    int lbspID = Reader.ReadInt32();
                    var lbsp = Cache.IndexItems.GetItemByID(lbspID);
                    int lbspAddress = Cache.IndexItems.GetItemByID(lbspID).Offset;

                    Reader.SeekTo(lbspAddress + 116);
                    sectionAddress = Reader.ReadInt32() - Cache.Magic;

                    Reader.SeekTo(lbspAddress + 244);
                    geomRawID = Reader.ReadInt32();
                    break;
                }
            }
            #endregion

            Reader.SeekTo(Address + 236);
            XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

            #region Clusters Block
            Reader.SeekTo(Address + 308);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            Clusters = new List<sbsp.Cluster>();
            for (int i = 0; i < iCount; i++)
                Clusters.Add(new Cluster(Cache, iOffset + 288 * i));
            #endregion

            #region Shaders Block
            Reader.SeekTo(Address + 320);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Shaders = new List<mode.Shader>();
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new ReachBeta.render_model.Shader(Cache, iOffset + 44 * i));
            #endregion

            #region GeometryInstances Block
            Reader.SeekTo(Address + 620);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            GeomInstances = new List<sbsp.InstancedGeometry>();
            for (int i = 0; i < iCount; i++)
                GeomInstances.Add(new InstancedGeometry(Cache, iOffset + 168 * i));
            #endregion

            Reader.SeekTo(Address + 796);
            RawID1 = Reader.ReadInt32();

            Reader.SeekTo(Address + 976);
            RawID2 = Reader.ReadInt32();

            #region ModelParts Block
            Reader.SeekTo(Address + 1112);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            ModelSections = new List<mode.ModelSection>();
            for (int i = 0; i < iCount; i++)
                ModelSections.Add(new ReachBeta.render_model.ModelSection(Cache, sectionAddress + 92 * i));
            #endregion

            #region Bounding Boxes Block
            Reader.SeekTo(Address + 1124);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            BoundingBoxes = new List<mode.BoundingBox>();
            for (int i = 0; i < iCount; i++)
                BoundingBoxes.Add(new ReachBeta.render_model.BoundingBox(Cache, iOffset + 52 * i));
            #endregion

            Reader.SeekTo(Address + 1244);
            RawID3 = Reader.ReadInt32();
        }

        new public class Cluster : sbsp.Cluster
        {
            public Cluster(CacheBase Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

                Reader.SeekTo(Address + 208);
                SectionIndex = Reader.ReadInt16();
            }
        }

        new public class InstancedGeometry : sbsp.InstancedGeometry
        {
            public InstancedGeometry(CacheBase Cache, int Address)
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

                Reader.SeekTo(Address + 124);
                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());
            }
        }
    }
}

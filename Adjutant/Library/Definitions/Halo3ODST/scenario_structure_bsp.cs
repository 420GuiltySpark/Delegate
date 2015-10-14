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

namespace Adjutant.Library.Definitions.Halo3ODST
{
    public class scenario_structure_bsp : Halo3Retail.scenario_structure_bsp
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
                    Reader.SeekTo(item.Offset + 20);
                    int cnt = Reader.ReadInt32();
                    int ptr = Reader.ReadInt32() - Cache.Magic;

                    int bspIndex = 0;

                    for (int i = 0; i < cnt; i++)
                    {
                        Reader.SeekTo(ptr + 108 * i + 12);
                        if (Cache.IndexItems.GetItemByID(Reader.ReadInt32()).Offset == Address)
                        {
                            bspIndex = i;
                            break;
                        }
                    }

                    Reader.SeekTo(item.Offset + 1852 + 12);
                    int sldtID = Reader.ReadInt32();
                    int sldtAddress = Cache.IndexItems.GetItemByID(sldtID).Offset;

                    Reader.SeekTo(sldtAddress + 4);
                    cnt = Reader.ReadInt32();
                    ptr = Reader.ReadInt32() - Cache.Magic;

                    Reader.SeekTo(ptr + 16 * bspIndex + 12);
                    int lbspID = Reader.ReadInt32();
                    int lbspAddress = Cache.IndexItems.GetItemByID(lbspID).Offset;

                    Reader.SeekTo(lbspAddress + 312);
                    sectionAddress = Reader.ReadInt32() - Cache.Magic;

                    Reader.SeekTo(lbspAddress + 428);
                    geomRawID = Reader.ReadInt32();
                    break;
                }
            }
            #endregion

            Reader.SeekTo(Address + 64);
            XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

            #region Clusters Block
            Reader.SeekTo(Address + 184);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Clusters.Add(new Cluster(Cache, iOffset + 220 * i));
            #endregion

            #region Shaders Block
            Reader.SeekTo(Address + 196);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new Halo3ODST.render_model.Shader(Cache, iOffset + 36 * i));
            #endregion

            #region GeometryInstances Block
            Reader.SeekTo(Address + 436);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                GeomInstances.Add(new InstancedGeometry(Cache, iOffset + 120 * i));
            #endregion

            Reader.SeekTo(Address + 584);
            RawID1 = Reader.ReadInt32();

            #region ModelParts Block
            Reader.SeekTo(Address + 744);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                ModelSections.Add(new Halo3ODST.render_model.ModelSection(Cache, sectionAddress + 76 * i));
            #endregion

            #region Bounding Boxes Block
            Reader.SeekTo(Address + 756);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                BoundingBoxes.Add(new Halo3ODST.render_model.BoundingBox(Cache, iOffset + 44 * i));
            #endregion

            Reader.SeekTo(Address + 864);
            RawID2 = Reader.ReadInt32();

            Reader.SeekTo(Address + 896);
            RawID3 = Reader.ReadInt32();
        }
    }
}

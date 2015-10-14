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

namespace Adjutant.Library.Definitions.Halo4Retail
{
    public class scenario_structure_bsp : ReachRetail.scenario_structure_bsp
    {
        public scenario_structure_bsp(CacheBase Cache, int Address)
        {
            cache = Cache;
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            #region sldt/lbsp ID
            //lbsp's sections address will be used instead of the one in sbsp
            int sectCount = 0, sectionAddress = 0, bbCount = 0, bbAddr = 0;
            foreach (var item in Cache.IndexItems)
            {
                if (item.ClassCode == "scnr")
                {
                    Reader.SeekTo(item.Offset + 160);
                    int cnt = Reader.ReadInt32();
                    int ptr = Reader.ReadInt32() - Cache.Magic;

                    int bspIndex = 0;

                    for (int i = 0; i < cnt; i++)
                    {
                        Reader.SeekTo(ptr + 336 * i + 12);
                        if (Cache.IndexItems.GetItemByID(Reader.ReadInt32()).Offset == Address)
                        {
                            bspIndex = i;
                            break;
                        }
                    }

                    Reader.SeekTo(item.Offset + 1896 + 12);
                    int sldtID = Reader.ReadInt32();
                    int sldtAddress = Cache.IndexItems.GetItemByID(sldtID).Offset;

                    Reader.SeekTo(sldtAddress + 4);
                    cnt = Reader.ReadInt32();
                    ptr = Reader.ReadInt32() - Cache.Magic;

                    Reader.SeekTo(ptr + 32 * bspIndex + 12);
                    int lbspID = Reader.ReadInt32();
                    int lbspAddress = Cache.IndexItems.GetItemByID(lbspID).Offset;

                    Reader.SeekTo(lbspAddress + 320); //320, 512, 692
                    sectCount = Reader.ReadInt32();
                    sectionAddress = Reader.ReadInt32() - Cache.Magic;

                    Reader.SeekTo(lbspAddress + 344); //344, 536, 716
                    bbCount = Reader.ReadInt32();
                    bbAddr = Reader.ReadInt32() - Cache.Magic;

                    Reader.SeekTo(lbspAddress + 464); //464, 656, 836
                    geomRawID = Reader.ReadInt32();
                    break;
                }
            }
            #endregion

            Reader.SeekTo(Address + 268);
            XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
            ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

            #region Clusters Block
            Reader.SeekTo(Address + 340);
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < 1; i++)
                Clusters.Add(new Cluster(Cache, iOffset + 140 * i));
            #endregion

            #region Shaders Block
            Reader.SeekTo(Address + 352);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new Halo4Retail.render_model.Shader(Cache, iOffset + 44 * i));
            #endregion

            #region GeometryInstances Block
            Reader.SeekTo(Address + 640);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < iCount; i++)
                GeomInstances.Add(new InstancedGeometry(Cache, iOffset + 4 * i));

            #region Load Fixup Data
            Reader.SeekTo(Address + 1364);
            int id = Reader.ReadInt32();
            var entry = Cache.zone.RawEntries[id & 0xFFFF];
            var er = new EndianReader(new MemoryStream(Cache.GetRawFromID(id)), EndianFormat.BigEndian);
            int addr = entry.Fixups[entry.Fixups.Count - 10].Offset;

            for (int i = 0; i < GeomInstances.Count; i++)
            {
                er.SeekTo(addr + 148 * i);
                var geom = GeomInstances[i];

                geom.TransformScale = er.ReadSingle();

                geom.TransformMatrix.m11 = er.ReadSingle();
                geom.TransformMatrix.m12 = er.ReadSingle();
                geom.TransformMatrix.m13 = er.ReadSingle();

                geom.TransformMatrix.m21 = er.ReadSingle();
                geom.TransformMatrix.m22 = er.ReadSingle();
                geom.TransformMatrix.m23 = er.ReadSingle();

                geom.TransformMatrix.m31 = er.ReadSingle();
                geom.TransformMatrix.m32 = er.ReadSingle();
                geom.TransformMatrix.m33 = er.ReadSingle();

                geom.TransformMatrix.m41 = er.ReadSingle();
                geom.TransformMatrix.m42 = er.ReadSingle();
                geom.TransformMatrix.m43 = er.ReadSingle();

                er.ReadUInt16();
                er.ReadUInt16();
                er.ReadInt32();
                er.ReadUInt16();
                geom.SectionIndex = er.ReadUInt16();
            }
            er.Close();
            er.Dispose(); 
            #endregion
            #endregion

            Reader.SeekTo(Address + 844);
            RawID1 = Reader.ReadInt32();

            Reader.SeekTo(Address + 1048);
            RawID2 = Reader.ReadInt32();

            #region ModelSections Block
            Reader.SeekTo(Address + 1144);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            if (sectionAddress == -Cache.Magic) sectionAddress = iOffset; //null address in lbsp
            for (int i = 0; i < sectCount; i++)
                ModelSections.Add(new Halo4Retail.render_model.ModelSection(Cache, sectionAddress + 112 * i));
            #endregion

            #region Bounding Boxes Block
            Reader.SeekTo(Address + 1168);
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            for (int i = 0; i < bbCount; i++)
                BoundingBoxes.Add(new Halo4Retail.render_model.BoundingBox(Cache, bbAddr + 52 * i));
            #endregion

            Reader.SeekTo(Address + 1288);
            RawID3 = Reader.ReadInt32();
        }
    }
}

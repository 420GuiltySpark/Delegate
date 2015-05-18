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
    internal class scenario_structure_bsp : sbsp
    {
        internal scenario_structure_bsp(CacheFile Cache, int Address)
        {
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

            Reader.SeekTo(Address + 340);

            #region Clusters Block
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            Clusters = new List<sbsp.Cluster>();
            for (int i = 0; i < 1; i++)
                Clusters.Add(new Cluster(Cache, iOffset + 140 * i));
            #endregion

            Reader.SeekTo(Address + 352);

            #region Shaders Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            Shaders = new List<sbsp.Shader>();
            for (int i = 0; i < iCount; i++)
                Shaders.Add(new Shader(Cache, iOffset + 44 * i));
            #endregion

            Reader.SeekTo(Address + 640);

            #region GeometryInstances Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            GeomInstances = new List<sbsp.InstancedGeometry>();

            for (int i = 0; i < iCount; i++)
                GeomInstances.Add(new InstancedGeometry(Cache, iOffset + 4 * i));

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

            Reader.SeekTo(Address + 844);
            RawID1 = Reader.ReadInt32();

            Reader.SeekTo(Address + 1048);
            RawID2 = Reader.ReadInt32();

            Reader.SeekTo(Address + 1144);

            #region ModelSections Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            if (sectionAddress == -Cache.Magic) sectionAddress = iOffset; //null address in lbsp
            ModelSections = new List<mode.ModelSection>();
            for (int i = 0; i < sectCount; i++)
                ModelSections.Add(new Halo4Retail.render_model.ModelSection(Cache, sectionAddress + 112 * i));
            #endregion

            Reader.SeekTo(Address + 1168);

            #region Bounding Boxes Block
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            BoundingBoxes = new List<mode.BoundingBox>();
            for (int i = 0; i < bbCount; i++)
                BoundingBoxes.Add(new Halo4Retail.render_model.BoundingBox(Cache, bbAddr + 52 * i));
            #endregion

            Reader.SeekTo(Address + 1288);
            RawID3 = Reader.ReadInt32();

            Reader.SeekTo(Address + 1388);

            #region Prefabs
            //iCount = Reader.ReadInt32();
            //iOffset = Reader.ReadInt32() - Cache.Magic;
            //Prefabs = new List<sbsp.Prefab>();
            //for (int i = 0; i < iCount; i++)
            //    Prefabs.Add(new Prefab(Cache, iOffset + 112 * i));
            #endregion
        }

        new internal class Cluster : sbsp.Cluster
        {
            internal Cluster(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                XBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                YBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());
                ZBounds = new RealBounds(Reader.ReadSingle(), Reader.ReadSingle());

                Reader.SeekTo(Address + 64);
                SectionIndex = Reader.ReadInt16();
            }
        }

        new internal class Shader : sbsp.Shader
        {
            internal Shader(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.SeekTo(Address + 12);

                tagID = Reader.ReadInt32();

                Reader.SeekTo(Address + 44);
            }
        }

        new internal class InstancedGeometry : sbsp.InstancedGeometry
        {
            internal InstancedGeometry(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());
            }
        }

        new internal class Prefab : sbsp.Prefab
        {
            internal Prefab(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Reader.Skip(16);
                Name = Cache.Strings.GetItemByID(Reader.ReadInt32());

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

                InstanceCount = Reader.ReadUInt16();
                InstanceIndex = Reader.ReadUInt16();
            }
        }
    }
}

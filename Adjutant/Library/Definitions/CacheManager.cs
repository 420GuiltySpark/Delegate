using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Adjutant.Library.Endian;
using Adjutant.Library.Definitions;

namespace Adjutant.Library.Definitions
{
    public static class CacheManager
    {
        public static CacheBase GetCache(string Filename)
        {
            CacheBase retCache = null;
            using (var fs = new FileStream(Filename, FileMode.Open, FileAccess.Read))
            {
                var Reader = new EndianReader((Stream)fs, EndianFormat.BigEndian);

                var head = Reader.ReadInt32();
                if (head == 1684104552) Reader.EndianType = EndianFormat.LittleEndian;
                var v = Reader.ReadInt32();

                switch (v)
                {
                    case 5:   //H1X
                    case 7:   //HPC
                    case 609: //HCE
                        Reader.SeekTo(64);
                        break;
                    case 8:   //H2?
                        Reader.SeekTo(36);
                        switch (Reader.ReadInt32())
                        {
                            case 0: //H2X
                                Reader.SeekTo(288);
                                break;
                            case -1: //H2V
                                Reader.SeekTo(300);
                                break;
                        }
                        break;
                    default:  //360
                        Reader.SeekTo(284);
                        break;
                }

                var build = Reader.ReadString(32);
                var node = CacheBase.GetBuildNode(build);
                switch (node.Attributes["definitions"].Value)
                {
                    case "Halo1PC":     retCache = new Halo1PC.CacheFile(Filename, build); break;
                    case "Halo1CE":     retCache = new Halo1CE.CacheFile(Filename, build); break;
                    case "Halo1AE":     retCache = new Halo1AE.CacheFile(Filename, build); break;
                    case "Halo2Xbox":   retCache = new Halo2Xbox.CacheFile(Filename, build); break;
                    case "Halo2Vista":  retCache = new Halo2Vista.CacheFile(Filename, build); break;
                    case "Halo3Beta":   retCache = new Halo3Beta.CacheFile(Filename, build); break;
                    case "Halo3Retail": retCache = new Halo3Retail.CacheFile(Filename, build); break;
                    case "Halo3ODST":   retCache = new Halo3ODST.CacheFile(Filename, build); break;
                    case "ReachBeta":   retCache = new ReachBeta.CacheFile(Filename, build); break;
                    case "ReachRetail": retCache = new ReachRetail.CacheFile(Filename, build); break;
                    case "Halo4Beta":   retCache = new Halo4Beta.CacheFile(Filename, build); break;
                    case "Halo4Retail": retCache = new Halo4Retail.CacheFile(Filename, build); break;
                }
            }

            if (retCache != null)
            {
                retCache.LoadPlayZone();
                return retCache;
            }
            else throw new NotSupportedException("Cache version is unknown, unsupported or invalid.");
        }
    }
}

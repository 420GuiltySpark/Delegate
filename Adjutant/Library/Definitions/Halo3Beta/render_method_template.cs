using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Endian;
using Adjutant.Library.DataTypes;
using rmt2 = Adjutant.Library.Definitions.render_method_template;

namespace Adjutant.Library.Definitions.Halo3Beta
{
    public class render_method_template : rmt2
    {
        public render_method_template(CacheFile Cache, int Address)
        {
            EndianReader Reader = Cache.Reader;
            Reader.SeekTo(Address);

            Reader.SeekTo(Address + 72);

            #region Usage Blocks
            int iCount = Reader.ReadInt32();
            int iOffset = Reader.ReadInt32() - Cache.Magic;
            ArgumentBlocks = new List<rmt2.ArgumentBlock>();
            for (int i = 0; i < iCount; i++)
                ArgumentBlocks.Add(new ArgumentBlock(Cache, iOffset + 4 * i));
            #endregion

            Reader.SeekTo(Address + 108);

            #region Usage Blocks
            iCount = Reader.ReadInt32();
            iOffset = Reader.ReadInt32() - Cache.Magic;
            UsageBlocks = new List<rmt2.UsageBlock>();
            for (int i = 0; i < iCount; i++)
                UsageBlocks.Add(new UsageBlock(Cache, iOffset + 4 * i));
            #endregion

            Reader.SeekTo(Address + 132);
        }

        new public class ArgumentBlock : rmt2.ArgumentBlock
        {
            public ArgumentBlock(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Argument = Cache.Strings.GetItemByID(Reader.ReadInt32());
            }
        }

        new public class UsageBlock : rmt2.UsageBlock
        {
            public UsageBlock(CacheFile Cache, int Address)
            {
                EndianReader Reader = Cache.Reader;
                Reader.SeekTo(Address);

                Usage = Cache.Strings.GetItemByID(Reader.ReadInt32());
            }
        }
    }
}

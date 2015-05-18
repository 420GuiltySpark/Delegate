using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adjutant.Library.Definitions
{
    public abstract class soundbank
    {
        public int unk0;
        public int unk1;
        public int unk2;
        public List<BankReference> BankRefs;
        public int unk3;
        public uint Address;

        public abstract class BankReference
        {
            public uint Address;
        }
    }
}

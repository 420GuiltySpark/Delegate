using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adjutant.Library.Definitions
{
    public abstract class multilingual_unicode_string_list
    {
        //using lists rather than a bunch of ints makes it easier
        //to access a specific language (just use language index)
        public List<int> Indices;
        public List<int> Lengths;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.Cache;

namespace Adjutant.Library.Controls.MetaViewerControls
{
    internal class MetaViewerControl : UserControl
    {
        protected iValue value;
        protected CacheFile cache;

        public virtual void Reload(int ParentAddress)
        {
            throw new NotImplementedException("MetaViewerControls must override the \"Reload\" method!");
        }
    }
}

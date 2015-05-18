using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.Cache;

namespace Adjutant.Library.Controls.MetaViewerControls
{
    internal partial class mHeader : MetaViewerControl
    {
        public mHeader()
        {
            InitializeComponent();
        }

        public mHeader(iValue Value, CacheFile Cache)
        {
            InitializeComponent();
            value = Value;
            cache = Cache;

            SetText(value.Node.Attributes["name"].Value);
        }

        public void SetText(string Text)
        {
            lblText.Text = Text;
        }

        public override void Reload(int ParentAddress)
        {
            return;
        }
    }
}

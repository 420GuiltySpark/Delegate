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
    internal partial class mComment : MetaViewerControl
    {
        public mComment()
        {
            InitializeComponent();
        }

        public mComment(iValue Value, CacheFile Cache)
        {
            InitializeComponent();
            value = Value;
            cache = Cache;

            SetText(value.Node.Attributes["name"].Value, value.Node.InnerText);
        }

        public void SetText(string Title, string description)
        {
            lblTitle.Text = Title;
            lblDesc.Text = description;
            if (description != "")
                this.Height = lblDesc.Height + 40;
            else
                this.Height = 37;
        }

        public override void Reload(int ParentAddress)
        {
            return;
        }
    }
}

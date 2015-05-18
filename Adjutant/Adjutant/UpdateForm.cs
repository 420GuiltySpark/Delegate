using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Adjutant
{
    public partial class UpdateForm : Form
    {
        public UpdateForm()
        {
            InitializeComponent();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            System.IO.File.WriteAllBytes(Application.StartupPath + '\\' + "update.exe", Properties.Resources.update);
            System.Diagnostics.Process.Start(Application.StartupPath + '\\' + "update.exe");
            Application.Exit();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

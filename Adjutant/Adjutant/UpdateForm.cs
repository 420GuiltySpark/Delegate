using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Adjutant
{
    public partial class UpdateForm : Form
    {
        private Settings settings;

        public UpdateForm(ref Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            var exeString = "https://dl.dropboxusercontent.com/u/39530625/Haquez%20Co/Adjutant/Adjutant.exe";
            var logString = "https://dl.dropboxusercontent.com/u/39530625/Haquez%20Co/Adjutant/changelog.txt";

            System.IO.File.WriteAllBytes(Application.StartupPath + '\\' + "update.exe", Properties.Resources.update);
            var startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = Application.StartupPath + '\\' + "update.exe";
            startInfo.Arguments = exeString + " " + logString;
            Process.Start(startInfo);
            Application.Exit();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

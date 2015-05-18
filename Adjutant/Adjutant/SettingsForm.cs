using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.Definitions;
using Adjutant.Library.Controls;
//using Adjutant.Security;

namespace Adjutant
{
    public partial class SettingsForm : Form
    {
        private Form1 MainForm;

        public SettingsForm(Form1 Owner)
        {
            InitializeComponent();
            MainForm = Owner;
        }

        #region Events
        private void SettingsForm_Load(object sender, EventArgs e)
        {
            var settings = MainForm.settings;

            txtMapPath.Text = settings.mapFolder;
            txtDataPath.Text = settings.dataFolder;
            txtPluginPath.Text = settings.pluginFolder;

            txtClassFilter.Text = settings.classFilter;
            txtPermFilter.Text = settings.permFilter;

            for (int i = 0; i < chkFlags.Items.Count; i++)
                chkFlags.SetItemChecked(i, settings.Flags.HasFlag((SettingsFlags)(1 << i)));

            btnColour.BackColor = settings.ViewerColour;

            cmbBitm.SelectedIndex = (int)settings.BitmFormat;
            cmbMode.SelectedIndex = (int)settings.ModeFormat;
            cmbSnd_.SelectedIndex = (int)settings.Snd_Format;
            cmbLang.SelectedIndex = (int)settings.Language;
        }

        private void btnBrowseMap_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                SelectedPath = MainForm.settings.mapFolder,
                ShowNewFolderButton = true,
                Description = "Select your map folder."
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            txtMapPath.Text = dialog.SelectedPath;
        }

        private void btnBrowseData_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                SelectedPath = MainForm.settings.mapFolder,
                ShowNewFolderButton = true,
                Description = "Select your extraction folder."
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            txtDataPath.Text = dialog.SelectedPath;
        }

        private void btnBrowsePlugins_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                SelectedPath = MainForm.settings.mapFolder,
                ShowNewFolderButton = true,
                Description = "Select your plugins folder."
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            txtPluginPath.Text = dialog.SelectedPath;
        }

        private void btnColour_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog()
            {
                CustomColors = new int[] { 15570276 }
            };

            if (cd.ShowDialog() != DialogResult.OK) return;

            btnColour.BackColor = cd.Color;
        }

        private void btnColour_BackColorChanged(object sender, EventArgs e)
        {
            btnColour.ForeColor = Color.FromArgb(0xFFFFFF - btnColour.BackColor.ToArgb());
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var settings = MainForm.settings;

            settings.mapFolder = txtMapPath.Text;
            settings.dataFolder = txtDataPath.Text;
            settings.pluginFolder = txtPluginPath.Text;

            settings.classFilter = txtClassFilter.Text;
            settings.permFilter = txtPermFilter.Text;

            var newFlags = new SettingsFlags();

            for (int i = 0; i < chkFlags.Items.Count; i++)
                if (chkFlags.GetItemChecked(i)) newFlags |= (SettingsFlags)(1 << i);

            settings.ViewerColour = btnColour.BackColor;

            settings.Flags = newFlags;

            settings.BitmFormat = (BitmapFormat)cmbBitm.SelectedIndex;
            settings.ModeFormat = (ModelFormat)cmbMode.SelectedIndex;
            settings.Snd_Format = (SoundFormat)cmbSnd_.SelectedIndex;
            settings.Language = (Language)cmbLang.SelectedIndex;

            MainForm.SaveSettings();
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
    }
}

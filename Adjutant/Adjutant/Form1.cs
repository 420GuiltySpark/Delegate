using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Controls;
using Microsoft.Win32;
using Adjutant.Library.Controls;
using Adjutant.Library.Definitions;
using System.Threading;
using System.Net;

namespace Adjutant
{
    public partial class Form1 : Form
    {
        public Settings settings;
        private int taskcount = 0;
        private int tasks
        {
            get { return taskcount; }
            set
            {
                taskcount = value;
                Invoke((MethodInvoker)delegate
                {
                    toolStripProgressBar1.Visible = taskcount != 0;
                });
            }
        }

        public Form1()
        {
            InitializeComponent();
            LoadSettings();
            versionToolStripMenuItem.Text = "v" + Application.ProductVersion;

            if (settings.Flags.HasFlag(SettingsFlags.AutoUpdateCheck))
                ThreadPool.QueueUserWorkItem(CheckUpdateThread, false);
        }

        #region Methods
        public void LoadSettings()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Haquez Co.\\Adjutant");
            try { settings = new Settings(new MemoryStream((byte[])key.GetValue("Settings"))); }
            catch { settings = new Settings(); }

            folderHierarchyToolStripMenuItem.Checked = settings.Flags.HasFlag(SettingsFlags.HierarchyView);
            tagClassToolStripMenuItem.Checked = !settings.Flags.HasFlag(SettingsFlags.HierarchyView);
        }

        public void SaveSettings()
        {
            if (folderHierarchyToolStripMenuItem.Checked)
                settings.Flags |= SettingsFlags.HierarchyView;
            else
                settings.Flags &= ~SettingsFlags.HierarchyView;

            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Haquez Co.\\Adjutant", "Settings", settings.ToStream().ToArray());

            tssStatus.Text = "Settings saved.";
        }
        #endregion

        #region Events

        #region Menu Item Click

        #region File
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog()
            {
                InitialDirectory = settings.mapFolder,
                Filter = "Halo x360 Map Files|*.map",
                Multiselect = true
            };

            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            settings.mapFolder = Directory.GetParent(ofd.FileName).FullName;

            foreach (string fName in ofd.FileNames)
                ThreadPool.QueueUserWorkItem(NewMapThread, fName);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog()
            {
                InitialDirectory = settings.mapFolder,
                Filter = "Halo x360 Map Files|*.map"
            };

            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            settings.mapFolder = Directory.GetParent(ofd.FileName).FullName;

            tssStatus.Text = "Loading " + ofd.SafeFileName + "...";

            var viewer = (MapViewer)tabControl1.SelectedTab.Controls[0];
            tabControl1.SelectedTab.Text = ofd.SafeFileName;

            viewer.CloseMap();
            viewer.LoadMap(ofd.FileName, folderHierarchyToolStripMenuItem.Checked);

            tssStatus.Text = "Loaded " + ofd.SafeFileName + ".";
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((MapViewer)tabControl1.SelectedTab.Controls[0]).ClassFilter = new List<string>(settings.classFilter.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            ((MapViewer)tabControl1.SelectedTab.Controls[0]).ReloadMap(folderHierarchyToolStripMenuItem.Checked);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((MapViewer)tabControl1.SelectedTab.Controls[0]).CloseMap();
            tabControl1.TabPages.Remove(tabControl1.SelectedTab);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region View
        private void tagClassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tagClassToolStripMenuItem.Checked) return; //already checked, ignore click

            tagClassToolStripMenuItem.Checked = !tagClassToolStripMenuItem.Checked;
            folderHierarchyToolStripMenuItem.Checked = !folderHierarchyToolStripMenuItem.Checked;

            if (tabControl1.TabPages.Count > 0)
                ((MapViewer)tabControl1.SelectedTab.Controls[0]).ReloadMap(folderHierarchyToolStripMenuItem.Checked);
        }

        private void folderHierarchyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderHierarchyToolStripMenuItem.Checked) return; //already checked, ignore click

            folderHierarchyToolStripMenuItem.Checked = !folderHierarchyToolStripMenuItem.Checked;
            tagClassToolStripMenuItem.Checked = !tagClassToolStripMenuItem.Checked;

            if (tabControl1.TabPages.Count > 0)
                ((MapViewer)tabControl1.SelectedTab.Controls[0]).ReloadMap(folderHierarchyToolStripMenuItem.Checked);
        }

        private void viewStringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((MapViewer)tabControl1.SelectedTab.Controls[0]).ViewStrings();
        }

        private void viewLocalesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((MapViewer)tabControl1.SelectedTab.Controls[0]).ViewLocales();
        }
        #endregion

        #region Tools
        private void eMF3ImporterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "Encrypted MaxScript Files|*.mse",
                FileName = "EMF3Importer.mse"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            File.WriteAllBytes(sfd.FileName, Properties.Resources.EMF3Importer);
            MessageBox.Show(this, "Done!");
        }

        private void downloadPluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new System.Windows.Forms.SaveFileDialog
            {
                InitialDirectory = Application.StartupPath,
                Filter = "zip files|*.zip",
                FileName = "plugins.zip"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            ThreadPool.QueueUserWorkItem(DownloadPluginsThread, sfd.FileName);
        }

        private void convertPluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Adjutant.Library.PluginConverter pc = new Adjutant.Library.PluginConverter();
            pc.ConvertPlugins();
        }
        #endregion

        #region Options
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sForm = new SettingsForm(this);
            sForm.ShowDialog(this);
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(CheckUpdateThread, true);
        }

        private void forceUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result =
            MessageBox.Show(this, "Are you sure you want to force an update?", "Adjutant Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != System.Windows.Forms.DialogResult.Yes)
                return;
            System.IO.File.WriteAllBytes(Application.StartupPath + '\\' + "update.exe", Properties.Resources.update);
            System.Diagnostics.Process.Start(Application.StartupPath + '\\' + "update.exe");
            Application.Exit();
        }
        #endregion

        #region Help
        private void forumPostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://forum.halomaps.org/index.cfm?page=topic&topicID=41388");
        }

        private void viewAdjutantHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllBytes("AdjutantHelp.chm", Properties.Resources.Adjutant_Help);

            System.Diagnostics.Process.Start("AdjutantHelp.chm");
        }
        #endregion

        #endregion

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSettings();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
            {
                openToolStripMenuItem.Enabled = reloadToolStripMenuItem.Enabled = closeToolStripMenuItem.Enabled = false;
                viewStringsToolStripMenuItem.Enabled = viewLocalesToolStripMenuItem.Enabled = false;
                return;
            }

            openToolStripMenuItem.Enabled = reloadToolStripMenuItem.Enabled = closeToolStripMenuItem.Enabled = true;
            viewStringsToolStripMenuItem.Enabled = viewLocalesToolStripMenuItem.Enabled = true;

            var viewer = (MapViewer)tabControl1.SelectedTab.Controls[0];

            folderHierarchyToolStripMenuItem.Checked = viewer.HierarchyMode;
            tagClassToolStripMenuItem.Checked = !viewer.HierarchyMode;
        }
        #endregion

        #region Threads
        private void NewMapThread(object Filename)
        {
            var fullname = (string)Filename;
            var name = fullname.Substring(fullname.LastIndexOf("\\") + 1);

            tasks++;

            Invoke((MethodInvoker)delegate
            {
                tssStatus.Text = "Loading " + name + "...";
            });

            var tab = new TabPage(name);
            var mv = new MapViewer(settings);
            tab.Controls.Add(mv);
            mv.Dock = DockStyle.Fill;
            try
            {
                mv.ClassFilter = new List<string>(settings.classFilter.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                mv.LoadMap(fullname, folderHierarchyToolStripMenuItem.Checked);

                Invoke((MethodInvoker)delegate
                {
                    tabControl1.TabPages.Add(tab);
                    tabControl1_SelectedIndexChanged(null, null);
                    tssStatus.Text = "Loaded " + name + ".";
                });
            }
            catch
            {
                Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show(this, "Error loading " + name + "! File is invalid, unsupported or in use.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    tssStatus.Text = "Error loading " + name + ".";
                });
            }

            tasks--;
        }

        private void ReloadMapThread(object MapTab)
        {
            throw new NotImplementedException();
        }

        private void DownloadPluginsThread(object Filename)
        {
            this.Invoke((MethodInvoker)delegate { tssStatus.Text = "Downloading plugins.zip..."; });

            tasks++;

            try
            {
                WebClient client = new WebClient();
                client.DownloadFile("http://db.tt/1KXOcb6Z", (string)Filename);

                this.Invoke((MethodInvoker)delegate { tssStatus.Text = "Downloaded plugins.zip."; });
            }
            catch { this.Invoke((MethodInvoker)delegate { tssStatus.Text = "Error downloading plugins.zip."; }); }

            tasks--;
        }

        private void CheckUpdateThread(object UserRequest)
        {
            var userReq = (bool)UserRequest;

            this.Invoke((MethodInvoker)delegate { tssStatus.Text = "Checking for updates..."; });

            tasks++;

            try
            {
                WebClient client = new WebClient();
                string str = client.DownloadString("http://db.tt/8FYGAY8r");

                string[] newver = str.Split('.');
                string[] current = Application.ProductVersion.Split('.');
                bool update = false;
                string changes = "";

                for (int i = 0; i < 4; i++)
                {
                    if (int.Parse(newver[i]) > int.Parse(current[i]))
                    {
                        update = true;
                        changes = client.DownloadString("http://db.tt/P6TGVI5m");
                        break;
                    }
                    else if (int.Parse(newver[i]) < int.Parse(current[i]))
                    {
                        update = false;
                        break;
                    }
                }

                if (update)
                {
                    UpdateForm frm = new UpdateForm();
                    frm.lblCurrent.Text = Application.ProductVersion;
                    frm.lblNew.Text = str;

                    frm.rtbChangelog.Text = client.DownloadString("http://db.tt/P6TGVI5m");
                    this.Invoke((MethodInvoker)delegate
                    {
                        frm.ShowDialog(this);
                    });
                }
                else
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        tssStatus.Text = "No updates available.";
                        if (userReq)
                            MessageBox.Show(this, "No updates available.", "Adjutant Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                }
            }
            catch
            {
                if (userReq)
                    this.Invoke((MethodInvoker)delegate
                    {
                        tssStatus.Text = "Error checking for updates.";
                        MessageBox.Show(this, "There was an error checking for updates.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
            }

            tasks--;
        }
        #endregion
    }
}

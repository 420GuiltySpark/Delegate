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
        private List<KeyValuePair<int, string>> Tasks;

        public Form1(string[] args)
        {
            InitializeComponent();
            LoadSettings();
            versionToolStripMenuItem.Text = Application.ProductVersion;
            Tasks = new List<KeyValuePair<int, string>>();
            
            //if (settings.Flags.HasFlag(SettingsFlags.AutoUpdateCheck))
            //        ThreadPool.QueueUserWorkItem(CheckUpdateThread, false);

            foreach (string fName in args)
                ThreadPool.QueueUserWorkItem(NewMapThread, fName);
        }

        #region Task Managing
        public int AddTask(string message)
        {
            int id = 0;

            foreach (var task in Tasks)
                if (task.Key >= id) id = task.Key + 1;

            Tasks.Add(new KeyValuePair<int, string>(id, message));
            this.Invoke((MethodInvoker)delegate 
            {
                toolStripProgressBar1.Visible = true;
                tssStatus.Text = message; 
            });

            return id;
        }

        public void TaskDone(int ID)
        {
            for (int i = 0; i < Tasks.Count; i++)
                if (Tasks[i].Key == ID)
                {
                    Tasks.RemoveAt(i);
                    break;
                }

            if (Tasks.Count == 0)
                this.Invoke((MethodInvoker)delegate
                {
                    toolStripProgressBar1.Visible = false;
                    tssStatus.Text = "Done.";
                });
            else
                this.Invoke((MethodInvoker)delegate
                {
                    try { tssStatus.Text = Tasks[Tasks.Count - 1].Value; }
                    catch
                    {
                        toolStripProgressBar1.Visible = false;
                        tssStatus.Text = "Done.";
                    }
                });
        }

        public void TaskError(int ID, string errorMessage)
        {
            for (int i = 0; i < Tasks.Count; i++)
                if (Tasks[i].Key == ID)
                {
                    Tasks.RemoveAt(i);
                    break;
                }

            if (Tasks.Count == 0)
                this.Invoke((MethodInvoker)delegate 
                { 
                    toolStripProgressBar1.Visible = false;
                    tssStatus.Text = errorMessage;
                });
            else
                this.Invoke((MethodInvoker)delegate
                {
                    tssStatus.Text = Tasks[Tasks.Count - 1].Value;
                });
        }
        #endregion

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

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSettings();
        }

        #region Menu Item Click

        #region File
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog()
            {
                InitialDirectory = settings.mapFolder,
                Filter = "Xbox Halo Files|*.map;*.s3dpak",
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
                Filter = "Xbox Halo Files|*.map"//;*.s3dpak"
            };

            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            settings.mapFolder = Directory.GetParent(ofd.FileName).FullName;

            tssStatus.Text = "Loading " + ofd.SafeFileName + "...";

            var viewer = (MapViewer)tabControl1.SelectedTab.Controls[0];
            tabControl1.SelectedTab.Text = ofd.SafeFileName;

            viewer.CloseFile();

            var fileName = ofd.FileName;
            var ext = fileName.Substring(fileName.LastIndexOf(".") + 1);

            if (ext.ToLower() == "map")
                viewer.LoadMap(ofd.FileName, folderHierarchyToolStripMenuItem.Checked);
            else if (ext.ToLower() == "s3dpak")
                viewer.LoadPak(fileName);
            else
                throw new NotSupportedException();

            tssStatus.Text = "Loaded " + ofd.SafeFileName + ".";
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((MapViewer)tabControl1.SelectedTab.Controls[0]).ClassFilter = new List<string>(settings.classFilter.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            ((MapViewer)tabControl1.SelectedTab.Controls[0]).ReloadMap(folderHierarchyToolStripMenuItem.Checked);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((MapViewer)tabControl1.SelectedTab.Controls[0]).CloseFile();
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

        private void aMFImporterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "MaxScript Files|*.ms",
                FileName = "AMF2Importer.ms"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            File.WriteAllBytes(sfd.FileName, Properties.Resources.AMFImporter);
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

        //private void generateKeyToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    new KeyForm().ShowDialog(this);
        //}
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

            var exeString = "https://dl.dropboxusercontent.com/u/39530625/Haquez%20Co/Adjutant/Adjutant.exe";
            var logString = "https://dl.dropboxusercontent.com/u/39530625/Haquez%20Co/Adjutant/changelog.txt";

#if REFLEX
            exeString = "https://dl.dropboxusercontent.com/u/39530625/Haquez%20Co/Adjutant/Reflex/Adjutant.exe";
#endif

            System.IO.File.WriteAllBytes(Application.StartupPath + '\\' + "update.exe", Properties.Resources.update);
            var startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = Application.StartupPath + '\\' + "update.exe";
            startInfo.Arguments = exeString + " " + logString;
            System.Diagnostics.Process.Start(startInfo);
            Application.Exit();
        }
        #endregion

        #region Help
        private void viewAdjutantHelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllBytes("AdjutantHelp.chm", Properties.Resources.Adjutant_Help);

            System.Diagnostics.Process.Start("AdjutantHelp.chm");
        }
        #endregion

        #endregion

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
            {
                openToolStripMenuItem.Enabled = reloadToolStripMenuItem.Enabled = closeToolStripMenuItem.Enabled = false;
                viewStringsToolStripMenuItem.Enabled = viewLocalesToolStripMenuItem.Enabled = false;
                return;
            }

            var viewer = (MapViewer)tabControl1.SelectedTab.Controls[0];

            openToolStripMenuItem.Enabled = reloadToolStripMenuItem.Enabled = closeToolStripMenuItem.Enabled = true;
            viewStringsToolStripMenuItem.Enabled = viewLocalesToolStripMenuItem.Enabled = viewer.isCache;
            openToolStripMenuItem.Enabled = reloadToolStripMenuItem.Enabled = viewer.isCache;

            folderHierarchyToolStripMenuItem.Checked = viewer.HierarchyMode;
            tagClassToolStripMenuItem.Checked = !viewer.HierarchyMode;

            folderHierarchyToolStripMenuItem.Enabled = tagClassToolStripMenuItem.Enabled = viewer.isCache;
        }
        #endregion

        #region Threads
        private void NewMapThread(object Filename)
        {
            var fullname = (string)Filename;
            var name = fullname.Substring(fullname.LastIndexOf("\\") + 1);
            var ext = name.Substring(name.LastIndexOf(".") + 1);

            int taskID = AddTask("Loading " + name + "...");

            var tab = new TabPage(name);
            var mv = new MapViewer(settings);
            tab.Controls.Add(mv);
            mv.Dock = DockStyle.Fill;
            try
            {
                mv.ClassFilter = new List<string>(settings.classFilter.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

                if (ext.ToLower() == "map")
                    mv.LoadMap(fullname, folderHierarchyToolStripMenuItem.Checked);
                else if (ext.ToLower() == "s3dpak")
                    mv.LoadPak(fullname);
                else
                    throw new NotSupportedException();

                Invoke((MethodInvoker)delegate
                {
                    tabControl1.TabPages.Add(tab);
                    tabControl1_SelectedIndexChanged(null, null);
                });

                TaskDone(taskID);
            }
            catch
            {
                TaskError(taskID, "Error loading " + name + ".");
                Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show(this, "Error loading " + name + "! File is invalid, unsupported or in use.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        private void ReloadMapThread(object MapTab)
        {
            throw new NotImplementedException();
        }

        private void DownloadPluginsThread(object Filename)
        {
            int taskID = AddTask("Downloading plugins.zip...");

            try
            {
                WebClient client = new WebClient();
                client.DownloadFile("<plugins.zip>", (string)Filename);

                TaskDone(taskID);
            }
            catch { TaskError(taskID, "Error downloading plugins.zip."); }

        }

        private void CheckUpdateThread(object UserRequest)
        {
            var userReq = (bool)UserRequest;
            int taskID = AddTask("Checking for updates...");
            var verString = "<version>";
            var changeString = "<change>";

            try
            {
                WebClient client = new WebClient();
                string str = client.DownloadString(verString);

                string[] newver = str.Split('.');
                string[] current = Application.ProductVersion.Split('.');
                bool update = false;

                for (int i = 0; i < 4; i++)
                {
                    if (int.Parse(newver[i]) > int.Parse(current[i]))
                    {
                        update = true;
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
                    UpdateForm frm = new UpdateForm(ref settings);
                    frm.lblCurrent.Text = Application.ProductVersion;
                    frm.lblNew.Text = str;

                    frm.rtbChangelog.Text = client.DownloadString(changeString);
                    this.Invoke((MethodInvoker)delegate
                    {
                        frm.ShowDialog(this);
                    });

                    TaskDone(taskID);
                }
                else
                {
                    TaskDone(taskID);
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (userReq)
                            MessageBox.Show(this, "No updates available.", "Adjutant Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    });
                }
            }
            catch (Exception ex)
            {
                TaskError(taskID, "Error checking for updates.");
                if (userReq)
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show(this, "There was an error checking for updates.\n\r" + ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.S3D;
using Adjutant.Library.Cache;
using Adjutant.Library.Controls;
using Adjutant.Library.Definitions;
using System.IO;

namespace Adjutant.Controls
{
    public partial class Extractor : UserControl
    {
        private int tagsExtracted = 0;
        private int tagsMissed = 0;

        public Extractor()
        {
            InitializeComponent();
        }

        public override string Text
        {
            get { return rtbOutput.Text; }
            set { rtbOutput.Text = value; }
        }

        public void AddLine(string Text)
        {
            //we need to support cross-thread calls here...
            this.Invoke((MethodInvoker)delegate
            {
                rtbOutput.AppendText(rtbOutput.Text == "" ? Text : "\r\n" + Text);
            });
        }

        public void Clear()
        {
            //we need to support cross-thread calls here...
            this.Invoke((MethodInvoker)delegate
            {
                rtbOutput.Clear();
            });
        }
        
        public void BeginExtraction(CacheFile Cache, List<TreeNode> Parents, Settings Settings, string Destination)
        {
            if (backgroundWorker1.IsBusy)
            {
                MessageBox.Show(this, "There is already an extraction running. You must wait for it to finish or cancel it before starting another one.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var args = new ExtractionArgs()
            {
                cache = Cache, parents = Parents, settings = Settings, destination = Destination
            };

            backgroundWorker1.RunWorkerAsync(args);
        }

        public void BeginExtraction(PakFile Pak, List<TreeNode> Parents, Settings Settings, string Destination)
        {
            if (backgroundWorker1.IsBusy)
            {
                MessageBox.Show(this, "There is already an extraction running. You must wait for it to finish or cancel it before starting another one.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var args = new ExtractionArgs()
            {
                pak = Pak,
                parents = Parents,
                settings = Settings,
                destination = Destination
            };

            backgroundWorker1.RunWorkerAsync(args);
        }

        private void BatchExtract(CacheFile cache, List<TreeNode> parents, Settings settings, string dest, BackgroundWorker worker)
        {
            foreach (TreeNode parent in parents)
            {
                foreach (TreeNode child in parent.Nodes)
                {
                    if (worker.CancellationPending) return;

                    if (child.Nodes.Count > 0)
                    {
                        BatchExtract(cache, new List<TreeNode>() { child }, settings, dest, worker);
                        continue;
                    }

                    var tag = child.Tag as CacheFile.IndexItem;
                    var fName = dest + /*"\\" + tag.ClassName +*/ "\\" + tag.Filename;
                    var tName = tag.Filename + "." + tag.ClassCode;

                    switch (tag.ClassCode)
                    {
                        #region bink
                        case "bink":
                            try
                            {
                                //only supports H4R
                                if (cache.Version < DefinitionSet.Halo4Retail) continue;

                                fName += ".bik";

                                AddLine("Extracting " + tName + "...");
                                if (settings.Flags.HasFlag(SettingsFlags.OverwriteTags) || !File.Exists(fName))
                                {
                                    BinkExtractor.SaveBink(fName, cache, tag);
                                    AddLine("Extracted " + tName + ".");
                                    tagsExtracted++;
                                }
                            }

                            catch (Exception ex)
                            {
                                AddLine("Error extracting " + tName + ":");
                                AddLine("--" + ex.Message.Replace("\r\n", " "));
                                tagsMissed++;
                            }
                            break;
                        #endregion
                        #region bitm
                        case "bitm":
                            try
                            {
                                switch (settings.BitmFormat)
                                {
                                    case BitmapFormat.TIF:
                                        fName += ".tif";
                                        break;
                                    case BitmapFormat.DDS:
                                        fName += ".dds";
                                        break;
                                    case BitmapFormat.RAW:
                                        fName += ".bin";
                                        break;
                                }

                                if (settings.Flags.HasFlag(SettingsFlags.OverwriteTags) || !File.Exists(fName))
                                {
                                    BitmapExtractor.SaveAllImages(fName, cache, tag, settings.BitmFormat, settings.Flags.HasFlag(SettingsFlags.BitmapAlpha));
                                    AddLine("Extracted " + tName + ".");
                                    tagsExtracted++;
                                }
                            }
                            catch (Exception ex)
                            {
                                AddLine("Error extracting " + tName + ":");
                                AddLine("--" + ex.Message.Replace("\r\n", " "));
                                tagsMissed++;
                            }
                            break;
                        #endregion
                        #region mode
                        case "mode":
                            try
                            {
                                switch (settings.ModeFormat)
                                {
                                    case ModelFormat.EMF:
                                        fName += ".emf";
                                        break;
                                    case ModelFormat.JMS:
                                        fName += ".jms";
                                        break;
                                    case ModelFormat.OBJ:
                                        fName += ".obj";
                                        break;
                                    case ModelFormat.AMF:
                                        fName += ".amf";
                                        break;
                                }

                                if (settings.Flags.HasFlag(SettingsFlags.OverwriteTags) || !File.Exists(fName))
                                {
                                    ModelExtractor.SaveAllModelParts(fName, cache, tag, settings.ModeFormat, settings.Flags.HasFlag(SettingsFlags.SplitMeshes));
                                    AddLine("Extracted " + tName + ".");
                                    tagsExtracted++;
                                }
                            }
                            catch (Exception ex)
                            {
                                AddLine("Error extracting " + tName + ":");
                                AddLine("--" + ex.Message.Replace("\r\n", " "));
                                tagsMissed++;
                            }
                            break;
                        #endregion
                        #region sbsp
                        case "sbsp":
                            try
                            {
                                AddLine("Extracting " + tName + "...");
                                switch (settings.ModeFormat)
                                {
                                    case ModelFormat.EMF:
                                        fName += ".emf";
                                        break;
                                    case ModelFormat.OBJ:
                                        fName += ".obj";
                                        break;
                                    case ModelFormat.AMF:
                                    case ModelFormat.JMS:
                                        fName += ".amf";
                                        break;
                                }

                                if (settings.Flags.HasFlag(SettingsFlags.OverwriteTags) || !File.Exists(fName))
                                {
                                    BSPExtractor.SaveAllBSPParts(fName, cache, tag, settings.ModeFormat);
                                    AddLine("Extracted " + tName + ".");
                                    tagsExtracted++;
                                }
                            }
                            catch (Exception ex)
                            {
                                AddLine("Error extracting " + tName + ":");
                                AddLine("--" + ex.Message.Replace("\r\n", " "));
                                tagsMissed++;
                            }
                            break;
                        #endregion
                        #region snd!
                        case "snd!":
                            try
                            {
                                //H2, H3B not supported
                                if (cache.Version <= DefinitionSet.Halo3Beta) continue;

                                AddLine("Extracting " + tName + "...");
                                if (cache.Version < DefinitionSet.Halo4Retail)
                                    SoundExtractor.SaveAllAsSeparate(dest + tag.Filename, cache, tag, settings.Snd_Format, settings.Flags.HasFlag(SettingsFlags.OverwriteTags));
                                else
                                    (new SoundExtractorH4()).SaveAllAsSeparate(dest + tag.Filename, cache, tag, settings.Snd_Format, settings.Flags.HasFlag(SettingsFlags.OverwriteTags));

                                //SoundExtractor.SaveAllAsSingle(fName, cache, tag, settings.Snd_Format);
                                AddLine("Extracted " + tag.Filename + "." + tag.ClassCode + ".");
                                tagsExtracted++;

                            }
                            catch (Exception ex)
                            {
                                AddLine("Error extracting " + tName + ":");
                                AddLine("--" + ex.Message.Replace("\r\n", " "));
                                tagsMissed++;
                            }
                            break;
                        #endregion
                        #region unic
                        case "unic":
                            try
                            {
                                //H2 not supported
                                if (cache.Version < DefinitionSet.Halo3Beta) continue;

                                fName += ".txt";

                                AddLine("Extracting " + tName + "...");
                                if (settings.Flags.HasFlag(SettingsFlags.OverwriteTags) || !File.Exists(fName))
                                {
                                    StringsViewer.SaveUnicStrings(fName, cache, tag, settings.Language);
                                    AddLine("Extracted " + tName + ".");
                                    tagsExtracted++;
                                }
                            }
                            catch (Exception ex)
                            {
                                AddLine("Error extracting " + tName + ":");
                                AddLine("--" + ex.Message.Replace("\r\n", " "));
                                tagsMissed++;
                            }
                            break;
                        #endregion
                    }
                }
            }
        }

        private void BatchExtract(PakFile pak, List<TreeNode> parents, Settings settings, string dest, BackgroundWorker worker)
        {
            foreach (TreeNode parent in parents)
            {
                foreach (TreeNode child in parent.Nodes)
                {
                    if (worker.CancellationPending) return;

                    if (child.Nodes.Count > 0)
                    {
                        BatchExtract(pak, new List<TreeNode>() { child }, settings, dest, worker);
                        continue;
                    }

                    var item = child.Tag as PakFile.PakTag;
                    var fName = dest + "\\" + item.Name;
                    var tName = "[" + item.Class.ToString("D2") + "] " + item.Name;

                    switch (item.Class)
                    {
                        #region bitmap
                        case TagType.Textures:
                        case TagType.TexturesMips64:
                            try
                            {
                                switch (settings.BitmFormat)
                                {
                                    case BitmapFormat.TIF:
                                        fName += ".tif";
                                        break;
                                    case BitmapFormat.DDS:
                                        fName += ".dds";
                                        break;
                                    case BitmapFormat.RAW:
                                        fName += ".bin";
                                        break;
                                }

                                if (settings.Flags.HasFlag(SettingsFlags.OverwriteTags) || !File.Exists(fName))
                                {
                                    BitmapExtractor.SaveImage(fName, pak, item, settings.BitmFormat, settings.Flags.HasFlag(SettingsFlags.BitmapAlpha));
                                    AddLine("Extracted " + tName + ".");
                                    tagsExtracted++;
                                }
                            }
                            catch (Exception ex)
                            {
                                AddLine("Error extracting " + tName + ":");
                                AddLine("--" + ex.Message.Replace("\r\n", " "));
                                tagsMissed++;
                            }
                            break;
                        #endregion
                    }
                }
            }
        }

        public void CancelExtraction()
        {
            backgroundWorker1.CancelAsync();
        }

        #region Events
        private void cancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CancelExtraction();
        }

        private void copyOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(rtbOutput.Text.Replace("\n", "\r\n"));
        }

        private void saveOutputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Text Files|*.txt|Rich Text Files|*.rtf",
                AddExtension = true
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            if (dialog.FilterIndex == 1)
            {
                System.IO.File.WriteAllLines(dialog.FileName, rtbOutput.Lines);
            }
            else
                rtbOutput.SaveFile(dialog.FileName);
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            tagsExtracted = tagsMissed = 0;

            Invoke((MethodInvoker)delegate
            {
                cancelToolStripMenuItem.Enabled = true;
            });

            var args = e.Argument as ExtractionArgs;

            Clear();
            string fName = "";
            foreach (TreeNode node in args.parents)
                fName += node.Name + ", ";
            fName = fName.Substring(0, fName.Length - 2);
            AddLine("Starting batch extract: " + fName);

            var stopwatch = new System.Diagnostics.Stopwatch();
            
            stopwatch.Start();
            if (args.cache != null)
                BatchExtract(args.cache, args.parents, args.settings, args.destination, backgroundWorker1);
            else
                BatchExtract(args.pak, args.parents, args.settings, args.destination, backgroundWorker1);
            stopwatch.Stop();

            if (!backgroundWorker1.CancellationPending)
                AddLine("Batch extract complete.");
            else
                AddLine("Batch extract cancelled.");

            float percentage = (tagsExtracted == (tagsExtracted + tagsMissed)) ? 100 : (float)tagsExtracted / (float)(tagsExtracted + tagsMissed) * 100;
            AddLine(string.Format("{0} of {1} ({2}%) valid tags were extracted in {3} seconds.",  tagsExtracted, (tagsExtracted + tagsMissed), (int)percentage, stopwatch.ElapsedMilliseconds / 1000));

            Invoke((MethodInvoker)delegate
            {
                cancelToolStripMenuItem.Enabled = false;
            });
        }
        #endregion

        private class ExtractionArgs
        {
            public CacheFile cache;
            public PakFile pak;
            public List<TreeNode> parents;
            public Settings settings;
            public string destination;
        }
    }
}

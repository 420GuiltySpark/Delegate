using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library.Cache;
using Adjutant.Library.Controls;

namespace Adjutant.Controls
{
    public partial class TagViewer : UserControl
    {
        #region Init
        private Settings settings;
        private CacheFile cache;
        private CacheFile.IndexItem tag;
        private Extractor output;

        private MetaViewer vMeta;
        private BitmapExtractor eBitm;
        private ModelExtractor eMode;
        private SoundExtractor eSnd_;
        private StringsViewer vUnic;
        private ModelViewer vMode;
        private TabPage tabMeta, tabRaw, tabModel;
        private bool metaLoaded, rawLoaded, modelLoaded;
        #endregion

        public TagViewer(Settings Settings, Extractor Output)
        {
            InitializeComponent();
            settings = Settings;
            output = Output;

            vMeta = new MetaViewer();
            eBitm = new BitmapExtractor();
            eMode = new ModelExtractor();
            eSnd_ = new SoundExtractor();
            vUnic = new StringsViewer();

            eBitm.TagExtracted += new TagExtractedEventHandler(extractor_TagExtracted);
            eMode.TagExtracted += new TagExtractedEventHandler(extractor_TagExtracted);
            eSnd_.TagExtracted += new TagExtractedEventHandler(extractor_TagExtracted);
            eBitm.ErrorExtracting += new ErrorExtractingEventHandler(extractor_ErrorExtracting);
            eMode.ErrorExtracting += new ErrorExtractingEventHandler(extractor_ErrorExtracting);
            eSnd_.ErrorExtracting += new ErrorExtractingEventHandler(extractor_ErrorExtracting);
            eMode.FinishedRecursiveExtract += new FinishedRecursiveExtractEventHandler(eMode_FinishedRecursiveExtract);

            tabMeta = new TabPage("Meta Viewer");
            tabMeta.Controls.Add(vMeta);
            vMeta.Dock = DockStyle.Fill;

            tabRaw = new TabPage("Raw Extractor");
            tabModel = new TabPage("Model Viewer");

            tabControl1.TabPages.Add(tabMeta);
        }

        public void LoadTag(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            if (vMode == null)
            {
                vMode = new ModelViewer();      //this cant go in the constructor because the
                tabModel.Controls.Clear();      //ElementHost in the ModelViewer requires an
                tabModel.Controls.Add(vMode);   //STA thread, whereas the map open thread 
                vMode.Dock = DockStyle.Fill;    //is MTA because it's done via ThreadPool

                vMode.TagExtracted += new TagExtractedEventHandler(extractor_TagExtracted);
                vMode.ErrorExtracting += new ErrorExtractingEventHandler(extractor_ErrorExtracting);
            }

            cache = Cache;
            tag = Tag;
            metaLoaded = rawLoaded = modelLoaded = false;

            switch (Tag.ClassCode)
            {
                #region bitm
                case "bitm":
                    tabControl1.TabPages.Remove(tabModel);

                    tabRaw.Controls.Clear();

                    if (!tabControl1.TabPages.Contains(tabRaw))
                        tabControl1.TabPages.Add(tabRaw);

                    tabRaw.Controls.Add(eBitm);
                    eBitm.Dock = DockStyle.Fill;

                    if (tabControl1.SelectedTab == tabMeta)
                    {
                        vMeta.LoadTagMeta(cache, tag, settings.Flags.HasFlag(SettingsFlags.ShowInvisibles), settings.pluginFolder);
                        metaLoaded = true;
                    }
                    else if (tabControl1.SelectedTab == tabRaw)
                    {
                        eBitm.LoadBitmapTag(cache, tag);
                        rawLoaded = true;
                    }
                    break;
                #endregion

                #region mode
                case "mode":
                    tabRaw.Controls.Clear();

                    if (!tabControl1.TabPages.Contains(tabRaw))
                        tabControl1.TabPages.Add(tabRaw);

                    if (!tabControl1.TabPages.Contains(tabModel))
                        tabControl1.TabPages.Add(tabModel);

                    tabRaw.Controls.Add(eMode);
                    eMode.Dock = DockStyle.Fill;

                    if (tabControl1.SelectedTab == tabMeta)
                    {
                        vMeta.LoadTagMeta(cache, tag, settings.Flags.HasFlag(SettingsFlags.ShowInvisibles), settings.pluginFolder);
                        metaLoaded = true;
                    }
                    else if (tabControl1.SelectedTab == tabRaw)
                    {
                        eMode.DataFolder = settings.dataFolder;
                        eMode.PermFilter = settings.permFilter;
                        eMode.LoadModelTag(cache, tag);
                        rawLoaded = true;
                    }
                    else if (tabControl1.SelectedTab == tabModel)
                    {
                        //vMode.RenderBackColor = settings.ViewerColour;
                        vMode.PermutationFilter = new List<string>(settings.permFilter.Split(' '));
                        vMode.LoadModelTag(cache, tag,
                            settings.Flags.HasFlag(SettingsFlags.LoadSpecular),
                            settings.Flags.HasFlag(SettingsFlags.UsePermFilter),
                            settings.Flags.HasFlag(SettingsFlags.ForceLoadModels));
                        modelLoaded = true;
                    }
                    break;
                #endregion

                #region snd!
                case "snd!":
                    tabControl1.TabPages.Remove(tabModel);

                    tabRaw.Controls.Clear();

                    if (!tabControl1.TabPages.Contains(tabRaw))
                        tabControl1.TabPages.Add(tabRaw);

                    tabRaw.Controls.Add(eSnd_);
                    eSnd_.Dock = DockStyle.Fill;

                    if (tabControl1.SelectedTab == tabMeta)
                    {
                        vMeta.LoadTagMeta(cache, tag, settings.Flags.HasFlag(SettingsFlags.ShowInvisibles), settings.pluginFolder);
                        metaLoaded = true;
                    }
                    else if (tabControl1.SelectedTab == tabRaw)
                    {
                        eSnd_.LoadSoundTag(cache, tag);
                        rawLoaded = true;
                    }
                    break;
                #endregion

                #region unic
                case "unic":
                    tabControl1.TabPages.Remove(tabModel);

                    tabRaw.Controls.Clear();

                    if (!tabControl1.TabPages.Contains(tabRaw))
                        tabControl1.TabPages.Add(tabRaw);

                    tabRaw.Controls.Add(vUnic);
                    vUnic.Dock = DockStyle.Fill;

                    if (tabControl1.SelectedTab == tabMeta)
                    {
                        vMeta.LoadTagMeta(cache, tag, settings.Flags.HasFlag(SettingsFlags.ShowInvisibles), settings.pluginFolder);
                        metaLoaded = true;
                    }
                    else if (tabControl1.SelectedTab == tabRaw)
                    {
                        vUnic.LoadUnicTag(cache, tag);
                        rawLoaded = true;
                    }
                    break;
                #endregion

                #region other
                default:
                    tabControl1.TabPages.Remove(tabModel);
                    tabControl1.TabPages.Remove(tabRaw);

                    if (tabControl1.SelectedTab == tabMeta)
                    {
                        vMeta.LoadTagMeta(cache, tag, settings.Flags.HasFlag(SettingsFlags.ShowInvisibles), settings.pluginFolder);
                        metaLoaded = true;
                    }
                    break;
                #endregion
            }
        }

        new public void Dispose()
        {
            tabControl1.TabPages.Clear();
            vMeta.Dispose();
            eBitm.Dispose();
            eMode.Dispose();
            eSnd_.Dispose();
            vUnic.Dispose();
            if (vMode != null) vMode.Dispose();

            base.Dispose();
        }

        #region Events
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            #region Meta Tab
            if (tabControl1.SelectedTab == tabMeta)
            {
                if (!metaLoaded)
                {
                    vMeta.LoadTagMeta(cache, tag, settings.Flags.HasFlag(SettingsFlags.ShowInvisibles), settings.pluginFolder);
                    metaLoaded = true;
                }
            }
            #endregion

            #region Raw Tab
            else if (tabControl1.SelectedTab == tabRaw)
            {
                switch (tag.ClassCode)
                {
                    case "bitm":
                        if (!rawLoaded)
                        {
                            eBitm.LoadBitmapTag(cache, tag);
                            rawLoaded = true;
                        }
                        break;

                    case "mode":
                        if (!rawLoaded)
                        {
                            eMode.DataFolder = settings.dataFolder;
                            eMode.PermFilter = settings.permFilter;
                            eMode.LoadModelTag(cache, tag);
                            rawLoaded = true;
                        }
                        break;

                    case "snd!":
                        if (!rawLoaded)
                        {
                            eSnd_.LoadSoundTag(cache, tag);
                            rawLoaded = true;
                        }
                        break;

                    case "unic":
                        if (!rawLoaded)
                        {
                            vUnic.LoadUnicTag(cache, tag);
                            rawLoaded = true;
                        }
                        break;
                }
            }
            #endregion

            #region Model Viewer Tab
            else if (tabControl1.SelectedTab == tabModel)
            {
                if (!modelLoaded)
                {
                    //vMode.RenderBackColor = settings.ViewerColour;
                    vMode.PermutationFilter = new List<string>(settings.permFilter.Split(' '));
                    vMode.LoadModelTag(cache, tag,
                        settings.Flags.HasFlag(SettingsFlags.LoadSpecular),
                        settings.Flags.HasFlag(SettingsFlags.UsePermFilter),
                        settings.Flags.HasFlag(SettingsFlags.ForceLoadModels));
                    modelLoaded = true;
                }
            }
            #endregion
        }

        private void extractor_TagExtracted(object sender, CacheFile.IndexItem Tag)
        {
            output.AddLine("Extracted " + Tag.Filename + "." + Tag.ClassCode + ".");
        }

        private void extractor_ErrorExtracting(object sender, CacheFile.IndexItem Tag, Exception Error)
        {
            output.AddLine("Error extracting " + Tag.Filename + "." + Tag.ClassCode + ":");
            output.AddLine("--" + Error.Message);
        }

        private void eMode_FinishedRecursiveExtract(object sender, CacheFile.IndexItem Tag)
        {
            output.AddLine("Bitmap extraction complete.");
        }
        #endregion
    }
}

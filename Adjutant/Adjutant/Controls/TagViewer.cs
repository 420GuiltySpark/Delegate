using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library;
using Adjutant.Library.S3D;
using Adjutant.Library.Cache;
using Adjutant.Library.Controls;
using Adjutant.Library.Definitions;

namespace Adjutant.Controls
{
    public partial class TagViewer : UserControl
    {
        #region Init
        private Settings settings;
        
        private CacheFile cache;
        private CacheFile.IndexItem tag;
        private Extractor output;
        private S3DPak pak;
        private S3DPak.PakItem item;

        private MetaViewer vMeta;
        private BitmapExtractor eBitm;
        private ModelExtractor eMode;
        private SoundExtractor eSnd_;
        private SoundExtractorH4 eSnd4;
        private StringsViewer vUnic;
        private ModelViewer vMode;
        private BSPViewer vSbsp;
        private BSPExtractor eSbsp;
        private S3DExplorer vS3D;

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
            eSnd4 = new SoundExtractorH4();
            vUnic = new StringsViewer();
            eSbsp = new BSPExtractor();
            vS3D = new S3DExplorer();

            eBitm.TagExtracted += new TagExtractedEventHandler(extractor_TagExtracted);
            eMode.TagExtracted += new TagExtractedEventHandler(extractor_TagExtracted);
            eSnd_.TagExtracted += new TagExtractedEventHandler(extractor_TagExtracted);
            eSnd4.TagExtracted += new TagExtractedEventHandler(extractor_TagExtracted);
            eSbsp.TagExtracted += new TagExtractedEventHandler(extractor_TagExtracted);
            eBitm.ErrorExtracting += new ErrorExtractingEventHandler(extractor_ErrorExtracting);
            eMode.ErrorExtracting += new ErrorExtractingEventHandler(extractor_ErrorExtracting);
            eSnd_.ErrorExtracting += new ErrorExtractingEventHandler(extractor_ErrorExtracting);
            eSnd4.ErrorExtracting += new ErrorExtractingEventHandler(extractor_ErrorExtracting);
            eSbsp.ErrorExtracting += new ErrorExtractingEventHandler(extractor_ErrorExtracting);
            eMode.FinishedRecursiveExtract += new FinishedRecursiveExtractEventHandler(eMode_FinishedRecursiveExtract);
            eSbsp.FinishedRecursiveExtract += new FinishedRecursiveExtractEventHandler(eMode_FinishedRecursiveExtract);

            eBitm.DefaultBitmFormat = settings.BitmFormat;
            eMode.DefaultBitmFormat = settings.BitmFormat;
            eMode.DefaultModeFormat = settings.ModeFormat;
            eSnd_.DefaultSnd_Format = settings.Snd_Format;
            eSbsp.DefaultBitmFormat = settings.BitmFormat;
            eSbsp.DefaultModeFormat = settings.ModeFormat;

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
                //tabModel.Controls.Clear();    //ElementHost in the ModelViewer requires an
                tabModel.Controls.Add(vMode);   //STA thread, whereas the map open thread 
                vMode.Dock = DockStyle.Fill;    //is MTA because it's done via ThreadPool
                vMode.DefaultModeFormat = settings.ModeFormat;

                vMode.TagExtracted += new TagExtractedEventHandler(extractor_TagExtracted);
                vMode.ErrorExtracting += new ErrorExtractingEventHandler(extractor_ErrorExtracting);
            }
            else vMode.Clear();

            if (vSbsp == null)
            {
                vSbsp = new BSPViewer();
                //tabModel.Controls.Clear();
                tabModel.Controls.Add(vSbsp);
                vSbsp.Dock = DockStyle.Fill;
                vSbsp.DefaultModeFormat = settings.ModeFormat;

                vSbsp.TagExtracted += new TagExtractedEventHandler(extractor_TagExtracted);
                vSbsp.ErrorExtracting += new ErrorExtractingEventHandler(extractor_ErrorExtracting);
            }
            else vSbsp.Clear();

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
                    vSbsp.Visible = false;
                    vMode.Visible = true;

                    if (!tabControl1.TabPages.Contains(tabRaw))
                        tabControl1.TabPages.Add(tabRaw);

                    if (!tabControl1.TabPages.Contains(tabModel))
                        tabControl1.TabPages.Add(tabModel);

                    tabRaw.Controls.Add(eMode);
                    eMode.Dock = DockStyle.Fill;

                    //tabModel.Controls.Clear();
                    //tabModel.Controls.Add(vMode);
                    //vMode.Dock = DockStyle.Fill;

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

                #region sbsp
                case "sbsp":
                    tabRaw.Controls.Clear();
                    vMode.Visible = false;
                    vSbsp.Visible = true;

                    if (!tabControl1.TabPages.Contains(tabRaw))
                        tabControl1.TabPages.Add(tabRaw);

                    if (!tabControl1.TabPages.Contains(tabModel))
                        tabControl1.TabPages.Add(tabModel);

                    tabRaw.Controls.Add(eSbsp);
                    eSbsp.Dock = DockStyle.Fill;

                    //tabModel.Controls.Clear();
                    //tabModel.Controls.Add(vSbsp);
                    //vSbsp.Dock = DockStyle.Fill;

                    if (tabControl1.SelectedTab == tabMeta)
                    {
                        vMeta.LoadTagMeta(cache, tag, settings.Flags.HasFlag(SettingsFlags.ShowInvisibles), settings.pluginFolder);
                        metaLoaded = true;
                    }
                    else if (tabControl1.SelectedTab == tabRaw)
                    {
                        eSbsp.DataFolder = settings.dataFolder;
                        eSbsp.LoadBSPTag(cache, tag);
                        rawLoaded = true;
                    }
                    else if (tabControl1.SelectedTab == tabModel)
                    {
                        vSbsp.LoadBSPTag(cache, tag, settings.Flags.HasFlag(SettingsFlags.ForceLoadModels));
                        modelLoaded = true;
                    }
                    break;
                #endregion

                #region snd!
                case "snd!":
                    tabControl1.TabPages.Remove(tabModel);

                    tabRaw.Controls.Clear();

                    if (cache.Version < DefinitionSet.Halo3Retail)
                        tabControl1.TabPages.Remove(tabRaw);
                    else
                    {

                        if (!tabControl1.TabPages.Contains(tabRaw))
                            tabControl1.TabPages.Add(tabRaw);

                        if (cache.Version < DefinitionSet.Halo4Retail)
                        {
                            tabRaw.Controls.Add(eSnd_);
                            eSnd_.Dock = DockStyle.Fill;
                        }
                        else
                        {
                            tabRaw.Controls.Add(eSnd4);
                            eSnd4.Dock = DockStyle.Fill;
                        }
                    }


                    if (tabControl1.SelectedTab == tabMeta)
                    {
                        vMeta.LoadTagMeta(cache, tag, settings.Flags.HasFlag(SettingsFlags.ShowInvisibles), settings.pluginFolder);
                        metaLoaded = true;
                    }
                    else if (tabControl1.SelectedTab == tabRaw)
                    {
                        if (cache.Version < DefinitionSet.Halo4Retail)
                            eSnd_.LoadSoundTag(cache, tag);
                        else
                            eSnd4.LoadSoundTag(cache, tag);
                        
                        rawLoaded = true;
                    }
                    break;
                #endregion

                #region unic
                case "unic":
                    tabControl1.TabPages.Remove(tabModel);

                    tabRaw.Controls.Clear();

                    if (cache.Version < DefinitionSet.Halo3Beta)
                        tabControl1.TabPages.Remove(tabRaw);
                    else
                    {
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

        public void LoadPakItem(S3DPak Pak, S3DPak.PakItem Item)
        {
            tabMeta.Controls.Clear();
            tabMeta.Controls.Add(vS3D);
            vS3D.Dock = DockStyle.Fill;

            //tabControl1.TabPages.Remove(tabMeta);
            if (vMode == null)
            {
                vMode = new ModelViewer();      //this cant go in the constructor because the
                //tabModel.Controls.Clear();    //ElementHost in the ModelViewer requires an
                tabModel.Controls.Add(vMode);   //STA thread, whereas the map open thread 
                vMode.Dock = DockStyle.Fill;    //is MTA because it's done via ThreadPool
                vMode.DefaultModeFormat = settings.ModeFormat;

                vMode.TagExtracted += new TagExtractedEventHandler(extractor_TagExtracted);
                vMode.ErrorExtracting += new ErrorExtractingEventHandler(extractor_ErrorExtracting);
            }
            else vMode.Clear();

            if (vSbsp == null)
            {
                vSbsp = new BSPViewer();
                //tabModel.Controls.Clear();
                tabModel.Controls.Add(vSbsp);
                vSbsp.Dock = DockStyle.Fill;
                vSbsp.DefaultModeFormat = settings.ModeFormat;

                vSbsp.TagExtracted += new TagExtractedEventHandler(extractor_TagExtracted);
                vSbsp.ErrorExtracting += new ErrorExtractingEventHandler(extractor_ErrorExtracting);
            }
            else vSbsp.Clear();

            pak = Pak;
            item = Item;
            metaLoaded = rawLoaded = modelLoaded = false;

            switch (item.unk0)
            {
                #region bitmaps
                case 6:
                case 7:
                    tabControl1.TabPages.Remove(tabModel);

                    tabRaw.Controls.Clear();

                    if (!tabControl1.TabPages.Contains(tabRaw))
                        tabControl1.TabPages.Add(tabRaw);

                    tabRaw.Controls.Add(eBitm);
                    eBitm.Dock = DockStyle.Fill;

                    if (tabControl1.SelectedTab == tabRaw)
                    {
                        eBitm.LoadBitmapTag(pak, item);
                        rawLoaded = true;
                    }
                    break;
                #endregion

                #region models
                case 12:
                    tabControl1.TabPages.Remove(tabRaw);
                    vMode.Visible = true;
                    vSbsp.Visible = false;

                    //if (!tabControl1.TabPages.Contains(tabRaw))
                    //    tabControl1.TabPages.Add(tabRaw);

                    if (!tabControl1.TabPages.Contains(tabModel))
                        tabControl1.TabPages.Add(tabModel);

                    //tabRaw.Controls.Add(eMode);
                    //eMode.Dock = DockStyle.Fill;

                    //tabModel.Controls.Clear();
                    //tabModel.Controls.Add(vMode);
                    //vMode.Dock = DockStyle.Fill;

                    if (tabControl1.SelectedTab == tabMeta)
                    {
                        //vMeta.LoadTagMeta(cache, tag, settings.Flags.HasFlag(SettingsFlags.ShowInvisibles), settings.pluginFolder);
                        vS3D.LoadModelHierarchy(pak, item, false);
                        metaLoaded = true;
                    }
                    //else if (tabControl1.SelectedTab == tabRaw)
                    //{
                    //    eMode.DataFolder = settings.dataFolder;
                    //    eMode.PermFilter = settings.permFilter;
                    //    eMode.LoadModelTag(cache, tag);
                    //    rawLoaded = true;
                    //}
                    else if (tabControl1.SelectedTab == tabModel)
                    {
                        //vMode.RenderBackColor = settings.ViewerColour;
                        vMode.PermutationFilter = new List<string>(settings.permFilter.Split(' '));
                        vMode.LoadModelTag(pak, item,
                            settings.Flags.HasFlag(SettingsFlags.LoadSpecular),
                            settings.Flags.HasFlag(SettingsFlags.ForceLoadModels));
                        modelLoaded = true;
                    }
                    break;
                #endregion

                #region bsps
                case 16:
                    tabControl1.TabPages.Remove(tabRaw);
                    vMode.Visible = false;
                    vSbsp.Visible = true;

                    if (!tabControl1.TabPages.Contains(tabModel))
                        tabControl1.TabPages.Add(tabModel);

                    //tabRaw.Controls.Add(eSbsp);
                    //eMode.Dock = DockStyle.Fill;

                    //tabModel.Controls.Clear();
                    //tabModel.Controls.Add(vSbsp);
                    //vSbsp.Dock = DockStyle.Fill;

                    if (tabControl1.SelectedTab == tabMeta)
                    {
                        //vMeta.LoadTagMeta(cache, tag, settings.Flags.HasFlag(SettingsFlags.ShowInvisibles), settings.pluginFolder);
                        vS3D.LoadModelHierarchy(pak, item, true);
                        metaLoaded = true;
                    }
                    //else if (tabControl1.SelectedTab == tabRaw)
                    //{
                    //    eMode.DataFolder = settings.dataFolder;
                    //    eMode.PermFilter = settings.permFilter;
                    //    eMode.LoadModelTag(cache, tag);
                    //    rawLoaded = true;
                    //}
                    if (tabControl1.SelectedTab == tabModel)
                    {
                        vSbsp.LoadBSPTag(pak, item, settings.Flags.HasFlag(SettingsFlags.ForceLoadModels));
                        modelLoaded = true;
                    }
                    break;
                #endregion

                #region other
                default:
                    tabControl1.TabPages.Remove(tabModel);
                    tabControl1.TabPages.Remove(tabRaw);

                    if (item.unk0 == 1) vS3D.displayDataInfo(new S3DDATA(pak, item), item.Offset);
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
            if (vMode != null) vMode.Clear();
            if (vSbsp != null) vSbsp.Clear();

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
                    if (cache == null) vS3D.LoadModelHierarchy(pak, item, (item.Type == PakType.BSP));
                    else vMeta.LoadTagMeta(cache, tag, settings.Flags.HasFlag(SettingsFlags.ShowInvisibles), settings.pluginFolder);
                    metaLoaded = true;
                }
            }
            #endregion

            #region Raw Tab
            else if (tabControl1.SelectedTab == tabRaw)
            {
                if (cache == null && !rawLoaded) { eBitm.LoadBitmapTag(pak, item); rawLoaded = true; return; }

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

                    case "sbsp":
                        if (!rawLoaded)
                        {
                            eSbsp.DataFolder = settings.dataFolder;
                            eSbsp.LoadBSPTag(cache, tag);
                            rawLoaded = true;
                        }
                        break;

                    case "snd!":
                        if (!rawLoaded)
                        {
                            if(cache.Version < DefinitionSet.Halo4Retail)
                                eSnd_.LoadSoundTag(cache, tag);
                            else
                                eSnd4.LoadSoundTag(cache, tag);

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
                if (cache == null)
                {
                    if (modelLoaded) return;

                    switch (item.Type)
                    {
                        case PakType.Models:
                            vSbsp.Visible = false;
                            vMode.Visible = true;
                            vMode.LoadModelTag(pak, item, false, settings.Flags.HasFlag(SettingsFlags.ForceLoadModels));
                            modelLoaded = true;
                            break;
                        case PakType.BSP:
                            vMode.Visible = false;
                            vSbsp.Visible = true;
                            vSbsp.LoadBSPTag(pak, item, settings.Flags.HasFlag(SettingsFlags.ForceLoadModels));
                            modelLoaded = true;
                            break;
                    }
                    return; 
                }

                switch(tag.ClassCode)
                {
                    case "mode":
                        if (!modelLoaded)
                        {
                            vSbsp.Visible = false;
                            vMode.Visible = true;

                            //vMode.RenderBackColor = settings.ViewerColour;
                            vMode.PermutationFilter = new List<string>(settings.permFilter.Split(' '));
                            vMode.LoadModelTag(cache, tag,
                                settings.Flags.HasFlag(SettingsFlags.LoadSpecular),
                                settings.Flags.HasFlag(SettingsFlags.UsePermFilter),
                                settings.Flags.HasFlag(SettingsFlags.ForceLoadModels));
                            modelLoaded = true;
                        }
                        break;

                    case "sbsp":
                        if (!modelLoaded)
                        {
                            vMode.Visible = false;
                            vSbsp.Visible = true;

                            vSbsp.LoadBSPTag(cache, tag, settings.Flags.HasFlag(SettingsFlags.ForceLoadModels));
                            modelLoaded = true;
                        }
                        break;
                }
            }
            #endregion
        }

        private void extractor_TagExtracted(object sender, object Tag)
        {
            if (Tag is CacheFile.IndexItem)
            {
                var t = (CacheFile.IndexItem)Tag;
                output.AddLine("Extracted " + t.Filename + "." + t.ClassCode + ".");
            }
            else
            {
                var t = (S3DPak.PakItem)Tag;
                output.AddLine("Extracted [" + t.unk0.ToString("D2") + "] " + t.Name + ".");
            }
        }

        private void extractor_ErrorExtracting(object sender, object Tag, Exception Error)
        {
            if (Tag is CacheFile.IndexItem)
            {
                var t = (CacheFile.IndexItem)Tag;
                output.AddLine("Error extracting " + t.Filename + "." + t.ClassCode + ":");
            }
            else
            {
                var t = (S3DPak.PakItem)Tag;
                output.AddLine("Error extracting [" + t.unk0.ToString("D2") + "] " + t.Name + ":");
            }
            output.AddLine("--" + Error.Message);
        }

        private void eMode_FinishedRecursiveExtract(object sender, CacheFile.IndexItem Tag)
        {
            output.AddLine("Bitmap extraction complete.");
        }
        #endregion
    }
}

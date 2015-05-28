using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using System.IO;
using Composer;
using Composer.Wwise;

/**************************************************************
 * Please note this sound extractor is integrated with Composer
 * and contains code from Composer itself. Most Composer
 * code is contained in the "Composer" region of the code.
 ***************************************************************/

namespace Adjutant.Library.Controls
{
    public partial class SoundExtractorH4 : UserControl
    {
        public SoundExtractorH4()
        {
            InitializeComponent();
        }

        private CacheFile cache;
        private CacheFile.IndexItem tag;
        private sound snd;
        private soundbank sbnk;
        private SoundScanner scanner;

        bool isDisplay = false;

        private System.Media.SoundPlayer player = new System.Media.SoundPlayer() { SoundLocation = Path.GetTempPath() + "temp_playback.wav" };

        private List<SoundBank> _soundbanks = new List<SoundBank>();
        private List<SoundFileInfo> _perms = new List<SoundFileInfo>();

        #region Footers
        private static byte[] monoFooter = new byte[]
                {
			        0x58, 0x4D, 0x41, 0x32, 0x2C, 0x00, 0x00, 0x00, 0x04, 
			        0x01, 0x00, 0xFF, 0x00, 0x00, 0x01, 0x80, 0x00, 0x00, 
			        0x8A, 0x00, 0x00, 0x00, 0xAB, 0xD2, 0x00, 0x00, 0x10, 
			        0xD6, 0x00, 0x00, 0x3D, 0x14, 0x00, 0x01, 0x00, 0x00, 
			        0x00, 0x00, 0x8A, 0x00, 0x00, 0x00, 0x88, 0x80, 0x00, 
			        0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x73, 0x65, 
			        0x65, 0x6B, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x8A, 
			        0x00
		        };

        private static byte[] stereoFooter = new byte[]
                {
			        0x58, 0x4D, 0x41, 0x32, 0x2C, 0x00, 0x00, 0x00, 0x04, 
			        0x01, 0x00, 0xFF, 0x00, 0x00, 0x01, 0x80, 0x00, 0x01, 
			        0x0F, 0x80, 0x00, 0x00, 0xAC, 0x44, 0x00, 0x00, 0x10, 
			        0xD6, 0x00, 0x00, 0x3D, 0x14, 0x00, 0x01, 0x00, 0x00, 
			        0x00, 0x01, 0x10, 0x00, 0x00, 0x01, 0x0E, 0x00, 0x00, 
			        0x00, 0x00, 0x01, 0x02, 0x00, 0x02, 0x01, 0x73, 0x65, 
			        0x65, 0x6B, 0x04, 0x00, 0x00, 0x00, 0x00, 0x01, 0x10, 
			        0x00
		        };
        #endregion

        public SoundFormat DefaultSnd_Format = SoundFormat.WAV;

        public void LoadSoundTag(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            cache = Cache;
            tag = Tag;

            snd = DefinitionsManager.snd_(cache, tag);
            if (snd.SoundBankTagID != -1) sbnk = DefinitionsManager.sbnk(cache, cache.IndexItems.GetItemByID(snd.SoundBankTagID));
            else sbnk = null;

            if (Cache.Version != DefinitionSet.Halo4Retail) throw new Exception("This is for H4 ONLY");

            LoadCacheSoundPacks(cache);

            lstPerms.Items.Clear();
            _perms.Clear();
            _soundbanks.Clear();

            ObjectLoadWorker();

            if (lstPerms.Items.Count > 0)
            {
                Enabled = true;
                lstPerms.SelectedIndex = 0;
                label1.Text = _perms[0].Format.ToString();
            }
            else
            {
                label1.Text = "";
                Enabled = false;
            }
        }

        public void SaveTemp(int index)
        {
            var info = _perms[index];

            var fileName = Path.GetTempPath() + "temp_playback.wav";

            RIFX rifx = ReadRIFX(info);

            switch (info.Format)
            {
                case Composer.SoundFormat.XMA:
                    SoundExtraction.ExtractXMAToWAV(info.Reader, info.Offset, rifx, fileName);
                    break;

                case Composer.SoundFormat.WwiseOGG:
                    throw new NotSupportedException("OGG preview not supported: extract it instead.");

                case Composer.SoundFormat.XWMA:
                    SoundExtraction.ExtractXWMAToWAV(info.Reader, info.Offset, rifx, fileName);
                    break;

                default:
                    throw new Exception("no");
            }
        }

        #region Events
        private void SoundExtractorH4_Load(object sender, EventArgs e)
        {
            scanner = new SoundScanner();
            scanner.FoundSoundBankFile += FoundSoundBankFile;
            scanner.FoundSoundPackFile += FoundSoundPackFile;

            isDisplay = true;
        }

        private void btnSaveSelected_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog()
            {
                AddExtension = false,
                FileName = Path.GetFileName(tag.Filename),// tag.Filename.Substring(tag.Filename.LastIndexOf('\\') + 1),
                Filter = "WAV Files|*.wav",//|XMA Files|*.xma|Raw Data|*.bin",
                FilterIndex = (int)DefaultSnd_Format + 1
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                for (int i = 0; i < lstPerms.Items.Count; i++)
                {
                    if (!lstPerms.SelectedIndices.Contains(i)) continue;
                    
                    var info = _perms[i];

                    var fileName = sfd.FileName.Substring(0, sfd.FileName.LastIndexOf("\\") + 1) + Path.GetFileNameWithoutExtension(sfd.FileName) + " [" + i.ToString() + "]" + Path.GetExtension(sfd.FileName);

                    RIFX rifx = ReadRIFX(info);

                    switch (info.Format)
                    {
                        case Composer.SoundFormat.XMA:
                            SoundExtraction.ExtractXMAToWAV(info.Reader, info.Offset, rifx, fileName);
                            break;

                        case Composer.SoundFormat.WwiseOGG:
                            SoundExtraction.ExtractWwiseToOGG(info.Reader, info.Offset, info.Size, fileName);
                            break;

                        case Composer.SoundFormat.XWMA:
                            SoundExtraction.ExtractXWMAToWAV(info.Reader, info.Offset, rifx, fileName);
                            break;

                        default:
                            throw new Exception("no");
                    }
                }

                //SaveSelected(sfd.FileName, cache, tag, (SoundFormat)(sfd.FilterIndex - 1), indices, true);
                TagExtracted(this, tag);
            }
            catch (Exception ex) { ErrorExtracting(this, tag, ex); }
        }

        private void btnSaveSingle_Click(object sender, EventArgs e)
        {
            //var sfd = new SaveFileDialog()
            //{
            //    FileName = tag.Filename.Substring(tag.Filename.LastIndexOf('\\') + 1),
            //    Filter = "WAV Files|*.wav|XMA Files|*.xma|Raw Data|*.bin",
            //    FilterIndex = (int)DefaultSnd_Format * 2
            //};

            //if (sfd.ShowDialog() != DialogResult.OK) return;

            //try
            //{
            //    SaveAllAsSingle(sfd.FileName, cache, tag, (SoundFormat)(sfd.FilterIndex - 1));
            //    TagExtracted(this, tag);
            //}
            //catch (Exception ex) { ErrorExtracting(this, tag, ex); }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            try
            {
                //for (int i = 0; i < lstPerms.Items.Count; i++)
                {
                    SaveTemp(lstPerms.SelectedIndex);
                    player.Play();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            player.Stop();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lstPerms.Items.Count; i++)
                lstPerms.SetSelected(i, true);
        }

        private void selectNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lstPerms.Items.Count; i++)
                lstPerms.SetSelected(i, false);
        }

        private void lstPerms_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                SaveTemp(lstPerms.SelectedIndex);
                player.Play();
            }
            catch { }

        }
        #endregion

        /// <summary>
        /// Save all permutations of a sound tag as separate sound files.
        /// </summary>
        /// <param name="Folder">The base filename. Permutation names will be appended accordingly.</param>
        /// <param name="Cache">The CacheFile containing the tag.</param>
        /// <param name="Tag">The sound tag.</param>
        /// <param name="Format">The format in which to save the data.</param>
        public void SaveAllAsSeparate(string Folder, CacheFile Cache, CacheFile.IndexItem Tag, SoundFormat Format, bool Overwrite)
        {
            if (Format != SoundFormat.WAV) throw new NotSupportedException("Halo4Retail only supports WAV.");

            if (scanner == null)
            {
                scanner = new SoundScanner();
                scanner.FoundSoundBankFile += FoundSoundBankFile;
                scanner.FoundSoundPackFile += FoundSoundPackFile;
            }

            cache = Cache;
            tag = Tag;

            snd = DefinitionsManager.snd_(cache, tag);
            if (snd.SoundBankTagID != -1) sbnk = DefinitionsManager.sbnk(cache, cache.IndexItems.GetItemByID(snd.SoundBankTagID));
            else sbnk = null;

            if (Cache.Version != DefinitionSet.Halo4Retail) throw new Exception("This is for H4 ONLY");

            LoadCacheSoundPacks(cache);

            if (cache.H4SoundFiles == null)
            {
                cache.H4SoundFiles = new Dictionary<uint, List<SoundFileInfo>>();
                ObjectLoadWorker();
            }

            bool s1, s2;
            List<SoundFileInfo> sfi1, sfi2, sfi3;
            s1 = cache.H4SoundFiles.TryGetValue(snd.SoundAddress1, out sfi1);
            s2 = cache.H4SoundFiles.TryGetValue(snd.SoundAddress2, out sfi2);

            if (!s1 && !s2) throw new Exception("No permutations found.");

            sfi3 = new List<SoundFileInfo>();
            if (s1) sfi3.AddRange(sfi1);
            if (s2) sfi3.AddRange(sfi2);

            for (int i = 0; i < sfi3.Count; i++)
            {
                var info = sfi3[i];

                var fName = Path.GetFileName(tag.Filename);

                fName = Folder + "\\" + fName + " [" + i.ToString() + "]" + ".wav";

                if (File.Exists(fName) && !Overwrite) return;

                RIFX rifx = ReadRIFX(info);

                switch (info.Format)
                {
                    case Composer.SoundFormat.XMA:
                        if (!Directory.GetParent(fName).Exists) Directory.GetParent(fName).Create();
                        SoundExtraction.ExtractXMAToWAV(info.Reader, info.Offset, rifx, fName);
                        break;

                    case Composer.SoundFormat.WwiseOGG:
                        if (!Directory.GetParent(fName).Exists) Directory.GetParent(fName).Create();
                        SoundExtraction.ExtractWwiseToOGG(info.Reader, info.Offset, info.Size, fName);
                        break;

                    default:
                        throw new NotSupportedException(info.Format.ToString() + " not supported.");
                }
            }
        }

        private static void LoadCacheSoundPacks(CacheFile Cache)
        {
            if (Cache.H4SoundPacks == null)
            {
                Cache.H4SoundPacks = new List<Composer.SoundPackInfo>();

                var bank = new SoundPackInfo();
                bank.Name = "SoundBank";
                bank.Reader = new EndianReader(File.OpenRead(Cache.FilePath + @"\soundbank.pck"), EndianFormat.BigEndian);
                bank.Pack = new SoundPack(bank.Reader);
                Cache.H4SoundPacks.Add(bank);

                var stream = new SoundPackInfo();
                stream.Name = "SoundStream";
                stream.Reader = new EndianReader(File.OpenRead(Cache.FilePath + @"\soundstream.pck"), EndianFormat.BigEndian);
                stream.Pack = new SoundPack(stream.Reader);
                Cache.H4SoundPacks.Add(stream);
            }
        }

        #region Composer
        private void ObjectLoadWorker()
        {
            // Load everything from the packs into the scanner
            foreach (SoundPackInfo pack in cache.H4SoundPacks)
                LoadObjects(pack.Pack, pack.Reader, scanner);

            // Clear the TreeView and scan everything
            // The event handlers attached above are responsible for adding nodes to the tree
            for (int i = 0; i < _soundbanks.Count; i++)
            {
                foreach (SoundBankEvent ev in _soundbanks[i].Events)
                {
                    if (!isDisplay || snd.SoundAddress1 == ev.ID || snd.SoundAddress2 == ev.ID)
                        scanner.ScanEvent(_soundbanks[i], ev);
                }
            }
        }

        /// <summary>
        /// Event handler for the SoundScanner.FoundSoundBankFile event.
        /// </summary>
        private void FoundSoundBankFile(object sender, SoundFileEventArgs<SoundBankFile> e)
        {
            //if (e.SourceEvent.ID != snd.SoundAddress1 && e.SourceEvent.ID != snd.SoundAddress2) return;

            // Find the sound bank's pack file so we can determine the offset of the audio data
            uint bankId = e.File.ParentBank.ID;
            SoundPackInfo packInfo = null;
            SoundPackFile packFile = null;
            foreach (SoundPackInfo pack in cache.H4SoundPacks)
            {
                packFile = pack.Pack.FindFileByID(bankId);
                if (packFile != null)
                {
                    packInfo = pack;
                    break;
                }
            }
            if (packFile == null)
                return;

            // Calculate the offset of the audio data and add it
            int offset = packFile.Offset + e.File.ParentBank.DataOffset + e.File.Offset;
            AddSound(packInfo.Reader, offset, e.File.Size, e.File.ID, e.SourceEvent.ID);
        }

        /// <summary>
        /// Event handler for the SoundScanner.FoundSoundPackFile event.
        /// </summary>
        private void FoundSoundPackFile(object sender, SoundFileEventArgs<SoundPackFile> e)
        {
            //if (e.SourceEvent.ID != snd.SoundAddress1 && e.SourceEvent.ID != snd.SoundAddress2) return;

            EndianReader reader = null;
            foreach (SoundPackInfo pack in cache.H4SoundPacks)
            {
                if (pack.Pack.FindFileByID(e.File.ID) != null)
                {
                    reader = pack.Reader;
                    break;
                }
            }
            AddSound(reader, e.File.Offset, e.File.Size, e.File.ID, e.SourceEvent.ID);
        }

        private void AddSound(EndianReader reader, int offset, int size, uint id, uint sourceID)
        {
            // Read the sound's RIFX data so we can identify its format
            reader.SeekTo(offset);
            RIFX rifx = new RIFX(reader, size);
            Composer.SoundFormat format = FormatIdentification.IdentifyFormat(rifx);

            // Create information to tag the tree node with
            SoundFileInfo info = new SoundFileInfo
            {
                Reader = reader,
                Offset = offset,
                Size = size,
                ID = id,
                Format = format
            };

            if (!isDisplay)
            {
                List<SoundFileInfo> sfi;
                if (cache.H4SoundFiles.TryGetValue(sourceID, out sfi))
                    sfi.Add(info);
                else
                    cache.H4SoundFiles.Add(sourceID, new List<SoundFileInfo>() { info });

                return;
            }

            lstPerms.Items.Add(tag.Filename.Substring(tag.Filename.LastIndexOf("\\") + 1) + " [" + lstPerms.Items.Count.ToString() + "]");
            _perms.Add(info);
        }

        private void LoadObjects(SoundPack pack, EndianReader reader, SoundScanner scanner)
        {
            foreach (SoundPackFolder folder in pack.Folders)
            {
                foreach (SoundPackFile file in folder.Files)
                {
                    reader.SeekTo(file.Offset);
                    int magic = reader.ReadInt32();

                    switch (magic)
                    {
                        case 0x52494658: // RIFX - Embedded sound file
                            scanner.RegisterGlobalObject(file);
                            break;

                        case 0x424B4844: // BKHD - Sound bank
                            reader.SeekTo(file.Offset);
                            SoundBank bank = new SoundBank(reader, file.Size);
                            scanner.RegisterSoundBank(bank);
                            _soundbanks.Add(bank);
                            break;
                    }
                }
            }
        }

        private static RIFX ReadRIFX(SoundFileInfo info)
        {
            info.Reader.SeekTo(info.Offset);
            return new RIFX(info.Reader, info.Size);
        }
        #endregion

        public event TagExtractedEventHandler TagExtracted;
        public event ErrorExtractingEventHandler ErrorExtracting;
    }
}

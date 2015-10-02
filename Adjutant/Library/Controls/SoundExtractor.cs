using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Adjutant.Library;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.Endian;
using System.IO;
using System.Diagnostics;
using Composer;
using Composer.Wwise;
using ugh_ = Adjutant.Library.Definitions.sound_cache_file_gestalt;

namespace Adjutant.Library.Controls
{
    public partial class SoundExtractor : UserControl
    {
        public SoundExtractor()
        {
            InitializeComponent();
        }

        private static string towav = "Helpers\\towav.exe";

        private CacheBase cache;
        private CacheBase.IndexItem tag;
        private ugh_ ugh;
        private sound snd;
        private ugh_.Playback playback;
        private List<ugh_.SoundPermutation> Perms;
        private System.Media.SoundPlayer player = new System.Media.SoundPlayer() { SoundLocation = Path.GetTempPath() + "temp_playback.wav" };

        public SoundFormat DefaultSnd_Format = SoundFormat.WAV;

        public void LoadSoundTag(CacheBase Cache, CacheBase.IndexItem Tag)
        {
            cache = Cache;
            tag = Tag;

            snd = DefinitionsManager.snd_(cache, tag);

            if (cache.ugh_ == null)
            {
                lstPerms.Items.Clear();
                Enabled = false;
                return;
            }
            else
                Enabled = true;

            ugh = cache.ugh_;
            playback = ugh.PlayBacks[snd.PlaybackIndex];

            Perms = new List<ugh_.SoundPermutation>();

            for (int i = 0; i < playback.PermutationCount; i++)
                Perms.Add(ugh.SoundPermutations[playback.FirstPermutation + i]);

            lstPerms.Items.Clear();

            foreach (var perm in Perms)
                lstPerms.Items.Add(ugh.SoundNames[perm.NameIndex]);

            lstPerms.SelectedIndex = 0;
        }

        public void SaveTemp(int index)
        {
            var tempName = Path.GetTempPath() + "temp_playback.xma";

            var playback = ugh.PlayBacks[snd.PlaybackIndex];
            var perm = cache.ugh_.SoundPermutations[playback.FirstPermutation + index];
            var data = cache.GetSoundRaw(snd.RawID, GetTotalSize(ugh, playback));
            byte[] buffer;

            if (index == -1)
                buffer = data;
            else
                buffer = GetPermData(data, ugh, perm);

            var codec = cache.ugh_.Codecs[snd.CodecIndex];
            var xma = GetXMA(buffer, snd.SampleRate, codec.Type);

            var fs = File.OpenWrite(tempName);
            EndianWriter sw = new EndianWriter(fs, EndianFormat.BigEndian);
            sw.Write(xma);

            sw.Close();
            sw.Dispose();

            var info = new ProcessStartInfo(towav, tempName)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetTempPath()
            };

            Process.Start(info).WaitForExit();
            if (File.Exists(tempName)) File.Delete(tempName);
        }

        #region Events
        private void btnSaveSelected_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog()
            {
                AddExtension = false,
                FileName = tag.Filename.Substring(tag.Filename.LastIndexOf('\\') + 1),
                Filter = "WAV Files|*.wav|XMA Files|*.xma|Raw Data|*.bin",
                FilterIndex = (int)DefaultSnd_Format + 1
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                List<int> indices = new List<int>();

                foreach (int i in lstPerms.SelectedIndices) indices.Add(i);

                SaveSelected(sfd.FileName, cache, tag, (SoundFormat)(sfd.FilterIndex - 1), indices, true);
                TagExtracted(this, tag);
            }
            catch (Exception ex) { ErrorExtracting(this, tag, ex); }
        }

        private void btnSaveSingle_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog()
            {
                FileName = tag.Filename.Substring(tag.Filename.LastIndexOf('\\') + 1),
                Filter = "WAV Files|*.wav|XMA Files|*.xma|Raw Data|*.bin",
                FilterIndex = (int)DefaultSnd_Format + 1
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                SaveAllAsSingle(sfd.FileName, cache, tag, (SoundFormat)(sfd.FilterIndex - 1));
                TagExtracted(this, tag);
            }
            catch (Exception ex) { ErrorExtracting(this, tag, ex); }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            try
            {
                SaveTemp(-1);
                player.Play();
            }
            catch { }
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

        #region Static Methods
        /// <summary>
        /// Saves selected permutations of a sound tag.
        /// </summary>
        /// <param name="Folder">The folder to save all files in. Each file will be named as the permutation name.</param>
        /// <param name="Cache">The CacheFile containing the tag.</param>
        /// <param name="Tag">The sound tag.</param>
        /// <param name="Format">The format in which to save the data.</param>
        /// <param name="Indices">The indices of the permutations to extract.</param>
        public static void SaveSelected(string Folder, CacheBase Cache, CacheBase.IndexItem Tag, SoundFormat Format, List<int> Indices, bool Overwrite)
        {
            var snd_ = DefinitionsManager.snd_(Cache, Tag);
            List<byte[]> perms = new List<byte[]>();

            var ugh_ = Cache.ugh_;
            var playback = ugh_.PlayBacks[snd_.PlaybackIndex];
            var data = Cache.GetSoundRaw(snd_.RawID, GetTotalSize(ugh_, playback));

            if (playback.PermutationCount == 1)
                perms.Add(data);
            else
            {
                Folder = Directory.GetParent(Folder) + "\\" + Path.GetFileNameWithoutExtension(Folder);

                for (int i = 0; i < playback.PermutationCount; i++)
                {
                    var perm = Cache.ugh_.SoundPermutations[playback.FirstPermutation + i];
                    perms.Add(GetPermData(data, ugh_, perm));
                }
            }

            #region XMA
            if (Format == SoundFormat.XMA)
            {
                foreach (int index in Indices)
                {
                    string Filename = (playback.PermutationCount == 1) ? Folder : Folder + "\\" + ugh_.SoundNames[ugh_.SoundPermutations[playback.FirstPermutation + index].NameIndex].Name + ".xma";
                    if (!Filename.EndsWith(".xma")) Filename += ".xma";
                    
                    if (File.Exists(Filename) && !Overwrite) continue;

                    byte[] buffer = perms[index];
                    var codec = Cache.ugh_.Codecs[snd_.CodecIndex];
                    var xma = GetXMA(buffer, snd_.SampleRate, codec.Type);

                    if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();

                    var fs = new FileStream(Filename, FileMode.Create);
                    EndianWriter sw = new EndianWriter(fs, EndianFormat.BigEndian);
                    sw.Write(xma);

                    sw.Close();
                    sw.Dispose();
                }
            }
            #endregion
            #region WAV
            else if (Format == SoundFormat.WAV)
            {
                foreach (int index in Indices)
                {
                    string Filename = (playback.PermutationCount == 1) ? Folder : Folder + "\\" + ugh_.SoundNames[ugh_.SoundPermutations[playback.FirstPermutation + index].NameIndex].Name + ".wav";
                    if (!Filename.EndsWith(".wav")) Filename += ".wav";
                    
                    if (File.Exists(Filename) && !Overwrite) continue;

                    var tempName = Path.GetTempFileName();

                    #region Write XMA
                    var buffer = perms[index];
                    var codec = Cache.ugh_.Codecs[snd_.CodecIndex];
                    var xma = GetXMA(buffer, snd_.SampleRate, codec.Type);

                    var fs = File.OpenWrite(tempName);
                    EndianWriter sw = new EndianWriter(fs, EndianFormat.BigEndian);
                    sw.Write(xma);

                    sw.Close();
                    sw.Dispose();
                    #endregion

                    var info = new ProcessStartInfo(towav, tempName)
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WorkingDirectory = Directory.GetParent(tempName).FullName
                    };

                    Process.Start(info).WaitForExit();

                    if (File.Exists(Filename)) File.Delete(Filename);
                    if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();
                    File.Move(Path.ChangeExtension(tempName, "wav"), Filename);
                    if (File.Exists(tempName)) File.Delete(tempName);
                }
            }
            #endregion
            #region RAW
            else if (Format == SoundFormat.RAW)
            {
                foreach (int index in Indices)
                {
                    string Filename = (playback.PermutationCount == 1) ? Folder : Folder + "\\" + ugh_.SoundNames[ugh_.SoundPermutations[playback.FirstPermutation + index].NameIndex].Name + ".bin";
                    if (!Filename.EndsWith(".bin")) Filename += ".bin";

                    if (File.Exists(Filename) && !Overwrite) continue;
                    
                    byte[] buffer = perms[index];

                    if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();

                    var fs = new FileStream(Filename, FileMode.Create);
                    BinaryWriter sw = new BinaryWriter(fs);

                    sw.Write(buffer);
                    sw.Close();
                    sw.Dispose();
                }
            }
            #endregion
        }

        /// <summary>
        /// Saves all permutations of a sound tag concatenated as a single sound file.
        /// </summary>
        /// <param name="Filename">The file to save the data to.</param>
        /// <param name="Cache">The CacheFile containing the tag.</param>
        /// <param name="Tag">The sound tag.</param>
        /// <param name="Format">The format in which to save the data.</param>
        public static void SaveAllAsSingle(string Filename, CacheBase Cache, CacheBase.IndexItem Tag, SoundFormat Format)
        {
            var snd_ = DefinitionsManager.snd_(Cache, Tag);

            #region XMA
            if (Format == SoundFormat.XMA)
            {
                var total = GetTotalSize(Cache.ugh_, Cache.ugh_.PlayBacks[snd_.PlaybackIndex]);

                byte[] buffer = Cache.GetSoundRaw(snd_.RawID, total);

                if (buffer.Length == 0) throw new Exception("Empty raw data.");
                var codec = Cache.ugh_.Codecs[snd_.CodecIndex];
                var xma = GetXMA(buffer, snd_.SampleRate, codec.Type);

                if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();

                var fs = File.OpenWrite(Filename);
                EndianWriter sw = new EndianWriter(fs, EndianFormat.BigEndian);
                sw.Write(xma);

                sw.Close();
                sw.Dispose();
            }
            #endregion
            #region WAV
            else if (Format == SoundFormat.WAV)
            {
                var tempName = Path.GetTempFileName();

                SaveAllAsSingle(tempName, Cache, Tag, SoundFormat.XMA);

                var info = new ProcessStartInfo(towav, tempName)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = Directory.GetParent(tempName).FullName
                };

                Process.Start(info).WaitForExit();

                if (File.Exists(Filename)) File.Delete(Filename);
                if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();
                File.Move(Path.ChangeExtension(tempName, "wav"), Filename);
                if (File.Exists(tempName)) File.Delete(tempName);
            }
            #endregion
            #region RAW
            else if (Format == SoundFormat.RAW)
            {
                byte[] buffer = Cache.GetSoundRaw(snd_.RawID, GetTotalSize(Cache.ugh_, Cache.ugh_.PlayBacks[snd_.PlaybackIndex]));
                
                if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();

                var fs = new FileStream(Filename, FileMode.Create);
                BinaryWriter sw = new BinaryWriter(fs);

                sw.Write(buffer);
                sw.Close();
                sw.Dispose();
            }
            #endregion
            #region Other
            else
                throw new InvalidOperationException("Invalid sound format received.");
            #endregion

        }

        /// <summary>
        /// Save all permutations of a sound tag as separate sound files.
        /// </summary>
        /// <param name="Folder">The base filename. Permutation names will be appended accordingly.</param>
        /// <param name="Cache">The CacheFile containing the tag.</param>
        /// <param name="Tag">The sound tag.</param>
        /// <param name="Format">The format in which to save the data.</param>
        public static void SaveAllAsSeparate(string Folder, CacheBase Cache, CacheBase.IndexItem Tag, SoundFormat Format, bool Overwrite)
        {
            var snd_ = DefinitionsManager.snd_(Cache, Tag);
            var ugh_ = Cache.ugh_;

            var indices = new List<int>();

            for (int i = 0; i < ugh_.PlayBacks[snd_.PlaybackIndex].PermutationCount; i++)
                indices.Add(i);

            SaveSelected(Folder, Cache, Tag, Format, indices, Overwrite);
        }

        private static byte[] GetPermData(byte[] Data, sound_cache_file_gestalt ugh, sound_cache_file_gestalt.SoundPermutation Perm)
        {
            List<byte[]> fragments = new List<byte[]>();
            int totalSize = 0;
            var ms = new MemoryStream(Data);
            var br = new BinaryReader(ms);

            for (int i = 0; i < Perm.ChunkCount; i++)
            {
                var chunk = ugh.RawChunks[Perm.RawChunkIndex + i];
                totalSize += chunk.Size;

                br.BaseStream.Position = chunk.FileOffset;
                fragments.Add(br.ReadBytes(chunk.Size));
            }

            byte[] newData = new byte[totalSize];
            int pos = 0;

            for (int i = 0; i < Perm.ChunkCount; i++)
            {
                Array.Copy(fragments[i], 0, newData, pos, fragments[i].Length);
                pos += fragments[i].Length;
            }

            return newData;
        }

        private static int GetTotalSize(sound_cache_file_gestalt ugh, sound_cache_file_gestalt.Playback snd)
        {
            int total = 0;

            for (int i = 0; i < snd.PermutationCount; i++)
            {
                var perm = ugh.SoundPermutations[snd.FirstPermutation + i];

                for (int j = 0; j < perm.ChunkCount; j++)
                {
                    var chunk = ugh.RawChunks[perm.RawChunkIndex + j];
                    total += chunk.Size;
                }
            }

            return total;
        }

        public static byte[] GetXMA(byte[] buffer, SampleRate sRate, SoundType sType)
        {
            int rate;
            switch (sRate)
            {
                case SampleRate._22050Hz:
                    rate = 22050;
                    break;

                case SampleRate._44100Hz:
                    rate = 44100;
                    break;

                default:
                    rate = 44100;
                    break;
                    //throw new Exception("Check sample rate.");
            }

            int cCount;
            switch (sType)
            {
                case SoundType.Mono:
                    cCount = 1;
                    break;
                case SoundType.Stereo:
                    cCount = 2;
                    break;
                //case SoundType.Unknown2:
                //    cCount = 2;
                //    footer = stereoFooter;
                //    break;
                //case SoundType.Unknown3:
                //    cCount = 2;
                //    footer = stereoFooter;
                //    break;
                default:
                    throw new NotSupportedException("Unsupported Sound Type.");
            }

            var ms = new MemoryStream();
            EndianWriter sw = new EndianWriter(ms, EndianFormat.BigEndian);

            sw.Write(0x52494646); // 'RIFF'
            sw.EndianType = EndianFormat.LittleEndian;
            sw.Write(buffer.Length + 0x34);
            sw.EndianType = EndianFormat.BigEndian;
            sw.Write(RIFFFormat.WAVE);

            // Generate the 'fmt ' chunk
            sw.Write(0x666D7420); // 'fmt '
            sw.EndianType = EndianFormat.LittleEndian;
            sw.Write(0x20);
            sw.Write((short)0x165);
            sw.Write((short)16);
            sw.Write((short)0);
            sw.Write((short)0);
            sw.Write((short)1);
            sw.Write((byte)0);
            sw.Write((byte)3);
            sw.Write(0);
            sw.Write(rate);
            sw.Write(0);
            sw.Write(0);
            sw.Write((byte)0);
            sw.Write((byte)cCount);
            sw.Write((short)0x0002);

            // 'data' chunk
            sw.EndianType = EndianFormat.BigEndian;
            sw.Write(0x64617461); // 'data'
            sw.EndianType = EndianFormat.LittleEndian;
            sw.Write(buffer.Length);
            sw.Write(buffer);

            sw.Close();
            sw.Dispose();

            return ms.ToArray();
        }
        #endregion

        public event TagExtractedEventHandler TagExtracted;
        public event ErrorExtractingEventHandler ErrorExtracting;
    }
}

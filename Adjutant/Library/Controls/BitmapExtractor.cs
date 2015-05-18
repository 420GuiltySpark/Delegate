using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Adjutant.Library.Cache;
using Adjutant.Library.Definitions;
using Adjutant.Library.Imaging;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Adjutant.Library.Controls
{
    public partial class BitmapExtractor : UserControl
    {
        private CacheFile cache;
        private CacheFile.IndexItem tag;
        private bitmap bitm;
        private bool isWorking;

        public BitmapFormat DefaultBitmFormat = BitmapFormat.TIF;

        public BitmapExtractor()
        {
            InitializeComponent();
            cmbFormat.Items.AddRange(Enum.GetNames(typeof(TextureFormat)));
        }

        #region Methods
        public void LoadBitmapTag(CacheFile Cache, CacheFile.IndexItem Tag)
        {
            cache = Cache;
            tag = Tag;

            bitm = DefinitionsManager.bitm(cache, tag);

            lstBitmaps.Items.Clear();
            var list = GetBitmapsByTag(cache, tag, PixelFormat.Format32bppArgb);
            for (int i = 0; i < list.Count; i++)
            {
                var submap = bitm.Bitmaps[i];
                lstBitmaps.Items.Add(new ListViewItem(new string[]
                {
                        i.ToString(), 
                        submap.Width.ToString(), 
                        submap.Height.ToString(),
                        submap.Type.ToString(),
                        submap.Format.ToString()})
                {
                    Tag = (list[i] == null) ? GetErrorImage() : list[i]
                });
            }

            lstBitmaps.FocusedItem = lstBitmaps.Items[0];
            lstBitmaps_SelectedIndexChanged(null, null);
        }
        
        private Bitmap GetErrorImage()
        {
            Bitmap image = new Bitmap(256, 256);
            Graphics graphics = Graphics.FromImage(image);
            graphics.Clear(Color.CornflowerBlue);
            var font = new Font(FontFamily.GenericSerif, 18f);
            string text = "No Preview Available";
            SizeF ef = graphics.MeasureString(text, font);
            graphics.DrawString(text, font, Brushes.Black, new PointF(128f - (ef.Width / 2f), 128f - (ef.Height / 2f)));
            return image;
        }

        private void UpdateBoxSize()
        {
            var img = picImage.Image;
            var maxSize = 384f;
            var scale = maxSize / Math.Max(img.Width, img.Height);

            if (img.Width == img.Height)
                picImage.Size = (img.Width < maxSize) ? new Size(img.Width, img.Height) : new Size((int)maxSize, (int)maxSize);
            else if (img.Width <= maxSize && img.Height <= maxSize)
                picImage.Size = img.Size;
            else
                picImage.Size = new Size((int)(img.Width * scale), (int)(img.Height * scale));
        }

        #region Static Methods

        #region GetBitmapByTag
        /// <summary>
        /// Gets an image from a bitmap tag.
        /// </summary>
        /// <param name="Cache">The CacheFile containing the bitmap tag.</param>
        /// <param name="Tag">The bitmap tag.</param>
        /// <param name="Index">The index of the BitmapData chunk to use.</param>
        /// <param name="Alpha">Whether to include the alpha channel in the image.</param>
        /// <returns>The image from the bitmap tag as a Bitmap.</returns>
        public static Bitmap GetBitmapByTag(CacheFile Cache, CacheFile.IndexItem Tag, int Index, PixelFormat PF)
        {
            var bitm = DefinitionsManager.bitm(Cache, Tag);
            return GetBitmapByTag(Cache, bitm, Index, PF);
        }


        /// <summary>
        /// Gets an image from a bitmap tag.
        /// </summary>
        /// <param name="Cache">The CacheFile containing the bitmap tag.</param>
        /// <param name="bitm">The bitmap tag.</param>
        /// <param name="Index">The index of the BitmapData chunk to use.</param>
        /// <param name="Alpha">Whether to include the alpha channel in the image.</param>
        /// <returns>The image from the bitmap tag as a Bitmap.</returns>
        public static Bitmap GetBitmapByTag(CacheFile Cache, bitmap bitm, int Index, PixelFormat PF)
        {
            try
            {
                var submap = bitm.Bitmaps[Index];

                byte[] raw;
                if (bitm.RawChunkBs.Count > 0)
                {
                    int rawID = bitm.RawChunkBs[submap.InterleavedIndex].RawID;
                    byte[] buffer = Cache.GetRawFromID(rawID);
                    raw = new byte[submap.RawSize];
                    Array.Copy(buffer, submap.Index2 * submap.RawSize, raw, 0, submap.RawSize);
                }
                else
                {
                    int rawID = bitm.RawChunkAs[Index].RawID;
                    raw = Cache.GetRawFromID(rawID, submap.RawSize);
                }

                int vHeight = submap.VirtualHeight;
                int vWidth = submap.VirtualWidth;

                if (submap.Type == TextureType.CubeMap)
                    return DXTDecoder.DecodeCubeMap(raw, submap, PF);

                raw = DXTDecoder.DecodeBitmap(raw, submap);

                //PixelFormat PF = (Alpha) ? PixelFormat.Format32bppArgb : PixelFormat.Format32bppRgb;
                Bitmap bitmap2 = new Bitmap(submap.Width, submap.Height, PF);
                Rectangle rect = new Rectangle(0, 0, submap.Width, submap.Height);
                BitmapData bitmapdata = bitmap2.LockBits(rect, ImageLockMode.WriteOnly, PF);
                byte[] destinationArray = new byte[(submap.Width * submap.Height) * 4];

                for (int j = 0; j < submap.Height; j++)
                    Array.Copy(raw, j * vWidth * 4, destinationArray, j * submap.Width * 4, submap.Width * 4);

                Marshal.Copy(destinationArray, 0, bitmapdata.Scan0, destinationArray.Length);
                bitmap2.UnlockBits(bitmapdata);

                return bitmap2;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region GetBitmapsByTag
        /// <summary>
        /// Gets all images from the a bitmap tag.
        /// </summary>
        /// <param name="Cache">The CacheFile containing the bitmap tag.</param>
        /// <param name="Tag">The bitmap tag.</param>
        /// <param name="Alpha">Whether to include the alpha channels in the images.</param>
        /// <returns>A List containing each image as a Bitmap.</returns>
        public static List<Bitmap> GetBitmapsByTag(CacheFile Cache, CacheFile.IndexItem Tag, PixelFormat PF)
        {
            var bitm = DefinitionsManager.bitm(Cache, Tag);
            return GetBitmapsByTag(Cache, bitm, PF);
        }

        /// <summary>
        /// Gets all images from the a bitmap tag.
        /// </summary>
        /// <param name="Cache">The CacheFile containing the bitmap tag.</param>
        /// <param name="bitm">The bitmap tag.</param>
        /// <param name="Alpha">Whether to include the alpha channels in the images.</param>
        /// <returns>A List containing each image as a Bitmap.</returns>
        public static List<Bitmap> GetBitmapsByTag(CacheFile Cache, bitmap bitm, PixelFormat PF)
        {
            List<Bitmap> list = new List<Bitmap>();

            for (int i = 0; i < bitm.Bitmaps.Count; i++)
                list.Add(GetBitmapByTag(Cache, bitm, i, PF));

            return list;
        }
        #endregion

        #region SaveImage
        /// <summary>
        /// Saves an image from a bitmap tag to disk.
        /// </summary>
        /// <param name="Filename">The full path and filename to save to.</param>
        /// <param name="Cache">The CacheFile containing the bitmap tag.</param>
        /// <param name="Tag">The bitmap tag.</param>
        /// <param name="Index">The index of the BitmapData chunk to use.</param>
        /// <param name="Format">The format to save the image in.</param>
        /// <param name="Alpha">Whether to include the alpha channel in the image. Only applies when saving in TIF format.</param>
        public static void SaveImage(string Filename, CacheFile Cache, CacheFile.IndexItem Tag, int Index, BitmapFormat Format, bool Alpha)
        {
            var bitm = DefinitionsManager.bitm(Cache, Tag);
            SaveImage(Filename, Cache, bitm, Index, Format, Alpha);
        }

        /// <summary>
        /// Saves an image from a bitmap tag to disk.
        /// </summary>
        /// <param name="Filename">The full path and filename to save to.</param>
        /// <param name="Cache">The CacheFile containing the bitmap tag.</param>
        /// <param name="Tag">The bitmap tag.</param>
        /// <param name="Index">The index of the BitmapData chunk to use.</param>
        /// <param name="Format">The format to save the image in.</param>
        /// <param name="Alpha">Whether to include the alpha channel in the image. Only applies when saving in TIF format.</param>
        public static void SaveImage(string Filename, CacheFile Cache, bitmap bitm, int Index, BitmapFormat Format, bool Alpha)
        {
            var submap = bitm.Bitmaps[Index];

            byte[] raw;
            if (bitm.RawChunkBs.Count > 0)
            {
                int rawID = bitm.RawChunkBs[submap.InterleavedIndex].RawID;
                byte[] buffer = Cache.GetRawFromID(rawID);
                raw = new byte[submap.RawSize];
                Array.Copy(buffer, submap.Index2 * submap.RawSize, raw, 0, submap.RawSize);
            }
            else
            {
                int rawID = bitm.RawChunkAs[Index].RawID;
                raw = Cache.GetRawFromID(rawID, submap.RawSize);
            }

            int vHeight = submap.VirtualHeight;
            int vWidth = submap.VirtualWidth;

            if (Format == BitmapFormat.TIF || Format == BitmapFormat.PNG64)
            {
                string ext = (Format == BitmapFormat.TIF) ? ".tif" : ".png";
                int pLength = (Format == BitmapFormat.TIF) ? 4 : 8;
                ImageFormat IF = (Format == BitmapFormat.TIF) ? ImageFormat.Tiff : ImageFormat.Png;

                if (!Filename.EndsWith(ext)) Filename += ext;

                if (submap.Type == TextureType.CubeMap)
                {
                    if (Format == BitmapFormat.PNG64) throw new Exception("Cubemaps not supported in 64bpp.");

                    var img = DXTDecoder.DecodeCubeMap(raw, submap, Alpha ? PixelFormat.Format32bppArgb : PixelFormat.Format32bppRgb);
                    if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();
                    img.Save(Filename, ImageFormat.Tiff);
                    return;
                }

                raw = DXTDecoder.DecodeBitmap(raw, submap);

                if (Format == BitmapFormat.PNG64)
                {
                    var br = new BinaryWriter(new MemoryStream());

                    for (int i = 0; i < raw.Length; i++)
                    {
                        if (!Alpha && i % 4 == 3)
                            br.Write((ushort)0xFFFF);
                        else
                            br.Write((ushort)(raw[i] * 257));
                    }

                    br.BaseStream.Position = 0;
                    raw = (new BinaryReader(br.BaseStream)).ReadBytes((int)br.BaseStream.Length);
                }

                PixelFormat PF = (Alpha) ? PixelFormat.Format32bppArgb : PixelFormat.Format32bppRgb;
                if (Format == BitmapFormat.PNG64) PF = PixelFormat.Format64bppArgb;

                Bitmap bitmap2 = new Bitmap(submap.Width, submap.Height, PF);
                Rectangle rect = new Rectangle(0, 0, submap.Width, submap.Height);
                BitmapData bitmapdata = bitmap2.LockBits(rect, ImageLockMode.WriteOnly, PF);
                byte[] destinationArray = new byte[(submap.Width * submap.Height) * pLength];

                for (int j = 0; j < submap.Height; j++)
                    Array.Copy(raw, j * vWidth * pLength, destinationArray, j * submap.Width * pLength, submap.Width * pLength);

                Marshal.Copy(destinationArray, 0, bitmapdata.Scan0, destinationArray.Length);
                bitmap2.UnlockBits(bitmapdata);

                if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();

                bitmap2.Save(Filename, IF);
            }
            else if (Format == BitmapFormat.DDS)
            {
                if (!Filename.EndsWith(".dds")) Filename += ".dds";

                if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();

                var fs = new FileStream(Filename, FileMode.Create, FileAccess.Write);
                var bw = new BinaryWriter(fs);

                if (submap.Flags.Values[3])
                    raw = DXTDecoder.ConvertToLinearTexture(raw, vWidth, vHeight, submap.Format);

                if (submap.Format != TextureFormat.A8R8G8B8)
                    for (int i = 0; i < raw.Length; i += 2)
                        Array.Reverse(raw, i, 2);
                else
                    for (int i = 0; i < (raw.Length); i += 4)
                        Array.Reverse(raw, i, 4);

                new DDS(submap).Write(bw);
                bw.Write(raw);

                bw.Close();
                bw.Dispose();
            }
            else if (Format == BitmapFormat.RAW)
            {
                if (!Filename.EndsWith(".bin")) Filename += ".bin";

                if (!Directory.GetParent(Filename).Exists) Directory.GetParent(Filename).Create();

                File.WriteAllBytes(Filename, raw);
            }
            else 
                throw new InvalidOperationException("Invalid BitmapFormat received.");
        }
        #endregion

        #region SaveAllImages
        /// <summary>
        /// Saves all images from a bitmap tag to disk.
        /// </summary>
        /// <param name="Filename">The full path and filename of the first bitmap to save. All subsequent images will be named accordingly.</param>
        /// <param name="Cache">The CacheFile containing the bitmap tag.</param>
        /// <param name="Tag">The bitmap tag.</param>
        /// <param name="Format">The format to save the images in.</param>
        /// <param name="Alpha">Whether to include the alpha channel in the images. Only applies when saving in TIF format.</param>
        public static void SaveAllImages(string Filename, CacheFile Cache, CacheFile.IndexItem Tag, BitmapFormat Format, bool Alpha)
        {
            var bitm = DefinitionsManager.bitm(Cache, Tag);
            SaveAllImages(Filename, Cache, bitm, Format, Alpha);
        }

        /// <summary>
        /// Saves all images from a bitmap tag to disk.
        /// </summary>
        /// <param name="Filename">The full path and filename of the first bitmap to save. All subsequent images will be named accordingly.</param>
        /// <param name="Cache">The CacheFile containing the bitmap tag.</param>
        /// <param name="Tag">The bitmap tag.</param>
        /// <param name="Format">The format to save the images in.</param>
        /// <param name="Alpha">Whether to include the alpha channel in the images. Only applies when saving in TIF format.</param>
        public static void SaveAllImages(string Filename, CacheFile Cache, bitmap bitm, BitmapFormat Format, bool Alpha)
        {
            string ext;
            switch (Format)
            {
                case BitmapFormat.TIF:
                    ext = ".tif";
                    break;

                case BitmapFormat.DDS:
                    ext = ".dds";
                    break;

                case BitmapFormat.RAW:
                    ext = ".bin";
                    break;

                case BitmapFormat.PNG64:
                    ext = ".png";
                    break;

                default:
                    throw new InvalidOperationException("Invalid BitmapFormat received.");
            }

            if (Filename.EndsWith(ext))
                Filename = Filename.Substring(0, Filename.LastIndexOf('.'));

            for (int i = 0; i < bitm.Bitmaps.Count; i++)
                SaveImage(((i == 0) ? Filename + ext : Filename + " [" + i.ToString() + "]" + ext), Cache, bitm, i, Format, Alpha);
        }
        #endregion

        #endregion
        #endregion

        #region Events
        private void lstBitmaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstBitmaps.FocusedItem == null) return;

            var img = lstBitmaps.FocusedItem.Tag as Bitmap;
            picImage.Image = chkAlpha.Checked ? img : img.Clone(new Rectangle(0, 0, img.Width, img.Height), PixelFormat.Format32bppRgb);
            isWorking = true;
            cmbFormat.SelectedIndex = (int)bitm.Bitmaps[lstBitmaps.FocusedItem.Index].Format;
            isWorking = false;
            UpdateBoxSize();
        }

        private void chkAlpha_CheckedChanged(object sender, EventArgs e)
        {
            lstBitmaps_SelectedIndexChanged(null, null);
        }

        private void cmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isWorking) return;

            try
            {
                bitm.Bitmaps[lstBitmaps.FocusedItem.Index].Format = (TextureFormat)cmbFormat.SelectedIndex;
                lstBitmaps.FocusedItem.Tag = GetBitmapByTag(cache, bitm, lstBitmaps.FocusedItem.Index, PixelFormat.Format32bppArgb);
                lstBitmaps_SelectedIndexChanged(null, null);
            }
            catch
            {
                lstBitmaps.FocusedItem.Tag = GetErrorImage();
                lstBitmaps_SelectedIndexChanged(null, null);
            }
        }

        private void exportThisImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var index = lstBitmaps.FocusedItem.Index;
            var fName = tag.Filename.Substring(tag.Filename.LastIndexOf('\\') + 1);
            fName = (index == 0) ? fName : fName + " [" + index.ToString() + "]";

            var sfd = new SaveFileDialog()
            {
                FileName = fName,
                Filter = "TIF Files|*.tif|DDS Files|*.dds|Raw Data|*.bin|64bpp PNG Files|*.png",
                FilterIndex = (int)DefaultBitmFormat + 1
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                SaveImage(sfd.FileName, cache, bitm, index, (BitmapFormat)(sfd.FilterIndex - 1), chkAlpha.Checked);
                TagExtracted(this, tag);
            }
            catch (Exception ex) { ErrorExtracting(this, tag, ex); }
        }

        private void exportAllImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog()
            {
                FileName = tag.Filename.Substring(tag.Filename.LastIndexOf('\\') + 1),
                Filter = "TIF Files|*.tif|DDS Files|*.dds|Raw Data|*.bin|64bpp PNG Files|*.png",
                FilterIndex = (int)DefaultBitmFormat + 1
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                SaveAllImages(sfd.FileName, cache, bitm, (BitmapFormat)(sfd.FilterIndex - 1), chkAlpha.Checked);
                TagExtracted(this, tag);
            }
            catch (Exception ex) { ErrorExtracting(this, tag, ex); }
        }

        private void picImage_Click(object sender, EventArgs e)
        {
            picImage.BackColor = Color.FromArgb(0xFFFFFF - picImage.BackColor.ToArgb());
        }
        #endregion

        public event ErrorExtractingEventHandler ErrorExtracting;
        public event TagExtractedEventHandler TagExtracted;
    }
}

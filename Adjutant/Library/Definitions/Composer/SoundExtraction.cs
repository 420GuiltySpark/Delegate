using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Composer.IO;
using Composer.Wwise;
using Adjutant.Library.Endian;

namespace Composer
{
    /// <summary>
    /// Provides methods for extracting and converting Wwise sound files.
    /// </summary>
    public static class SoundExtraction
    {
        /// <summary>
        /// Extracts the raw contents of a sound to a file.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <param name="offset">The offset of the data to extract.</param>
        /// <param name="size">The size of the data to extract.</param>
        /// <param name="outPath">The path of the file to save to.</param>
        public static void ExtractRaw(EndianReader reader, int offset, int size, string outPath)
        {
            using (EndianWriter output = new EndianWriter(File.OpenWrite(outPath), EndianFormat.BigEndian))
            {
                // Just copy the data over to the output stream
                reader.SeekTo(offset);
                StreamUtil.Copy(reader, output, size);
            }
        }

        /// <summary>
        /// Extracts an XMA sound and converts it to a WAV.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <param name="offset">The offset of the data to extract.</param>
        /// <param name="rifx">The RIFX data for the sound.</param>
        /// <param name="outPath">The path of the file to save to.</param>
        public static void ExtractXMAToWAV(EndianReader reader, int offset, RIFX rifx, string outPath)
        {
            // Create a temporary file to write an XMA to
            string tempFile = Path.GetTempFileName();

            try
            {
                using (EndianWriter output = new EndianWriter(File.OpenWrite(tempFile), EndianFormat.BigEndian))
                {
                    // Generate an XMA header
                    // ADAPTED FROM wwisexmabank - I DO NOT TAKE ANY CREDIT WHATSOEVER FOR THE FOLLOWING CODE.
                    // See http://hcs64.com/vgm_ripping.html for more information
                    output.Write(0x52494646); // 'RIFF'
                    output.EndianType = EndianFormat.LittleEndian;
                    output.Write(rifx.DataSize + 0x34);
                    output.EndianType = EndianFormat.BigEndian;
                    output.Write(RIFFFormat.WAVE);

                    // Generate the 'fmt ' chunk
                    output.Write(0x666D7420); // 'fmt '
                    output.EndianType = EndianFormat.LittleEndian;
                    output.Write(0x20);
                    output.Write((short)0x165); // WAVE_FORMAT_XMA
                    output.Write((short)16);    // 16 bits per sample
                    output.Write((short)0);     // encode options **
                    output.Write((short)0);     // largest skip
                    output.Write((short)1);     // # streams
                    output.Write((byte)0);      // loops
                    output.Write((byte)3);      // encoder version
                    output.Write(0);            // bytes per second **
                    output.Write(rifx.SampleRate); // sample rate
                    output.Write(0);            // loop start
                    output.Write(0);            // loop end
                    output.Write((byte)0);      // subframe loop data
                    output.Write((byte)rifx.ChannelCount); // channels
                    output.Write((short)0x0002);// channel mask

                    // 'data' chunk
                    output.EndianType = EndianFormat.BigEndian;
                    output.Write(0x64617461); // 'data'
                    output.EndianType = EndianFormat.LittleEndian;
                    output.Write(rifx.DataSize);

                    // Copy the data chunk contents from the original RIFX
                    reader.SeekTo(offset + rifx.DataOffset);
                    StreamUtil.Copy(reader, output, rifx.DataSize);

                    // END ADAPTED CODE
                }

                // Convert it with towav
                RunProgramSilently("Helpers/towav.exe", "\"" + Path.GetFileName(tempFile) + "\"", Path.GetDirectoryName(tempFile));

                // Move the WAV to the destination path
                if (File.Exists(outPath))
                    File.Delete(outPath);
                File.Move(Path.ChangeExtension(tempFile, "wav"), outPath);
            }
            finally
            {
                // Delete the temporary XMA file
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Extracts an xWMA sound and converts it to WAV.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <param name="offset">The offset of the data to extract.</param>
        /// <param name="rifx">The RIFX data for the sound.</param>
        /// <param name="outPath">The path of the file to save to.</param>
        public static void ExtractXWMAToWAV(EndianReader reader, int offset, RIFX rifx, string outPath)
        {
            // Create a temporary file to write an XWMA to
            string tempFile = Path.GetTempFileName();

            try
            {
                using (EndianWriter output = new EndianWriter(File.OpenWrite(tempFile), EndianFormat.BigEndian))
                {
                    // Generate a little-endian XWMA header
                    // TODO: move this into a class?
                    output.Write(0x52494646); // 'RIFF'

                    // Recompute the file size because the one Wwise gives us is trash
                    // fileSize = header size (always 0x2C) + dpds data size + data header size (always 0x8) + data size
                    int fileSize = 0x2C + rifx.SeekOffsets.Length * 0x4 + 0x8 + rifx.DataSize;
                    output.EndianType = EndianFormat.LittleEndian;
                    output.Write(fileSize);

                    output.EndianType = EndianFormat.BigEndian;
                    output.Write(RIFFFormat.XWMA);

                    // Generate the 'fmt ' chunk
                    output.Write(0x666D7420); // 'fmt '
                    output.EndianType = EndianFormat.LittleEndian;
                    output.Write(0x18); // Chunk size
                    output.Write(rifx.Codec);
                    output.Write(rifx.ChannelCount);
                    output.Write(rifx.SampleRate);
                    output.Write(rifx.BytesPerSecond);
                    output.Write(rifx.BlockAlign);
                    output.Write(rifx.BitsPerSample);

                    // Write the extradata
                    // Bytes 4 and 5 have to be flipped because they make up an int16
                    // TODO: add error checking to make sure the extradata is the correct size (0x6)
                    output.Write((short)0x6);
                    output.WriteBlock(rifx.ExtraData, 0, 4);
                    output.Write(rifx.ExtraData[5]);
                    output.Write(rifx.ExtraData[4]);

                    // Generate the 'dpds' chunk
                    // It's really just the 'seek' chunk from the original data but renamed
                    output.EndianType = EndianFormat.BigEndian;
                    output.Write(0x64706473); // 'dpds'

                    output.EndianType = EndianFormat.LittleEndian;
                    output.Write(rifx.SeekOffsets.Length * 4); // One uint32 per offset
                    foreach (int seek in rifx.SeekOffsets)
                        output.Write(seek);

                    // 'data' chunk
                    output.EndianType = EndianFormat.BigEndian;
                    output.Write(0x64617461); // 'data'
                    output.EndianType = EndianFormat.LittleEndian;
                    output.Write(rifx.DataSize);

                    // Copy the data chunk contents from the original RIFX
                    reader.SeekTo(offset + rifx.DataOffset);
                    StreamUtil.Copy(reader, output, rifx.DataSize);
                }

                // Convert it with xWMAEncode
                RunProgramSilently("Helpers/xWMAEncode.exe", "\"" + tempFile + "\" \"" + outPath + "\"", Directory.GetCurrentDirectory());
            }
            finally
            {
                // Delete the temporary XWMA file
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Extracts an xWMA sound and converts it to another format.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <param name="offset">The offset of the data to extract.</param>
        /// <param name="rifx">The RIFX data for the sound.</param>
        /// <param name="outPath">The path of the file to save to. The file extension will determine the output format.</param>
        public static void ExtractAndConvertXWMA(EndianReader reader, int offset, RIFX rifx, string outPath)
        {
            // Extract a WAV to a temporary file and then convert it
            string tempPath = Path.GetTempFileName();
            try
            {
                ExtractXWMAToWAV(reader, offset, rifx, tempPath);
                ConvertFile(tempPath, outPath);
            }
            finally
            {
                // Delete the temporary file
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        /// <summary>
        /// Extracts a Wwise OGG and converts it to a "regular" OGG file.
        /// </summary>
        /// <param name="reader">The stream to read from.</param>
        /// <param name="offset">The offset of the data to extract.</param>
        /// <param name="size">The size of the data to extract.</param>
        /// <param name="outPath">The path of the file to save to.</param>
        public static void ExtractWwiseToOGG(EndianReader reader, int offset, int size, string outPath)
        {
            // Just extract the RIFX to a temporary file
            string tempFile = Path.GetTempFileName();

            try
            {
                using (EndianWriter output = new EndianWriter(File.OpenWrite(tempFile), EndianFormat.BigEndian))
                {
                    reader.SeekTo(offset);
                    StreamUtil.Copy(reader, output, size);
                }

                // Run ww2ogg to convert the resulting RIFX to an OGG
                RunProgramSilently("Helpers/ww2ogg.exe",
                    string.Format("\"{0}\" -o \"{1}\" --pcb Helpers/packed_codebooks_aoTuV_603.bin", tempFile, outPath),
                    Directory.GetCurrentDirectory());

                // Run revorb to fix up the OGG
                RunProgramSilently("Helpers/revorb.exe", "\"" + outPath + "\"", Directory.GetCurrentDirectory());
            }
            finally
            {
                // Delete the old RIFX file
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Converts an audio file from one format to another.
        /// The output format is determined based upon the file extension.
        /// </summary>
        /// <param name="wavPath">The path of the audio file to convert.</param>
        /// <param name="outPath">The path of the file to save to.</param>
        public static void ConvertFile(string wavPath, string outPath)
        {
            RunProgramSilently("Helpers/ffmpeg.exe",
                string.Format("-y -v quiet -i \"{0}\" \"{1}\"", wavPath, outPath),
                Directory.GetCurrentDirectory());
        }

        /// <summary>
        /// Silently executes a program and waits for it to finish.
        /// </summary>
        /// <param name="path">The path to the program to execute.</param>
        /// <param name="arguments">Command-line arguments to pass to the program.</param>
        /// <param name="workingDirectory">The working directory to run in the program in.</param>
        private static void RunProgramSilently(string path, string arguments, string workingDirectory)
        {
            ProcessStartInfo info = new ProcessStartInfo(path, arguments);
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.UseShellExecute = false;
            info.WorkingDirectory = workingDirectory;

            Process proc = Process.Start(info);
            proc.WaitForExit();

            string output = proc.StandardError.ReadToEnd();
        }
    }
}

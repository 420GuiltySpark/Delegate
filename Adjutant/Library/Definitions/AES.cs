using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Adjutant.Library.Endian;
using System.IO;

namespace Adjutant.Library.Definitions
{
    public static class AES
    {
        /// <summary>
        /// Decrypts a segment of an EndianReader stream.
        /// </summary>
        /// <param name="Reader">The EndianReader to read from.</param>
        /// <param name="StartPosition">The stream position to decrypt from.</param>
        /// <param name="Length">The number of bytes to decrypt.</param>
        /// <param name="Key">The decryption key as a string.</param>
        /// <returns>A new EndianReader stream containing only the decrypted segment.</returns>
        public static EndianReader DecryptSegment(EndianReader Reader, int StartPosition, int Length, string Key)
        {
            Reader.SeekTo(StartPosition);
            if (Length % 16 != 0) Length += 16 - Length % 16;
            byte[] data = Reader.ReadBytes(Length);
            byte[] bKey = System.Text.Encoding.ASCII.GetBytes(Key);
            byte[] XOR = new byte[bKey.Length];
            byte[] IV = new byte[bKey.Length];

            for (int i = 0; i < bKey.Length; i++)
            {
                XOR[i] = (byte)(bKey[i] ^ 0xFFA5);
                IV[i] = (byte)(XOR[i] ^ 0x3C);
            }

            var aes = new AesManaged()
            {
                Mode = CipherMode.CBC,
                Key = XOR,
                IV = IV,
                Padding = PaddingMode.Zeros
            };

            Stream stream = (Stream)new MemoryStream(aes.CreateDecryptor(aes.Key, aes.IV).TransformFinalBlock(data, 0, data.Length));
            return new EndianReader(stream, EndianFormat.BigEndian);

        }
    }
}

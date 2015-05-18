using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Adjutant.Library.Endian
{
    public class EndianReader : BinaryReader
    {
        public EndianFormat EndianType;

        /// <summary>
        /// Creates a new instance of the EndianReader class.
        /// </summary>
        /// <param name="Stream">The Stream to read from.</param>
        /// <param name="Type">The default EndianFormat the EndianReader will use.</param>
        public EndianReader(Stream Stream, EndianFormat Type)
            : base(Stream)
        {
            EndianType = Type;
        }

        #region Param-less Overrides
        /// <summary>
        /// Reads a Double value in the EndianReader's default EndianFormat.
        /// </summary>
        /// <returns></returns>
        public override double ReadDouble()
        {
            return ReadDouble(EndianType);
        }

        /// <summary>
        /// Reads an Int16 value in the EndianReader's default EndianFormat.
        /// </summary>
        /// <returns></returns>
        public override short ReadInt16()
        {
            return ReadInt16(EndianType);
        }

        /// <summary>
        /// Reads an Int32 value in the EndianReader's default EndianFormat.
        /// </summary>
        /// <returns></returns>
        public override int ReadInt32()
        {
            return ReadInt32(EndianType);
        }

        /// <summary>
        /// Reads an Int64 value in the EndianReader's default EndianFormat.
        /// </summary>
        /// <returns></returns>
        public override long ReadInt64()
        {
            return ReadInt64(EndianType);
        }

        /// <summary>
        /// Reads a Single value in the EndianReader's default EndianFormat.
        /// </summary>
        /// <returns></returns>
        public override float ReadSingle()
        {
            return ReadSingle(EndianType);
        }

        /// <summary>
        /// Reads a UInt16 value in the EndianReader's default EndianFormat.
        /// </summary>
        /// <returns></returns>
        public override ushort ReadUInt16()
        {
            return ReadUInt16(EndianType);
        }

        /// <summary>
        /// Reads a UInt32 value in the EndianReader's default EndianFormat.
        /// </summary>
        /// <returns></returns>
        public override uint ReadUInt32()
        {
            return ReadUInt32(EndianType);
        }

        /// <summary>
        /// Reads a UInt64 value in the EndianReader's default EndianFormat.
        /// </summary>
        /// <returns></returns>
        public override ulong ReadUInt64()
        {
            return ReadUInt64(EndianType);
        }
        #endregion

        #region EndianFormat Overloads
        /// <summary>
        /// Reads a Double value in the specified EndianFormat.
        /// </summary>
        /// <param name="Type">The EndianFormat of the value.</param>
        /// <returns></returns>
        public double ReadDouble(EndianFormat Type)
        {
            if (Type == EndianFormat.LittleEndian)
                return base.ReadDouble();

            byte[] bytes = base.ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        /// Reads an Int16 value in the specified EndianFormat.
        /// </summary>
        /// <param name="Type">The EndianFormat of the value.</param>
        /// <returns></returns>
        public short ReadInt16(EndianFormat Type)
        {
            if (Type == EndianFormat.LittleEndian)
                return base.ReadInt16();

            byte[] bytes = base.ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Reads an Int32 value in the specified EndianFormat.
        /// </summary>
        /// <param name="Type">The EndianFormat of the value.</param>
        /// <returns></returns>
        public int ReadInt32(EndianFormat Type)
        {
            if (Type == EndianFormat.LittleEndian)
                return base.ReadInt32();

            byte[] bytes = base.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Reads an Int64 value in the specified EndianFormat.
        /// </summary>
        /// <param name="Type">The EndianFormat of the value.</param>
        /// <returns></returns>
        public long ReadInt64(EndianFormat Type)
        {
            if (Type == EndianFormat.LittleEndian)
                return base.ReadInt64();

            byte[] bytes = base.ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        /// Reads a Single value in the specified EndianFormat.
        /// </summary>
        /// <param name="Type">The EndianFormat of the value.</param>
        /// <returns></returns>
        public float ReadSingle(EndianFormat Type)
        {
            if (Type == EndianFormat.LittleEndian)
                return base.ReadSingle();

            byte[] bytes = base.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Reads a UInt16 value in the specified EndianFormat.
        /// </summary>
        /// <param name="Type">The EndianFormat of the value.</param>
        /// <returns></returns>
        public ushort ReadUInt16(EndianFormat Type)
        {
            if (Type == EndianFormat.LittleEndian)
                return base.ReadUInt16();

            byte[] bytes = base.ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// Reads a UInt32 value in the specified EndianFormat.
        /// </summary>
        /// <param name="Type">The EndianFormat of the value.</param>
        /// <returns></returns>
        public uint ReadUInt32(EndianFormat Type)
        {
            if (Type == EndianFormat.LittleEndian)
                return base.ReadUInt32();

            byte[] bytes = base.ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Reads a UInt64 value in the specified EndianFormat.
        /// </summary>
        /// <param name="Type">The EndianFormat of the value.</param>
        /// <returns></returns>
        public ulong ReadUInt64(EndianFormat Type)
        {
            if (Type == EndianFormat.LittleEndian)
                return base.ReadUInt64();

            byte[] bytes = base.ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }
        #endregion

        #region Read String
        /// <summary>
        /// Reads a UTF8 string of specified length.
        /// </summary>
        /// <param name="Length">The number of characters to read into the string.</param>
        /// <param name="Trim">Weather to trim white-space from the string. Defaults to true.</param>
        /// <returns></returns>
        public string ReadString(int Length, bool Trim = true)
        {
            string str = Encoding.UTF8.GetString(ReadBytes(Length));

            if (Trim)
                str = str.Trim().Replace("\0", "");

            return str;
        }

        /// <summary>
        /// Reads a null-terminated UTF8 string of indefinite length.
        /// </summary>
        /// <returns></returns>
        public string ReadNullTerminatedString()
        {
            var bytes = new List<byte>();
            byte b;
            while ((b = ReadByte()) != 0)
                bytes.Add(b);

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Reads a null-terminated UTF8 string of length up to MaxLength and advances the stream position by MaxLength bytes.
        /// </summary>
        /// <param name="MaxLength">The maximum number of characters to read.</param>
        /// <returns></returns>
        public string ReadNullTerminatedString(int MaxLength)
        {
            string str = Encoding.UTF8.GetString(ReadBytes(MaxLength));
            return str.Substring(0, str.IndexOf('\0'));
        }
        #endregion
    }
}

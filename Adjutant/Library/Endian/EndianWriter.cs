using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Adjutant.Library.Endian
{
    public class EndianWriter : BinaryWriter
    {
        public EndianFormat EndianType;

        /// <summary>
        /// Creates a new instance of the EndianWriter class.
        /// </summary>
        /// <param name="Stream">The Stream to write to.</param>
        /// <param name="Type">The default EndianFormat the EndianWriter will use.</param>
        public EndianWriter(Stream Stream, EndianFormat Type)
            : base(Stream)
        {
            EndianType = Type;
        }

        #region Param-less Overrides
        /// <summary>
        /// Writes a Double value in the EndianWriter's default EndianFormat.
        /// </summary>
        public override void Write(double value)
        {
            Write(value, EndianType);
        }

        /// <summary>
        /// Writes a Single value in the EndianWriter's default EndianFormat.
        /// </summary>
        public override void Write(float value)
        {
            Write(value, EndianType);
        }

        /// <summary>
        /// Writes an Int32 value in the EndianWriter's default EndianFormat.
        /// </summary>
        public override void Write(int value)
        {
            Write(value, EndianType);
        }

        /// <summary>
        /// Writes an Int64 value in the EndianWriter's default EndianFormat.
        /// </summary>
        public override void Write(long value)
        {
            Write(value, EndianType);
        }

        /// <summary>
        /// Writes an Int16 value in the EndianWriter's default EndianFormat.
        /// </summary>
        public override void Write(short value)
        {
            Write(value, EndianType);
        }

        /// <summary>
        /// Writes a UInt32 value in the EndianWriter's default EndianFormat.
        /// </summary>
        public override void Write(uint value)
        {
            Write(value, EndianType);
        }

        /// <summary>
        /// Writes a UInt64 value in the EndianWriter's default EndianFormat.
        /// </summary>
        public override void Write(ulong value)
        {
            Write(value, EndianType);
        }

        /// <summary>
        /// Writes a UInt16 value in the EndianWriter's default EndianFormat.
        /// </summary>
        public override void Write(ushort value)
        {
            Write(value, EndianType);
        }
        #endregion

        #region EndianFormat Overloads
        /// <summary>
        /// Writes a Double value in the specified EndianFormat.
        /// </summary>
        /// <param name="value">The value to write to the stream.</param>
        /// <param name="Type">The EndianFormat to write the value in.</param>
        public void Write(double value, EndianFormat Type)
        {
            byte[] bits = BitConverter.GetBytes(value);

            if (Type == EndianFormat.BigEndian)
                Array.Reverse(bits);

            base.Write(bits);
        }

        /// <summary>
        /// Writes a Single value in the specified EndianFormat.
        /// </summary>
        /// <param name="value">The value to write to the stream.</param>
        /// <param name="Type">The EndianFormat to write the value in.</param>
        public void Write(float value, EndianFormat Type)
        {
            byte[] bits = BitConverter.GetBytes(value);

            if (Type == EndianFormat.BigEndian)
                Array.Reverse(bits);

            base.Write(bits);
        }

        /// <summary>
        /// Writes an Int32 value in the specified EndianFormat.
        /// </summary>
        /// <param name="value">The value to write to the stream.</param>
        /// <param name="Type">The EndianFormat to write the value in.</param>
        public void Write(int value, EndianFormat Type)
        {
            byte[] bits = BitConverter.GetBytes(value);

            if (Type == EndianFormat.BigEndian)
                Array.Reverse(bits);

            base.Write(bits);
        }

        /// <summary>
        /// Writes an Int64 value in the specified EndianFormat.
        /// </summary>
        /// <param name="value">The value to write to the stream.</param>
        /// <param name="Type">The EndianFormat to write the value in.</param>
        public void Write(long value, EndianFormat Type)
        {
            byte[] bits = BitConverter.GetBytes(value);

            if (Type == EndianFormat.BigEndian)
                Array.Reverse(bits);

            base.Write(bits);
        }

        /// <summary>
        /// Writes an Int16 value in the specified EndianFormat.
        /// </summary>
        /// <param name="value">The value to write to the stream.</param>
        /// <param name="Type">The EndianFormat to write the value in.</param>
        public void Write(short value, EndianFormat Type)
        {
            byte[] bits = BitConverter.GetBytes(value);

            if (Type == EndianFormat.BigEndian)
                Array.Reverse(bits);

            base.Write(bits);
        }

        /// <summary>
        /// Writes a UInt32 value in the specified EndianFormat.
        /// </summary>
        /// <param name="value">The value to write to the stream.</param>
        /// <param name="Type">The EndianFormat to write the value in.</param>
        public void Write(uint value, EndianFormat Type)
        {
            byte[] bits = BitConverter.GetBytes(value);

            if (Type == EndianFormat.BigEndian)
                Array.Reverse(bits);

            base.Write(bits);
        }

        /// <summary>
        /// Writes a UInt64 value in the specified EndianFormat.
        /// </summary>
        /// <param name="value">The value to write to the stream.</param>
        /// <param name="Type">The EndianFormat to write the value in.</param>
        public void Write(ulong value, EndianFormat Type)
        {
            byte[] bits = BitConverter.GetBytes(value);

            if (Type == EndianFormat.BigEndian)
                Array.Reverse(bits);

            base.Write(bits);
        }

        /// <summary>
        /// Writes a UInt16 value in the specified EndianFormat.
        /// </summary>
        /// <param name="value">The value to write to the stream.</param>
        /// <param name="Type">The EndianFormat to write the value in.</param>
        public void Write(ushort value, EndianFormat Type)
        {
            byte[] bits = BitConverter.GetBytes(value);

            if (Type == EndianFormat.BigEndian)
                Array.Reverse(bits);

            base.Write(bits);
        }
        #endregion

        public void WriteBlock(byte[] data)
        {
            BaseStream.Write(data, 0, data.Length);
        }

        public void WriteBlock(byte[] data, int offset, int length)
        {
            BaseStream.Write(data, offset, length);
        }
    }
}

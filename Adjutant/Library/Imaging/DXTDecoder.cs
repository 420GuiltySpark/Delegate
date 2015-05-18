﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Adjutant.Library.Definitions;
using Adjutant.Library.S3D;

namespace Adjutant.Library.Imaging
{
    /***************************************************************
     * The following code is derived from the HaloDeveloper project
     * created by Anthony and Xenomega. I take no credit for it.
     * 
     * The following are exceptions:
     *     -DecodeDXNMA
     *     -DecodeDXT3A
     *     -DecodeDXT5A
     *     -DecodeCubeMap
     *     -S3D related functions
     *     -Everything in the Swizzle region is from troymac1ure's Entity.
     *     
     * Many edits and additons have been made to the derived code.
     ***************************************************************/

    public static class DXTDecoder
    {

        public static byte[] ConvertFromLinearTexture(byte[] data, int width, int height, TextureFormat texture)
        {
            return ModifyLinearTexture(data, width, height, texture, false);
        }

        public static byte[] ConvertToLinearTexture(byte[] data, int width, int height, TextureFormat texture)
        {
            return ModifyLinearTexture(data, width, height, texture, true);
        }

        #region Format Decoding
        private static byte[] DecodeP8(byte[] data, int width, int height)
        {
            var buffer = new byte[data.Length * 4];
            for (int i = 0; i < data.Length; i++)
            {
                buffer[i * 4 + 0] = data[i];
                buffer[i * 4 + 1] = data[i];
                buffer[i * 4 + 2] = data[i];
                buffer[i * 4 + 3] = 255;
            }
            return buffer;
        }

        private static byte[] DecodeA8R8G8B8(byte[] data, int width, int height)
        {
            for (int i = 0; i < (data.Length); i += 4)
                Array.Reverse(data, i, 4);

            return data;
        }

        private static byte[] DecodeA1R5G5B5(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[width * height * 4];
            for (int i = 0; i < (width * height * 2); i += 2)
            {
                short temp = (short)(data[i] | (data[i + 1] << 8));
                buffer[i * 2] = (byte)(temp & 0x1F);
                buffer[i * 2 + 1] = (byte)((temp >> 5) & 0x3F);
                buffer[i * 2 + 2] = (byte)((temp >> 11) & 0x1F);
                buffer[i * 2 + 3] = 0xFF;
            }
            return buffer;
        }

        private static byte[] DecodeA4R4G4B4(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[width * height * 4];
            for (int i = 0; i < (width * height * 2); i += 2)
            {
                buffer[i * 2 + 3] = (byte)(data[i] & 0xF0);
                buffer[i * 2 + 2] = (byte)(data[i] & 0x0F);
                buffer[i * 2 + 1] = (byte)(data[i + 1] & 0xF0);
                buffer[i * 2] = (byte)(data[i + 1] & 0x0F);
            }
            return buffer;
        }

        private static byte[] DecodeA8(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[height * width * 4];
            for (int i = 0; i < (height * width); i++)
            {
                int index = i * 4;
                buffer[index] = 0xFF;
                buffer[index + 1] = 0xFF;
                buffer[index + 2] = 0xFF;
                buffer[index + 3] = data[i];
            }

            return buffer;
        }

        private static byte[] DecodeA8Y8(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[height * width * 4];
            for (int i = 0; i < (height * width * 2); i += 2)
            {
                buffer[i * 2] = data[i + 1];
                buffer[i * 2 + 1] = data[i + 1];
                buffer[i * 2 + 2] = data[i + 1];
                buffer[i * 2 + 3] = data[i];
            }
            return buffer;
        }

        private static byte[] DecodeAY8(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[height * width * 4];
            for (int i = 0; i < height * width; i++)
            {
                int index = i * 4;
                buffer[index] = data[i];
                buffer[index + 1] = data[i];
                buffer[index + 2] = data[i];
                buffer[index + 3] = data[i];
            }
            return buffer;
        }

        private static byte[] DecodeCTX1(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[width * height * 4];
            int index = 0;

            for (int i = 0; i < (width * height); i += 16)
            {
                int c1 = (data[index + 1] << 8) | data[index];
                int c2 = (data[index + 3] << 8) | data[index + 2];

                RGBAColor[] colorArray = new RGBAColor[4];
                colorArray[0].R = data[index];
                colorArray[0].G = data[index + 1];
                colorArray[1].R = data[index + 2];
                colorArray[1].G = data[index + 3];

                if (c1 > c2)
                {
                    colorArray[2] = GradientColors(colorArray[0], colorArray[1]);
                    colorArray[3] = GradientColors(colorArray[1], colorArray[0]);
                }
                else
                {
                    colorArray[2] = GradientColorsHalf(colorArray[0], colorArray[1]);
                    colorArray[3] = colorArray[0];
                }

                int cData = ((data[index + 5] | (data[index + 4] << 8)) | (data[index + 7] << 0x10)) | (data[index + 6] << 0x18);
                int chunkNum = i / 16;

                int xPos = chunkNum % (width / 4);
                int yPos = (chunkNum - xPos) / (width / 4);

                int sizeh = (height < 4) ? height : 4;
                int sizew = (width < 4) ? width : 4;

                for (int j = 0; j < sizeh; j++)
                {
                    for (int k = 0; k < sizew; k++)
                    {
                        RGBAColor color = colorArray[cData & 3];
                        cData = cData >> 2;
                        int temp = (((((yPos * 4) + j) * width) + (xPos * 4)) + k) * 4;
                        float x = ((((float)color.R) / 255f) * 2f) - 1f;
                        float y = ((((float)color.G) / 255f) * 2f) - 1f;
                        float z = (float)Math.Sqrt((double)Math.Max(0f, Math.Min((float)1f, (float)((1f - (x * x)) - (y * y)))));
                        buffer[temp] = (byte)(((z + 1f) / 2f) * 255f);
                        buffer[temp + 1] = (byte)color.G;
                        buffer[temp + 2] = (byte)color.R;
                        buffer[temp + 3] = 0xFF;
                    }
                }
                index += 8;
            }
            return buffer;
        }

        private static byte[] DecodeDXN(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[height * width * 4];
            int chunks = width / 4;

            if (chunks == 0)
                chunks = 1;

            for (int i = 0; i < (width * height); i += 16)
            {
                byte rMin = data[i + 1];
                byte rMax = data[i];
                byte[] rIndices = new byte[16];
                int temp = ((data[i + 5] << 16) | (data[i + 2] << 8)) | data[i + 3];
                int indices = 0;
                while (indices < 8)
                {
                    rIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                temp = ((data[i + 6] << 16) | (data[i + 7] << 8)) | data[i + 4];
                while (indices < 16)
                {
                    rIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                byte gMin = data[i + 9];
                byte gMax = data[i + 8];
                byte[] gIndices = new byte[16];
                temp = ((data[i + 13] << 16) | (data[i + 10] << 8)) | data[i + 11];
                indices = 0;
                while (indices < 8)
                {
                    gIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                temp = ((data[i + 14] << 16) | (data[i + 15] << 8)) | data[i + 12];
                while (indices < 16)
                {
                    gIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                byte[] redTable = new byte[8];
                redTable[0] = rMin;
                redTable[1] = rMax;
                if (redTable[0] > redTable[1])
                {
                    redTable[2] = (byte)((6 * redTable[0] + 1 * redTable[1]) / 7.0f);
                    redTable[3] = (byte)((5 * redTable[0] + 2 * redTable[1]) / 7.0f);
                    redTable[4] = (byte)((4 * redTable[0] + 3 * redTable[1]) / 7.0f);
                    redTable[5] = (byte)((3 * redTable[0] + 4 * redTable[1]) / 7.0f);
                    redTable[6] = (byte)((2 * redTable[0] + 5 * redTable[1]) / 7.0f);
                    redTable[7] = (byte)((1 * redTable[0] + 6 * redTable[1]) / 7.0f);
                }
                else
                {
                    redTable[2] = (byte)((4 * redTable[0] + 1 * redTable[1]) / 5.0f);
                    redTable[3] = (byte)((3 * redTable[0] + 2 * redTable[1]) / 5.0f);
                    redTable[4] = (byte)((2 * redTable[0] + 3 * redTable[1]) / 5.0f);
                    redTable[5] = (byte)((1 * redTable[0] + 4 * redTable[1]) / 5.0f);
                    redTable[6] = (byte)0;
                    redTable[7] = (byte)255;
                }
                byte[] grnTable = new byte[8];
                grnTable[0] = gMin;
                grnTable[1] = gMax;
                if (grnTable[0] > grnTable[1])
                {
                    grnTable[2] = (byte)((6 * grnTable[0] + 1 * grnTable[1]) / 7.0f);
                    grnTable[3] = (byte)((5 * grnTable[0] + 2 * grnTable[1]) / 7.0f);
                    grnTable[4] = (byte)((4 * grnTable[0] + 3 * grnTable[1]) / 7.0f);
                    grnTable[5] = (byte)((3 * grnTable[0] + 4 * grnTable[1]) / 7.0f);
                    grnTable[6] = (byte)((2 * grnTable[0] + 5 * grnTable[1]) / 7.0f);
                    grnTable[7] = (byte)((1 * grnTable[0] + 6 * grnTable[1]) / 7.0f);
                }
                else
                {
                    grnTable[2] = (byte)((4 * grnTable[0] + 1 * grnTable[1]) / 5.0f);
                    grnTable[3] = (byte)((3 * grnTable[0] + 2 * grnTable[1]) / 5.0f);
                    grnTable[4] = (byte)((2 * grnTable[0] + 3 * grnTable[1]) / 5.0f);
                    grnTable[5] = (byte)((1 * grnTable[0] + 4 * grnTable[1]) / 5.0f);
                    grnTable[6] = (byte)0;
                    grnTable[7] = (byte)255;
                }
                int chunkNum = i / 16;
                int xPos = chunkNum % chunks;
                int yPos = (chunkNum - xPos) / chunks;
                int sizeh = (height < 4) ? height : 4;
                int sizew = (width < 4) ? width : 4;
                for (int j = 0; j < sizeh; j++)
                {
                    for (int k = 0; k < sizew; k++)
                    {
                        RGBAColor color;
                        color.R = redTable[rIndices[(j * sizeh) + k]];
                        color.G = grnTable[gIndices[(j * sizeh) + k]];
                        float x = ((((float)color.R) / 255f) * 2f) - 1f;
                        float y = ((((float)color.G) / 255f) * 2f) - 1f;
                        float z = (float)Math.Sqrt((double)Math.Max(0f, Math.Min(1f, (1f - (x * x)) - (y * y))));
                        color.B = (byte)(((z + 1f) / 2f) * 255f);
                        color.A = 0xFF;
                        temp = (((((yPos * 4) + j) * width) + (xPos * 4)) + k) * 4;
                        buffer[temp] = (byte)color.B;
                        buffer[temp + 1] = (byte)color.G;
                        buffer[temp + 2] = (byte)color.R;
                        buffer[temp + 3] = (byte)color.A;
                    }
                }
            }
            return buffer;
        }

        private static byte[] DecodeDXNMA(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[height * width * 4];
            int chunks = width / 4;

            if (chunks == 0)
                chunks = 1;

            for (int i = 0; i < (width * height); i += 16)
            {
                byte mMin = data[i + 1];
                byte mMax = data[i];
                byte[] mIndices = new byte[16];
                int temp = ((data[i + 5] << 0x10) | (data[i + 2] << 8)) | data[i + 3];
                int indices = 0;
                while (indices < 8)
                {
                    mIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                temp = ((data[i + 6] << 0x10) | (data[i + 7] << 8)) | data[i + 4];
                while (indices < 16)
                {
                    mIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                byte aMin = data[i + 9];
                byte aMax = data[i + 8];
                byte[] aIndices = new byte[16];
                temp = ((data[i + 13] << 0x10) | (data[i + 10] << 8)) | data[i + 11];
                indices = 0;
                while (indices < 8)
                {
                    aIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                temp = ((data[i + 14] << 0x10) | (data[i + 15] << 8)) | data[i + 12];
                while (indices < 16)
                {
                    aIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                byte[] monoTable = new byte[8];
                monoTable[0] = mMin;
                monoTable[1] = mMax;
                if (monoTable[0] > monoTable[1])
                {
                    monoTable[2] = (byte)((6 * monoTable[0] + 1 * monoTable[1]) / 7.0f);
                    monoTable[3] = (byte)((5 * monoTable[0] + 2 * monoTable[1]) / 7.0f);
                    monoTable[4] = (byte)((4 * monoTable[0] + 3 * monoTable[1]) / 7.0f);
                    monoTable[5] = (byte)((3 * monoTable[0] + 4 * monoTable[1]) / 7.0f);
                    monoTable[6] = (byte)((2 * monoTable[0] + 5 * monoTable[1]) / 7.0f);
                    monoTable[7] = (byte)((1 * monoTable[0] + 6 * monoTable[1]) / 7.0f);
                }
                else
                {
                    monoTable[2] = (byte)((4 * monoTable[0] + 1 * monoTable[1]) / 5.0f);
                    monoTable[3] = (byte)((3 * monoTable[0] + 2 * monoTable[1]) / 5.0f);
                    monoTable[4] = (byte)((2 * monoTable[0] + 3 * monoTable[1]) / 5.0f);
                    monoTable[5] = (byte)((1 * monoTable[0] + 4 * monoTable[1]) / 5.0f);
                    monoTable[6] = (byte)0;
                    monoTable[7] = (byte)255;
                }
                byte[] alphaTable = new byte[8];
                alphaTable[0] = aMin;
                alphaTable[1] = aMax;
                if (alphaTable[0] > alphaTable[1])
                {
                    alphaTable[2] = (byte)((6 * alphaTable[0] + 1 * alphaTable[1]) / 7.0f);
                    alphaTable[3] = (byte)((5 * alphaTable[0] + 2 * alphaTable[1]) / 7.0f);
                    alphaTable[4] = (byte)((4 * alphaTable[0] + 3 * alphaTable[1]) / 7.0f);
                    alphaTable[5] = (byte)((3 * alphaTable[0] + 4 * alphaTable[1]) / 7.0f);
                    alphaTable[6] = (byte)((2 * alphaTable[0] + 5 * alphaTable[1]) / 7.0f);
                    alphaTable[7] = (byte)((1 * alphaTable[0] + 6 * alphaTable[1]) / 7.0f);
                }
                else
                {
                    alphaTable[2] = (byte)((4 * alphaTable[0] + 1 * alphaTable[1]) / 5.0f);
                    alphaTable[3] = (byte)((3 * alphaTable[0] + 2 * alphaTable[1]) / 5.0f);
                    alphaTable[4] = (byte)((2 * alphaTable[0] + 3 * alphaTable[1]) / 5.0f);
                    alphaTable[5] = (byte)((1 * alphaTable[0] + 4 * alphaTable[1]) / 5.0f);
                    alphaTable[6] = (byte)0;
                    alphaTable[7] = (byte)255;
                }
                int chunkNum = i / 16;
                int xPos = chunkNum % chunks;
                int yPos = (chunkNum - xPos) / chunks;
                int sizeh = (height < 4) ? height : 4;
                int sizew = (width < 4) ? width : 4;
                for (int j = 0; j < sizeh; j++)
                {
                    for (int k = 0; k < sizew; k++)
                    {
                        RGBAColor color;
                        color.B = color.G = color.R = monoTable[mIndices[(j * sizeh) + k]];
                        color.A = alphaTable[aIndices[(j * sizeh) + k]];
                        temp = (((((yPos * 4) + j) * width) + (xPos * 4)) + k) * 4;
                        buffer[temp] = (byte)color.B;
                        buffer[temp + 1] = (byte)color.G;
                        buffer[temp + 2] = (byte)color.R;
                        buffer[temp + 3] = (byte)color.A;
                    }
                }
            }
            return buffer;
        }

        private static byte[] DecodeDXT1(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[(width * height) * 4];
            int xBlocks = width / 4;
            int yBlocks = height / 4;
            for (int i = 0; i < yBlocks; i++)
            {
                for (int j = 0; j < xBlocks; j++)
                {
                    int index = ((i * xBlocks) + j) * 8;
                    uint colour0 = (uint)((data[index] << 8) + data[index + 1]);
                    uint colour1 = (uint)((data[index + 2] << 8) + data[index + 3]);
                    uint code = BitConverter.ToUInt32(data, index + 4);

                    ushort r0 = 0, g0 = 0, b0 = 0, r1 = 0, g1 = 0, b1 = 0;

                    r0 = (ushort)(8 * (colour0 & 0x1F));
                    g0 = (ushort)(4 * ((colour0 >> 5) & 0x3F));
                    b0 = (ushort)(8 * ((colour0 >> 11) & 0x1F));

                    r1 = (ushort)(8 * (colour1 & 0x1F));
                    g1 = (ushort)(4 * ((colour1 >> 5) & 0x3F));
                    b1 = (ushort)(8 * ((colour1 >> 11) & 0x1F));

                    for (int k = 0; k < 4; k++)
                    {
                        int x = k ^ 1;
                        for (int m = 0; m < 4; m++)
                        {
                            int dataStart = ((width * ((i * 4) + x)) * 4) + (((j * 4) + m) * 4);
                            switch (code & 3)
                            {
                                case 0:
                                    buffer[dataStart] = (byte)r0;
                                    buffer[dataStart + 1] = (byte)g0;
                                    buffer[dataStart + 2] = (byte)b0;
                                    buffer[dataStart + 3] = 0xFF;
                                    break;

                                case 1:
                                    buffer[dataStart] = (byte)r1;
                                    buffer[dataStart + 1] = (byte)g1;
                                    buffer[dataStart + 2] = (byte)b1;
                                    buffer[dataStart + 3] = 0xFF;
                                    break;

                                case 2:
                                    buffer[dataStart + 3] = 0xFF;
                                    if (colour0 <= colour1)
                                    {
                                        buffer[dataStart] = (byte)((r0 + r1) / 2);
                                        buffer[dataStart + 1] = (byte)((g0 + g1) / 2);
                                        buffer[dataStart + 2] = (byte)((b0 + b1) / 2);
                                    }
                                    buffer[dataStart] = (byte)(((2 * r0) + r1) / 3);
                                    buffer[dataStart + 1] = (byte)(((2 * g0) + g1) / 3);
                                    buffer[dataStart + 2] = (byte)(((2 * b0) + b1) / 3);
                                    break;

                                case 3:
                                    if (colour0 <= colour1)
                                    {
                                        buffer[dataStart] = 0;
                                        buffer[dataStart + 1] = 0;
                                        buffer[dataStart + 2] = 0;
                                        buffer[dataStart + 3] = 0;
                                    }
                                    buffer[dataStart] = (byte)((r0 + (2 * r1)) / 3);
                                    buffer[dataStart + 1] = (byte)((g0 + (2 * g1)) / 3);
                                    buffer[dataStart + 2] = (byte)((b0 + (2 * b1)) / 3);
                                    buffer[dataStart + 3] = 0xFF;
                                    break;

                                default:
                                    break;
                            }
                            code = code >> 2;
                        }
                    }
                }
            }
            return buffer;
        }

        private static byte[] DecodeDXT3(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[width * height * 4];
            int xBlocks = width / 4;
            int yBlocks = height / 4;
            for (int y = 0; y < yBlocks; y++)
            {
                for (int x = 0; x < xBlocks; x++)
                {
                    int blockDataStart = ((y * xBlocks) + x) * 16;
                    ushort[] alphaData = new ushort[4];

                    alphaData[0] = (ushort)((data[blockDataStart + 0] << 8) + data[blockDataStart + 1]);
                    alphaData[1] = (ushort)((data[blockDataStart + 2] << 8) + data[blockDataStart + 3]);
                    alphaData[2] = (ushort)((data[blockDataStart + 4] << 8) + data[blockDataStart + 5]);
                    alphaData[3] = (ushort)((data[blockDataStart + 6] << 8) + data[blockDataStart + 7]);

                    byte[,] alpha = new byte[4, 4];
                    for (int j = 0; j < 4; j++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            alpha[i, j] = (byte)((alphaData[j] & 0xF) * 16);
                            alphaData[j] >>= 4;
                        }
                    }

                    ushort color0 = (ushort)((data[blockDataStart + 8] << 8) + data[blockDataStart + 9]);
                    ushort color1 = (ushort)((data[blockDataStart + 10] << 8) + data[blockDataStart + 11]);

                    uint code = BitConverter.ToUInt32(data, blockDataStart + 8 + 4);

                    ushort r0 = 0, g0 = 0, b0 = 0, r1 = 0, g1 = 0, b1 = 0;
                    r0 = (ushort)(8 * (color0 & 31));
                    g0 = (ushort)(4 * ((color0 >> 5) & 63));
                    b0 = (ushort)(8 * ((color0 >> 11) & 31));

                    r1 = (ushort)(8 * (color1 & 31));
                    g1 = (ushort)(4 * ((color1 >> 5) & 63));
                    b1 = (ushort)(8 * ((color1 >> 11) & 31));

                    for (int k = 0; k < 4; k++)
                    {
                        int j = k ^ 1;
                        for (int i = 0; i < 4; i++)
                        {
                            int pixDataStart = (width * (y * 4 + j) * 4) + ((x * 4 + i) * 4);
                            uint codeDec = code & 0x3;

                            buffer[pixDataStart + 3] = alpha[i, j];

                            switch (codeDec)
                            {
                                case 0:
                                    buffer[pixDataStart + 0] = (byte)r0;
                                    buffer[pixDataStart + 1] = (byte)g0;
                                    buffer[pixDataStart + 2] = (byte)b0;
                                    break;
                                case 1:
                                    buffer[pixDataStart + 0] = (byte)r1;
                                    buffer[pixDataStart + 1] = (byte)g1;
                                    buffer[pixDataStart + 2] = (byte)b1;
                                    break;
                                case 2:
                                    if (color0 > color1)
                                    {
                                        buffer[pixDataStart + 0] = (byte)((2 * r0 + r1) / 3);
                                        buffer[pixDataStart + 1] = (byte)((2 * g0 + g1) / 3);
                                        buffer[pixDataStart + 2] = (byte)((2 * b0 + b1) / 3);
                                    }
                                    else
                                    {
                                        buffer[pixDataStart + 0] = (byte)((r0 + r1) / 2);
                                        buffer[pixDataStart + 1] = (byte)((g0 + g1) / 2);
                                        buffer[pixDataStart + 2] = (byte)((b0 + b1) / 2);
                                    }
                                    break;
                                case 3:
                                    if (color0 > color1)
                                    {
                                        buffer[pixDataStart + 0] = (byte)((r0 + 2 * r1) / 3);
                                        buffer[pixDataStart + 1] = (byte)((g0 + 2 * g1) / 3);
                                        buffer[pixDataStart + 2] = (byte)((b0 + 2 * b1) / 3);
                                    }
                                    else
                                    {
                                        buffer[pixDataStart + 0] = 0;
                                        buffer[pixDataStart + 1] = 0;
                                        buffer[pixDataStart + 2] = 0;
                                    }
                                    break;
                            }

                            code >>= 2;
                        }
                    }
                }
            }
            return buffer;
        }

        private static byte[] DecodeDXT3A(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[(width * height) * 4];
            int xBlocks = width / 4;
            int yBlocks = height / 4;
            for (int y = 0; y < yBlocks; y++)
            {
                for (int x = 0; x < xBlocks; x++)
                {
                    int i;
                    int blockDataStart = ((y * xBlocks) + x) * 8;
                    ushort[] alphaData = new ushort[] { 
                        (ushort)((data[blockDataStart + 0] << 8) + data[blockDataStart + 1]), 
                        (ushort)((data[blockDataStart + 2] << 8) + data[blockDataStart + 3]), 
                        (ushort)((data[blockDataStart + 4] << 8) + data[blockDataStart + 5]), 
                        (ushort)((data[blockDataStart + 6] << 8) + data[blockDataStart + 7]) };
                    byte[,] alpha = new byte[4, 4];
                    int j = 0;
                    while (j < 4)
                    {
                        i = 0;
                        while (i < 4)
                        {
                            alpha[i, j] = (byte)((alphaData[j] & 15) * 16);
                            alphaData[j] = (ushort)(alphaData[j] >> 4);
                            i++;
                        }
                        j++;
                    }
                    uint code = BitConverter.ToUInt32(data, blockDataStart);
                    for (int k = 0; k < 4; k++)
                    {
                        j = k ^ 1;
                        for (i = 0; i < 4; i++)
                        {
                            int pixDataStart = ((width * ((y * 4) + j)) * 4) + (((x * 4) + i) * 4);

                            buffer[pixDataStart] = alpha[i, j];
                            buffer[pixDataStart + 1] = alpha[i, j];
                            buffer[pixDataStart + 2] = alpha[i, j];

                            //buffer[pixDataStart + 3] = (type == AType.alpha) ? alpha[i, j] : 0xFF
                            buffer[pixDataStart + 3] = alpha[i, j];

                            code = code >> 2;
                        }
                    }
                }
            }
            return buffer;
        }

        private static byte[] DecodeDXT5(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[width * height * 4];
            int xBlocks = width / 4;
            int yBlocks = height / 4;
            for (int y = 0; y < yBlocks; y++)
            {
                for (int x = 0; x < xBlocks; x++)
                {
                    int blockDataStart = ((y * xBlocks) + x) * 16;
                    uint[] alphas = new uint[8];
                    ulong alphaMask = 0;

                    alphas[0] = data[blockDataStart + 1];
                    alphas[1] = data[blockDataStart + 0];

                    alphaMask |= data[blockDataStart + 6];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 7];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 4];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 5];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 2];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 3];

                    if (alphas[0] > alphas[1])
                    {
                        alphas[2] = (byte)((6 * alphas[0] + 1 * alphas[1] + 3) / 7);
                        alphas[3] = (byte)((5 * alphas[0] + 2 * alphas[1] + 3) / 7);
                        alphas[4] = (byte)((4 * alphas[0] + 3 * alphas[1] + 3) / 7);
                        alphas[5] = (byte)((3 * alphas[0] + 4 * alphas[1] + 3) / 7);
                        alphas[6] = (byte)((2 * alphas[0] + 5 * alphas[1] + 3) / 7);
                        alphas[7] = (byte)((1 * alphas[0] + 6 * alphas[1] + 3) / 7);
                    }
                    else
                    {
                        alphas[2] = (byte)((4 * alphas[0] + 1 * alphas[1] + 2) / 5);
                        alphas[3] = (byte)((3 * alphas[0] + 2 * alphas[1] + 2) / 5);
                        alphas[4] = (byte)((2 * alphas[0] + 3 * alphas[1] + 2) / 5);
                        alphas[5] = (byte)((1 * alphas[0] + 4 * alphas[1] + 2) / 5);
                        alphas[6] = 0x00;
                        alphas[7] = 0xFF;
                    }

                    byte[,] alpha = new byte[4, 4];

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            alpha[j, i] = (byte)alphas[alphaMask & 7];
                            alphaMask >>= 3;
                        }
                    }

                    ushort color0 = (ushort)((data[blockDataStart + 8] << 8) + data[blockDataStart + 9]);
                    ushort color1 = (ushort)((data[blockDataStart + 10] << 8) + data[blockDataStart + 11]);

                    uint code = BitConverter.ToUInt32(data, blockDataStart + 8 + 4);

                    ushort r0 = 0, g0 = 0, b0 = 0, r1 = 0, g1 = 0, b1 = 0;
                    r0 = (ushort)(8 * (color0 & 31));
                    g0 = (ushort)(4 * ((color0 >> 5) & 63));
                    b0 = (ushort)(8 * ((color0 >> 11) & 31));

                    r1 = (ushort)(8 * (color1 & 31));
                    g1 = (ushort)(4 * ((color1 >> 5) & 63));
                    b1 = (ushort)(8 * ((color1 >> 11) & 31));

                    for (int k = 0; k < 4; k++)
                    {
                        int j = k ^ 1;
                        for (int i = 0; i < 4; i++)
                        {
                            int pixDataStart = (width * (y * 4 + j) * 4) + ((x * 4 + i) * 4);
                            uint codeDec = code & 0x3;

                            buffer[pixDataStart + 3] = alpha[i, j];

                            switch (codeDec)
                            {
                                case 0:
                                    buffer[pixDataStart + 0] = (byte)r0;
                                    buffer[pixDataStart + 1] = (byte)g0;
                                    buffer[pixDataStart + 2] = (byte)b0;
                                    break;
                                case 1:
                                    buffer[pixDataStart + 0] = (byte)r1;
                                    buffer[pixDataStart + 1] = (byte)g1;
                                    buffer[pixDataStart + 2] = (byte)b1;
                                    break;
                                case 2:
                                    if (color0 > color1)
                                    {
                                        buffer[pixDataStart + 0] = (byte)((2 * r0 + r1) / 3);
                                        buffer[pixDataStart + 1] = (byte)((2 * g0 + g1) / 3);
                                        buffer[pixDataStart + 2] = (byte)((2 * b0 + b1) / 3);
                                    }
                                    else
                                    {
                                        buffer[pixDataStart + 0] = (byte)((r0 + r1) / 2);
                                        buffer[pixDataStart + 1] = (byte)((g0 + g1) / 2);
                                        buffer[pixDataStart + 2] = (byte)((b0 + b1) / 2);
                                    }
                                    break;
                                case 3:
                                    if (color0 > color1)
                                    {
                                        buffer[pixDataStart + 0] = (byte)((r0 + 2 * r1) / 3);
                                        buffer[pixDataStart + 1] = (byte)((g0 + 2 * g1) / 3);
                                        buffer[pixDataStart + 2] = (byte)((b0 + 2 * b1) / 3);
                                    }
                                    else
                                    {
                                        buffer[pixDataStart + 0] = 0;
                                        buffer[pixDataStart + 1] = 0;
                                        buffer[pixDataStart + 2] = 0;
                                    }
                                    break;
                            }

                            code >>= 2;
                        }
                    }
                }
            }
            return buffer;
        }

        private static byte[] DecodeDXT5A(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[height * width * 4];

            int chunks = width / 4;
            if (chunks == 0)
                chunks = 1;

            for (int i = 0; i < (width * height / 2); i += 8)
            {
                byte mMin = data[i + 1];
                byte mMax = data[i];
                byte[] rIndices = new byte[16];
                int temp = ((data[i + 5] << 16) | (data[i + 2] << 8)) | data[i + 3];

                int indices = 0;
                while (indices < 8)
                {
                    rIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }

                temp = ((data[i + 6] << 16) | (data[i + 7] << 8)) | data[i + 4];
                while (indices < 16)
                {
                    rIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }

                byte[] monoTable = new byte[8];
                monoTable[0] = mMin;
                monoTable[1] = mMax;
                if (monoTable[0] > monoTable[1])
                {
                    monoTable[2] = (byte)((6 * monoTable[0] + 1 * monoTable[1]) / 7.0f);
                    monoTable[3] = (byte)((5 * monoTable[0] + 2 * monoTable[1]) / 7.0f);
                    monoTable[4] = (byte)((4 * monoTable[0] + 3 * monoTable[1]) / 7.0f);
                    monoTable[5] = (byte)((3 * monoTable[0] + 4 * monoTable[1]) / 7.0f);
                    monoTable[6] = (byte)((2 * monoTable[0] + 5 * monoTable[1]) / 7.0f);
                    monoTable[7] = (byte)((1 * monoTable[0] + 6 * monoTable[1]) / 7.0f);
                }
                else
                {
                    monoTable[2] = (byte)((4 * monoTable[0] + 1 * monoTable[1]) / 5.0f);
                    monoTable[3] = (byte)((3 * monoTable[0] + 2 * monoTable[1]) / 5.0f);
                    monoTable[4] = (byte)((2 * monoTable[0] + 3 * monoTable[1]) / 5.0f);
                    monoTable[5] = (byte)((1 * monoTable[0] + 4 * monoTable[1]) / 5.0f);
                    monoTable[6] = (byte)0;
                    monoTable[7] = (byte)255;
                }

                int chunkNum = i / 8;
                int xPos = chunkNum % chunks;
                int yPos = (chunkNum - xPos) / chunks;
                int sizeh = (height < 4) ? height : 4;
                int sizew = (width < 4) ? width : 4;

                for (int j = 0; j < sizeh; j++)
                {
                    for (int k = 0; k < sizew; k++)
                    {
                        RGBAColor color;
                        color.R = color.G = color.B = color.A = monoTable[rIndices[(j * sizeh) + k]];
                        temp = (((((yPos * 4) + j) * width) + (xPos * 4)) + k) * 4;
                        buffer[temp] = (byte)color.B;
                        buffer[temp + 1] = (byte)color.G;
                        buffer[temp + 2] = (byte)color.R;

                        //buffer[temp + 3] = (type == AType.alpha) ? (byte)color.A : 0xFF;
                        buffer[temp + 3] = (byte)color.A;
                    }
                }
            }
            return buffer;
        }

        private static byte[] DecodeR5G6B5(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[width * height * 4];
            for (int i = 0; i < (width * height * 2); i += 2)
            {
                short temp = (short)(data[i] | (data[i + 1] << 8));
                buffer[i * 2] = (byte)(temp & 0x1F);
                buffer[(i * 2) + 1] = (byte)((temp >> 5) & 0x3F);
                buffer[(i * 2) + 2] = (byte)((temp >> 11) & 0x1F);
                buffer[(i * 2) + 3] = 0xFF;
            }
            return buffer;
        }

        private static byte[] DecodeY8(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[height * width * 4];
            for (int i = 0; i < (height * width); i++)
            {
                int index = i * 4;
                buffer[index] = data[i];
                buffer[index + 1] = data[i];
                buffer[index + 2] = data[i];
                buffer[index + 3] = 0xFF;
            }
            return buffer;
        }
        #endregion

        public static Bitmap DecodeCubeMap(byte[] data, bitmap.BitmapData submap, PixelFormat PF, DefinitionSet version)
        {
            List<Bitmap> images = new List<Bitmap>();
            int imageSize = submap.VirtualWidth * submap.VirtualHeight * 4;
            int tImageSize = submap.RawSize / 6;

            switch (submap.Format)
            {
                case TextureFormat.DXT1:
                    imageSize = Math.Max(imageSize / 8, 8);
                    break;
                case TextureFormat.DXT3:
                case TextureFormat.DXT5:
                    imageSize = Math.Max(imageSize / 4, 16);
                    break;
                case TextureFormat.A8R8G8B8:
                    imageSize = Math.Max(data.Length / 6, 16);
                    break;
            }

            for (int i = 0; i < 6; i++)
            {
                byte[] buffer = new byte[imageSize];
                Array.Copy(data, i * tImageSize, buffer, 0, imageSize);
                buffer = DecodeBitmap(buffer, submap, version);

                //PixelFormat PF = (alpha) ? PixelFormat.Format32bppArgb : PixelFormat.Format32bppRgb;
                Bitmap bitmap = new Bitmap(submap.Width, submap.Height, PF);
                Rectangle rect = new Rectangle(0, 0, submap.Width, submap.Height);
                BitmapData bitmapdata = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PF);
                byte[] destinationArray = new byte[submap.Width * submap.Height * 4];

                for (int j = 0; j < submap.Height; j++)
                    Array.Copy(buffer, j * submap.VirtualWidth * 4, destinationArray, j * submap.Width * 4, submap.Width * 4);

                Marshal.Copy(destinationArray, 0, bitmapdata.Scan0, destinationArray.Length);
                bitmap.UnlockBits(bitmapdata);
                images.Add(bitmap);
            }

            Bitmap finalImage = new Bitmap(4 * submap.Width, 3 * submap.Height);

            using (Graphics g = Graphics.FromImage(finalImage))
            {
                // set background color
                g.Clear(Color.Empty);

                int[] crossX = new int[6] { 0, 2, 1, 3, 0, 0 }; // Front, Left, Right, Back, Top, Bottom
                int[] crossY = new int[6] { 1, 1, 1, 1, 0, 2 }; // Front, Left, Right, Back, Top, Bottom
                
                // go through each image and draw it on the final image
                int xOffset = 0;
                int yOffset = 0;
                int tempCount = 0;
                foreach (Bitmap image in images)
                {
                    switch (tempCount)
                    {
                        case 0:
                        case 4:
                        case 5:
                            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 1:
                            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        case 2:
                            image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                    }
                    xOffset = crossX[tempCount] * image.Width;
                    yOffset = crossY[tempCount] * image.Height;

                    g.DrawImage(image, new Rectangle(xOffset, yOffset, image.Width, image.Height));
                    tempCount++;
                }
            }

            return finalImage;
        }

        public static Bitmap DecodeCubeMap(byte[] data, S3DPICT pict, PixelFormat PF)
        {
            List<Bitmap> images = new List<Bitmap>();
            int imageSize = pict.VirtualWidth * pict.VirtualHeight * 4;
            int tImageSize = pict.RawSize / 6;

            switch (pict.Format)
            {
                case TextureFormat.DXT1:
                    imageSize = Math.Max(imageSize / 8, 8);
                    break;
                case TextureFormat.DXT3:
                case TextureFormat.DXT5:
                    imageSize = Math.Max(imageSize / 4, 16);
                    break;
                case TextureFormat.A8R8G8B8:
                    imageSize = Math.Max(data.Length / 6, 16);
                    break;
            }

            for (int i = 0; i < 6; i++)
            {
                byte[] buffer = new byte[imageSize];
                Array.Copy(data, i * tImageSize, buffer, 0, imageSize);
                buffer = DecodeBitmap(buffer, pict);

                //PixelFormat PF = (alpha) ? PixelFormat.Format32bppArgb : PixelFormat.Format32bppRgb;
                Bitmap bitmap = new Bitmap(pict.Width, pict.Height, PF);
                Rectangle rect = new Rectangle(0, 0, pict.Width, pict.Height);
                BitmapData bitmapdata = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PF);
                byte[] destinationArray = new byte[pict.Width * pict.Height * 4];

                for (int j = 0; j < pict.Height; j++)
                    Array.Copy(buffer, j * pict.VirtualWidth * 4, destinationArray, j * pict.Width * 4, pict.Width * 4);

                Marshal.Copy(destinationArray, 0, bitmapdata.Scan0, destinationArray.Length);
                bitmap.UnlockBits(bitmapdata);
                images.Add(bitmap);
            }

            Bitmap finalImage = new Bitmap(4 * pict.Width, 3 * pict.Height);

            using (Graphics g = Graphics.FromImage(finalImage))
            {
                // set background color
                g.Clear(Color.Empty);

                int[] crossX = new int[6] { 0, 2, 1, 3, 0, 0 }; // Front, Left, Right, Back, Top, Bottom
                int[] crossY = new int[6] { 1, 1, 1, 1, 0, 2 }; // Front, Left, Right, Back, Top, Bottom

                // go through each image and draw it on the final image
                int xOffset = 0;
                int yOffset = 0;
                int tempCount = 0;
                foreach (Bitmap image in images)
                {
                    switch (tempCount)
                    {
                        case 0:
                        case 4:
                        case 5:
                            image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 1:
                            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                        case 2:
                            image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                    }
                    xOffset = crossX[tempCount] * image.Width;
                    yOffset = crossY[tempCount] * image.Height;

                    g.DrawImage(image, new Rectangle(xOffset, yOffset, image.Width, image.Height));
                    tempCount++;
                }
            }

            return finalImage;
        }

        public static byte[] DecodeBitmap(byte[] bitmRaw, S3DPICT pict)
        {
            #region switch format
            switch (pict.Format)
            {
                case TextureFormat.A8:
                    bitmRaw = DXTDecoder.DecodeA8(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.Y8:
                    bitmRaw = DXTDecoder.DecodeY8(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.AY8:
                    bitmRaw = DXTDecoder.DecodeAY8(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.A8Y8:
                    bitmRaw = DXTDecoder.DecodeA8Y8(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.R5G6B5:
                    bitmRaw = DXTDecoder.DecodeR5G6B5(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.A1R5G5B5:
                    bitmRaw = DXTDecoder.DecodeA1R5G5B5(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.A4R4G4B4:
                    bitmRaw = DXTDecoder.DecodeA4R4G4B4(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.X8R8G8B8:
                case TextureFormat.A8R8G8B8:
                    bitmRaw = DXTDecoder.DecodeA8R8G8B8(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.DXT1:
                    bitmRaw = DXTDecoder.DecodeDXT1(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.DXT3:
                    bitmRaw = DXTDecoder.DecodeDXT3(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.DXT5:
                    bitmRaw = DXTDecoder.DecodeDXT5(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.DXT5a:
                case TextureFormat.DXT5a_alpha:
                case TextureFormat.DXT5a_mono:
                    bitmRaw = DXTDecoder.DecodeDXT5A(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.DXT3a_alpha:
                case TextureFormat.DXT3a_mono:
                    bitmRaw = DXTDecoder.DecodeDXT3A(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.DXN_mono_alpha:
                    bitmRaw = DXTDecoder.DecodeDXNMA(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.DXN:
                    bitmRaw = DXTDecoder.DecodeDXN(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.CTX1:
                    bitmRaw = DXTDecoder.DecodeCTX1(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                case TextureFormat.P8:
                case TextureFormat.P8_bump:
                    bitmRaw = DXTDecoder.DecodeP8(bitmRaw, pict.VirtualWidth, pict.VirtualHeight);
                    break;

                default:
                    throw new NotSupportedException("Unsupported bitmap format.");
            }
            #endregion

            return bitmRaw;
        }

        public static byte[] DecodeBitmap(byte[] bitmRaw, bitmap.BitmapData submap, DefinitionSet Version)
        {
            if (Version <= DefinitionSet.Halo2Vista)
            {
                if (submap.Flags.Values[3]) bitmRaw = Swizzle(bitmRaw, 0, submap.Width, submap.Height, 1, 8, true);

                for (int i = 0; i < bitmRaw.Length; i += submap.BlockSize)
                    Array.Reverse(bitmRaw, i, submap.BlockSize);
            }
            else
            {
                if (submap.Flags.Values[3])
                    bitmRaw = DXTDecoder.ConvertToLinearTexture(bitmRaw, submap.VirtualWidth, submap.VirtualHeight, submap.Format);
            }

            #region switch format
            switch (submap.Format)
            {
                case TextureFormat.A8:
                    bitmRaw =  DXTDecoder.DecodeA8(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.Y8:
                    bitmRaw =  DXTDecoder.DecodeY8(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.AY8:
                    bitmRaw =  DXTDecoder.DecodeAY8(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.A8Y8:
                    bitmRaw =  DXTDecoder.DecodeA8Y8(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.R5G6B5:
                    bitmRaw =  DXTDecoder.DecodeR5G6B5(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.A1R5G5B5:
                    bitmRaw =  DXTDecoder.DecodeA1R5G5B5(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.A4R4G4B4:
                    bitmRaw =  DXTDecoder.DecodeA4R4G4B4(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.X8R8G8B8:
                case TextureFormat.A8R8G8B8:
                    bitmRaw =  DXTDecoder.DecodeA8R8G8B8(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.DXT1:
                    bitmRaw =  DXTDecoder.DecodeDXT1(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.DXT3:
                    bitmRaw =  DXTDecoder.DecodeDXT3(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.DXT5:
                    bitmRaw =  DXTDecoder.DecodeDXT5(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.DXT5a:
                case TextureFormat.DXT5a_alpha:
                case TextureFormat.DXT5a_mono:
                    bitmRaw = DXTDecoder.DecodeDXT5A(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.DXT3a_alpha:
                case TextureFormat.DXT3a_mono:
                    bitmRaw = DXTDecoder.DecodeDXT3A(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.DXN_mono_alpha:
                    bitmRaw = DXTDecoder.DecodeDXNMA(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.DXN:
                    bitmRaw =  DXTDecoder.DecodeDXN(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.CTX1:
                    bitmRaw = DXTDecoder.DecodeCTX1(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.P8:
                case TextureFormat.P8_bump:
                    bitmRaw = DXTDecoder.DecodeP8(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                default:
                    throw new NotSupportedException("Unsupported bitmap format.");
            }
            #endregion

            return bitmRaw;
        }

        private static RGBAColor GradientColors(RGBAColor Color1, RGBAColor Color2)
        {
            RGBAColor color;
            color.R = (byte)(((Color1.R * 2) + Color2.R) / 3);
            color.G = (byte)(((Color1.G * 2) + Color2.G) / 3);
            color.B = (byte)(((Color1.B * 2) + Color2.B) / 3);
            color.A = 0xFF;
            return color;
        }

        private static RGBAColor GradientColorsHalf(RGBAColor Color1, RGBAColor Color2)
        {
            RGBAColor color;
            color.R = (byte)((Color1.R / 2) + (Color2.R / 2));
            color.G = (byte)((Color1.G / 2) + (Color2.G / 2));
            color.B = (byte)((Color1.B / 2) + (Color2.B / 2));
            color.A = 0xFF;
            return color;
        }

        private static byte[] ModifyLinearTexture(byte[] data, int width, int height, TextureFormat texture, bool toLinear)
        {
            byte[] destinationArray = new byte[data.Length];

            int blockSizeX, blockSizeY, texPitch;

            switch (texture)
            {
                case TextureFormat.DXT5a_mono:
                case TextureFormat.DXT5a_alpha:
                case TextureFormat.DXT1:
                case TextureFormat.CTX1:
                case TextureFormat.DXT5a:
                case TextureFormat.DXT3a_alpha:
                case TextureFormat.DXT3a_mono:
                    blockSizeX = 4;
                    blockSizeY = 4;
                    texPitch = 8;
                    break;

                case TextureFormat.DXT3:
                case TextureFormat.DXT5:
                case TextureFormat.DXN:
                case TextureFormat.DXN_mono_alpha:
                    blockSizeX = 4;
                    blockSizeY = 4;
                    texPitch = 16;
                    break;

                case TextureFormat.AY8:
                case TextureFormat.Y8:
                    blockSizeX = 1;
                    blockSizeY = 1;
                    texPitch = 1;
                    break;

                case TextureFormat.A8R8G8B8:
                    blockSizeX = 1;
                    blockSizeY = 1;
                    texPitch = 4;
                    break;

                default:
                    blockSizeX = 1;
                    blockSizeY = 1;
                    texPitch = 2;
                    break;
            }

            int xChunks = width / blockSizeX;
            int yChunks = height / blockSizeY;
            try
            {
                for (int i = 0; i < yChunks; i++)
                {
                    for (int j = 0; j < xChunks; j++)
                    {
                        int offset = (i * xChunks) + j;
                        int x = XGAddress2DTiledX(offset, xChunks, texPitch);
                        int y = XGAddress2DTiledY(offset, xChunks, texPitch);
                        int sourceIndex = ((i * xChunks) * texPitch) + (j * texPitch);
                        int destinationIndex = ((y * xChunks) * texPitch) + (x * texPitch);
                        if (toLinear)
                            Array.Copy(data, sourceIndex, destinationArray, destinationIndex, texPitch);
                        else
                            Array.Copy(data, destinationIndex, destinationArray, sourceIndex, texPitch);
                    }
                }
            }
            catch { }
            return destinationArray;
        }

        private static int XGAddress2DTiledX(int Offset, int Width, int TexelPitch)
        {
            int alignedWidth = (Width + 31) & ~31;

            int logBPP = (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
            int offsetB = Offset << logBPP;
            int offsetT = (((offsetB & ~4095) >> 3) + ((offsetB & 1792) >> 2)) + (offsetB & 63);
            int offsetM = offsetT >> (7 + logBPP);

            int macroX = (offsetM % (alignedWidth >> 5)) << 2;
            int tile = (((offsetT >> (5 + logBPP)) & 2) + (offsetB >> 6)) & 3;
            int Macro = (macroX + tile) << 3;
            int Micro = ((((offsetT >> 1) & ~15) + (offsetT & 15)) & ((TexelPitch << 3) - 1)) >> logBPP;

            return (Macro + Micro);
        }

        private static int XGAddress2DTiledY(int Offset, int Width, int TexelPitch)
        {
            int alignedWidth = (Width + 31) & ~31;

            int logBPP = (TexelPitch >> 2) + ((TexelPitch >> 1) >> (TexelPitch >> 2));
            int offsetB = Offset << logBPP;
            int offsetT = (((offsetB & ~4095) >> 3) + ((offsetB & 1792) >> 2)) + (offsetB & 63);
            int offsetM = offsetT >> (7 + logBPP);

            int macroY = (offsetM / (alignedWidth >> 5)) << 2;
            int tile = ((offsetT >> (6 + logBPP)) & 1) + ((offsetB & 2048) >> 10);
            int Macro = (macroY + tile) << 3;
            int Micro = (((offsetT & (((TexelPitch << 6) - 1) & ~31)) + ((offsetT & 15) << 1)) >> (3 + logBPP)) & ~1;

            return ((Macro + Micro) + ((offsetT & 16) >> 4));
        }

        public struct RGBAColor
        {
            public int R, G, B, A;

            public RGBAColor(int Red, int Green, int Blue, int Alpha)
            {
                R = Red;
                G = Green;
                B = Blue;
                A = Alpha;
            }
        }

        #region Swizzle
        private class MaskSet
        {
            public readonly int x;
            public readonly int y;
            public readonly int z;

            public MaskSet(int w, int h, int d)
            {
                int bit = 1;
                int index = 1;

                while (bit < w || bit < h || bit < d)
                {
                    // if (bit == 0) { break; }
                    if (bit < w)
                    {
                        x |= index;
                        index <<= 1;
                    }

                    if (bit < h)
                    {
                        y |= index;
                        index <<= 1;
                    }

                    if (bit < d)
                    {
                        z |= index;
                        index <<= 1;
                    }

                    bit <<= 1;
                }
            }
        }

        public static byte[] Swizzle(byte[] raw, int offset, int width, int height, int depth, int bitCount, bool deswizzle)
        {
            if (raw.Length == 0)
                return new byte[0];

            if (depth < 1) depth = 1;

            bitCount /= 8;
            int a = 0, b = 0;
            int tempsize = raw.Length; // width * height * bitCount;
            byte[] data = new byte[tempsize];
            MaskSet masks = new MaskSet(width, height, depth);

            offset = 0;

            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (deswizzle)
                        {
                            a = ((((z * height) + y) * width) + x) * bitCount;
                            b = Swizzle(x, y, z, masks) * bitCount;

                            // a = ((y * width) + x) * bitCount;
                            // b = (Swizzle(x, y, -1, masks)) * bitCount;
                        }
                        else
                        {
                            b = ((((z * height) + y) * width) + x) * bitCount;
                            a = Swizzle(x, y, z, masks) * bitCount;

                            // b = ((y * width) + x) * bitCount;
                            // a = (Swizzle(x, y, -1, masks)) * bitCount;
                        }

                        for (int i = offset; i < bitCount + offset; i++)
                            data[a + i] = raw[b + i];
                    }
                }
            }

            // for(int u = 0; u < offset; u++)
            // data[u] = raw[u];
            // for(int v = offset + (height * width * depth * bitCount); v < data.Length; v++)
            // 	data[v] = raw[v];
            return data;
        }

        private static int Swizzle(int x, int y, int z, MaskSet masks)
        {
            return SwizzleAxis(x, masks.x) | SwizzleAxis(y, masks.y) | (z == -1 ? 0 : SwizzleAxis(z, masks.z));
        }

        private static int SwizzleAxis(int val, int mask)
        {
            int bit = 1;
            int result = 0;

            while (bit <= mask)
            {
                int test = mask & bit;
                if (test != 0) result |= val & bit;
                else val <<= 1;

                bit <<= 1;
            }

            return result;
        }
        #endregion
    }
}

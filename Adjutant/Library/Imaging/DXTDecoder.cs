using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Adjutant.Library.Definitions;

namespace Adjutant.Library.Imaging
{
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
                byte xMin = data[i + 1];
                byte xMax = data[i];
                byte[] xIndices = new byte[16];
                int temp = ((data[i + 5] << 16) | (data[i + 2] << 8)) | data[i + 3];
                int indices = 0;
                while (indices < 8)
                {
                    xIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                temp = ((data[i + 6] << 16) | (data[i + 7] << 8)) | data[i + 4];
                while (indices < 16)
                {
                    xIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                byte yMin = data[i + 9];
                byte yMax = data[i + 8];
                byte[] yIndices = new byte[16];
                temp = ((data[i + 13] << 16) | (data[i + 10] << 8)) | data[i + 11];
                indices = 0;
                while (indices < 8)
                {
                    yIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                temp = ((data[i + 14] << 16) | (data[i + 15] << 8)) | data[i + 12];
                while (indices < 16)
                {
                    yIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                byte[] xTable = new byte[8];
                xTable[0] = xMin;
                xTable[1] = xMax;
                if (xTable[0] > xTable[1])
                {
                    xTable[2] = (byte)(((xMax - xMin) * 0.1428571f) + xMin);
                    xTable[3] = (byte)(((xMax - xMin) * 0.2857143f) + xMin);
                    xTable[4] = (byte)(((xMax - xMin) * 0.4285714f) + xMin);
                    xTable[5] = (byte)(((xMax - xMin) * 0.5714286f) + xMin);
                    xTable[6] = (byte)(((xMax - xMin) * 0.7142857f) + xMin);
                    xTable[7] = (byte)(((xMax - xMin) * 0.8571429f) + xMin);
                }
                else
                {
                    xTable[2] = (byte)(((xMax - xMin) * 0.2f) + xMin);
                    xTable[3] = (byte)(((xMax - xMin) * 0.4f) + xMin);
                    xTable[4] = (byte)(((xMax - xMin) * 0.6f) + xMin);
                    xTable[5] = (byte)(((xMax - xMin) * 0.8f) + xMin);
                    xTable[6] = xMin;
                    xTable[7] = xMax;
                }
                byte[] yTable = new byte[8];
                yTable[0] = yMin;
                yTable[1] = yMax;
                if (yTable[0] > yTable[1])
                {
                    yTable[2] = (byte)(((yMax - yMin) * 0.1428571f) + yMin);
                    yTable[3] = (byte)(((yMax - yMin) * 0.2857143f) + yMin);
                    yTable[4] = (byte)(((yMax - yMin) * 0.4285714f) + yMin);
                    yTable[5] = (byte)(((yMax - yMin) * 0.5714286f) + yMin);
                    yTable[6] = (byte)(((yMax - yMin) * 0.7142857f) + yMin);
                    yTable[7] = (byte)(((yMax - yMin) * 0.8571429f) + yMin);
                }
                else
                {
                    yTable[2] = (byte)(((yMax - yMin) * 0.2f) + yMin);
                    yTable[3] = (byte)(((yMax - yMin) * 0.4f) + yMin);
                    yTable[4] = (byte)(((yMax - yMin) * 0.6f) + yMin);
                    yTable[5] = (byte)(((yMax - yMin) * 0.8f) + yMin);
                    yTable[6] = yMin;
                    yTable[7] = yMax;
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
                        color.R = xTable[xIndices[(j * sizeh) + k]];
                        color.G = yTable[yIndices[(j * sizeh) + k]];
                        float x = ((((float)color.R) / 255f) * 2f) - 1f;
                        float y = ((((float)color.G) / 255f) * 2f) - 1f;
                        float z = (float)Math.Sqrt((double)Math.Max(0f, Math.Min((float)1f, (float)((1f - (x * x)) - (y * y)))));
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

        private static byte[] DecodeDXT5A(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[height * width * 4];

            int chunks = width / 4;
            if (chunks == 0)
                chunks = 1;

            for (int i = 0; i < (width * height / 2); i += 8)
            {
                byte zMin = data[i + 1];
                byte zMax = data[i];
                byte[] zIndices = new byte[16];
                int temp = ((data[i + 5] << 16) | (data[i + 2] << 8)) | data[i + 3];

                int indices = 0;
                while (indices < 8)
                {
                    zIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }

                temp = ((data[i + 6] << 16) | (data[i + 7] << 8)) | data[i + 4];
                while (indices < 16)
                {
                    zIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }

                byte[] zTable = new byte[8];
                zTable[0] = zMin;
                zTable[1] = zMax;
                if (zTable[0] > zTable[1])
                {
                    zTable[2] = (byte)(((zMax - zMin) * 0.1428571f) + zMin);
                    zTable[3] = (byte)(((zMax - zMin) * 0.2857143f) + zMin);
                    zTable[4] = (byte)(((zMax - zMin) * 0.4285714f) + zMin);
                    zTable[5] = (byte)(((zMax - zMin) * 0.5714286f) + zMin);
                    zTable[6] = (byte)(((zMax - zMin) * 0.7142857f) + zMin);
                    zTable[7] = (byte)(((zMax - zMin) * 0.8571429f) + zMin);
                }
                else
                {
                    zTable[2] = (byte)(((zMax - zMin) * 0.2f) + zMin);
                    zTable[3] = (byte)(((zMax - zMin) * 0.4f) + zMin);
                    zTable[4] = (byte)(((zMax - zMin) * 0.6f) + zMin);
                    zTable[5] = (byte)(((zMax - zMin) * 0.8f) + zMin);
                    zTable[6] = 0;
                    zTable[7] = 255;
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
                        color.R = color.G = color.B = color.A = zTable[zIndices[(j * sizeh) + k]];
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

        private static byte[] DecodeDXNMA(byte[] data, int width, int height)
        {
            byte[] buffer = new byte[height * width * 4];
            int chunks = width / 4;

            if (chunks == 0)
                chunks = 1;

            for (int i = 0; i < (width * height); i += 16)
            {
                byte xMin = data[i + 1];
                byte xMax = data[i];
                byte[] xIndices = new byte[16];
                int temp = ((data[i + 5] << 0x10) | (data[i + 2] << 8)) | data[i + 3];
                int indices = 0;
                while (indices < 8)
                {
                    xIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                temp = ((data[i + 6] << 0x10) | (data[i + 7] << 8)) | data[i + 4];
                while (indices < 16)
                {
                    xIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                byte yMin = data[i + 9];
                byte yMax = data[i + 8];
                byte[] yIndices = new byte[16];
                temp = ((data[i + 13] << 0x10) | (data[i + 10] << 8)) | data[i + 11];
                indices = 0;
                while (indices < 8)
                {
                    yIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                temp = ((data[i + 14] << 0x10) | (data[i + 15] << 8)) | data[i + 12];
                while (indices < 16)
                {
                    yIndices[indices] = (byte)(temp & 7);
                    temp = temp >> 3;
                    indices++;
                }
                byte[] xTable = new byte[8];
                xTable[0] = xMin;
                xTable[1] = xMax;
                if (xTable[0] > xTable[1])
                {
                    xTable[2] = (byte)(((xMax - xMin) * 0.1428571f) + xMin);
                    xTable[3] = (byte)(((xMax - xMin) * 0.2857143f) + xMin);
                    xTable[4] = (byte)(((xMax - xMin) * 0.4285714f) + xMin);
                    xTable[5] = (byte)(((xMax - xMin) * 0.5714286f) + xMin);
                    xTable[6] = (byte)(((xMax - xMin) * 0.7142857f) + xMin);
                    xTable[7] = (byte)(((xMax - xMin) * 0.8571429f) + xMin);
                }
                else
                {
                    xTable[2] = (byte)(((xMax - xMin) * 0.2f) + xMin);
                    xTable[3] = (byte)(((xMax - xMin) * 0.4f) + xMin);
                    xTable[4] = (byte)(((xMax - xMin) * 0.6f) + xMin);
                    xTable[5] = (byte)(((xMax - xMin) * 0.8f) + xMin);
                    xTable[6] = xMin;
                    xTable[7] = xMax;
                }
                byte[] yTable = new byte[8];
                yTable[0] = yMin;
                yTable[1] = yMax;
                if (yTable[0] > yTable[1])
                {
                    yTable[2] = (byte)(((yMax - yMin) * 0.1428571f) + yMin);
                    yTable[3] = (byte)(((yMax - yMin) * 0.2857143f) + yMin);
                    yTable[4] = (byte)(((yMax - yMin) * 0.4285714f) + yMin);
                    yTable[5] = (byte)(((yMax - yMin) * 0.5714286f) + yMin);
                    yTable[6] = (byte)(((yMax - yMin) * 0.7142857f) + yMin);
                    yTable[7] = (byte)(((yMax - yMin) * 0.8571429f) + yMin);
                }
                else
                {
                    yTable[2] = (byte)(((yMax - yMin) * 0.2f) + yMin);
                    yTable[3] = (byte)(((yMax - yMin) * 0.4f) + yMin);
                    yTable[4] = (byte)(((yMax - yMin) * 0.6f) + yMin);
                    yTable[5] = (byte)(((yMax - yMin) * 0.8f) + yMin);
                    yTable[6] = yMin;
                    yTable[7] = yMax;
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
                        color.B = color.G = color.R = xTable[xIndices[(j * sizeh) + k]];
                        color.A = yTable[yIndices[(j * sizeh) + k]];
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

        public static Bitmap DecodeCubeMap(byte[] data, bitmap.BitmapData submap, bool alpha)
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
                buffer = DecodeBitmap(buffer, submap);

                PixelFormat PF = (alpha) ? PixelFormat.Format32bppArgb : PixelFormat.Format32bppRgb;
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

        public static byte[] DecodeBitmap(byte[] bitmRaw, bitmap.BitmapData submap)
        {
            if (submap.Flags.Values[3])
                bitmRaw = DXTDecoder.ConvertToLinearTexture(bitmRaw, submap.VirtualWidth, submap.VirtualHeight, submap.Format);

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

                case TextureFormat.DXT5a_alpha:
                case TextureFormat.DXT5a_mono:
                //case TextureFormat.Unknown31:
                    bitmRaw =  DXTDecoder.DecodeDXT5A(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.DXT3a_alpha:
                case TextureFormat.DXT3a_mono:
                    bitmRaw =  DXTDecoder.DecodeDXT3A(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.DXN_mono_alpha:
                    bitmRaw =  DXTDecoder.DecodeDXNMA(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.DXN:
                    bitmRaw =  DXTDecoder.DecodeDXN(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                case TextureFormat.CTX1:
                    bitmRaw = DXTDecoder.DecodeCTX1(bitmRaw, submap.VirtualWidth, submap.VirtualHeight);
                    break;

                default:
                    throw new NotSupportedException("Unsupported bitmap format.");
            }

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

            int num1, num2, num3;

            switch (texture)
            {
                case TextureFormat.DXT5a_mono:
                case TextureFormat.DXT5a_alpha:
                case TextureFormat.DXT1:
                case TextureFormat.CTX1:
                case TextureFormat.Unknown31:
                case TextureFormat.DXT3a_alpha:
                case TextureFormat.DXT3a_mono:
                    num1 = 4;
                    num2 = 4;
                    num3 = 8;
                    break;

                case TextureFormat.DXT3:
                case TextureFormat.DXT5:
                case TextureFormat.DXN:
                case TextureFormat.DXN_mono_alpha:
                    num1 = 4;
                    num2 = 4;
                    num3 = 16;
                    break;

                case TextureFormat.AY8:
                case TextureFormat.Y8:
                    num1 = 1;
                    num2 = 1;
                    num3 = 1;
                    break;

                case TextureFormat.A8R8G8B8:
                    num1 = 1;
                    num2 = 1;
                    num3 = 4;
                    break;

                default:
                    num1 = 1;
                    num2 = 1;
                    num3 = 2;
                    break;
            }

            int xChunks = width / num1;
            int yChunks = height / num2;
            try
            {
                for (int i = 0; i < yChunks; i++)
                {
                    for (int j = 0; j < xChunks; j++)
                    {
                        int offset = (i * xChunks) + j;
                        int num9 = XGAddress2DTiledX(offset, xChunks, num3);
                        int num10 = XGAddress2DTiledY(offset, xChunks, num3);
                        int sourceIndex = ((i * xChunks) * num3) + (j * num3);
                        int destinationIndex = ((num10 * xChunks) * num3) + (num9 * num3);
                        if (toLinear)
                            Array.Copy(data, sourceIndex, destinationArray, destinationIndex, num3);
                        else
                            Array.Copy(data, destinationIndex, destinationArray, sourceIndex, num3);
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
    }
}

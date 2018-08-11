﻿using System;
using System.IO;

namespace Ohana3DS_Transfigured.Ohana.Compressions
{
    class BLZ
    {
        /// <summary>
        ///     Decompress data compressed with Backward LZ77 algorithm.
        /// </summary>
        /// <param name="data">Data to be decompressed</param>
        /// <returns></returns>
        public static byte[] Decompress(Stream data)
        {
            data.Seek(0, SeekOrigin.Begin);
            byte[] input = new byte[data.Length];
            data.Read(input, 0, input.Length);
            data.Close();

            uint inputOffset = (uint)input.Length;
            int incrementalLength = ReadInt(input, ref inputOffset);
            uint lengths = ReadUInt(input, ref inputOffset);
            uint headerLength = lengths >> 24;
            uint encodedLength = lengths & 0xffffff;
            uint decodedLength = (uint)((int)encodedLength + incrementalLength);
            uint totalLength = (uint)(decodedLength + (input.Length - encodedLength));
            inputOffset = (uint)(input.Length - headerLength);

            byte[] output = new byte[totalLength];
            long outputOffset = 0;

            byte mask = 0;
            byte header = 0;

            while (outputOffset < decodedLength)
            {
                if ((mask >>= 1) == 0)
                {
                    header = ReadByte(input, ref inputOffset);
                    mask = 0x80;
                }

                if ((header & mask) == 0)
                {
                    if (outputOffset == output.Length) break;
                    output[outputOffset++] = ReadByte(input, ref inputOffset);
                }
                else
                {
                    ushort value = ReadUShort(input, ref inputOffset);
                    int length = (value >> 12) + 3;
                    int position = (value & 0xfff) + 3;
                    while (length > 0)
                    {
                        output[outputOffset] = output[outputOffset - position];
                        outputOffset++;
                        if (outputOffset == output.Length) break;
                        length--;
                    }
                }
            }

            output = Invert(output);
            Buffer.BlockCopy(input, 0, output, 0, (int)(input.Length - encodedLength));
            return output;
        }

        private static byte[] Invert(byte[] data)
        {
            byte[] output = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                output[i] = data[(data.Length - 1) - i];
            }
            return output;
        }

        private static byte ReadByte(byte[] input, ref uint address)
        {
            return input[--address];
        }

        private static ushort ReadUShort(byte[] input, ref uint address)
        {
            uint high = input[--address];
            uint low = input[--address];
            return (ushort)(low | (high << 8));
        }

        private static uint ReadUInt(byte[] input, ref uint address)
        {
            uint a = input[--address];
            uint b = input[--address];
            uint c = input[--address];
            uint d = input[--address];
            return d | (c << 8) | (b << 16) | (a << 24);
        }

        private static int ReadInt(byte[] input, ref uint address)
        {
            uint a = input[--address];
            uint b = input[--address];
            uint c = input[--address];
            uint d = input[--address];
            return (int)(d | (c << 8) | (b << 16) | (a << 24));
        }
    }
}

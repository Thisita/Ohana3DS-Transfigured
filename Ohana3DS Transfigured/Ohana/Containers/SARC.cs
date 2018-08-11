﻿using System.IO;

namespace Ohana3DS_Transfigured.Ohana.Containers
{
    class SARC
    {
        /// <summary>
        ///     Reads a SARC archive.
        /// </summary>
        /// <param name="fileName">The File Name where the data is located</param>
        /// <returns>The container data</returns>
        public static OContainer Load(string fileName)
        {
            return Load(new FileStream(fileName, FileMode.Open));
        }

        /// <summary>
        ///     Reads a SARC archive.
        /// </summary>
        /// <param name="data">Stream of the data</param>
        /// <returns>The container data</returns>
        public static OContainer Load(Stream data)
        {
            OContainer output = new OContainer();
            BinaryReader input = new BinaryReader(data);

            string sarcMagic = IOUtils.ReadStringWithLength(input, 4);
            ushort sarcHeaderLength = input.ReadUInt16();
            ushort endian = input.ReadUInt16();
            uint fileLength = input.ReadUInt32();
            uint dataOffset = input.ReadUInt32();
            uint dataPadding = input.ReadUInt32();

            string sfatMagic = IOUtils.ReadStringWithLength(input, 4);
            ushort sfatHeaderLength = input.ReadUInt16();
            ushort entries = input.ReadUInt16();
            uint hashMultiplier = input.ReadUInt32();
            int sfntOffset = 0x20 + entries * 0x10 + 8;

            for (int i = 0; i < entries; i++)
            {
                data.Seek(0x20 + i * 0x10, SeekOrigin.Begin);

                uint nameHash = input.ReadUInt32();
                uint nameOffset = (input.ReadUInt32() & 0xffffff) << 2;
                uint offset = input.ReadUInt32();
                uint length = input.ReadUInt32() - offset;

                string name = IOUtils.ReadString(input, (uint)(sfntOffset + nameOffset));
                data.Seek(offset + dataOffset, SeekOrigin.Begin);
                byte[] buffer = new byte[length];
                data.Read(buffer, 0, buffer.Length);
                if (name == "") name = string.Format("file_{0:D5}{1}", i, FileIO.GetExtension(buffer));

                OContainer.FileEntry entry = new OContainer.FileEntry
                {
                    name = name,
                    data = buffer
                };
                output.content.Add(entry);
            }

            data.Close();
            return output;
        }
    }
}

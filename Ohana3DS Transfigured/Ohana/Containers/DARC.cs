using System.IO;

namespace Ohana3DS_Transfigured.Ohana.Containers
{
    class DARC
    {
        private struct FileEntry
        {
            public uint nameOffset;
            public byte flags;
            public uint offset;
            public uint length;
        }

        /// <summary>
        ///     Reads a DARC archive.
        /// </summary>
        /// <param name="fileName">The File Name where the data is located</param>
        /// <returns>The container data</returns>
        public static OContainer Load(string fileName)
        {
            return Load(new FileStream(fileName, FileMode.Open));
        }

        /// <summary>
        ///     Reads a DARC archive.
        /// </summary>
        /// <param name="data">Stream of the data</param>
        /// <returns>The container data</returns>
        public static OContainer Load(Stream data)
        {
            OContainer output = new OContainer();
            BinaryReader input = new BinaryReader(data);

            string darcMagic = IOUtils.ReadStringWithLength(input, 4);
            ushort endian = input.ReadUInt16();
            ushort headerLength = input.ReadUInt16();
            uint version = input.ReadUInt32();
            uint fileSize = input.ReadUInt32();
            uint tableOffset = input.ReadUInt32();
            uint tableLength = input.ReadUInt32();
            uint dataOffset = input.ReadUInt32();

            data.Seek(tableOffset, SeekOrigin.Begin);
            FileEntry root = GetEntry(input);
            int baseOffset = (int)data.Position;
            int namesOffset = (int)(tableOffset + root.length * 0xc);

            string currDir = null;
            for (int i = 0; i < root.length - 1; i++)
            {
                data.Seek(baseOffset + i * 0xc, SeekOrigin.Begin);

                FileEntry entry = GetEntry(input);

                if ((entry.flags & 1) > 0)
                {
                    //Folder
                    int index = i;
                    currDir = null;
                    for (;;)
                    {
                        uint parentIndex = entry.offset;
                        currDir = GetName(input, entry.nameOffset + namesOffset) + "/" + currDir;
                        if (parentIndex == 0 || parentIndex == index) break;
                        data.Seek(baseOffset + parentIndex * 0xc, SeekOrigin.Begin);
                        entry = GetEntry(input);
                        index = (int)parentIndex;
                    }

                    continue;
                }

                data.Seek(entry.offset, SeekOrigin.Begin);
                byte[] buffer = new byte[entry.length];
                data.Read(buffer, 0, buffer.Length);

                OContainer.FileEntry file = new OContainer.FileEntry
                {
                    name = currDir + GetName(input, entry.nameOffset + namesOffset),
                    data = buffer
                };

                output.content.Add(file);
            }

            data.Close();
            return output;
        }
        
        private static FileEntry GetEntry(BinaryReader input)
        {
            uint flags = input.ReadUInt32();
            return new FileEntry
            {
                nameOffset = flags & 0xffffff,
                flags = (byte)(flags >> 24),
                offset = input.ReadUInt32(),
                length = input.ReadUInt32()
            };
        }

        private static string GetName(BinaryReader input, long offset)
        {
            input.BaseStream.Seek(offset, SeekOrigin.Begin);
            string name = null;
            ushort character = 0;
            while ((character = input.ReadUInt16()) > 0)
            {
                name += (char)character;
            }

            return name;
        }
    }
}

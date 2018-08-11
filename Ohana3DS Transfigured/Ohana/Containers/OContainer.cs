using System.Collections.Generic;
using System.IO;

namespace Ohana3DS_Transfigured.Ohana.Containers
{
    public class OContainer
    {
        public struct FileEntry
        {
            public string name;
            public byte[] data;

            public bool loadFromDisk;
            public uint fileOffset;
            public uint fileLength;
        }

        public Stream data;
        public List<FileEntry> content;

        public OContainer()
        {
            content = new List<FileEntry>();
        }
    }
}

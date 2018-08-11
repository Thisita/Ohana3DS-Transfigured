using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Ohana3DS_Transfigured.Ohana.Textures
{
    class ZTEX
    {
        private struct TextureEntry
        {
            public string name;
            public int width, height;
            public uint offset;
            public uint length;
            public byte format;
        }

        /// <summary>
        ///     Loads a Fantasy Life ZTEX texture from a file.
        /// </summary>
        /// <param name="data">The full path to the file</param>
        /// <returns>The list of textures</returns>
        public static List<RenderBase.OTexture> Load(string fileName)
        {
            return Load(new MemoryStream(File.ReadAllBytes(fileName)));
        }

        /// <summary>
        ///     Loads a Fantasy Life ZTEX texture from a Stream.
        /// </summary>
        /// <param name="data">The Stream with the data</param>
        /// <returns>The list of textures</returns>
        public static List<RenderBase.OTexture> Load(Stream data)
        {
            List<RenderBase.OTexture> textures = new List<RenderBase.OTexture>();

            BinaryReader input = new BinaryReader(data);

            string ztexMagic = IOUtils.ReadString(input, 0, 4);
            ushort textureCount = input.ReadUInt16();
            input.ReadUInt16();
            input.ReadUInt32();

            List<TextureEntry> entries = new List<TextureEntry>();
            for (int i = 0; i < textureCount; i++)
            {
                TextureEntry entry = new TextureEntry
                {
                    name = IOUtils.ReadString(input, (uint)(0xc + (i * 0x58)))
                };
                data.Seek(0xc + (i * 0x58) + 0x40, SeekOrigin.Begin);

                input.ReadUInt32();
                entry.offset = input.ReadUInt32();
                input.ReadUInt32();
                entry.length = input.ReadUInt32();
                entry.width = input.ReadUInt16();
                entry.height = input.ReadUInt16();
                input.ReadByte();
                entry.format = input.ReadByte();
                input.ReadUInt16();

                entries.Add(entry);
            }

            foreach (TextureEntry entry in entries)
            {
                data.Seek(entry.offset, SeekOrigin.Begin);
                byte[] buffer = new byte[entry.length];
                data.Read(buffer, 0, buffer.Length);

                Bitmap bmp = null;
                switch (entry.format)
                {
                    case 1: bmp = TextureCodec.Decode(buffer, entry.width, entry.height, RenderBase.OTextureFormat.rgb565); break;
                    case 5: bmp = TextureCodec.Decode(buffer, entry.width, entry.height, RenderBase.OTextureFormat.rgba4); break;
                    case 9: bmp = TextureCodec.Decode(buffer, entry.width, entry.height, RenderBase.OTextureFormat.rgba8); break;
                    case 0x18: bmp = TextureCodec.Decode(buffer, entry.width, entry.height, RenderBase.OTextureFormat.etc1); break;
                    case 0x19: bmp = TextureCodec.Decode(buffer, entry.width, entry.height, RenderBase.OTextureFormat.etc1a4); break;
                }
                
                textures.Add(new RenderBase.OTexture(bmp, entry.name));
            }

            data.Close();

            return textures;
        }
    }
}

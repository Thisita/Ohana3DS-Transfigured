using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Ohana3DS_Transfigured.GUI.Forms;
using Ohana3DS_Transfigured.Ohana.Models;
using Ohana3DS_Transfigured.Ohana.Models.GenericFormats;
using Ohana3DS_Transfigured.Ohana.Models.PocketMonsters;
using Ohana3DS_Transfigured.Ohana.Textures;
using Ohana3DS_Transfigured.Ohana.Textures.PocketMonsters;
using Ohana3DS_Transfigured.Ohana.Compressions;
using Ohana3DS_Transfigured.Ohana.Containers;
using Ohana3DS_Transfigured.Ohana.Animations;
using Ohana3DS_Transfigured.Ohana.Animations.PocketMonsters;
using Ohana3DS_Rebirth.Ohana.Models.PocketMonsters;

namespace Ohana3DS_Transfigured.Ohana
{
    public class FileIO
    {
        [Flags]
        public enum FormatType : uint
        {
            unsupported = 0,
            compression = 1 << 0,
            container = 1 << 1,
            image = 1 << 2,
            model = 1 << 3,
            texture = 1 << 4,
            anims = 1 << 5,
            animation = 0x20,
            all = 0xffffffff
        }

        public struct File
        {
            public object data;
            public FormatType type;
        }

        public static File Load(string fileName)
        {
            switch (Path.GetExtension(fileName).ToLower())
            {
                case ".mbn": return new File { data = MBN.Load(fileName), type = FormatType.model };
                case ".xml": return new File { data = NLP.Load(fileName), type = FormatType.model };
                default: return Load(new FileStream(fileName, FileMode.Open));
            }
        }

        public static File Load(Stream data)
        {
            //Too small
            if (data.Length < 0x10)
            {
                data.Close();
                return new File { type = FormatType.unsupported };
            }

            BinaryReader input = new BinaryReader(data);
            uint magic, length;

            switch (Peek(input))
            {
                case 0x00010000: return new File { data = GfModel.Load(data), type = FormatType.model };
                case 0x00060000: return new File { data = GfMotion.LoadAnim(input), type = FormatType.anims };
                case 0x15041213: return new File { data = GfTexture.Load(data), type = FormatType.image };
                case 0x15122117:
                    RenderBase.OModelGroup mdls = new RenderBase.OModelGroup();
                    mdls.model.Add(GfModel.LoadModel(data));
                    return new File { data = mdls, type = FormatType.model };
            }

            switch (GetMagic(input, 5))
            {
                case "MODEL": return new File { data = DQVIIPack.Load(data), type = FormatType.container };
            }

            switch (GetMagic(input, 4))
            {
                case "CGFX": return new File { data = CGFX.Load(data), type = FormatType.model };
                case "CRAG": return new File { data = GARC.Load(data), type = FormatType.container };
                case "darc": return new File { data = DARC.Load(data), type = FormatType.container };
                case "FPT0": return new File { data = FPT0.Load(data), type = FormatType.container };
                case "IECP":
                    magic = input.ReadUInt32();
                    length = input.ReadUInt32();
                    return Load(new MemoryStream(LZSS.Decompress(data, length)));
                case "NLK2":
                    data.Seek(0x80, SeekOrigin.Begin);
                    return new File
                    {
                        data = CGFX.Load(data),
                        type = FormatType.model
                    };
                case "SARC": return new File { data = SARC.Load(data), type = FormatType.container };
                case "SMES": return new File { data = NLP.LoadMesh(data), type = FormatType.model };
                case "Yaz0":
                    magic = input.ReadUInt32();
                    length = IOUtils.EndianSwap(input.ReadUInt32());
                    data.Seek(8, SeekOrigin.Current);
                    return Load(new MemoryStream(Yaz0.Decompress(data, length)));
                case "zmdl": return new File { data = ZMDL.Load(data), type = FormatType.model };
                case "ztex": return new File { data = ZTEX.Load(data), type = FormatType.texture };
            }

            //Check if is a BCLIM or BFLIM file (header on the end)
            if (data.Length > 0x28)
            {
                data.Seek(-0x28, SeekOrigin.End);
                string clim = IOUtils.ReadStringWithLength(input, 4);
                if (clim == "CLIM" || clim == "FLIM") return new File { data = BCLIM.Load(data), type = FormatType.image };
            }

            switch (GetMagic(input, 3))
            {
                case "BCH":
                    byte[] buffer = new byte[data.Length];
                    input.Read(buffer, 0, buffer.Length);
                    data.Close();
                    return new File
                    {
                        data = BCH.Load(new MemoryStream(buffer)),
                        type = FormatType.model
                    };
                case "DMP": return new File { data = DMP.Load(data), type = FormatType.image };
            }

            string magic2b = GetMagic(input, 2);

            switch (magic2b)
            {
                case "AD": return new File { data = AD.Load(data), type = FormatType.model };
                case "BM": return new File { data = MM.Load(data), type = FormatType.model };
                case "BS": return new File { data = BS.Load(data), type = FormatType.anims };
                case "CM": return new File { data = CM.Load(data), type = FormatType.model };
                case "CP": return new File { data = CP.Load(data), type = FormatType.model };
                case "GR": return new File { data = GR.Load(data), type = FormatType.model };
                case "MM": return new File { data = MM.Load(data), type = FormatType.model };
                case "PC": return new File { data = PC.Load(data), type = FormatType.model };
                case "PT": return new File { data = PT.Load(data), type = FormatType.texture };
            }

            if (magic2b.Length == 2)
            {
                if ((magic2b[0] >= 'A' && magic2b[0] <= 'Z') &&
                    (magic2b[1] >= 'A' && magic2b[1] <= 'Z'))
                {
                    return new File { data = PkmnContainer.Load(data), type = FormatType.container };
                }
            }

            //Compressions
            data.Seek(0, SeekOrigin.Begin);
            uint cmp = input.ReadUInt32();
            if ((cmp & 0xff) == 0x13) cmp = input.ReadUInt32();
            switch (cmp & 0xff)
            {
                case 0x11: return Load(new MemoryStream(LZSS_Ninty.Decompress(data, cmp >> 8)));
                case 0x90:
                    byte[] buffer = BLZ.Decompress(data);
                    byte[] newData = new byte[buffer.Length - 1];
                    Buffer.BlockCopy(buffer, 1, newData, 0, newData.Length);
                    return Load(new MemoryStream(newData));
            }

            data.Close();
            return new File { type = FormatType.unsupported };
        }

        public static string GetExtension(byte[] data, int startIndex = 0)
        {
            if (data.Length > 3 + startIndex)
            {
                switch (GetMagic(data, 4, startIndex))
                {
                    case "CGFX": return ".bcres";
                }
            }

            if (data.Length > 2 + startIndex)
            {
                switch (GetMagic(data, 3, startIndex))
                {
                    case "BCH": return ".bch";
                }
            }

            if (data.Length > 1 + startIndex)
            {
                switch (GetMagic(data, 2, startIndex))
                {
                    case "AD": return ".ad";
                    case "BG": return ".bg";
                    case "BM": return ".bm";
                    case "BS": return ".bs";
                    case "CM": return ".cm";
                    case "GR": return ".gr";
                    case "MM": return ".mm";
                    case "PB": return ".pb";
                    case "PC": return ".pc";
                    case "PF": return ".pf";
                    case "PK": return ".pk";
                    case "PO": return ".po";
                    case "PT": return ".pt";
                    case "TM": return ".tm";
                }
            }

            return ".bin";
        }

        private static uint Peek(BinaryReader input)
        {
            uint value = input.ReadUInt32();
            input.BaseStream.Seek(-4, SeekOrigin.Current);
            return value;
        }

        private static string GetMagic(BinaryReader input, uint length)
        {
            string magic = IOUtils.ReadString(input, 0, length);
            input.BaseStream.Seek(0, SeekOrigin.Begin);
            return magic;
        }

        public static string GetMagic(byte[] data, int length, int startIndex = 0)
        {
            return Encoding.ASCII.GetString(data, startIndex, length);
        }

        public enum FileType
        {
            none,
            model,
            texture,
            skeletalAnimation,
            materialAnimation,
            visibilityAnimation
        }

        /// <summary>
        ///     Imports a file of the given type.
        ///     Returns data relative to the chosen type.
        /// </summary>
        /// <param name="type">The type of the data</param>
        /// <returns></returns>
        public static object Import(FileType type)
        {
            using (OpenFileDialog openDlg = new OpenFileDialog())
            {
                openDlg.Multiselect = true;

                switch (type)
                {
                    case FileType.model:
                        openDlg.Title = "Import models";
                        openDlg.Filter = "All files|*.*";

                        if (openDlg.ShowDialog() == DialogResult.OK)
                        {
                            List<RenderBase.OModel> output = new List<RenderBase.OModel>();
                            foreach (string fileName in openDlg.FileNames)
                            {
                                output.AddRange(((RenderBase.OModelGroup)Load(fileName).data).model);
                            }
                            return output;
                        }
                        break;
                    case FileType.texture:
                        openDlg.Title = "Import textures";
                        openDlg.Filter = "All files|*.*";

                        if (openDlg.ShowDialog() == DialogResult.OK)
                        {
                            List<RenderBase.OTexture> output = new List<RenderBase.OTexture>();
                            foreach (string fileName in openDlg.FileNames)
                            {
                                File file = Load(fileName);
                                switch (file.type)
                                {
                                    case FormatType.model: output.AddRange(((RenderBase.OModelGroup)file.data).texture); break;
                                    case FormatType.texture: output.AddRange((List<RenderBase.OTexture>)file.data); break;
                                    case FormatType.image: output.Add((RenderBase.OTexture)file.data); break;
                                }
                            }
                            return output;
                        }
                        break;
                    case FileType.skeletalAnimation:
                        openDlg.Title = "Import skeletal animations";
                        openDlg.Filter = "All files|*.*";

                        if (openDlg.ShowDialog() == DialogResult.OK)
                        {
                            RenderBase.OAnimationListBase output = new RenderBase.OAnimationListBase();
                            foreach (string fileName in openDlg.FileNames)
                            {
                                output.list.AddRange(((RenderBase.OModelGroup)Load(fileName).data).skeletalAnimation.list);
                            }
                            return output;
                        }
                        break;
                    case FileType.materialAnimation:
                        openDlg.Title = "Import material animations";
                        openDlg.Filter = "All files|*.*";

                        if (openDlg.ShowDialog() == DialogResult.OK)
                        {
                            RenderBase.OAnimationListBase output = new RenderBase.OAnimationListBase();
                            foreach (string fileName in openDlg.FileNames)
                            {
                                output.list.AddRange(((RenderBase.OModelGroup)Load(fileName).data).materialAnimation.list);
                            }
                            return output;
                        }
                        break;
                    case FileType.visibilityAnimation:
                        openDlg.Title = "Import visibility animations";
                        openDlg.Filter = "All files|*.*";

                        if (openDlg.ShowDialog() == DialogResult.OK)
                        {
                            RenderBase.OAnimationListBase output = new RenderBase.OAnimationListBase();
                            foreach (string fileName in openDlg.FileNames)
                            {
                                output.list.AddRange(((RenderBase.OModelGroup)Load(fileName).data).visibilityAnimation.list);
                            }
                            return output;
                        }
                        break;
                }
            }

            return null;
        }

        /// <summary>
        ///     Exports a file of a given type.
        ///     Formats available to export will depend on the type of the data.
        /// </summary>
        /// <param name="type">Type of the data to be exported</param>
        /// <param name="data">The data</param>
        /// <param name="arguments">Optional arguments to be used by the exporter</param>
        public static void Export(FileType type, object data, params object[] arguments)
        {
            using (SaveFileDialog saveDlg = new SaveFileDialog())
            {
                switch (type)
                {
                    case FileType.model:
                        OModelExportForm exportMdl = new OModelExportForm((RenderBase.OModelGroup)data, (int)arguments[0]);
                        exportMdl.Show();
                        break;
                    case FileType.texture:
                        OTextureExportForm exportTex = new OTextureExportForm((RenderBase.OModelGroup)data, (int)arguments[0]);
                        exportTex.Show();
                        break;
                    case FileType.skeletalAnimation:
                        saveDlg.Title = "Export Skeletal Animation";
                        saveDlg.Filter = "Source Model|*.smd";
                        if (saveDlg.ShowDialog() == DialogResult.OK)
                        {
                            switch (saveDlg.FilterIndex)
                            {
                                case 1:
                                    SMD.Export((RenderBase.OModelGroup)data, saveDlg.FileName, (int)arguments[0], (int)arguments[1]);
                                    break;
                            }
                        }
                        break;
                }
            }
        }
    }
}

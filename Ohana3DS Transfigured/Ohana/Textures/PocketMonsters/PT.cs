using System.Collections.Generic;
using System.IO;

using Ohana3DS_Transfigured.Ohana.Containers;

namespace Ohana3DS_Transfigured.Ohana.Textures.PocketMonsters
{
    class PT
    {
        public static List<RenderBase.OTexture> Load(string file)
        {
            return Load(File.Open(file, FileMode.Open));
        }

        /// <summary>
        ///     Loads all monster textures on a PT Pokémon container.
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>The monster textures</returns>
        public static List<RenderBase.OTexture> Load(Stream data)
        {
            List<RenderBase.OTexture> textures = new List<RenderBase.OTexture>();
            RenderBase.OModelGroup models = new RenderBase.OModelGroup();

            OContainer container = PkmnContainer.Load(data);
            for (int i = 0; i < container.content.Count; i++)
            {
                FileIO.File file = FileIO.Load(new MemoryStream(container.content[i].data));
                if (file.type == FileIO.FormatType.model) textures.AddRange(((RenderBase.OModelGroup)file.data).texture);
            }

            return textures;
        }
    }
}
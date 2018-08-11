using System.IO;

using Ohana3DS_Transfigured.Ohana.Containers;

namespace Ohana3DS_Transfigured.Ohana.Textures.PocketMonsters
{
    class AD
    {
        /// <summary>
        ///     Loads all map textures (and other data) on a AD Pokémon container.
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>The Model group with textures and stuff</returns>
        public static RenderBase.OModelGroup Load(Stream data)
        {
            RenderBase.OModelGroup models = new RenderBase.OModelGroup();

            OContainer container = PkmnContainer.Load(data);
            for (int i = 1; i < container.content.Count; i++)
            {
                FileIO.File file = FileIO.Load(new MemoryStream(container.content[i].data));
                if (file.type == FileIO.FormatType.model) models.Merge((RenderBase.OModelGroup)file.data);
            }

            return models;
        }
    }
}

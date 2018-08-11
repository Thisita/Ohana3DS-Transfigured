using System.IO;

using Ohana3DS_Transfigured.Ohana.Containers;

namespace Ohana3DS_Transfigured.Ohana.Models.PocketMonsters
{
    class PC
    {
        public static RenderBase.OModelGroup Load(string file)
        {
            RenderBase.OModelGroup group = Load(File.Open(file, FileMode.Open));

            return group;
        }

        /// <summary>
        ///     Loads a PC monster model from Pokémon.
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>The Model group with the monster meshes</returns>
        public static RenderBase.OModelGroup Load(Stream data)
        {
            RenderBase.OModelGroup models = new RenderBase.OModelGroup();

            OContainer container = PkmnContainer.Load(data);

            models = BCH.Load(new MemoryStream(container.content[0].data));

            return models;
        }
    }
}

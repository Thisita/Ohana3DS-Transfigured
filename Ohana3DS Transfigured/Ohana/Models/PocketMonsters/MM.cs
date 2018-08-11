using System.IO;

using Ohana3DS_Transfigured.Ohana.Containers;

namespace Ohana3DS_Transfigured.Ohana.Models.PocketMonsters
{
    class MM
    {
        /// <summary>
        ///     Loads a MM overworld chibi character model from Pokémon.
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>The Model group with the character meshes</returns>
        public static RenderBase.OModelGroup Load(Stream data)
        {
            RenderBase.OModelGroup models = new RenderBase.OModelGroup();

            OContainer container = PkmnContainer.Load(data);
            models = BCH.Load(new MemoryStream(container.content[0].data));

            return models;
        }
    }
}

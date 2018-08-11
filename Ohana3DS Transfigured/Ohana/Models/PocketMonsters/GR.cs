﻿using System.IO;

using Ohana3DS_Transfigured.Ohana.Containers;

namespace Ohana3DS_Transfigured.Ohana.Models.PocketMonsters
{
    class GR
    {
        /// <summary>
        ///     Loads a GR map model from Pokémon.
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>The Model group with the map meshes</returns>
        public static RenderBase.OModelGroup Load(Stream data)
        {
            RenderBase.OModelGroup models;

            OContainer container = PkmnContainer.Load(data);
            models = BCH.Load(new MemoryStream(container.content[1].data));

            return models;
        }
    }
}

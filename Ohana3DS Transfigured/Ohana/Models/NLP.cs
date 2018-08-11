//New Love Plus public Model loading methods.
using System.IO;

using Ohana3DS_Transfigured.Ohana.Models.NewLovePlus;

namespace Ohana3DS_Transfigured.Ohana.Models
{
    class NLP
    {
        /// <summary>
        ///     Loads a New Love Plus Model file.
        /// </summary>
        /// <param name="fileName">File Name of the XML Model file</param>
        /// <returns></returns>
        public static RenderBase.OModelGroup Load(string fileName)
        {
            return Model.Load(fileName);
        }

        /// <summary>
        ///     Loads a New Love Plus Mesh file.
        ///     Only the raw Mesh data will be loaded, without materials, skeleton or textures.
        /// </summary>
        /// <param name="data">Stream of the Mesh</param>
        /// <returns></returns>
        public static RenderBase.OModelGroup LoadMesh(Stream data)
        {
            RenderBase.OModelGroup models = new RenderBase.OModelGroup();
            RenderBase.OModel model = new RenderBase.OModel();
            Mesh.Load(data, model, true);
            model.name = "model";

            model.material.Add(new RenderBase.OMaterial());
            models.model.Add(model);
            return models;
        }

        /// <summary>
        ///     Loads a New Love Plus Texture file.
        /// </summary>
        /// <param name="data">XML Stream of the Texture</param>
        /// <returns></returns>
        public static RenderBase.OTexture LoadTexture(string fileName)
        {
            return Model.LoadTexture(fileName);
        }
    }
}

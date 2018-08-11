﻿using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Ohana3DS_Transfigured.Ohana.Models.GenericFormats
{
    class OBJ
    {
        /// <summary>
        ///     Exports a model to the Wavefront OBJ format.
        /// </summary>
        /// <param name="model">The model to be exported</param>
        /// <param name="fileName">The output file name</param>
        /// <param name="modelIndex">The index of the model that should be exported</param>
        public static void Export(RenderBase.OModelGroup model, string fileName, int modelIndex)
        {
            StringBuilder output = new StringBuilder();

            RenderBase.OModel mdl = model.model[modelIndex];

            int faceIndexBase = 1;
            for (int objIndex = 0; objIndex < mdl.mesh.Count; objIndex++)
            {
                output.AppendLine("g " + mdl.mesh[objIndex].name);
                output.AppendLine(null);

                output.AppendLine("usemtl " + mdl.material[mdl.mesh[objIndex].materialId].name0 + ".png");
                output.AppendLine(null);

                MeshUtils.OptimizedMesh obj = MeshUtils.OptimizeMesh(mdl.mesh[objIndex]);
                foreach (RenderBase.OVertex vertex in obj.vertices)
                {
                    output.AppendLine("v " + GetString(vertex.position.x) + " " + GetString(vertex.position.y) + " " + GetString(vertex.position.z));
                    output.AppendLine("vn " + GetString(vertex.normal.x) + " " + GetString(vertex.normal.y) + " " + GetString(vertex.normal.z));
                    output.AppendLine("vt " + GetString(vertex.texture0.x) + " " + GetString(vertex.texture0.y));
                }
                output.AppendLine(null);

                for (int i = 0; i < obj.indices.Count; i += 3)
                {
                    output.AppendLine(
                        string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                        faceIndexBase + obj.indices[i],
                        faceIndexBase + obj.indices[i + 1],
                        faceIndexBase + obj.indices[i + 2]));
                }
                faceIndexBase += obj.vertices.Count;
                output.AppendLine(null);
            }

            File.WriteAllText(fileName, output.ToString());
        }

        /// <summary>
        ///     Transforms a Float into a String that will always have "." into decimal places,
        ///     even if the region uses ",".
        /// </summary>
        /// <param name="value">The Float value</param>
        /// <returns></returns>
        private static string GetString(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}

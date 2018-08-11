using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Ohana3DS_Transfigured.Ohana.Models.GenericFormats
{
    public class DAE
    {
        [XmlRootAttribute("COLLADA", Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
        public class COLLADA
        {
            [XmlAttribute]
            public string version = "1.4.1";

            public DaeAsset asset = new DaeAsset();

            [XmlArrayItem("image")]
            public List<DaeImage> library_images = new List<DaeImage>();

            [XmlArrayItem("material")]
            public List<DaeMaterial> library_materials = new List<DaeMaterial>();

            [XmlArrayItem("effect")]
            public List<DaeEffect> library_effects = new List<DaeEffect>();

            [XmlArrayItem("geometry")]
            public List<DaeGeometry> library_geometries = new List<DaeGeometry>();

            [XmlArrayItem("controller")]
            public List<DaeController> library_controllers;

            [XmlArrayItem("visual_scene")]
            public List<DaeVisualScene> library_visual_scenes = new List<DaeVisualScene>();

            [XmlArrayItem("instance_visual_scene")]
            public List<DaeInstaceVisualScene> scene = new List<DaeInstaceVisualScene>();
        }

        public class DaeAsset
        {
            public string created;
            public string modified;
        }

        public class DaeImage
        {
            [XmlAttribute]
            public string id;

            [XmlAttribute]
            public string name;

            public string init_from;
        }

        public class DaeInstanceEffect
        {
            [XmlAttribute]
            public string url;
        }

        public class DaeMaterial
        {
            [XmlAttribute]
            public string id;

            [XmlAttribute]
            public string name;

            public DaeInstanceEffect instance_effect = new DaeInstanceEffect();
        }

        public class DaeParamSurfaceElement
        {
            [XmlAttribute]
            public string type;

            public string init_from;
            public string format;
        }

        public class DaeParamSampler2DElement
        {
            public string source;
            public string wrap_s;
            public string wrap_t;
            public string minfilter;
            public string magfilter;
            public string mipfilter;
        }

        public class DaeParam
        {
            [XmlAttribute]
            public string sid;

            [XmlElement(IsNullable = false)]
            public DaeParamSurfaceElement surface;

            [XmlElement(IsNullable = false)]
            public DaeParamSampler2DElement sampler2D;
        }

        public class DaePhongDiffuseTexture
        {
            [XmlAttribute]
            public string texture;

            [XmlAttribute]
            public string texcoord = "uv";
        }

        public class DaePhongDiffuse
        {
            public DaePhongDiffuseTexture texture = new DaePhongDiffuseTexture();
        }

        public class DaePhongTransparent
        {
            [XmlElement("float")]
            public string value;
        }

        public class DaeColor
        {
            public string color;

            public void Set(Color col)
            {
                color = string.Format(
                    "{0} {1} {2} {3}",
                    GetString(col.R / 255f),
                    GetString(col.G / 255f),
                    GetString(col.B / 255f),
                    GetString(col.A / 255f));
            }

            private string GetString(float value)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public class DaePhong
        {
            public DaeColor emission = new DaeColor();
            public DaeColor ambient = new DaeColor();
            public DaePhongDiffuse diffuse = new DaePhongDiffuse();
            public DaePhongTransparent transparency = new DaePhongTransparent();
            public DaeColor specular = new DaeColor();
        }

        public class DaeTechnique
        {
            [XmlAttribute]
            public string sid;

            public DaePhong phong = new DaePhong();
        }

        public class DaeProfile
        {
            [XmlAttribute]
            public string sid;

            [XmlElement("newparam")]
            public List<DaeParam> newparam = new List<DaeParam>();
            public DaeTechnique technique = new DaeTechnique();
        }

        public class DaeEffect
        {
            [XmlAttribute]
            public string id;

            [XmlAttribute]
            public string name;

            public DaeProfile profile_COMMON = new DaeProfile();
        }

        public class DaeFloatArray
        {
            [XmlAttribute]
            public string id;

            [XmlAttribute]
            public uint count;

            [XmlText]
            public string data;

            public void Set(List<float> content)
            {
                StringBuilder strArray = new StringBuilder();
                for (int i = 0; i < content.Count; i++)
                {
                    if (i < content.Count - 1)
                        strArray.Append(content[i].ToString(CultureInfo.InvariantCulture) + " ");
                    else
                        strArray.Append(content[i].ToString(CultureInfo.InvariantCulture));
                }
                count = (uint)content.Count;
                data = strArray.ToString();
            }

            public List<float> Get()
            {
                List<float> output = new List<float>();
                string[] values = data.Split(Convert.ToChar(" "));
                for (int i = 0; i < values.Length; i++) output.Add(float.Parse(values[i]));
                return output;
            }
        }

        public class DaeNameArray
        {
            [XmlAttribute]
            public string id;

            [XmlAttribute]
            public uint count;

            [XmlText]
            public string data;

            public void Set(List<string> content)
            {
                StringBuilder strArray = new StringBuilder();
                for (int i = 0; i < content.Count; i++)
                {
                    if (i < content.Count - 1)
                        strArray.Append(content[i] + " ");
                    else
                        strArray.Append(content[i]);
                }
                count = (uint)content.Count;
                data = strArray.ToString();
            }

            public List<string> Get()
            {
                List<string> output = new List<string>();
                string[] values = data.Split(Convert.ToChar(" "));
                output.AddRange(values);
                return output;
            }
        }

        public class DaeAccessorParam
        {
            [XmlAttribute]
            public string name;

            [XmlAttribute]
            public string type;
        }

        public class DaeAccessor
        {
            [XmlAttribute]
            public string source;

            [XmlAttribute]
            public uint count;

            [XmlAttribute]
            public uint stride;

            [XmlElement("param")]
            public List<DaeAccessorParam> param = new List<DaeAccessorParam>();

            public void AddParam(string name, string type)
            {
                DaeAccessorParam prm = new DaeAccessorParam
                {
                    name = name,
                    type = type
                };

                param.Add(prm);
            }
        }

        public class DaeMeshTechnique
        {
            public DaeAccessor accessor = new DaeAccessor();
        }

        public class DaeSource
        {
            [XmlAttribute]
            public string id;

            [XmlAttribute]
            public string name;

            [XmlElement(IsNullable = false)]
            public DaeNameArray Name_array;

            [XmlElement(IsNullable = false)]
            public DaeFloatArray float_array;

            public DaeMeshTechnique technique_common = new DaeMeshTechnique();
        }

        public class DaeInput
        {
            [XmlAttribute]
            public string semantic;

            [XmlAttribute]
            public string source;
        }

        public class DaeInputTable
        {
            [XmlAttribute]
            public string id;

            [XmlElement("input")]
            public List<DaeInput> input = new List<DaeInput>();

            public void AddInput(string semantic, string source)
            {
                DaeInput i = new DaeInput
                {
                    semantic = semantic,
                    source = source
                };

                input.Add(i);
            }
        }

        public class DaeInputOffset
        {
            [XmlAttribute]
            public string semantic;

            [XmlAttribute]
            public string source;

            [XmlAttribute]
            public uint offset;

            [XmlAttribute]
            public uint set;

            public bool ShouldSerializeset() { return semantic == "TEXCOORD"; }
        }

        public class DaeTriangles
        {
            [XmlAttribute]
            public string material;

            [XmlAttribute]
            public uint count;

            [XmlElement("input")]
            public List<DaeInputOffset> input = new List<DaeInputOffset>();

            public string p;

            public void AddInput(string semantic, string source, uint offset = 0, uint set = 0)
            {
                DaeInputOffset i = new DaeInputOffset
                {
                    semantic = semantic,
                    source = source,
                    offset = offset,
                    set = set
                };

                input.Add(i);
            }

            public void Set(List<uint> indices)
            {
                StringBuilder strArray = new StringBuilder();
                for (int i = 0; i < indices.Count; i++)
                {
                    if (i < indices.Count - 1)
                        strArray.Append(indices[i].ToString(CultureInfo.InvariantCulture) + " ");
                    else
                        strArray.Append(indices[i].ToString(CultureInfo.InvariantCulture));
                }
                count = (uint)(indices.Count / 3);
                p = strArray.ToString();
            }

            public List<uint> Get()
            {
                List<uint> output = new List<uint>();
                string[] values = p.Split(Convert.ToChar(" "));
                for (int i = 0; i < values.Length; i++) output.Add(uint.Parse(values[i]));
                return output;
            }
        }

        public class DaeMesh
        {
            [XmlElement("source")]
            public List<DaeSource> source = new List<DaeSource>();

            public DaeInputTable vertices = new DaeInputTable();
            public DaeTriangles triangles = new DaeTriangles();
        }

        public class DaeGeometry
        {
            [XmlAttribute]
            public string id;

            [XmlAttribute]
            public string name;

            public DaeMesh mesh = new DaeMesh();
        }

        public class DaeVertexWeights
        {
            [XmlAttribute]
            public uint count;

            [XmlElement("input")]
            public List<DaeInputOffset> input = new List<DaeInputOffset>();

            public string vcount;
            public string v;

            public void AddInput(string semantic, string source, uint offset = 0)
            {
                DaeInputOffset i = new DaeInputOffset
                {
                    semantic = semantic,
                    source = source,
                    offset = offset
                };

                input.Add(i);
            }
        }

        public class DaeSkin
        {
            [XmlAttribute]
            public string source;

            public DaeMatrix bind_shape_matrix = new DaeMatrix();

            [XmlElement("source")]
            public List<DaeSource> src = new List<DaeSource>();

            public DaeInputTable joints = new DaeInputTable();
            public DaeVertexWeights vertex_weights = new DaeVertexWeights();
        }

        public class DaeController
        {
            [XmlAttribute]
            public string id;

            public DaeSkin skin = new DaeSkin();
        }

        public class DaeMatrix
        {
            [XmlText]
            public string data;

            public void Set(RenderBase.OMatrix mtx)
            {
                StringBuilder strArray = new StringBuilder();
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (i == 3 && j == 3)
                            strArray.Append(mtx[j, i].ToString(CultureInfo.InvariantCulture));
                        else
                            strArray.Append(mtx[j, i].ToString(CultureInfo.InvariantCulture) + " ");
                    }

                }
                data = strArray.ToString();
            }

            public RenderBase.OMatrix Get()
            {
                RenderBase.OMatrix output = new RenderBase.OMatrix();
                string[] values = data.Split(Convert.ToChar(" "));
                int k = 0;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        output[j, i] = float.Parse(values[k++]);
                    }

                }
                return output;
            }
        }

        public class DaeBindMaterialInstace
        {
            [XmlAttribute]
            public string symbol;

            [XmlAttribute]
            public string target;
        }

        public class DaeBindMaterial
        {
            public DaeBindMaterialInstace instance_material = new DaeBindMaterialInstace();
        }

        public class DaeBindMaterialTechnique
        {
            public DaeBindMaterial technique_common = new DaeBindMaterial();
        }

        public class DaeInstanceGeometry
        {
            [XmlAttribute]
            public string url;

            public DaeBindMaterialTechnique bind_material = new DaeBindMaterialTechnique();
        }

        public class DaeInstanceController
        {
            [XmlAttribute]
            public string url;

            public string skeleton;
            public DaeBindMaterialTechnique bind_material = new DaeBindMaterialTechnique();
        }

        public class DaeNode
        {
            [XmlAttribute]
            public string id;

            [XmlAttribute]
            public string name;

            [XmlAttribute]
            public string sid;

            [XmlAttribute]
            public string type = "NODE";

            public DaeMatrix matrix = new DaeMatrix();

            [XmlElement("node", IsNullable = false)]
            public List<DaeNode> childs;

            [XmlElement(IsNullable = false)]
            public DaeInstanceGeometry instance_geometry;

            [XmlElement(IsNullable = false)]
            public DaeInstanceController instance_controller;
        }

        public class DaeVisualScene
        {
            [XmlAttribute]
            public string id;

            [XmlAttribute]
            public string name;

            [XmlElement("node")]
            public List<DaeNode> node = new List<DaeNode>();
        }

        public class DaeInstaceVisualScene
        {
            [XmlAttribute]
            public string url;
        }

        /// <summary>
        ///     Exports a Model to the Collada format.
        ///     See: https://www.khronos.org/files/collada_spec_1_4.pdf for more information.
        /// </summary>
        /// <param name="model">The Model that will be exported</param>
        /// <param name="fileName">The output File Name</param>
        /// <param name="modelIndex">Index of the model to be exported</param>
        /// <param name="skeletalAnimationIndex">(Optional) Index of the skeletal animation</param>
        public static void Export(RenderBase.OModelGroup model, string fileName, int modelIndex, int skeletalAnimationIndex = -1)
        {
            RenderBase.OModel mdl = model.model[modelIndex];
            COLLADA dae = new COLLADA();
            COLLADA daeShiny = new COLLADA();

            dae.asset.created = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ssZ");
            dae.asset.modified = dae.asset.created;

            daeShiny.asset.created = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ssZ");
            daeShiny.asset.modified = dae.asset.created;

            bool hasShiny = false;

            // Attempt to detect packaged textures. See http://www.github.com/Quibilia/Ohana3DS-Transfigured for directions.
            try
            {
                string modelDir;
                string sDir;
                string[] texDirs;
                string[] texFiles;

                modelDir = Path.GetFileNameWithoutExtension(fileName).Remove(4);
                sDir = Path.GetDirectoryName(fileName);
                texDirs = Directory.GetDirectories(Path.Combine(sDir, "../../Textures/"));

                foreach (string texDir in texDirs)
                {
                    if (texDir.Contains(modelDir))
                    {
                        texFiles = Directory.GetFiles(texDir);

                        if (Directory.Exists(texDir.Replace("Textures", "Models/DAE")) == false && texDir.Contains("Xtra") == false)
                        {
                            Directory.CreateDirectory(texDir.Replace("Textures", "Models/DAE"));
                        }

                        bool hasIris = false;

                        foreach (string texFile in texFiles)
                        {
                            if (texFile.Contains("Iris1"))
                            {
                                hasIris = true;
                            }
                        }

                        foreach (string texFile in texFiles)
                        {
                            if (texFile.Contains("Xtra") == false)
                            {
                                Bitmap tempBMP = (Bitmap)Bitmap.FromFile(texFile);

                                if (texFile.Contains("Body") && texFile.Contains("Nor") == false)
                                {
                                    int w = tempBMP.Width / 2;

                                    Bitmap scaledBMP = new Bitmap(tempBMP, tempBMP.Width / 2, tempBMP.Height);
                                    Bitmap reverseBMP = new Bitmap(tempBMP, tempBMP.Width / 2, tempBMP.Height);
                                    reverseBMP.RotateFlip(RotateFlipType.RotateNoneFlipX);

                                    Bitmap finalBMP = new Bitmap(tempBMP.Width, tempBMP.Height);

                                    for (int x = 0; x < scaledBMP.Width; x++)
                                    {
                                        for (int y = 0; y < scaledBMP.Height; y++)
                                        {
                                            finalBMP.SetPixel(x, y, scaledBMP.GetPixel(x, y));
                                        }
                                    }

                                    for (int x = 0; x < reverseBMP.Width; x++)
                                    {
                                        for (int y = 0; y < reverseBMP.Height; y++)
                                        {
                                            finalBMP.SetPixel(x + reverseBMP.Width, y, reverseBMP.GetPixel(x, y));
                                        }
                                    }

                                    if (texFile.Contains("BodyA2") == false && texFile.Contains("BodyB2") == false && texFile.Contains("Body2") == false)
                                    {
                                        finalBMP.Save(texFile.Replace("Textures", "Models/DAE"));

                                        model.texture.Add(new RenderBase.OTexture(finalBMP, texFile.Replace("Textures", "Models/DAE")));
                                    }
                                }
                                else if (texFile.Contains("Iris1"))
                                {
                                    Bitmap eyeBMP = (Bitmap)Bitmap.FromFile(texFile.Replace("Iris", "Eye"));

                                    foreach (string norFile in texFiles)
                                    {
                                        if (norFile.Contains("Xtra") == false && norFile.Contains("EyeNor"))
                                        {
                                            Bitmap thisEye = new Bitmap(eyeBMP.Width / 2, eyeBMP.Height / 4);
                                            Bitmap norBMP = (Bitmap)Bitmap.FromFile(norFile);
                                            Bitmap thisNor = new Bitmap(norBMP.Width / 4, norBMP.Height / 4);
                                            Bitmap finalBMP = new Bitmap(eyeBMP.Width * 2, eyeBMP.Height);

                                            int eye = 0;
                                            int half = 0;
                                            bool reflectEye = false;
                                            bool reflectIris = true;
                                            bool reflectNor = true;

                                            for (int eyeY = 0; eyeY < 4; eyeY++)
                                            {
                                                eye = 0;

                                                for (int eyeX = 0; eyeX < 2; eyeX++)
                                                {
                                                    reflectEye = !reflectEye;
                                                    reflectNor = !reflectNor;
                                                    reflectIris = !reflectIris;

                                                    for (half = 0; half < 2; half++)
                                                    {
                                                        for (int x = half * (eyeBMP.Width / 2); x < (half + 1) * (eyeBMP.Width / 2); x++)
                                                        {
                                                            for (int y = eyeY * (eyeBMP.Height / 4); y < (eyeY + 1) * (eyeBMP.Height / 4); y++)
                                                            {
                                                                thisEye.SetPixel(x - (half * (eyeBMP.Width / 2)), y - (eyeY * (eyeBMP.Height / 4)), eyeBMP.GetPixel(x, y));
                                                            }
                                                        }

                                                        for (int x = (half * 2) * (norBMP.Width / 4); x < ((half * 2) + 1) * (norBMP.Width / 4); x++)
                                                        {
                                                            for (int y = eyeY * (norBMP.Height / 4); y < (eyeY + 1) * (norBMP.Height / 4); y++)
                                                            {
                                                                thisNor.SetPixel(x - ((half * 2) * (norBMP.Width / 4)), y - (eyeY * (norBMP.Height / 4)), norBMP.GetPixel(x, y));
                                                            }
                                                        }

                                                        if (reflectEye == true)
                                                        {
                                                            thisEye.RotateFlip(RotateFlipType.RotateNoneFlipX);
                                                        }

                                                        reflectEye = !reflectEye;

                                                        if (reflectNor == true)
                                                        {
                                                            thisNor.RotateFlip(RotateFlipType.RotateNoneFlipX);
                                                        }

                                                        if (reflectIris == true)
                                                        {
                                                            tempBMP.RotateFlip(RotateFlipType.RotateNoneFlipX);
                                                            reflectIris = false;
                                                        }

                                                        // By now, tempBMP, thisEye, and thisNor should all be the same size.

                                                        for (int x = 0; x < tempBMP.Width; x++)
                                                        {
                                                            for (int y = 0; y < tempBMP.Height; y++)
                                                            {
                                                                if (tempBMP.GetPixel(x, y).A >= 0x7F && thisNor.GetPixel(x, y).A >= 0x7F)
                                                                {
                                                                    thisEye.SetPixel(x, y, tempBMP.GetPixel(x, y));
                                                                }
                                                            }
                                                        }

                                                        for (int x = eye * (norBMP.Width / 4); x < (eye + 1) * (norBMP.Width / 4); x++)
                                                        {
                                                            for (int y = eyeY * (norBMP.Height / 4); y < (eyeY + 1) * (norBMP.Height / 4); y++)
                                                            {
                                                                finalBMP.SetPixel(x, y, thisEye.GetPixel(x - (eye * (norBMP.Width / 4)), y - (eyeY * (norBMP.Height / 4))));
                                                            }
                                                        }

                                                        if (eye == 3)
                                                        {
                                                            eye--;
                                                        }
                                                        else if (eye == 1)
                                                        {
                                                            eye += 2;
                                                        }
                                                        else if (eye == 0)
                                                        {
                                                            eye++;
                                                        }
                                                        else if (eye == 2)
                                                        {
                                                            eye -= 2;
                                                        }
                                                    }
                                                }
                                            }

                                            finalBMP.Save(texFile.Replace("Textures", "Models/DAE"));
                                            model.texture.Add(new RenderBase.OTexture(finalBMP, texFile.Replace("Textures", "Models/DAE")));
                                        }
                                    }
                                }
                                else if (texFile.Contains("Mouth1") && texFile.Contains("Nor") == false)
                                {
                                    bool hasNormals = false;

                                    foreach (string norFile in texFiles)
                                    {
                                        // If the mouth has a normal file, the UV coordinates expect it split across the image border.

                                        if (norFile.Contains("Mouth") && norFile.Contains("Nor"))
                                        {
                                            hasNormals = true;
                                        }
                                    }

                                    if (hasNormals == true)
                                    {
                                        Bitmap thisHalf = new Bitmap(tempBMP.Width / 2, tempBMP.Height / 4);
                                        Bitmap finalBMP = new Bitmap(tempBMP.Width * 2, tempBMP.Height);

                                        for (int halfY = 0; halfY < 4; halfY++)
                                        {
                                            for (int halfX = 0; halfX < 2; halfX++)
                                            {
                                                for (int x = halfX * (tempBMP.Width / 2); x < (halfX + 1) * (tempBMP.Width / 2); x++)
                                                {
                                                    for (int y = halfY * (tempBMP.Height / 4); y < (halfY + 1) * (tempBMP.Height / 4); y++)
                                                    {
                                                        thisHalf.SetPixel(x - (halfX * (tempBMP.Width / 2)), y - (halfY * (tempBMP.Height / 4)), tempBMP.GetPixel(x, y));
                                                    }
                                                }

                                                for (int x = halfX * (tempBMP.Width / 2); x < (halfX + 1) * (tempBMP.Width / 2); x++)
                                                {
                                                    for (int y = halfY * (tempBMP.Height / 4); y < (halfY + 1) * (tempBMP.Height / 4); y++)
                                                    {
                                                        finalBMP.SetPixel(x, y, thisHalf.GetPixel(x - (halfX * (tempBMP.Width / 2)), y - (halfY * (tempBMP.Height / 4))));
                                                    }
                                                }

                                                for (int x = halfX * (tempBMP.Width / 2); x < (halfX + 1) * (tempBMP.Width / 2); x++)
                                                {
                                                    for (int y = halfY * (tempBMP.Height / 4); y < (halfY + 1) * (tempBMP.Height / 4); y++)
                                                    {
                                                        finalBMP.SetPixel(finalBMP.Width - (x + 1), y, thisHalf.GetPixel(x - (halfX * (tempBMP.Width / 2)), y - (halfY * (tempBMP.Height / 4))));
                                                    }
                                                }
                                            }
                                        }

                                        finalBMP.Save(texFile.Replace("Textures", "Models/DAE"));
                                        model.texture.Add(new RenderBase.OTexture(finalBMP, texFile.Replace("Textures", "Models/DAE")));
                                    }
                                    else
                                    {
                                        // If the mouth doesn't have a normal file, the UV coordinates expect it as-is. Hooray!

                                        tempBMP.Save(texFile.Replace("Textures", "Models/DAE"));
                                        model.texture.Add(new RenderBase.OTexture(tempBMP, texFile.Replace("Textures", "Models/DAE")));
                                    }
                                }
                                else if (texFile.Contains("Eye1") && hasIris == false && texFile.Contains("AEye") == false && texFile.Contains("BEye") == false & texFile.Contains("CEye") == false)
                                {
                                    if (tempBMP.Width == tempBMP.Height)
                                    {
                                        tempBMP.Save(texFile.Replace("Textures", "Models/DAE"));
                                        model.texture.Add(new RenderBase.OTexture(tempBMP, texFile.Replace("Textures", "Models/DAE")));
                                    }
                                    else
                                    {
                                        Bitmap thisEye = new Bitmap(tempBMP.Width / 2, tempBMP.Height / 4);
                                        Bitmap finalBMP = new Bitmap(tempBMP.Width * 2, tempBMP.Height);

                                        for (int eyeY = 0; eyeY < 4; eyeY++)
                                        {
                                            for (int eyeX = 0; eyeX < 2; eyeX++)
                                            {
                                                for (int x = eyeX * (tempBMP.Width / 2); x < (eyeX + 1) * (tempBMP.Width / 2); x++)
                                                {
                                                    for (int y = eyeY * (tempBMP.Height / 4); y < (eyeY + 1) * (tempBMP.Height / 4); y++)
                                                    {
                                                        thisEye.SetPixel(x - (eyeX * (tempBMP.Width / 2)), y - (eyeY * (tempBMP.Height / 4)), tempBMP.GetPixel(x, y));
                                                    }
                                                }

                                                for (int x = eyeX * (tempBMP.Width / 2); x < (eyeX + 1) * (tempBMP.Width / 2); x++)
                                                {
                                                    for (int y = eyeY * (tempBMP.Height / 4); y < (eyeY + 1) * (tempBMP.Height / 4); y++)
                                                    {
                                                        finalBMP.SetPixel(x, y, thisEye.GetPixel(x - (eyeX * (tempBMP.Width / 2)), y - (eyeY * (tempBMP.Height / 4))));
                                                    }
                                                }

                                                for (int x = eyeX * (tempBMP.Width / 2); x < (eyeX + 1) * (tempBMP.Width / 2); x++)
                                                {
                                                    for (int y = eyeY * (tempBMP.Height / 4); y < (eyeY + 1) * (tempBMP.Height / 4); y++)
                                                    {
                                                        finalBMP.SetPixel(finalBMP.Width - (x + 1), y, thisEye.GetPixel(x - (eyeX * (tempBMP.Width / 2)), y - (eyeY * (tempBMP.Height / 4))));
                                                    }
                                                }
                                            }
                                        }

                                        finalBMP.Save(texFile.Replace("Textures", "Models/DAE"));
                                        model.texture.Add(new RenderBase.OTexture(finalBMP, texFile.Replace("Textures", "Models/DAE")));
                                    }
                                }
                                else if (texFile.Contains("AEye") || texFile.Contains("BEye") || texFile.Contains("CEye"))
                                {
                                    Bitmap finalBMP = new Bitmap(tempBMP, tempBMP.Width / 2, tempBMP.Height);

                                    finalBMP.Save(texFile.Replace("Textures", "Models/DAE"));
                                    model.texture.Add(new RenderBase.OTexture(finalBMP, texFile.Replace("Textures", "Models/DAE")));
                                }
                                else if (texFile.Contains("Fire"))
                                {
                                    Bitmap finalBMP = new Bitmap(tempBMP);

                                    foreach (string bodyFile in texFiles)
                                    {
                                        if ((bodyFile.Contains("Body1") || bodyFile.Contains("BodyA1")) && bodyFile.Contains("Nor") == false)
                                        {
                                            Bitmap bodyBMP = (Bitmap)Bitmap.FromFile(bodyFile);

                                            byte r, g, b;

                                            // Most-Frequent Body Color
                                            Color MFBC = Color.FromArgb(0xFF, 0xFF, 0x00, 0x00), LFBC = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00);
                                            int LF = 0, SF = 65536;

                                            List<Color> colors = new List<Color>();
                                            List<int> freqs = new List<int>();

                                            for (int x = 0; x < bodyBMP.Width; x++)
                                            {
                                                for (int y = 0; y < bodyBMP.Height; y++)
                                                {
                                                    r = bodyBMP.GetPixel(x, y).R;
                                                    g = bodyBMP.GetPixel(x, y).G;
                                                    b = bodyBMP.GetPixel(x, y).B;

                                                    if (bodyBMP.GetPixel(x, y).A != 0x00)
                                                    {
                                                        bool matchFound = false;

                                                        foreach (Color c in colors)
                                                        {
                                                            if (c.R == r && c.G == g && c.B == b)
                                                            {
                                                                matchFound = true;
                                                                int freq = freqs[colors.IndexOf(c)];
                                                                freq++;

                                                                freqs[colors.IndexOf(c)] = freq;
                                                            }
                                                        }

                                                        if (matchFound == false)
                                                        {
                                                            colors.Add(Color.FromArgb(r, g, b));
                                                            freqs.Add(1);
                                                        }
                                                    }
                                                }
                                            }

                                            int tolerance = 18;

                                            foreach (Color c in colors)
                                            {
                                                if (freqs[colors.IndexOf(c)] > LF)
                                                {
                                                    LF = freqs[colors.IndexOf(c)];
                                                    MFBC = c;
                                                }
                                                else if (freqs[colors.IndexOf(c)] + 150 < SF)
                                                {
                                                    if (c.R <= (c.G - tolerance) || c.R >= (c.G + tolerance))
                                                    {
                                                        if (c.B <= (c.G - tolerance) || c.B >= (c.G + tolerance))
                                                        {
                                                            SF = freqs[colors.IndexOf(c)];
                                                            LFBC = c;
                                                        }
                                                    }
                                                }
                                            }

                                            bool mfbcgray = false;
                                            bool lfbcgray = false;
                                            bool bothgray = false;
                                            bool neithergray = false;

                                            if (MFBC.R > (MFBC.G - tolerance) && MFBC.R < (MFBC.G + tolerance))
                                            {
                                                if (MFBC.B > (MFBC.G - tolerance) && MFBC.B < (MFBC.G + tolerance))
                                                {
                                                    mfbcgray = true;
                                                }
                                            }

                                            if (LFBC.R > (LFBC.G - tolerance) && LFBC.R < (LFBC.G + tolerance))
                                            {
                                                if (LFBC.B > (LFBC.G - tolerance) && LFBC.B < (LFBC.G + tolerance))
                                                {
                                                    lfbcgray = true;
                                                }
                                            }

                                            if (mfbcgray && lfbcgray)
                                            {
                                                bothgray = true;
                                            }

                                            if (!mfbcgray && !lfbcgray)
                                            {
                                                neithergray = true;
                                            }

                                            Color baseOperator;
                                            Color accentOperator;

                                            if (bothgray)
                                            {
                                                baseOperator = Color.FromArgb(0xFF, 0xFF, 0x00, 0x00);
                                                accentOperator = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00);
                                            }
                                            else if (neithergray)
                                            {
                                                baseOperator = Color.FromArgb(0xFF, 0xFF, 0x00, 0x00);
                                                accentOperator = Color.FromArgb(0xFF, 0xFF, 0xFF, 0x00);
                                            }
                                            else
                                            {
                                                if (mfbcgray)
                                                {
                                                    baseOperator = LFBC;
                                                    accentOperator = LFBC;
                                                }
                                                else if (lfbcgray)
                                                {
                                                    baseOperator = MFBC;
                                                    accentOperator = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
                                                }
                                                else
                                                {
                                                    // Just to satisfy C#...
                                                    baseOperator = MFBC;
                                                    accentOperator = LFBC;
                                                }
                                            }

                                            for (int x = 0; x < finalBMP.Width; x++)
                                            {
                                                for (int y = 0; y < finalBMP.Height; y++)
                                                {
                                                    Color tempColor = tempBMP.GetPixel(x, y);
                                                    Color finalColor;

                                                    if (tempColor.R < 0x10 && tempColor.G < 0x10 && tempColor.B < 0x10)
                                                    {
                                                        finalColor = Color.FromArgb(0x1F, baseOperator.R, baseOperator.G, baseOperator.B);
                                                    }
                                                    else
                                                    {
                                                        finalColor = Color.FromArgb(0x1F, tempColor.R & accentOperator.R, tempColor.G & accentOperator.G, tempColor.B & accentOperator.B);
                                                    }

                                                    finalBMP.SetPixel(x, y, finalColor);
                                                }
                                            }

                                            finalBMP.Save(texFile.Replace("Textures", "Models/DAE"));
                                            model.texture.Add(new RenderBase.OTexture(finalBMP, texFile.Replace("Textures", "Models/DAE")));
                                        }
                                    }
                                }
                                else
                                {
                                    if (texFile.Contains("Iris2") == false && texFile.Contains("Eye2") == false && texFile.Contains("Mouth2") == false)
                                    {
                                        tempBMP.Save(texFile.Replace("Textures", "Models/DAE"));
                                        model.texture.Add(new RenderBase.OTexture(tempBMP, texFile.Replace("Textures", "Models/DAE")));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch // No packaged textures detected...
            {
            }

            foreach (RenderBase.OTexture tex in model.texture)
            {
                DaeImage img = new DaeImage();

                string n = Path.GetFileNameWithoutExtension(tex.name);

                img.id = n;
                img.name = n;

                if (tex.name.Contains("Shiny"))
                {
                    img.id += "_shiny";
                    img.name += "_shiny";

                    hasShiny = true;
                }

                img.id += "_id";

                img.init_from = tex.name;

                if (img.id.Contains("_shiny"))
                {
                    daeShiny.library_images.Add(img);
                }
                else
                {
                    dae.library_images.Add(img);
                }
            }

            #region Normal

            int currentMat = 0;

            foreach (RenderBase.OMaterial mat in mdl.material)
            {
                if (mat.name0 == null)
                {
                    mat.name0 = mat.name;
                }

                mdl.material[currentMat].name = mat.name;
                mdl.material[currentMat].name0 = mat.name0;

                currentMat++;

                DaeMaterial mtl = new DaeMaterial
                {
                    name = mat.name + "_mat"
                };
                mtl.id = mtl.name + "_id";
                mtl.instance_effect.url = "#eff_" + mtl.id;

                dae.library_materials.Add(mtl);

                DaeEffect eff = new DaeEffect
                {
                    id = "eff_" + mtl.id,
                    name = "eff_" + mtl.name
                };

                DaeParam surface = new DaeParam
                {
                    surface = new DaeParamSurfaceElement(),
                    sid = "img_surface"
                };
                surface.surface.type = "2D";
                surface.surface.init_from = mat.name0 + "_id";
                surface.surface.format = "PNG";
                eff.profile_COMMON.newparam.Add(surface);

                bool fire = false;

                for (int i = 0; i < dae.library_images.Count; i++)
                {
                    if (dae.library_images[i].init_from.Contains(mat.name) || dae.library_images[i].id.Contains(mat.name) || dae.library_images[i].init_from.Contains(mat.name0) || dae.library_images[i].id.Contains(mat.name0))
                    {
                        if (dae.library_images[i].init_from.Contains("Nor") == false)
                        {
                            if (mat.name.Contains("Fire"))
                            {
                                fire = true;

                                dae.library_images[i].name = dae.library_images[i].name.Replace("Mask", "1");
                                dae.library_images[i].id = dae.library_images[i].id.Replace("Mask", "1");
                                dae.library_images[i].init_from = dae.library_images[i].init_from.Replace("Mask", "1");

                                mat.name = mat.name.Replace("Mask", "1");
                                mat.name0 = mat.name0.Replace("Mask", "1");
                            }

                            surface.surface.init_from = dae.library_images[i].id;
                        }
                    }
                }

                DaeParam sampler = new DaeParam
                {
                    sampler2D = new DaeParamSampler2DElement(),
                    sid = "img_sampler"
                };
                sampler.sampler2D.source = "img_surface";

                switch (mat.textureMapper[0].wrapU)
                {
                    case RenderBase.OTextureWrap.repeat: sampler.sampler2D.wrap_s = "WRAP"; break;
                    case RenderBase.OTextureWrap.mirroredRepeat: sampler.sampler2D.wrap_s = "MIRROR"; break;
                    case RenderBase.OTextureWrap.clampToEdge: sampler.sampler2D.wrap_s = "CLAMP"; break;
                    case RenderBase.OTextureWrap.clampToBorder: sampler.sampler2D.wrap_s = "BORDER"; break;
                    default: sampler.sampler2D.wrap_s = "WRAP"; break;
                }

                switch (mat.textureMapper[0].wrapV)
                {
                    case RenderBase.OTextureWrap.repeat: sampler.sampler2D.wrap_t = "WRAP"; break;
                    case RenderBase.OTextureWrap.mirroredRepeat: sampler.sampler2D.wrap_t = "MIRROR"; break;
                    case RenderBase.OTextureWrap.clampToEdge: sampler.sampler2D.wrap_t = "CLAMP"; break;
                    case RenderBase.OTextureWrap.clampToBorder: sampler.sampler2D.wrap_t = "BORDER"; break;
                    default: sampler.sampler2D.wrap_t = "WRAP"; break;
                }

                switch (mat.textureMapper[0].minFilter)
                {
                    case RenderBase.OTextureMinFilter.linearMipmapLinear: sampler.sampler2D.minfilter = "LINEAR_MIPMAP_LINEAR"; break;
                    case RenderBase.OTextureMinFilter.linearMipmapNearest: sampler.sampler2D.minfilter = "LINEAR_MIPMAP_NEAREST"; break;
                    case RenderBase.OTextureMinFilter.nearestMipmapLinear: sampler.sampler2D.minfilter = "NEAREST_MIPMAP_LINEAR"; break;
                    case RenderBase.OTextureMinFilter.nearestMipmapNearest: sampler.sampler2D.minfilter = "NEAREST_MIPMAP_NEAREST"; break;
                    default: sampler.sampler2D.minfilter = "NONE"; break;
                }

                switch (mat.textureMapper[0].magFilter)
                {
                    case RenderBase.OTextureMagFilter.linear: sampler.sampler2D.magfilter = "LINEAR"; break;
                    case RenderBase.OTextureMagFilter.nearest: sampler.sampler2D.magfilter = "NEAREST"; break;
                    default: sampler.sampler2D.magfilter = "NONE"; break;
                }

                sampler.sampler2D.mipfilter = sampler.sampler2D.magfilter;

                eff.profile_COMMON.newparam.Add(sampler);

                eff.profile_COMMON.technique.sid = "img_technique";

                eff.profile_COMMON.technique.phong.emission.Set(Color.Black);
                eff.profile_COMMON.technique.phong.ambient.Set(Color.Black);
                eff.profile_COMMON.technique.phong.specular.Set(Color.White);

                eff.profile_COMMON.technique.phong.diffuse.texture.texture = "img_sampler";

                eff.profile_COMMON.technique.phong.transparency = new DaePhongTransparent();

                if (fire)
                {
                    eff.profile_COMMON.technique.phong.transparency.value = "0.1";
                }
                else
                {
                    eff.profile_COMMON.technique.phong.transparency.value = "1.0";
                }

                dae.library_effects.Add(eff);
            }

            string jointNames = null;
            string invBindPoses = null;
            for (int index = 0; index < mdl.skeleton.Count; index++)
            {
                RenderBase.OMatrix transform = new RenderBase.OMatrix();
                TransformSkeleton(mdl.skeleton, index, ref transform);

                jointNames += mdl.skeleton[index].name;
                DaeMatrix mtx = new DaeMatrix();
                mtx.Set(transform.Invert());
                invBindPoses += mtx.data;
                if (index < mdl.skeleton.Count - 1)
                {
                    jointNames += " ";
                    invBindPoses += " ";
                }
            }

            int meshIndex = 0;
            DaeVisualScene vs = new DaeVisualScene
            {
                name = "vs_" + mdl.name
            };
            vs.id = vs.name + "_id";
            if (mdl.skeleton.Count > 0) WriteSkeleton(mdl.skeleton, 0, ref vs.node);

            bool rightIris = false;

            foreach (RenderBase.OMesh obj in mdl.mesh)
            {
                float largestUV = 0.0f;

                //Geometry
                DaeGeometry geometry = new DaeGeometry();

                string meshName = "mesh_" + meshIndex++ + "_" + obj.name;
                geometry.id = meshName + "_id";
                geometry.name = meshName;

                MeshUtils.OptimizedMesh mesh = MeshUtils.OptimizeMesh(obj);
                List<float> positions = new List<float>();
                List<float> normals = new List<float>();
                List<float> uv0 = new List<float>();
                List<float> uv1 = new List<float>();
                List<float> uv2 = new List<float>();
                List<float> colors = new List<float>();

                if (obj.name.Contains("Iris"))
                {
                    foreach (RenderBase.OVertex vtx in mesh.vertices)
                    {
                        if (mesh.texUVCount > 0)
                        {
                            if (vtx.texture0.x > largestUV)
                            {
                                largestUV = vtx.texture0.x;
                            }
                        }
                    }

                    foreach (RenderBase.OVertex vtx in mesh.vertices)
                    {
                        if (mesh.texUVCount > 0)
                        {
                            vtx.texture0.x = largestUV - vtx.texture0.x;

                            if (rightIris)
                            {
                                vtx.texture0.x = 1.25f - vtx.texture0.x;
                            }
                        }

                        if (mesh.texUVCount > 1)
                        {
                            vtx.texture1.x = largestUV - vtx.texture1.x;

                            if (rightIris)
                            {
                                vtx.texture1.x = 1.25f - vtx.texture1.x;
                            }
                        }

                        if (mesh.texUVCount > 2)
                        {
                            vtx.texture2.x = largestUV - vtx.texture2.x;

                            if (rightIris)
                            {
                                vtx.texture2.x = 1.25f - vtx.texture2.x;
                            }
                        }
                    }

                    rightIris = !rightIris;
                }
                else if (obj.name.Contains("Eye"))
                {
                    foreach (RenderBase.OVertex vtx in mesh.vertices)
                    {
                        if (mesh.texUVCount > 0)
                        {
                            if (vtx.texture0.x > largestUV)
                            {
                                largestUV = vtx.texture0.x;
                            }
                        }
                    }
                    foreach (RenderBase.OVertex vtx in mesh.vertices)
                    {
                        if (mesh.texUVCount > 0)
                        {
                            vtx.texture0.x = largestUV - vtx.texture0.x;
                        }

                        if (mesh.texUVCount > 1)
                        {
                            vtx.texture1.x = largestUV - vtx.texture1.x;
                        }

                        if (mesh.texUVCount > 2)
                        {
                            vtx.texture2.x = largestUV - vtx.texture2.x;
                        }
                    }
                }

                foreach (RenderBase.OVertex vtx in mesh.vertices)
                {
                    positions.Add(vtx.position.x);
                    positions.Add(vtx.position.y);
                    positions.Add(vtx.position.z);

                    if (mesh.hasNormal)
                    {
                        normals.Add(vtx.normal.x);
                        normals.Add(vtx.normal.y);
                        normals.Add(vtx.normal.z);
                    }

                    if (mesh.texUVCount > 0)
                    {
                        uv0.Add(vtx.texture0.x);
                        uv0.Add(vtx.texture0.y);
                    }

                    if (mesh.texUVCount > 1)
                    {
                        uv1.Add(vtx.texture1.x);
                        uv1.Add(vtx.texture1.y);
                    }

                    if (mesh.texUVCount > 2)
                    {
                        uv2.Add(vtx.texture2.x);
                        uv2.Add(vtx.texture2.y);
                    }

                    if (mesh.hasColor)
                    {
                        colors.Add(((vtx.diffuseColor >> 16) & 0xff) / 255f);
                        colors.Add(((vtx.diffuseColor >> 8) & 0xff) / 255f);
                        colors.Add((vtx.diffuseColor & 0xff) / 255f);
                        colors.Add(((vtx.diffuseColor >> 24) & 0xff) / 255f);
                    }
                }

                DaeSource position = new DaeSource
                {
                    name = meshName + "_position"
                };
                position.id = position.name + "_id";
                position.float_array = new DaeFloatArray
                {
                    id = position.name + "_array_id"
                };
                position.float_array.Set(positions);
                position.technique_common.accessor.source = "#" + position.float_array.id;
                position.technique_common.accessor.count = (uint)mesh.vertices.Count;
                position.technique_common.accessor.stride = 3;
                position.technique_common.accessor.AddParam("X", "float");
                position.technique_common.accessor.AddParam("Y", "float");
                position.technique_common.accessor.AddParam("Z", "float");

                geometry.mesh.source.Add(position);

                DaeSource normal = new DaeSource();
                if (mesh.hasNormal)
                {
                    normal.name = meshName + "_normal";
                    normal.id = normal.name + "_id";
                    normal.float_array = new DaeFloatArray
                    {
                        id = normal.name + "_array_id"
                    };
                    normal.float_array.Set(normals);
                    normal.technique_common.accessor.source = "#" + normal.float_array.id;
                    normal.technique_common.accessor.count = (uint)mesh.vertices.Count;
                    normal.technique_common.accessor.stride = 3;
                    normal.technique_common.accessor.AddParam("X", "float");
                    normal.technique_common.accessor.AddParam("Y", "float");
                    normal.technique_common.accessor.AddParam("Z", "float");

                    geometry.mesh.source.Add(normal);
                }

                DaeSource[] texUV = new DaeSource[3];
                for (int i = 0; i < mesh.texUVCount; i++)
                {
                    texUV[i] = new DaeSource
                    {
                        name = meshName + "_uv" + i
                    };
                    texUV[i].id = texUV[i].name + "_id";
                    texUV[i].float_array = new DaeFloatArray
                    {
                        id = texUV[i].name + "_array_id"
                    };
                    texUV[i].technique_common.accessor.source = "#" + texUV[i].float_array.id;
                    texUV[i].technique_common.accessor.count = (uint)mesh.vertices.Count;
                    texUV[i].technique_common.accessor.stride = 2;
                    texUV[i].technique_common.accessor.AddParam("S", "float");
                    texUV[i].technique_common.accessor.AddParam("T", "float");

                    geometry.mesh.source.Add(texUV[i]);
                }

                DaeSource color = new DaeSource();
                if (mesh.hasColor)
                {
                    color.name = meshName + "_color";
                    color.id = color.name + "_id";
                    color.float_array = new DaeFloatArray
                    {
                        id = color.name + "_array_id"
                    };
                    color.float_array.Set(colors);
                    color.technique_common.accessor.source = "#" + color.float_array.id;
                    color.technique_common.accessor.count = (uint)mesh.vertices.Count;
                    color.technique_common.accessor.stride = 4;
                    color.technique_common.accessor.AddParam("R", "float");
                    color.technique_common.accessor.AddParam("G", "float");
                    color.technique_common.accessor.AddParam("B", "float");
                    color.technique_common.accessor.AddParam("A", "float");

                    geometry.mesh.source.Add(color);
                }

                geometry.mesh.vertices.id = meshName + "_vertices_id";
                geometry.mesh.vertices.AddInput("POSITION", "#" + position.id);

                geometry.mesh.triangles.material = mdl.material[obj.materialId].name + "_mat";
                geometry.mesh.triangles.AddInput("VERTEX", "#" + geometry.mesh.vertices.id);
                if (mesh.hasNormal) geometry.mesh.triangles.AddInput("NORMAL", "#" + normal.id);
                if (mesh.hasColor) geometry.mesh.triangles.AddInput("COLOR", "#" + color.id);
                if (mesh.texUVCount > 0)
                {
                    texUV[0].float_array.Set(uv0);
                    geometry.mesh.triangles.AddInput("TEXCOORD", "#" + texUV[0].id);
                }
                if (mesh.texUVCount > 1)
                {
                    texUV[1].float_array.Set(uv1);
                    geometry.mesh.triangles.AddInput("TEXCOORD", "#" + texUV[1].id, 0, 1);
                }
                if (mesh.texUVCount > 2)
                {
                    texUV[2].float_array.Set(uv2);
                    geometry.mesh.triangles.AddInput("TEXCOORD", "#" + texUV[2].id, 0, 2);
                }
                geometry.mesh.triangles.Set(mesh.indices);

                dae.library_geometries.Add(geometry);

                bool hasNode = obj.vertices[0].node.Count > 0;
                bool hasWeight = obj.vertices[0].weight.Count > 0;
                bool hasController = hasNode && hasWeight;

                //Controller
                DaeController controller = new DaeController();
                if (hasController)
                {
                    controller.id = meshName + "_ctrl_id";

                    controller.skin.source = "#" + geometry.id;
                    controller.skin.bind_shape_matrix.Set(new RenderBase.OMatrix());

                    DaeSource joints = new DaeSource
                    {
                        id = meshName + "_ctrl_joint_names_id",
                        Name_array = new DaeNameArray()
                    };
                    joints.Name_array.id = meshName + "_ctrl_joint_names_array_id";
                    joints.Name_array.count = (uint)mdl.skeleton.Count;
                    joints.Name_array.data = jointNames;
                    joints.technique_common.accessor.source = "#" + joints.Name_array.id;
                    joints.technique_common.accessor.count = joints.Name_array.count;
                    joints.technique_common.accessor.stride = 1;
                    joints.technique_common.accessor.AddParam("JOINT", "Name");

                    controller.skin.src.Add(joints);

                    DaeSource bindPoses = new DaeSource
                    {
                        id = meshName + "_ctrl_inv_bind_poses_id",
                        float_array = new DaeFloatArray()
                    };
                    bindPoses.float_array.id = meshName + "_ctrl_inv_bind_poses_array_id";
                    bindPoses.float_array.count = (uint)(mdl.skeleton.Count * 16);
                    bindPoses.float_array.data = invBindPoses;
                    bindPoses.technique_common.accessor.source = "#" + bindPoses.float_array.id;
                    bindPoses.technique_common.accessor.count = (uint)mdl.skeleton.Count;
                    bindPoses.technique_common.accessor.stride = 16;
                    bindPoses.technique_common.accessor.AddParam("TRANSFORM", "float4x4");

                    controller.skin.src.Add(bindPoses);

                    DaeSource weights = new DaeSource
                    {
                        id = meshName + "_ctrl_weights_id",
                        float_array = new DaeFloatArray
                        {
                            id = meshName + "_ctrl_weights_array_id"
                        }
                    };
                    weights.technique_common.accessor.source = "#" + weights.float_array.id;
                    weights.technique_common.accessor.stride = 1;
                    weights.technique_common.accessor.AddParam("WEIGHT", "float");

                    StringBuilder w = new StringBuilder();
                    StringBuilder vcount = new StringBuilder();
                    StringBuilder v = new StringBuilder();

                    float[] wLookBack = new float[32];
                    uint wLookBackIndex = 0;
                    int buffLen = 0;

                    int wIndex = 0;
                    int wCount = 0;
                    foreach (RenderBase.OVertex vtx in mesh.vertices)
                    {
                        int count = Math.Min(vtx.node.Count, vtx.weight.Count);

                        vcount.Append(count + " ");
                        for (int n = 0; n < count; n++)
                        {
                            v.Append(vtx.node[n] + " ");
                            bool found = false;
                            uint bPos = (wLookBackIndex - 1) & 0x1f;
                            for (int i = 0; i < buffLen; i++)
                            {
                                if (wLookBack[bPos] == vtx.weight[n])
                                {
                                    v.Append(wIndex - (i + 1) + " ");
                                    found = true;
                                    break;
                                }
                                bPos = (bPos - 1) & 0x1f;
                            }

                            if (!found)
                            {
                                v.Append(wIndex++ + " ");
                                w.Append(vtx.weight[n].ToString(CultureInfo.InvariantCulture) + " ");
                                wCount++;

                                wLookBack[wLookBackIndex] = vtx.weight[n];
                                wLookBackIndex = (wLookBackIndex + 1) & 0x1f;
                                if (buffLen < wLookBack.Length) buffLen++;
                            }
                        }
                    }

                    weights.float_array.data = w.ToString().TrimEnd();
                    weights.float_array.count = (uint)wCount;
                    weights.technique_common.accessor.count = (uint)wCount;

                    controller.skin.src.Add(weights);
                    controller.skin.vertex_weights.vcount = vcount.ToString().TrimEnd();
                    controller.skin.vertex_weights.v = v.ToString().TrimEnd();
                    controller.skin.vertex_weights.count = (uint)mesh.vertices.Count;
                    controller.skin.joints.AddInput("JOINT", "#" + joints.id);
                    controller.skin.joints.AddInput("INV_BIND_MATRIX", "#" + bindPoses.id);

                    controller.skin.vertex_weights.AddInput("JOINT", "#" + joints.id);
                    controller.skin.vertex_weights.AddInput("WEIGHT", "#" + weights.id, 1);

                    if (dae.library_controllers == null) dae.library_controllers = new List<DaeController>();
                    dae.library_controllers.Add(controller);
                }

                //Visual scene node
                DaeNode node = new DaeNode
                {
                    name = "vsn_" + meshName
                };
                node.id = node.name + "_id";
                node.matrix.Set(new RenderBase.OMatrix());
                if (hasController)
                {
                    node.instance_controller = new DaeInstanceController
                    {
                        url = "#" + controller.id,
                        skeleton = "#" + mdl.skeleton[0].name + "_bone_id"
                    };
                    node.instance_controller.bind_material.technique_common.instance_material.symbol = mdl.material[obj.materialId].name + "_mat";
                    node.instance_controller.bind_material.technique_common.instance_material.target = "#" + mdl.material[obj.materialId].name + "_mat_id";
                }
                else
                {
                    node.instance_geometry = new DaeInstanceGeometry
                    {
                        url = "#" + geometry.id
                    };
                    node.instance_geometry.bind_material.technique_common.instance_material.symbol = mdl.material[obj.materialId].name + "_mat";
                    node.instance_geometry.bind_material.technique_common.instance_material.target = "#" + mdl.material[obj.materialId].name + "_mat_id";
                }

                vs.node.Add(node);
            }
            dae.library_visual_scenes.Add(vs);

            DaeInstaceVisualScene scene = new DaeInstaceVisualScene
            {
                url = "#" + vs.id
            };
            dae.scene.Add(scene);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true
            };

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "http://www.collada.org/2005/11/COLLADASchema");
            XmlSerializer serializer = new XmlSerializer(typeof(COLLADA));
            XmlWriter output = XmlWriter.Create(new FileStream(fileName.Replace(".bch", ".dae").Replace("BCH", "DAE"), FileMode.Create), settings);
            serializer.Serialize(output, dae, ns);
            output.Close();

            #endregion

            #region Shiny

            if (hasShiny)
            {
                currentMat = 0;

                foreach (RenderBase.OMaterial mat in mdl.material)
                {
                    if (mat.name0 == null)
                    {
                        mat.name0 = mat.name;
                    }

                    mat.name += "_shiny";
                    mat.name0 += "_shiny";

                    mdl.material[currentMat].name = mat.name;
                    mdl.material[currentMat].name0 = mat.name0;

                    currentMat++;

                    DaeMaterial mtl = new DaeMaterial
                    {
                        name = mat.name + "_mat"
                    };
                    mtl.id = mtl.name + "_id";
                    mtl.instance_effect.url = "#eff_" + mtl.id;

                    daeShiny.library_materials.Add(mtl);

                    DaeEffect eff = new DaeEffect
                    {
                        id = "eff_" + mtl.id,
                        name = "eff_" + mtl.name
                    };

                    DaeParam surface = new DaeParam
                    {
                        surface = new DaeParamSurfaceElement(),
                        sid = "img_surface"
                    };
                    surface.surface.type = "2D";
                    surface.surface.init_from = mat.name0 + "_id";
                    surface.surface.format = "PNG";
                    eff.profile_COMMON.newparam.Add(surface);

                    bool fire = false;

                    for (int i = 0; i < daeShiny.library_images.Count; i++)
                    {
                        if (daeShiny.library_images[i].init_from.Contains(mat.name) || daeShiny.library_images[i].id.Contains(mat.name) || daeShiny.library_images[i].init_from.Contains(mat.name0) || daeShiny.library_images[i].id.Contains(mat.name0))
                        {
                            if (daeShiny.library_images[i].init_from.Contains("Nor") == false)
                            {
                                if (mat.name.Contains("Fire"))
                                {
                                    fire = true;

                                    daeShiny.library_images[i].name = daeShiny.library_images[i].name.Replace("Mask", "1");
                                    daeShiny.library_images[i].id = daeShiny.library_images[i].id.Replace("Mask", "1");
                                    daeShiny.library_images[i].init_from = daeShiny.library_images[i].init_from.Replace("Mask", "1");

                                    mat.name = mat.name.Replace("Mask", "1");
                                    mat.name0 = mat.name0.Replace("Mask", "1");
                                }

                                surface.surface.init_from = daeShiny.library_images[i].id;
                            }
                        }
                    }

                    if (mat.name.Contains("Fire"))
                    {
                        fire = true;
                    }

                    DaeParam sampler = new DaeParam
                    {
                        sampler2D = new DaeParamSampler2DElement(),
                        sid = "img_sampler"
                    };
                    sampler.sampler2D.source = "img_surface";

                    switch (mat.textureMapper[0].wrapU)
                    {
                        case RenderBase.OTextureWrap.repeat: sampler.sampler2D.wrap_s = "WRAP"; break;
                        case RenderBase.OTextureWrap.mirroredRepeat: sampler.sampler2D.wrap_s = "MIRROR"; break;
                        case RenderBase.OTextureWrap.clampToEdge: sampler.sampler2D.wrap_s = "CLAMP"; break;
                        case RenderBase.OTextureWrap.clampToBorder: sampler.sampler2D.wrap_s = "BORDER"; break;
                        default: sampler.sampler2D.wrap_s = "WRAP"; break;
                    }

                    switch (mat.textureMapper[0].wrapV)
                    {
                        case RenderBase.OTextureWrap.repeat: sampler.sampler2D.wrap_t = "WRAP"; break;
                        case RenderBase.OTextureWrap.mirroredRepeat: sampler.sampler2D.wrap_t = "MIRROR"; break;
                        case RenderBase.OTextureWrap.clampToEdge: sampler.sampler2D.wrap_t = "CLAMP"; break;
                        case RenderBase.OTextureWrap.clampToBorder: sampler.sampler2D.wrap_t = "BORDER"; break;
                        default: sampler.sampler2D.wrap_t = "WRAP"; break;
                    }

                    switch (mat.textureMapper[0].minFilter)
                    {
                        case RenderBase.OTextureMinFilter.linearMipmapLinear: sampler.sampler2D.minfilter = "LINEAR_MIPMAP_LINEAR"; break;
                        case RenderBase.OTextureMinFilter.linearMipmapNearest: sampler.sampler2D.minfilter = "LINEAR_MIPMAP_NEAREST"; break;
                        case RenderBase.OTextureMinFilter.nearestMipmapLinear: sampler.sampler2D.minfilter = "NEAREST_MIPMAP_LINEAR"; break;
                        case RenderBase.OTextureMinFilter.nearestMipmapNearest: sampler.sampler2D.minfilter = "NEAREST_MIPMAP_NEAREST"; break;
                        default: sampler.sampler2D.minfilter = "NONE"; break;
                    }

                    switch (mat.textureMapper[0].magFilter)
                    {
                        case RenderBase.OTextureMagFilter.linear: sampler.sampler2D.magfilter = "LINEAR"; break;
                        case RenderBase.OTextureMagFilter.nearest: sampler.sampler2D.magfilter = "NEAREST"; break;
                        default: sampler.sampler2D.magfilter = "NONE"; break;
                    }

                    sampler.sampler2D.mipfilter = sampler.sampler2D.magfilter;

                    eff.profile_COMMON.newparam.Add(sampler);

                    eff.profile_COMMON.technique.sid = "img_technique";

                    eff.profile_COMMON.technique.phong.emission.Set(Color.Black);
                    eff.profile_COMMON.technique.phong.ambient.Set(Color.Black);
                    eff.profile_COMMON.technique.phong.specular.Set(Color.White);

                    eff.profile_COMMON.technique.phong.diffuse.texture.texture = "img_sampler";

                    eff.profile_COMMON.technique.phong.transparency = new DaePhongTransparent();

                    if (fire)
                    {
                        eff.profile_COMMON.technique.phong.transparency.value = "0.1";
                    }
                    else
                    {
                        eff.profile_COMMON.technique.phong.transparency.value = "1.0";
                    }

                    daeShiny.library_effects.Add(eff);
                }

                jointNames = null;
                invBindPoses = null;
                for (int index = 0; index < mdl.skeleton.Count; index++)
                {
                    RenderBase.OMatrix transform = new RenderBase.OMatrix();
                    TransformSkeleton(mdl.skeleton, index, ref transform);

                    jointNames += mdl.skeleton[index].name;
                    DaeMatrix mtx = new DaeMatrix();
                    mtx.Set(transform.Invert());
                    invBindPoses += mtx.data;
                    if (index < mdl.skeleton.Count - 1)
                    {
                        jointNames += " ";
                        invBindPoses += " ";
                    }
                }

                meshIndex = 0;
                vs = new DaeVisualScene
                {
                    name = "vs_" + mdl.name
                };
                vs.id = vs.name + "_id";
                if (mdl.skeleton.Count > 0) WriteSkeleton(mdl.skeleton, 0, ref vs.node);

                rightIris = false;

                foreach (RenderBase.OMesh obj in mdl.mesh)
                {
                    float largestUV = 0.0f;

                    //Geometry
                    DaeGeometry geometry = new DaeGeometry();

                    string meshName = "mesh_" + meshIndex++ + "_" + obj.name;
                    geometry.id = meshName + "_id";
                    geometry.name = meshName;

                    MeshUtils.OptimizedMesh mesh = MeshUtils.OptimizeMesh(obj);
                    List<float> positions = new List<float>();
                    List<float> normals = new List<float>();
                    List<float> uv0 = new List<float>();
                    List<float> uv1 = new List<float>();
                    List<float> uv2 = new List<float>();
                    List<float> colors = new List<float>();

                    if (obj.name.Contains("Iris"))
                    {
                        foreach (RenderBase.OVertex vtx in mesh.vertices)
                        {
                            if (mesh.texUVCount > 0)
                            {
                                if (vtx.texture0.x > largestUV)
                                {
                                    largestUV = vtx.texture0.x;
                                }
                            }
                        }

                        foreach (RenderBase.OVertex vtx in mesh.vertices)
                        {
                            if (mesh.texUVCount > 0)
                            {
                                vtx.texture0.x = largestUV - vtx.texture0.x;

                                if (rightIris)
                                {
                                    vtx.texture0.x = 1.25f - vtx.texture0.x;
                                }
                            }

                            if (mesh.texUVCount > 1)
                            {
                                vtx.texture1.x = largestUV - vtx.texture1.x;

                                if (rightIris)
                                {
                                    vtx.texture1.x = 1.25f - vtx.texture1.x;
                                }
                            }

                            if (mesh.texUVCount > 2)
                            {
                                vtx.texture2.x = largestUV - vtx.texture2.x;

                                if (rightIris)
                                {
                                    vtx.texture2.x = 1.25f - vtx.texture2.x;
                                }
                            }
                        }

                        rightIris = !rightIris;
                    }
                    else if (obj.name.Contains("Eye"))
                    {
                        foreach (RenderBase.OVertex vtx in mesh.vertices)
                        {
                            if (mesh.texUVCount > 0)
                            {
                                if (vtx.texture0.x > largestUV)
                                {
                                    largestUV = vtx.texture0.x;
                                }
                            }
                        }

                        foreach (RenderBase.OVertex vtx in mesh.vertices)
                        {
                            if (mesh.texUVCount > 0)
                            {
                                vtx.texture0.x = largestUV - vtx.texture0.x;
                            }

                            if (mesh.texUVCount > 1)
                            {
                                vtx.texture1.x = largestUV - vtx.texture1.x;
                            }

                            if (mesh.texUVCount > 2)
                            {
                                vtx.texture2.x = largestUV - vtx.texture2.x;
                            }
                        }
                    }

                    foreach (RenderBase.OVertex vtx in mesh.vertices)
                    {
                        positions.Add(vtx.position.x);
                        positions.Add(vtx.position.y);
                        positions.Add(vtx.position.z);

                        if (mesh.hasNormal)
                        {
                            normals.Add(vtx.normal.x);
                            normals.Add(vtx.normal.y);
                            normals.Add(vtx.normal.z);
                        }

                        if (mesh.texUVCount > 0)
                        {
                            uv0.Add(vtx.texture0.x);
                            uv0.Add(vtx.texture0.y);
                        }

                        if (mesh.texUVCount > 1)
                        {
                            uv1.Add(vtx.texture1.x);
                            uv1.Add(vtx.texture1.y);
                        }

                        if (mesh.texUVCount > 2)
                        {
                            uv2.Add(vtx.texture2.x);
                            uv2.Add(vtx.texture2.y);
                        }

                        if (mesh.hasColor)
                        {
                            colors.Add(((vtx.diffuseColor >> 16) & 0xff) / 255f);
                            colors.Add(((vtx.diffuseColor >> 8) & 0xff) / 255f);
                            colors.Add((vtx.diffuseColor & 0xff) / 255f);
                            colors.Add(((vtx.diffuseColor >> 24) & 0xff) / 255f);
                        }
                    }

                    DaeSource position = new DaeSource
                    {
                        name = meshName + "_position"
                    };
                    position.id = position.name + "_id";
                    position.float_array = new DaeFloatArray
                    {
                        id = position.name + "_array_id"
                    };
                    position.float_array.Set(positions);
                    position.technique_common.accessor.source = "#" + position.float_array.id;
                    position.technique_common.accessor.count = (uint)mesh.vertices.Count;
                    position.technique_common.accessor.stride = 3;
                    position.technique_common.accessor.AddParam("X", "float");
                    position.technique_common.accessor.AddParam("Y", "float");
                    position.technique_common.accessor.AddParam("Z", "float");

                    geometry.mesh.source.Add(position);

                    DaeSource normal = new DaeSource();
                    if (mesh.hasNormal)
                    {
                        normal.name = meshName + "_normal";
                        normal.id = normal.name + "_id";
                        normal.float_array = new DaeFloatArray
                        {
                            id = normal.name + "_array_id"
                        };
                        normal.float_array.Set(normals);
                        normal.technique_common.accessor.source = "#" + normal.float_array.id;
                        normal.technique_common.accessor.count = (uint)mesh.vertices.Count;
                        normal.technique_common.accessor.stride = 3;
                        normal.technique_common.accessor.AddParam("X", "float");
                        normal.technique_common.accessor.AddParam("Y", "float");
                        normal.technique_common.accessor.AddParam("Z", "float");

                        geometry.mesh.source.Add(normal);
                    }

                    DaeSource[] texUV = new DaeSource[3];
                    for (int i = 0; i < mesh.texUVCount; i++)
                    {
                        texUV[i] = new DaeSource
                        {
                            name = meshName + "_uv" + i
                        };
                        texUV[i].id = texUV[i].name + "_id";
                        texUV[i].float_array = new DaeFloatArray
                        {
                            id = texUV[i].name + "_array_id"
                        };
                        texUV[i].technique_common.accessor.source = "#" + texUV[i].float_array.id;
                        texUV[i].technique_common.accessor.count = (uint)mesh.vertices.Count;
                        texUV[i].technique_common.accessor.stride = 2;
                        texUV[i].technique_common.accessor.AddParam("S", "float");
                        texUV[i].technique_common.accessor.AddParam("T", "float");

                        geometry.mesh.source.Add(texUV[i]);
                    }

                    DaeSource color = new DaeSource();
                    if (mesh.hasColor)
                    {
                        color.name = meshName + "_color";
                        color.id = color.name + "_id";
                        color.float_array = new DaeFloatArray
                        {
                            id = color.name + "_array_id"
                        };
                        color.float_array.Set(colors);
                        color.technique_common.accessor.source = "#" + color.float_array.id;
                        color.technique_common.accessor.count = (uint)mesh.vertices.Count;
                        color.technique_common.accessor.stride = 4;
                        color.technique_common.accessor.AddParam("R", "float");
                        color.technique_common.accessor.AddParam("G", "float");
                        color.technique_common.accessor.AddParam("B", "float");
                        color.technique_common.accessor.AddParam("A", "float");

                        geometry.mesh.source.Add(color);
                    }

                    geometry.mesh.vertices.id = meshName + "_vertices_id";
                    geometry.mesh.vertices.AddInput("POSITION", "#" + position.id);

                    geometry.mesh.triangles.material = mdl.material[obj.materialId].name + "_mat";
                    geometry.mesh.triangles.AddInput("VERTEX", "#" + geometry.mesh.vertices.id);
                    if (mesh.hasNormal) geometry.mesh.triangles.AddInput("NORMAL", "#" + normal.id);
                    if (mesh.hasColor) geometry.mesh.triangles.AddInput("COLOR", "#" + color.id);
                    if (mesh.texUVCount > 0)
                    {
                        texUV[0].float_array.Set(uv0);
                        geometry.mesh.triangles.AddInput("TEXCOORD", "#" + texUV[0].id);
                    }
                    if (mesh.texUVCount > 1)
                    {
                        texUV[1].float_array.Set(uv1);
                        geometry.mesh.triangles.AddInput("TEXCOORD", "#" + texUV[1].id, 0, 1);
                    }
                    if (mesh.texUVCount > 2)
                    {
                        texUV[2].float_array.Set(uv2);
                        geometry.mesh.triangles.AddInput("TEXCOORD", "#" + texUV[2].id, 0, 2);
                    }
                    geometry.mesh.triangles.Set(mesh.indices);

                    daeShiny.library_geometries.Add(geometry);

                    bool hasNode = obj.vertices[0].node.Count > 0;
                    bool hasWeight = obj.vertices[0].weight.Count > 0;
                    bool hasController = hasNode && hasWeight;

                    //Controller
                    DaeController controller = new DaeController();
                    if (hasController)
                    {
                        controller.id = meshName + "_ctrl_id";

                        controller.skin.source = "#" + geometry.id;
                        controller.skin.bind_shape_matrix.Set(new RenderBase.OMatrix());

                        DaeSource joints = new DaeSource
                        {
                            id = meshName + "_ctrl_joint_names_id",
                            Name_array = new DaeNameArray
                            {
                                id = meshName + "_ctrl_joint_names_array_id",
                                count = (uint)mdl.skeleton.Count,
                                data = jointNames
                            }
                        };
                        joints.technique_common.accessor.source = "#" + joints.Name_array.id;
                        joints.technique_common.accessor.count = joints.Name_array.count;
                        joints.technique_common.accessor.stride = 1;
                        joints.technique_common.accessor.AddParam("JOINT", "Name");

                        controller.skin.src.Add(joints);

                        DaeSource bindPoses = new DaeSource
                        {
                            id = meshName + "_ctrl_inv_bind_poses_id",
                            float_array = new DaeFloatArray
                            {
                                id = meshName + "_ctrl_inv_bind_poses_array_id",
                                count = (uint)(mdl.skeleton.Count * 16),
                                data = invBindPoses
                            }
                        };
                        bindPoses.technique_common.accessor.source = "#" + bindPoses.float_array.id;
                        bindPoses.technique_common.accessor.count = (uint)mdl.skeleton.Count;
                        bindPoses.technique_common.accessor.stride = 16;
                        bindPoses.technique_common.accessor.AddParam("TRANSFORM", "float4x4");

                        controller.skin.src.Add(bindPoses);

                        DaeSource weights = new DaeSource
                        {
                            id = meshName + "_ctrl_weights_id",
                            float_array = new DaeFloatArray
                            {
                                id = meshName + "_ctrl_weights_array_id"
                            }
                        };
                        weights.technique_common.accessor.source = "#" + weights.float_array.id;
                        weights.technique_common.accessor.stride = 1;
                        weights.technique_common.accessor.AddParam("WEIGHT", "float");

                        StringBuilder w = new StringBuilder();
                        StringBuilder vcount = new StringBuilder();
                        StringBuilder v = new StringBuilder();

                        float[] wLookBack = new float[32];
                        uint wLookBackIndex = 0;
                        int buffLen = 0;

                        int wIndex = 0;
                        int wCount = 0;
                        foreach (RenderBase.OVertex vtx in mesh.vertices)
                        {
                            int count = Math.Min(vtx.node.Count, vtx.weight.Count);

                            vcount.Append(count + " ");
                            for (int n = 0; n < count; n++)
                            {
                                v.Append(vtx.node[n] + " ");
                                bool found = false;
                                uint bPos = (wLookBackIndex - 1) & 0x1f;
                                for (int i = 0; i < buffLen; i++)
                                {
                                    if (wLookBack[bPos] == vtx.weight[n])
                                    {
                                        v.Append(wIndex - (i + 1) + " ");
                                        found = true;
                                        break;
                                    }
                                    bPos = (bPos - 1) & 0x1f;
                                }

                                if (!found)
                                {
                                    v.Append(wIndex++ + " ");
                                    w.Append(vtx.weight[n].ToString(CultureInfo.InvariantCulture) + " ");
                                    wCount++;

                                    wLookBack[wLookBackIndex] = vtx.weight[n];
                                    wLookBackIndex = (wLookBackIndex + 1) & 0x1f;
                                    if (buffLen < wLookBack.Length) buffLen++;
                                }
                            }
                        }

                        weights.float_array.data = w.ToString().TrimEnd();
                        weights.float_array.count = (uint)wCount;
                        weights.technique_common.accessor.count = (uint)wCount;

                        controller.skin.src.Add(weights);
                        controller.skin.vertex_weights.vcount = vcount.ToString().TrimEnd();
                        controller.skin.vertex_weights.v = v.ToString().TrimEnd();
                        controller.skin.vertex_weights.count = (uint)mesh.vertices.Count;
                        controller.skin.joints.AddInput("JOINT", "#" + joints.id);
                        controller.skin.joints.AddInput("INV_BIND_MATRIX", "#" + bindPoses.id);

                        controller.skin.vertex_weights.AddInput("JOINT", "#" + joints.id);
                        controller.skin.vertex_weights.AddInput("WEIGHT", "#" + weights.id, 1);

                        if (daeShiny.library_controllers == null) daeShiny.library_controllers = new List<DaeController>();
                        daeShiny.library_controllers.Add(controller);
                    }

                    //Visual scene node
                    DaeNode node = new DaeNode
                    {
                        name = "vsn_" + meshName
                    };
                    node.id = node.name + "_id";
                    node.matrix.Set(new RenderBase.OMatrix());
                    if (hasController)
                    {
                        node.instance_controller = new DaeInstanceController
                        {
                            url = "#" + controller.id,
                            skeleton = "#" + mdl.skeleton[0].name + "_bone_id"
                        };
                        node.instance_controller.bind_material.technique_common.instance_material.symbol = mdl.material[obj.materialId].name + "_mat";
                        node.instance_controller.bind_material.technique_common.instance_material.target = "#" + mdl.material[obj.materialId].name + "_mat_id";
                    }
                    else
                    {
                        node.instance_geometry = new DaeInstanceGeometry
                        {
                            url = "#" + geometry.id
                        };
                        node.instance_geometry.bind_material.technique_common.instance_material.symbol = mdl.material[obj.materialId].name + "_mat";
                        node.instance_geometry.bind_material.technique_common.instance_material.target = "#" + mdl.material[obj.materialId].name + "_mat_id";
                    }

                    vs.node.Add(node);
                }
                daeShiny.library_visual_scenes.Add(vs);

                scene = new DaeInstaceVisualScene
                {
                    url = "#" + vs.id
                };
                daeShiny.scene.Add(scene);

                settings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true
                };

                ns = new XmlSerializerNamespaces();
                ns.Add("", "http://www.collada.org/2005/11/COLLADASchema");
                serializer = new XmlSerializer(typeof(COLLADA));
                output = XmlWriter.Create(new FileStream(fileName.Replace(".bch", "_Shiny.dae").Replace("BCH", "DAE"), FileMode.Create), settings);
                serializer.Serialize(output, daeShiny, ns);
                output.Close();
            }
            #endregion
        }

        /// <summary>
        ///     Transforms a Skeleton from relative to absolute positions.
        /// </summary>
        /// <param name="skeleton">The skeleton</param>
        /// <param name="index">Index of the bone to convert</param>
        /// <param name="target">Target matrix to save bone transformation</param>
        private static void TransformSkeleton(List<RenderBase.OBone> skeleton, int index, ref RenderBase.OMatrix target)
        {
            target *= RenderBase.OMatrix.RotateX(skeleton[index].rotation.x);
            target *= RenderBase.OMatrix.RotateY(skeleton[index].rotation.y);
            target *= RenderBase.OMatrix.RotateZ(skeleton[index].rotation.z);
            target *= RenderBase.OMatrix.Translate(skeleton[index].translation);
            if (skeleton[index].parentId > -1) TransformSkeleton(skeleton, skeleton[index].parentId, ref target);
        }

        /// <summary>
        ///     Writes the skeleton hierarchy to the DAE.
        /// </summary>
        /// <param name="skeleton">The skeleton</param>
        /// <param name="index">Index of the current bone (root bone when it's not a recursive call)</param>
        /// <param name="nodes">List with the DAE nodes</param>
        private static void WriteSkeleton(List<RenderBase.OBone> skeleton, int index, ref List<DaeNode> nodes)
        {
            DaeNode node = new DaeNode
            {
                name = skeleton[index].name
            };
            node.id = node.name + "_bone_id";
            node.sid = node.name;
            node.type = "JOINT";

            RenderBase.OMatrix transform = new RenderBase.OMatrix();
            transform *= RenderBase.OMatrix.RotateX(skeleton[index].rotation.x);
            transform *= RenderBase.OMatrix.RotateY(skeleton[index].rotation.y);
            transform *= RenderBase.OMatrix.RotateZ(skeleton[index].rotation.z);
            transform *= RenderBase.OMatrix.Translate(skeleton[index].translation);

            node.matrix.Set(transform);

            for (int i = 0; i < skeleton.Count; i++)
            {
                if (skeleton[i].parentId == index)
                {
                    if (node.childs == null) node.childs = new List<DaeNode>();
                    WriteSkeleton(skeleton, i, ref node.childs);
                }
            }

            nodes.Add(node);
        }
    }
}

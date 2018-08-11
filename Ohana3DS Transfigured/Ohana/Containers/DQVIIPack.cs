//Dragon Quest VII Container parser made by gdkchan for Ohana3DS
using System.Collections.Generic;
using System.IO;

using Ohana3DS_Transfigured.Ohana.Models;

namespace Ohana3DS_Transfigured.Ohana.Containers
{
    public class DQVIIPack
    {
        private struct SectionEntry
        {
            public uint offset;
            public uint length;
        }

        private struct Node
        {
            public string name;
            public int parentId;
            public RenderBase.OMatrix transform;
        }

        /// <summary>
        ///     Reads the Model PACKage from Dragon Quest VII.
        /// </summary>
        /// <param name="fileName">The File Name where the data is located</param>
        /// <returns></returns>
        public static OContainer Load(string fileName)
        {
            return Load(new FileStream(fileName, FileMode.Open));
        }

        /// <summary>
        ///     Reads the Model PACKage from Dragon Quest VII.
        /// </summary>
        /// <param name="data">Stream of the data</param>
        /// <returns></returns>
        public static OContainer Load(Stream data)
        {
            BinaryReader input = new BinaryReader(data);
            OContainer output = new OContainer();

            List<SectionEntry> mainSection = GetSection(input);

            //World nodes section
            data.Seek(mainSection[0].offset, SeekOrigin.Begin);
            List<Node> nodes = new List<Node>();
            List<SectionEntry> worldNodesSection = GetSection(input);
            foreach (SectionEntry entry in worldNodesSection)
            {
                data.Seek(entry.offset, SeekOrigin.Begin);

                Node n = new Node();

                //Geometry node
                input.ReadUInt32(); //GNOD magic number
                input.ReadUInt32();
                input.ReadUInt32();
                n.parentId = input.ReadInt32();
                n.name = IOUtils.ReadString(input, (uint)data.Position);

                data.Seek(entry.offset + 0x20, SeekOrigin.Begin);
                n.transform = new RenderBase.OMatrix();
                RenderBase.OVector4 t = new RenderBase.OVector4(input.ReadSingle(), input.ReadSingle(), input.ReadSingle(), input.ReadSingle());
                RenderBase.OVector4 r = new RenderBase.OVector4(input.ReadSingle(), input.ReadSingle(), input.ReadSingle(), input.ReadSingle());
                RenderBase.OVector4 s = new RenderBase.OVector4(input.ReadSingle(), input.ReadSingle(), input.ReadSingle(), input.ReadSingle());
                n.transform *= RenderBase.OMatrix.Scale(new RenderBase.OVector3(s.x, s.y, s.z));
                n.transform *= RenderBase.OMatrix.RotateX(r.x);
                n.transform *= RenderBase.OMatrix.RotateY(r.y);
                n.transform *= RenderBase.OMatrix.RotateZ(r.z);
                n.transform *= RenderBase.OMatrix.Translate(new RenderBase.OVector3(t.x, t.y, t.z));

                nodes.Add(n);
            }

            RenderBase.OMatrix[] nodesTransform = new RenderBase.OMatrix[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                RenderBase.OMatrix transform = new RenderBase.OMatrix();
                TransformNode(nodes, i, ref transform);
                nodesTransform[i] = transform;
            }

            //Models section
            data.Seek(mainSection[1].offset, SeekOrigin.Begin);
            List<SectionEntry> modelsSection = GetSection(input);
            foreach (SectionEntry entry in modelsSection)
            {
                data.Seek(entry.offset, SeekOrigin.Begin);
                
                //Field Data section
                /*
                 * Usually have 3 entries.
                 * 1st entry: Model CGFX
                 * 2nd entry: Unknow CGFX, possibly animations
                 * 3rd entry: Another FieldData section, possibly child object
                 */

                List<SectionEntry> fieldDataSection = GetSection(input);
                data.Seek(fieldDataSection[0].offset, SeekOrigin.Begin);
                uint length = fieldDataSection[0].length;
                while ((length & 0x7f) != 0) length++; //Align
                byte[] buffer = new byte[length];
                input.Read(buffer, 0, buffer.Length);

                OContainer.FileEntry file = new OContainer.FileEntry
                {
                    name = CGFX.GetName(new MemoryStream(buffer)) + ".bcmdl",
                    data = buffer
                };

                output.content.Add(file);
            }

            //FILE section
            data.Seek(mainSection[2].offset, SeekOrigin.Begin);
            //TODO

            //Collision section
            data.Seek(mainSection[3].offset, SeekOrigin.Begin);
            //TODO

            //PARM(???) section
            data.Seek(mainSection[4].offset, SeekOrigin.Begin);
            //TODO

            //Textures CGFX
            data.Seek(mainSection[5].offset, SeekOrigin.Begin);
            byte[] texBuffer = new byte[mainSection[5].length];
            input.Read(texBuffer, 0, texBuffer.Length);

            OContainer.FileEntry texFile = new OContainer.FileEntry
            {
                name = "textures.bctex",
                data = texBuffer
            };

            output.content.Add(texFile);

            data.Close();

            return output;
        }

        /// <summary>
        ///     Gets a generic section from the file.
        /// </summary>
        /// <param name="input">BinaryReader of the data</param>
        /// <returns></returns>
        private static List<SectionEntry> GetSection(BinaryReader input)
        {
            uint baseAddress = (uint)input.BaseStream.Position;
            input.BaseStream.Seek(0x10, SeekOrigin.Current); //Section magic number (padded with 0x0)
            uint addressCount = input.ReadUInt32();
            baseAddress += input.ReadUInt32();
            uint addressSectionLength = input.ReadUInt32();
            input.ReadUInt32(); //Padding

            List<SectionEntry> sections = new List<SectionEntry>();
            for (int i = 0; i < addressCount; i++)
            {
                SectionEntry entry = new SectionEntry
                {
                    offset = input.ReadUInt32() + baseAddress,
                    length = input.ReadUInt32()
                };

                sections.Add(entry);
            }

            return sections;
        }

        /// <summary>
        ///     Transforms a Node from relative to absolute positions.
        /// </summary>
        /// <param name="nodes">A list with all nodes</param>
        /// <param name="index">Index of the node to convert</param>
        /// <param name="target">Target matrix to save node transformation</param>
        private static void TransformNode(List<Node> nodes, int index, ref RenderBase.OMatrix target)
        {
            target *= nodes[index].transform;
            if (nodes[index].parentId > -1) TransformNode(nodes, nodes[index].parentId, ref target);
        }
    }
}

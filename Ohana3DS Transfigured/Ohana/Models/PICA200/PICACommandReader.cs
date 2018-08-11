using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Ohana3DS_Transfigured.Ohana.Models.PICA200
{
    class PICACommandReader
    {
        readonly List<float>[] floatUniform = new List<float>[96];
        List<float> uniform = new List<float>();
        readonly float[] lookUpTable = new float[256];
        readonly uint lutIndex;

        private readonly uint[] commands = new uint[0x10000];
        readonly uint currentUniform;

        /// <summary>
        ///     Creates a new PICA200 Command Buffer reader and reads the content.
        /// </summary>
        /// <param name="input">The Input stream where the buffer is located</param>
        /// <param name="wordCount">Total number of words (4 bytes per word) that the buffer have</param>
        /// <param name="ignoreAlign">(Optional) Set to true to ignore potential 0x0 padding words</param>
        public PICACommandReader(Stream input, uint wordCount, bool ignoreAlign = false)
        {
            BinaryReader reader = new BinaryReader(input);

            uint readedWords = 0;
            while (readedWords < wordCount)
            {
                uint parameter = reader.ReadUInt32();
                uint header = reader.ReadUInt32();
                readedWords += 2;

                ushort id = (ushort)(header & 0xffff);
                uint mask = (header >> 16) & 0xf;
                uint extraParameters = (header >> 20) & 0x7ff;
                bool consecutiveWriting = (header & 0x80000000) > 0;

                commands[id] = (GetParameter(id) & (~mask & 0xf)) | (parameter & (0xfffffff0 | mask));
                if (id == PICACommand.blockEnd) break;
                else if (id == PICACommand.vertexShaderFloatUniformConfig) currentUniform = parameter & 0x7fffffff;
                else if (id == PICACommand.vertexShaderFloatUniformData) uniform.Add(ToFloat(commands[id]));
                else if (id == PICACommand.fragmentShaderLookUpTableData) lookUpTable[lutIndex++] = commands[id];
                for (int i = 0; i < extraParameters; i++)
                {
                    if (consecutiveWriting) id++;
                    commands[id] = (GetParameter(id) & (~mask & 0xf)) | (reader.ReadUInt32() & (0xfffffff0 | mask));
                    readedWords++;

                    if (id > PICACommand.vertexShaderFloatUniformConfig && id < PICACommand.vertexShaderFloatUniformData + 8) uniform.Add(ToFloat(commands[id]));
                    else if (id == PICACommand.fragmentShaderLookUpTableData) lookUpTable[lutIndex++] = commands[id];
                }

                if (uniform.Count > 0)
                {
                    if (floatUniform[currentUniform] == null) floatUniform[currentUniform] = new List<float>();
                    floatUniform[currentUniform].AddRange(uniform);
                    uniform.Clear();
                }
                lutIndex = 0;

                if (!ignoreAlign) while ((input.Position & 7) != 0) reader.ReadUInt32(); //Ignore 0x0 padding Words
            }
        }

        /// <summary>
        ///     Gets the lastest written parameter of a given Command in the buffer.
        /// </summary>
        /// <param name="commandId">ID code of the command</param>
        /// <returns></returns>
        public uint GetParameter(ushort commandId)
        {
            return commands[commandId];
        }

        /// <summary>
        ///     Converts a IEEE 754 encoded float on uint to float.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private float ToFloat(uint value)
        {
            byte[] buffer = new byte[4];
            buffer[0] = (byte)(value & 0xff);
            buffer[1] = (byte)((value >> 8) & 0xff);
            buffer[2] = (byte)((value >> 16) & 0xff);
            buffer[3] = (byte)((value >> 24) & 0xff);
            return BitConverter.ToSingle(buffer, 0);
        }

        /// <summary>
        ///     Gets the Total Attributes minus 1.
        /// </summary>
        /// <returns></returns>
        public uint GetVSHTotalAttributes()
        {
            return GetParameter(PICACommand.vertexShaderTotalAttributes);
        }

        /// <summary>
        ///     Gets an array containing all Vertex Atrributes permutation order.
        /// </summary>
        /// <returns></returns>
        public PICACommand.VshAttribute[] GetVSHAttributesBufferPermutation()
        {
            ulong permutation = GetParameter(PICACommand.vertexShaderAttributesPermutationLow);
            permutation |= GetParameter(PICACommand.vertexShaderAttributesPermutationHigh) << 32;

            PICACommand.VshAttribute[] attributes = new PICACommand.VshAttribute[23];
            for (int attribute = 0; attribute < attributes.Length; attribute++)
            {
                attributes[attribute] = (PICACommand.VshAttribute)((permutation >> (attribute * 4)) & 0xf);
            }
            return attributes;
        }


        /// <summary>
        ///     Gets the Address where the Attributes Buffer is located.
        ///     Note that it may be a Relative address, and may need to be relocated.
        /// </summary>
        /// <returns></returns>
        public uint GetVSHAttributesBufferAddress()
        {
            return GetParameter(PICACommand.vertexShaderAttributesBufferAddress);
        }

        /// <summary>
        ///     Gets an array containing all formats of the main Attributes Buffer.
        ///     It have the length of each attribute, and the format (byte, float, short...).
        /// </summary>
        /// <returns></returns>
        public PICACommand.AttributeFormat[] GetVSHAttributesBufferFormat()
        {
            ulong format = GetParameter(PICACommand.vertexShaderAttributesBufferFormatLow);
            format |= GetParameter(PICACommand.vertexShaderAttributesBufferFormatHigh) << 32;

            PICACommand.AttributeFormat[] formats = new PICACommand.AttributeFormat[23];
            for (int attribute = 0; attribute < formats.Length; attribute++)
            {
                byte value = (byte)((format >> (attribute * 4)) & 0xf);
                formats[attribute].type = (PICACommand.AttributeFormatType)(value & 3);
                formats[attribute].attributeLength = (uint)(value >> 2);
            }
            return formats;
        }

        /// <summary>
        ///     Gets the Address of a given Attributes Buffer.
        ///     It is relative to the Main Buffer address.
        /// </summary>
        /// <param name="bufferIndex">Index number of the buffer (0-11)</param>
        /// <returns></returns>
        public uint GetVSHAttributesBufferAddress(byte bufferIndex)
        {
            return GetParameter((ushort)(PICACommand.vertexShaderAttributesBuffer0Address + (bufferIndex * 3)));
        }

        /// <summary>
        ///     Gets the Total Attributes of a given Attributes Buffer.
        /// </summary>
        /// <param name="bufferIndex">Index number of the buffer (0-11)</param>
        /// <returns></returns>
        public uint GetVSHTotalAttributes(byte bufferIndex)
        {
            uint value = GetParameter((ushort)(PICACommand.vertexShaderAttributesBuffer0Stride + (bufferIndex * 3)));
            return value >> 28;
        }

        /// <summary>
        ///     Gets Uniform Booleans used on Vertex Shader.
        /// </summary>
        /// <returns></returns>
        public bool[] GetVSHBooleanUniforms()
        {
            bool[] output = new bool[16];

            uint value = GetParameter((ushort)PICACommand.vertexShaderBooleanUniforms);
            for (int i = 0; i < 16; i++) output[i] = (value & (1 << i)) > 0;
            return output;
        }

        /// <summary>
        ///     Gets the Permutation of a given Attributes Buffer.
        ///     Values corresponds to a value on the main Permutation.
        /// </summary>
        /// <param name="bufferIndex">Index number of the buffer (0-11)</param>
        /// <returns></returns>
        public uint[] GetVSHAttributesBufferPermutation(byte bufferIndex)
        {
            ulong permutation = GetParameter((ushort)(PICACommand.vertexShaderAttributesBuffer0Permutation + (bufferIndex * 3)));
            permutation |= (GetParameter((ushort)(PICACommand.vertexShaderAttributesBuffer0Stride + (bufferIndex * 3))) & 0xffff) << 32;

            uint[] attributes = new uint[23];
            for (int attribute = 0; attribute < attributes.Length; attribute++)
            {
                attributes[attribute] = (uint)((permutation >> (attribute * 4)) & 0xf);
            }
            return attributes;
        }

        /// <summary>
        ///     Gets the Stride of a given Attributes Buffer.
        /// </summary>
        /// <param name="bufferIndex">Index number of the buffer (0-11)</param>
        /// <returns></returns>
        public byte GetVSHAttributesBufferStride(byte bufferIndex)
        {
            uint value = GetParameter((ushort)(PICACommand.vertexShaderAttributesBuffer0Stride + (bufferIndex * 3)));
            return (byte)((value >> 16) & 0xff);
        }

        /// <summary>
        ///     Gets the Float Uniform data array from the given register.
        /// </summary>
        /// <param name="register">Index number of the register (observed values: 6 and 7)</param>
        /// <returns></returns>
        public Stack<float> GetVSHFloatUniformData(uint register)
        {
            Stack<float> data = new Stack<float>();
            foreach (float value in floatUniform[register]) data.Push(value);
            return data;
        }

        /// <summary>
        ///     Gets the Address where the Index Buffer is located.
        /// </summary>
        /// <returns></returns>
        public uint GetIndexBufferAddress()
        {
            return GetParameter(PICACommand.indexBufferConfig) & 0x7fffffff;
        }

        /// <summary>
        ///     Gets the Format of the Index Buffer (byte or short).
        /// </summary>
        /// <returns></returns>
        public PICACommand.IndexBufferFormat GetIndexBufferFormat()
        {
            return (PICACommand.IndexBufferFormat)(GetParameter(PICACommand.indexBufferConfig) >> 31);
        }

        /// <summary>
        ///     Gets the total number of vertices indexed by the Index Buffer.
        /// </summary>
        /// <returns></returns>
        public uint GetIndexBufferTotalVertices()
        {
            return GetParameter(PICACommand.indexBufferTotalVertices);
        }

        /// <summary>
        ///     Gets TEV Stage parameters.
        /// </summary>
        /// <param name="stage">The stage (0-5)</param>
        /// <returns></returns>
        public RenderBase.OTextureCombiner GetTevStage(byte stage)
        {
            RenderBase.OTextureCombiner output = new RenderBase.OTextureCombiner();

            ushort baseCommand = 0;
            switch (stage)
            {
                case 0: baseCommand = PICACommand.tevStage0Source; break;
                case 1: baseCommand = PICACommand.tevStage1Source; break;
                case 2: baseCommand = PICACommand.tevStage2Source; break;
                case 3: baseCommand = PICACommand.tevStage3Source; break;
                case 4: baseCommand = PICACommand.tevStage4Source; break;
                case 5: baseCommand = PICACommand.tevStage5Source; break;
                default: throw new Exception("PICACommandReader: Invalid TevStage number!");
            }

            //Source
            uint source = GetParameter(baseCommand);

            output.rgbSource[0] = (RenderBase.OCombineSource)(source & 0xf);
            output.rgbSource[1] = (RenderBase.OCombineSource)((source >> 4) & 0xf);
            output.rgbSource[2] = (RenderBase.OCombineSource)((source >> 8) & 0xf);

            output.alphaSource[0] = (RenderBase.OCombineSource)((source >> 16) & 0xf);
            output.alphaSource[1] = (RenderBase.OCombineSource)((source >> 20) & 0xf);
            output.alphaSource[2] = (RenderBase.OCombineSource)((source >> 24) & 0xf);

            //Operand
            uint operand = GetParameter((ushort)(baseCommand + 1));

            output.rgbOperand[0] = (RenderBase.OCombineOperandRgb)(operand & 0xf);
            output.rgbOperand[1] = (RenderBase.OCombineOperandRgb)((operand >> 4) & 0xf);
            output.rgbOperand[2] = (RenderBase.OCombineOperandRgb)((operand >> 8) & 0xf);

            output.alphaOperand[0] = (RenderBase.OCombineOperandAlpha)((operand >> 12) & 0xf);
            output.alphaOperand[1] = (RenderBase.OCombineOperandAlpha)((operand >> 16) & 0xf);
            output.alphaOperand[2] = (RenderBase.OCombineOperandAlpha)((operand >> 20) & 0xf);

            //Operator
            uint combine = GetParameter((ushort)(baseCommand + 2));

            output.combineRgb = (RenderBase.OCombineOperator)(combine & 0xffff);
            output.combineAlpha = (RenderBase.OCombineOperator)(combine >> 16);

            //Scale
            uint scale = GetParameter((ushort)(baseCommand + 4));

            output.rgbScale = (ushort)((scale & 0xffff) + 1);
            output.alphaScale = (ushort)((scale >> 16) + 1);

            return output;
        }

        /// <summary>
        ///     Gets the Fragment Buffer Color.
        /// </summary>
        /// <returns></returns>
        public Color GetFragmentBufferColor()
        {
            uint rgba = GetParameter(PICACommand.fragmentBufferColor);
            return Color.FromArgb((byte)(rgba >> 24),
                (byte)(rgba & 0xff),
                (byte)((rgba >> 8) & 0xff),
                (byte)((rgba >> 16) & 0xff));
        }

        /// <summary>
        ///     Gets Blending operation parameters.
        /// </summary>
        /// <returns></returns>
        public RenderBase.OBlendOperation GetBlendOperation()
        {
            RenderBase.OBlendOperation output = new RenderBase.OBlendOperation();

            uint value = GetParameter(PICACommand.blendConfig);
            output.rgbFunctionSource = (RenderBase.OBlendFunction)((value >> 16) & 0xf);
            output.rgbFunctionDestination = (RenderBase.OBlendFunction)((value >> 20) & 0xf);
            output.alphaFunctionSource = (RenderBase.OBlendFunction)((value >> 24) & 0xf);
            output.alphaFunctionDestination = (RenderBase.OBlendFunction)((value >> 28) & 0xf);
            output.rgbBlendEquation = (RenderBase.OBlendEquation)(value & 0xff);
            output.alphaBlendEquation = (RenderBase.OBlendEquation)((value >> 8) & 0xff);

            return output;
        }

        /// <summary>
        ///     Gets the Logical operation applied to Fragment colors.
        /// </summary>
        /// <returns></returns>
        public RenderBase.OLogicalOperation GetColorLogicOperation()
        {
            return (RenderBase.OLogicalOperation)(GetParameter(PICACommand.colorLogicOperationConfig) & 0xf);
        }

        /// <summary>
        ///     Gets the parameters used for Alpha testing.
        /// </summary>
        /// <returns></returns>
        public RenderBase.OAlphaTest GetAlphaTest()
        {
            RenderBase.OAlphaTest output = new RenderBase.OAlphaTest();

            uint value = GetParameter(PICACommand.alphaTestConfig);
            output.isTestEnabled = (value & 1) > 0;
            output.testFunction = (RenderBase.OTestFunction)((value >> 4) & 0xf);
            output.testReference = ((value >> 8) & 0xff);

            return output;
        }

        /// <summary>
        ///     Gets the parameters used for Stencil testing.
        /// </summary>
        /// <returns></returns>
        public RenderBase.OStencilOperation GetStencilTest()
        {
            RenderBase.OStencilOperation output = new RenderBase.OStencilOperation();

            //Test
            uint test = GetParameter(PICACommand.stencilTestConfig);

            output.isTestEnabled = (test & 1) > 0;
            output.testFunction = (RenderBase.OTestFunction)((test >> 4) & 0xf);
            output.testReference = (test >> 16) & 0xff;
            output.testMask = (test >> 24);

            //Operation
            uint operation = GetParameter(PICACommand.stencilOperationConfig);

            output.failOperation = (RenderBase.OStencilOp)(operation & 0xf);
            output.zFailOperation = (RenderBase.OStencilOp)((operation >> 4) & 0xf);
            output.passOperation = (RenderBase.OStencilOp)((operation >> 8) & 0xf);

            return output;
        }

        /// <summary>
        ///     Gets the parameters used for Depth testing.
        /// </summary>
        /// <returns></returns>
        public RenderBase.ODepthOperation GetDepthTest()
        {
            RenderBase.ODepthOperation output = new RenderBase.ODepthOperation();

            uint value = GetParameter(PICACommand.depthTestConfig);
            output.isTestEnabled = (value & 1) > 0;
            output.testFunction = (RenderBase.OTestFunction)((value >> 4) & 0xf);
            output.isMaskEnabled = (value & 0x1000) > 0;

            return output;
        }

        /// <summary>
        ///     Gets the Culling mode.
        /// </summary>
        /// <returns></returns>
        public RenderBase.OCullMode GetCullMode()
        {
            uint value = GetParameter(PICACommand.cullModeConfig);
            return (RenderBase.OCullMode)(value & 0xf);
        }

        /// <summary>
        ///     Gets the 1D LookUp table sampler used on the Fragment Shader lighting.
        /// </summary>
        /// <returns></returns>
        public float[] GetFSHLookUpTable()
        {
            return lookUpTable;
        }

        /// <summary>
        ///     Gets if the Absolute value should be used before using the LUT for each Input.
        /// </summary>
        /// <returns></returns>
        public PICACommand.FragmentSamplerAbsolute GetReflectanceSamplerAbsolute()
        {
            PICACommand.FragmentSamplerAbsolute output = new PICACommand.FragmentSamplerAbsolute();

            uint value = GetParameter(PICACommand.lutSamplerAbsolute);
            output.r = (value & 0x2000000) == 0;
            output.g = (value & 0x200000) == 0;
            output.b = (value & 0x20000) == 0;
            output.d0 = (value & 2) == 0;
            output.d1 = (value & 0x20) == 0;
            output.fresnel = (value & 0x2000) == 0;

            return output;
        }

        /// <summary>
        ///     Gets the Input used to pick a value from the LookUp Table on Fragment Shader.
        /// </summary>
        /// <returns></returns>
        public PICACommand.FragmentSamplerInput GetReflectanceSamplerInput()
        {
            PICACommand.FragmentSamplerInput output = new PICACommand.FragmentSamplerInput();

            uint value = GetParameter(PICACommand.lutSamplerInput);
            output.r = (RenderBase.OFragmentSamplerInput)((value >> 24) & 0xf);
            output.g = (RenderBase.OFragmentSamplerInput)((value >> 20) & 0xf);
            output.b = (RenderBase.OFragmentSamplerInput)((value >> 16) & 0xf);
            output.d0 = (RenderBase.OFragmentSamplerInput)(value & 0xf);
            output.d1 = (RenderBase.OFragmentSamplerInput)((value >> 4) & 0xf);
            output.fresnel = (RenderBase.OFragmentSamplerInput)((value >> 12) & 0xf);

            return output;
        }

        /// <summary>
        ///     Gets the scale used on the value on Fragment Shader.
        /// </summary>
        /// <returns></returns>
        public PICACommand.FragmentSamplerScale GetReflectanceSamplerScale()
        {
            PICACommand.FragmentSamplerScale output = new PICACommand.FragmentSamplerScale();

            uint value = GetParameter(PICACommand.lutSamplerScale);
            output.r = (RenderBase.OFragmentSamplerScale)((value >> 24) & 0xf);
            output.g = (RenderBase.OFragmentSamplerScale)((value >> 20) & 0xf);
            output.b = (RenderBase.OFragmentSamplerScale)((value >> 16) & 0xf);
            output.d0 = (RenderBase.OFragmentSamplerScale)(value & 0xf);
            output.d1 = (RenderBase.OFragmentSamplerScale)((value >> 4) & 0xf);
            output.fresnel = (RenderBase.OFragmentSamplerScale)((value >> 12) & 0xf);

            return output;
        }

        /// <summary>
        ///     Gets the Address of the texture at Texture Unit 0.
        /// </summary>
        /// <returns></returns>
        public uint GetTexUnit0Address()
        {
            return GetParameter(PICACommand.texUnit0Address);
        }

        /// <summary>
        ///     Gets the mapping parameters used on Texture Unit 0,
        ///     such as the wrapping mode, filtering and so on.
        /// </summary>
        /// <returns></returns>
        public RenderBase.OTextureMapper GetTexUnit0Mapper()
        {
            RenderBase.OTextureMapper output = new RenderBase.OTextureMapper();

            uint value = GetParameter(PICACommand.texUnit0Param);
            output.magFilter = (RenderBase.OTextureMagFilter)((value >> 1) & 1);
            output.minFilter = (RenderBase.OTextureMinFilter)(((value >> 2) & 1) | ((value >> 23) & 2));
            output.wrapU = (RenderBase.OTextureWrap)((value >> 12) & 0xf);
            output.wrapV = (RenderBase.OTextureWrap)((value >> 8) & 0xf);

            return output;
        }

        /// <summary>
        ///     Gets the border color used on Texture Unit 0,
        ///     when the wrapping mode is set to Border.
        /// </summary>
        /// <returns></returns>
        public Color GetTexUnit0BorderColor()
        {
            uint rgba = GetParameter(PICACommand.texUnit0BorderColor);
            return Color.FromArgb((byte)(rgba >> 24),
                (byte)(rgba & 0xff),
                (byte)((rgba >> 8) & 0xff),
                (byte)((rgba >> 16) & 0xff));
        }

        /// <summary>
        ///     Gets the resolution of the texture at Texture Unit 0.
        /// </summary>
        /// <returns></returns>
        public Size GetTexUnit0Size()
        {
            uint value = GetParameter(PICACommand.texUnit0Size);
            return new Size((int)(value >> 16), (int)(value & 0xffff));
        }

        /// <summary>
        ///     Gets the encoded format of the texture at Texture Unit 0.
        /// </summary>
        public RenderBase.OTextureFormat GetTexUnit0Format()
        {
            return (RenderBase.OTextureFormat)GetParameter(PICACommand.texUnit0Type);
        }

        /// <summary>
        ///     Gets the Address of the texture at Texture Unit 1.
        /// </summary>
        /// <returns></returns>
        public uint GetTexUnit1Address()
        {
            return GetParameter(PICACommand.texUnit1Address);
        }

        /// <summary>
        ///     Gets the mapping parameters used on Texture Unit 1,
        ///     such as the wrapping mode, filtering and so on.
        /// </summary>
        /// <returns></returns>
        public RenderBase.OTextureMapper GetTexUnit1Mapper()
        {
            RenderBase.OTextureMapper output = new RenderBase.OTextureMapper();

            uint value = GetParameter(PICACommand.texUnit1Param);
            output.magFilter = (RenderBase.OTextureMagFilter)((value >> 1) & 1);
            output.minFilter = (RenderBase.OTextureMinFilter)(((value >> 2) & 1) | ((value >> 23) & 2));
            output.wrapU = (RenderBase.OTextureWrap)((value >> 12) & 0xf);
            output.wrapV = (RenderBase.OTextureWrap)((value >> 8) & 0xf);

            return output;
        }

        /// <summary>
        ///     Gets the border color used on Texture Unit 1,
        ///     when the wrapping mode is set to Border.
        /// </summary>
        /// <returns></returns>
        public Color GetTexUnit1BorderColor()
        {
            uint rgba = GetParameter(PICACommand.texUnit1BorderColor);
            return Color.FromArgb((byte)(rgba >> 24),
                (byte)(rgba & 0xff),
                (byte)((rgba >> 8) & 0xff),
                (byte)((rgba >> 16) & 0xff));
        }

        /// <summary>
        ///     Gets the resolution of the texture at Texture Unit 1.
        /// </summary>
        /// <returns></returns>
        public Size GetTexUnit1Size()
        {
            uint value = GetParameter(PICACommand.texUnit1Size);
            return new Size((int)(value >> 16), (int)(value & 0xffff));
        }

        /// <summary>
        ///     Gets the encoded format of the texture at Texture Unit 1.
        /// </summary>
        public RenderBase.OTextureFormat GetTexUnit1Format()
        {
            return (RenderBase.OTextureFormat)GetParameter(PICACommand.texUnit1Type);
        }

        /// <summary>
        ///     Gets the Address of the texture at Texture Unit 2.
        /// </summary>
        /// <returns></returns>
        public uint GetTexUnit2Address()
        {
            return GetParameter(PICACommand.texUnit2Address);
        }

        /// <summary>
        ///     Gets the mapping parameters used on Texture Unit 2,
        ///     such as the wrapping mode, filtering and so on.
        /// </summary>
        /// <returns></returns>
        public RenderBase.OTextureMapper GetTexUnit2Mapper()
        {
            RenderBase.OTextureMapper output = new RenderBase.OTextureMapper();

            uint value = GetParameter(PICACommand.texUnit2Param);
            output.magFilter = (RenderBase.OTextureMagFilter)((value >> 1) & 1);
            output.minFilter = (RenderBase.OTextureMinFilter)(((value >> 2) & 1) | ((value >> 23) & 2));
            output.wrapU = (RenderBase.OTextureWrap)((value >> 12) & 0xf);
            output.wrapV = (RenderBase.OTextureWrap)((value >> 8) & 0xf);

            return output;
        }

        /// <summary>
        ///     Gets the border color used on Texture Unit 2,
        ///     when the wrapping mode is set to Border.
        /// </summary>
        /// <returns></returns>
        public Color GetTexUnit2BorderColor()
        {
            uint rgba = GetParameter(PICACommand.texUnit2BorderColor);
            return Color.FromArgb(
                (byte)(rgba >> 24),
                (byte)(rgba & 0xff),
                (byte)((rgba >> 8) & 0xff),
                (byte)((rgba >> 16) & 0xff));
        }

        /// <summary>
        ///     Gets the resolution of the texture at Texture Unit 2.
        /// </summary>
        /// <returns></returns>
        public Size GetTexUnit2Size()
        {
            uint value = GetParameter(PICACommand.texUnit2Size);
            return new Size((int)(value >> 16), (int)(value & 0xffff));
        }

        /// <summary>
        ///     Gets the encoded format of the texture at Texture Unit 2.
        /// </summary>
        public RenderBase.OTextureFormat GetTexUnit2Format()
        {
            return (RenderBase.OTextureFormat)GetParameter(PICACommand.texUnit2Type);
        }
    }
}

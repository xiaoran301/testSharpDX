using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WindowsFormsApp1.Foundation;

namespace WindowsFormsApp1.Water
{

    public class WaterShader                   // 347 lines
    {
        // Structs
        [StructLayout(LayoutKind.Sequential)]
        internal struct DMatrixBuffer
        {
            public Matrix world;
            public Matrix view;
            public Matrix projection;
            public Matrix reflection;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DCamNormBufferType
        {
            public Vector3 cameraPosition;
            public float padding1;
            public Vector2 normalMapTiling;
            public Vector2 padding2;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct DWaterBuffer
        {
            internal Vector4 refractionTint;
            internal Vector3 lightDirection;
            internal float waterTranslation;
            internal float reflectRefractScale;
            internal float specularShininess;
            internal Vector2 padding;
        }

        // Properties
        public VertexShader VertexShader { get; set; }
        public PixelShader PixelShader { get; set; }
        public InputLayout Layout { get; set; }
        public SharpDX.Direct3D11.Buffer ConstantMatrixBuffer { get; set; }
        public SharpDX.Direct3D11.Buffer ConstantCamNormalBuffer { get; set; }
        public SharpDX.Direct3D11.Buffer ConstantWaterBuffer { get; set; }
        public SamplerState SamplerState { get; set; }

        // Constructor
        public WaterShader() { }

        // Methods
        public bool Initialize(Device device, IntPtr windowsHandler)
        {
            // Initialize the vertex and pixel shaders.
            return InitializeShader(device, windowsHandler, "Water.vs", "Water.ps");
        }
        private bool InitializeShader(Device device, IntPtr windowsHandler, string vsFileName, string psFileName)
        {
            try
            {
                // Setup full pathes
                vsFileName = GameConfig.ShaderFilePath + vsFileName;
                psFileName = GameConfig.ShaderFilePath + psFileName;

                #region Initilize Shaders
                // Compile the Vertex Shader & Pixel Shader code.
                ShaderBytecode vertexShaderByteCode = ShaderBytecode.CompileFromFile(vsFileName, "WaterVertexShader", GameConfig.VertexShaderProfile, ShaderFlags.None, EffectFlags.None);
                ShaderBytecode pixelShaderByteCode = ShaderBytecode.CompileFromFile(psFileName, "WaterPixelShader", GameConfig.PixelShaderProfile, ShaderFlags.None, EffectFlags.None);

                // Create the Vertex & Pixel Shaders from the buffer.
                VertexShader = new VertexShader(device, vertexShaderByteCode);
                PixelShader = new PixelShader(device, pixelShaderByteCode);
                #endregion

                #region Initialize Input Layouts
                // Now setup the layout of the data that goes into the shader.
                // This setup needs to match the VertexType structure in the Model and in the shader.
                InputElement[] inputElements = new InputElement[]
                {
                    new InputElement()
                    {
                        SemanticName = "POSITION",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
                        Slot = 0,
                        AlignedByteOffset = 0,
                        Classification = InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "TEXCOORD",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification = InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    }
                };

                // Create the vertex input the layout. Kin dof like a Vertex Declaration.
                Layout = new InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderByteCode), inputElements);
                #endregion

                // Release the vertex and pixel shader buffers, since they are no longer needed.
                vertexShaderByteCode.Dispose();
                pixelShaderByteCode.Dispose();

                #region Initialize Sampler
                // Create a texture sampler state description.
                SamplerStateDescription samplerDesc = new SamplerStateDescription()
                {
                    Filter = Filter.MinMagMipLinear,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    MipLodBias = 0,
                    MaximumAnisotropy = 1,
                    ComparisonFunction = Comparison.Always,
                    BorderColor = new Color4(0, 0, 0, 0),  // Black Border.
                    MinimumLod = 0,
                    MaximumLod = float.MaxValue
                };

                // Create the texture sampler state.
                SamplerState = new SamplerState(device, samplerDesc);
                #endregion

                #region Initialize Matrix Buffer
                // Setup the description of the dynamic matrix constant Matrix buffer that is in the vertex shader.
                BufferDescription matrixBufferDescription = new BufferDescription()
                {
                    Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<DMatrixBuffer>(),
                    BindFlags = BindFlags.ConstantBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                // Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
                ConstantMatrixBuffer = new SharpDX.Direct3D11.Buffer(device, matrixBufferDescription);
                #endregion

                #region Initialize Reflection Buffer
                // Setup the description of the camera and normal tiling dynamic constant buffer that is in the vertex shader.
                BufferDescription cameraNormalBufferDesc = new BufferDescription()
                {
                    Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<DCamNormBufferType>(),
                    BindFlags = BindFlags.ConstantBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                // Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
                ConstantCamNormalBuffer = new SharpDX.Direct3D11.Buffer(device, cameraNormalBufferDesc);
                #endregion

                #region Initialize Water Buffer
                // Setup the description of the dynamic matrix constant buffer that is in the vertex shader.
                BufferDescription waterBufferDesc = new BufferDescription()
                {
                    Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<DWaterBuffer>(),
                    BindFlags = BindFlags.ConstantBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                // Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
                ConstantWaterBuffer = new SharpDX.Direct3D11.Buffer(device, waterBufferDesc);
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing shader. Error is " + ex.Message);
                return false;
            }
        }
        public void Destroy()
        {
            // Shutdown the vertex and pixel shaders as well as the related objects.
            DestroyShader();
        }
        private void DestroyShader()
        {
            // Release the water constant buffer.
            ConstantWaterBuffer?.Dispose();
            ConstantWaterBuffer = null;
            // Release the reflection constant buffer.
            ConstantCamNormalBuffer?.Dispose();
            ConstantCamNormalBuffer = null;
            // Release the sampler state.
            SamplerState?.Dispose();
            SamplerState = null;
            // Release the matrix constant buffer.
            ConstantMatrixBuffer?.Dispose();
            ConstantMatrixBuffer = null;
            // Release the layout.
            Layout?.Dispose();
            Layout = null;
            // Release the pixel shader.
            PixelShader?.Dispose();
            PixelShader = null;
            // Release the vertex shader.
            VertexShader?.Dispose();
            VertexShader = null;
        }
        public bool Render(DeviceContext deviceContext, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, Matrix reflectionMatrix, ShaderResourceView reflectionTexture, ShaderResourceView refractionTexture, ShaderResourceView normalTexture, Vector3 cameraPosition, Vector2 normalMapTiling, float waterTranslation, float reflectRefractScale, Vector4 reflectionTint, Vector3 lightDirection, float specularShininess)
        {
            // Set the shader parameters that it will use for rendering.
            if (!SetShaderParameters(deviceContext, worldMatrix, viewMatrix, projectionMatrix, reflectionMatrix, reflectionTexture, refractionTexture, normalTexture, cameraPosition, normalMapTiling, waterTranslation, reflectRefractScale, reflectionTint, lightDirection, specularShininess))
                return false;

            // Now render the prepared buffers with the shader.
            RenderShader(deviceContext, indexCount);

            return true;
        }
        private bool SetShaderParameters(DeviceContext deviceContext, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, Matrix reflectionMatrix, ShaderResourceView reflectionTexture, ShaderResourceView refractionTexture, ShaderResourceView normalTexture, Vector3 cameraPosition, Vector2 normalMapTiling, float waterTranslation, float reflectRefractScale, Vector4 reflectionTint, Vector3 lightDirection, float specularShininess)
        {
            try
            {
                #region Set Matrix Shader Resources
                // Transpose the matrices to prepare them for shader.
                worldMatrix.Transpose();
                viewMatrix.Transpose();
                projectionMatrix.Transpose();
                reflectionMatrix.Transpose();

                // Lock the constant buffer so it can be written to.
                DataStream mappedResource;
                deviceContext.MapSubresource(ConstantMatrixBuffer, MapMode.WriteDiscard, MapFlags.None, out mappedResource);

                // Copy the passed in matrices into the constant buffer.
                DMatrixBuffer matrixBuffer = new DMatrixBuffer()
                {
                    world = worldMatrix,
                    view = viewMatrix,
                    projection = projectionMatrix,
                    reflection = reflectionMatrix
                };
                mappedResource.Write(matrixBuffer);

                // Unlock the constant buffer.
                deviceContext.UnmapSubresource(ConstantMatrixBuffer, 0);

                // Set the position of the constant buffer in the vertex shader.
                int bufferPositionNumber = 0;

                // Finally set the constant buffer in the vertex shader with the updated values.
                deviceContext.VertexShader.SetConstantBuffer(bufferPositionNumber, ConstantMatrixBuffer);
                #endregion

                #region Set Reflection Vertex Shader
                // Lock the camera and normal tiling constant buffer so it can be written to.
                deviceContext.MapSubresource(ConstantCamNormalBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out mappedResource);

                // Copy the matrices into the constant buffer.
                DCamNormBufferType camNormalBuffer = new DCamNormBufferType()
                {
                    cameraPosition = cameraPosition,
                    padding1 = 0.0f,
                    normalMapTiling = normalMapTiling,
                    padding2 = Vector2.Zero
                };
                mappedResource.Write(camNormalBuffer);

                // Unlock the constant buffer.
                deviceContext.UnmapSubresource(ConstantCamNormalBuffer, 0);

                // Set the position of the constant buffer in the vertex shader.
                bufferPositionNumber = 1;

                // Finally set the constant buffer in the vertex shader with the updated values.
                deviceContext.VertexShader.SetConstantBuffer(bufferPositionNumber, ConstantCamNormalBuffer);
                #endregion

                #region Set Water Pixel Shader
                // Lock the water constant buffer so it can be written to.
                deviceContext.MapSubresource(ConstantWaterBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out mappedResource);

                // Copy the matrices into the constant buffer.
                DWaterBuffer waterBuffer = new DWaterBuffer()
                {
                    waterTranslation = waterTranslation,
                    reflectRefractScale = reflectRefractScale,
                    refractionTint = reflectionTint,
                    specularShininess = specularShininess,
                    lightDirection = lightDirection,
                    padding = Vector2.Zero
                };
                mappedResource.Write(waterBuffer);

                // Unlock the constant buffer.
                deviceContext.UnmapSubresource(ConstantWaterBuffer, 0);

                // Set the position of the constant buffer in the vertex shader.
                bufferPositionNumber = 0;

                // Finally set the constant buffer in the vertex shader with the updated values.
                deviceContext.PixelShader.SetConstantBuffer(bufferPositionNumber, ConstantWaterBuffer);
                #endregion

                #region Set Pixel Shader Resources
                // Set the reflection texture resource in the pixel shader.
                deviceContext.PixelShader.SetShaderResources(0, refractionTexture);

                // Set the refraction texture resource in the pixel shader.
                deviceContext.PixelShader.SetShaderResources(1, reflectionTexture);

                // Set the normal map texture resource in the pixel shader.
                deviceContext.PixelShader.SetShaderResources(2, normalTexture);
                #endregion

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void RenderShader(DeviceContext deviceContext, int indexCount)
        {
            // Set the vertex input layout.
            deviceContext.InputAssembler.InputLayout = Layout;

            // Set the vertex and pixel shaders that will be used to render this triangle.
            deviceContext.VertexShader.Set(VertexShader);
            deviceContext.PixelShader.Set(PixelShader);

            // Set the sampler state in the pixel shader.
            deviceContext.PixelShader.SetSampler(0, SamplerState);

            // Render the triangle.
            deviceContext.DrawIndexed(indexCount, 0, 0);
        }
    }

}


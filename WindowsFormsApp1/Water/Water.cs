using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Runtime.InteropServices;
using WindowsFormsApp1.Foundation;

namespace WindowsFormsApp1.Water
{
    public class Water                 // 179 lines
    {
        // Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct DWaterVertexType
        {
            public Vector3 position;
            public Vector2 texture;
        }

        // Properties
        public int VertexCount { get; set; }
        public int IndexCount { get; set; }
        public float WaterHeight { get; set; }
        public float WaterTranslation { get; set; }
        public float ReflectRefractScale { get; set; }
        public float SpecularShininess { get; set; }
        public Vector2 NormalMapTiling { get; set; }
        public Vector4 RefractionTint { get; set; }
        public Texture Texture { get; set; }
        public SharpDX.Direct3D11.Buffer VertexBuffer { get; set; }
        public SharpDX.Direct3D11.Buffer IndexBuffer { get; set; }

        // Methods
        public bool Init(SharpDX.Direct3D11.Device device, string textureFileName, float waterHeight, float waterRadius)
        {
            // Store the water height.
            WaterHeight = waterHeight;

            // Initialize the vertex and index buffer that hold the geometry for the triangle.
            if (!InitializeBuffers(device, waterRadius))
                return false;

            // Load the texture for this model.
            if (!LoadTexture(device, textureFileName))
                return false;

            // Set the tiling for the water normal maps.
            NormalMapTiling = new Vector2
            {
                X = 0.01f,  // Tile ten times over the quad.
                Y = 0.02f   // Tile five times over the quad.
            };

            // Initialize the water translation to zero.
            WaterTranslation = 0.0f;

            // Set the scaling value for the water normal map.
            ReflectRefractScale = 0.03f;

            // Set the tint of the refraction.
            RefractionTint = new Vector4(0.0f, 0.8f, 1.0f, 1.0f);

            // Set the specular shininess.
            SpecularShininess = 200.0f;

            return true;
        }
        private bool LoadTexture(SharpDX.Direct3D11.Device device, string textureFileName)
        {
            textureFileName = GameConfig.DataFilePath + textureFileName;

            // Create the texture object.
            Texture = new Texture();

            // Initialize the texture object.
            if (!Texture.Init(device, textureFileName))
                return false;

            return true;
        }
        private bool InitializeBuffers(SharpDX.Direct3D11.Device device, float waterRadius)
        {
            try
            {
                // Set the number of vertices in the vertex array.
                VertexCount = 6;
                // Set the number of indices in the index array.
                IndexCount = 6;

                // Create the vertex array.
                DWaterVertexType[] vertices = new DWaterVertexType[VertexCount];
                // Create the index array.
                int[] indices = new int[IndexCount];

                // Load the vertex array with data.
                vertices[0].position = new Vector3(-waterRadius, 0.0f, waterRadius);  // Top left.
                vertices[0].texture = new Vector2(0.0f, 0.0f);
                vertices[1].position = new Vector3(waterRadius, 0.0f, waterRadius);  // Top right.
                vertices[1].texture = new Vector2(1.0f, 0.0f);
                vertices[2].position = new Vector3(-waterRadius, 0.0f, -waterRadius);  // Bottom left.
                vertices[2].texture = new Vector2(0.0f, 1.0f);
                vertices[3].position = new Vector3(-waterRadius, 0.0f, -waterRadius);  // Bottom left.
                vertices[3].texture = new Vector2(0.0f, 1.0f);
                vertices[4].position = new Vector3(waterRadius, 0.0f, waterRadius);  // Top right.
                vertices[4].texture = new Vector2(1.0f, 0.0f);
                vertices[5].position = new Vector3(waterRadius, 0.0f, -waterRadius);  // Bottom right.
                vertices[5].texture = new Vector2(1.0f, 1.0f);

                // Load the index array with data.
                indices[0] = 0;
                indices[1] = 1;
                indices[2] = 2;
                indices[3] = 3;
                indices[4] = 4;
                indices[5] = 5;

                // Create the vertex buffer.
                VertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, vertices);

                // Create the index buffer.
                IndexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);

                // Release the arrays now that the buffers have been created and loaded.
                vertices = null;
                indices = null;

                return true;
            }
            catch
            {
                return false;
            }
        }
        public void Destroy()
        {
            // Release the model texture.
            ReleaseTexture();

            // Release the vertex and index buffers.
            ShutdownBuffers();
        }
        private void ShutdownBuffers()
        {
            // Return the index buffer.
            IndexBuffer?.Dispose();
            IndexBuffer = null;
            // Release the vertex buffer.
            VertexBuffer?.Dispose();
            VertexBuffer = null;
        }
        private void ReleaseTexture()
        {
            // Release the texture object.
            Texture?.Destroy();
            Texture = null;
        }
        public void Update()
        {
            // Update the position of the water to simulate motion.
            WaterTranslation += 0.003f;
            if (WaterTranslation > 1.0f)
                WaterTranslation -= 1.0f;
        }
        public void Render(DeviceContext deviceContext)
        {
            // Put the vertex and index buffers on the graphics pipeline to prepare them for drawing.
            RenderBuffers(deviceContext);
        }
        private void RenderBuffers(DeviceContext deviceContext)
        {
            // Set the vertex buffer to active in the input assembler so it can be rendered.
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<DWaterVertexType>(), 0));
            // Set the index buffer to active in the input assembler so it can be rendered.
            deviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            // Set the type of the primitive that should be rendered from this vertex buffer, in this case triangles.
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }
    }

}

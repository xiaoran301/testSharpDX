using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Drawing;
using System.Runtime.InteropServices;
using WindowsFormsApp1.Foundation;

namespace WindowsFormsApp1.Water
{
    class Terrain
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct DVertexType
        {
            internal Vector3 position;
            internal Vector2 texture;
            internal Vector3 normal;
            internal Vector3 tangent;
            internal Vector3 binormal;
            internal Vector4 color;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DHeightMapType
        {
            public float x, y, z;
            public float nx, ny, nz;
            public float r, g, b;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DVertexTempType
        {
            public float x, y, z;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DTempVertexType
        {
            public float x, y, z;
            public float tu, tv;
            public float nx, ny, nz;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DModelType
        {
            public float x, y, z;
            public float tu, tv;
            public float nx, ny, nz;
            public float tx, ty, tz;
            public float bx, by, bz;
            public float r, g, b;
        }

        // Variables
        private int m_TerrainWidth, m_TerrainHeight;

        // Properties
        private SharpDX.Direct3D11.Buffer VertexBuffer { get; set; }
        private SharpDX.Direct3D11.Buffer IndexBuffer { get; set; }
        private int VertexCount { get; set; }
        public int IndexCount { get; private set; }
        public List<DHeightMapType> HeightMap = new List<DHeightMapType>();
        public Texture ColorTexture { get; set; }
        public Texture NormalMapTexture { get; set; }
        public DModelType[] TerrainModel { get; set; }

        // Constructor
        public Terrain() { }

        // Methods.
        public bool Initialize(SharpDX.Direct3D11.Device device, string heightMapFileName, string colorMapFilename, float flattenAmount, string colorTextureFilename, string normalMapFilename)
        {
            // Load in the height map for the terrain.
            if (!LoadHeightMap(heightMapFileName))
                return false;

            // Load in the color map for the terrain.
            if (!LoadColourMap(colorMapFilename))
                return false;

            // Reduce or Normalize the height of the height map.
            ReduceHeightMap(flattenAmount);

            // Calculate the normals for the terrain data.
            CalculateNormals();

            // Construct a 3D model from the height map and normal data.
            BuildTerrainModel();

            // Calculate the normal, tangent, and binormal vectors for the terrain model.
            CalculateTerrainVectors();

            // Initialize the vertex and index buffer that hold the geometry for the terrain.
            if (!InitializeBuffers(device))
                return false;

            // Load the textures.
            if (!LoaTextures(device, colorTextureFilename, normalMapFilename))
                return false;

            // Release the height map and the model since the data is now loaded into the vertex and index buffers.
            ShutdownHeightMap();
            ReleaseTerrainModel();

            return true;
        }
        private bool LoadColourMap(string colorMapFilename)
        {
            Bitmap colourBitMap;

            try
            {
                // Open the color map file in binary.
                colourBitMap = new Bitmap(GameConfig.DataFilePath + colorMapFilename);
            }
            catch
            {
                return false;
            }

            // This check is optional.
            // Make sure the color map dimensions are the same as the terrain dimensions for easy 1 to 1 mapping.
            int m_ColourMapWidth = colourBitMap.Width;
            int m_ColourMapHeight = colourBitMap.Height;
            if ((m_ColourMapWidth != m_TerrainWidth) || (m_ColourMapHeight != m_TerrainHeight))
                return false;

            // Read the image data into the color map portion of the height map structure.
            int index;
            for (int j = 0; j < m_ColourMapHeight; j++)
                for (int i = 0; i < m_ColourMapWidth; i++)
                {
                    index = (m_ColourMapHeight * j) + i;
                    DHeightMapType tempCopy = HeightMap[index];
                    tempCopy.r = colourBitMap.GetPixel(i, j).R / 255.0f; // 117.75f; //// 0.678431392
                    tempCopy.g = colourBitMap.GetPixel(i, j).G / 255.0f;  //117.75f; // 0.619607866
                    tempCopy.b = colourBitMap.GetPixel(i, j).B / 255.0f;  // 117.75f; // 0.549019635
                    HeightMap[index] = tempCopy;
                }

            // Release the bitmap image data.
            colourBitMap.Dispose();
            colourBitMap = null;

            return true;
        }
        private bool LoaTextures(SharpDX.Direct3D11.Device device, string colorTextureFilename, string normalMapFilename)
        {
            colorTextureFilename = GameConfig.DataFilePath + colorTextureFilename;
            normalMapFilename = GameConfig.DataFilePath + normalMapFilename;

            // Create the color texture object.
            ColorTexture = new Texture();

            // Initialize the color texture object.
            if (!ColorTexture.Init(device, colorTextureFilename))
                return false;

            // Create the normal map texture object.
            NormalMapTexture = new Texture();

            // Initialize the normal map texture object.
            if (!NormalMapTexture.Init(device, normalMapFilename))
                return false;

            return true;
        }
        private void CalculateTerrainVectors()
        {
            // Calculate the number of faces in the terrain model.
            int faceCount = VertexCount / 3;

            // Initialize the index to the model data.
            int index = 0;
            DTempVertexType vertex1, vertex2, vertex3;
            DVertexTempType tangent = new DVertexTempType();
            DVertexTempType binormal = new DVertexTempType();

            // Go through all the faces and calculate the the tangent, binormal, and normal vectors.
            for (int i = 0; i < faceCount; i++)
            {
                // Get the three vertices for this face from the terrain model.
                vertex1.x = TerrainModel[index].x;
                vertex1.y = TerrainModel[index].y;
                vertex1.z = TerrainModel[index].z;
                vertex1.tu = TerrainModel[index].tu;
                vertex1.tv = TerrainModel[index].tv;
                vertex1.nx = TerrainModel[index].nx;
                vertex1.ny = TerrainModel[index].ny;
                vertex1.nz = TerrainModel[index].nz;
                index++;

                vertex2.x = TerrainModel[index].x;
                vertex2.y = TerrainModel[index].y;
                vertex2.z = TerrainModel[index].z;
                vertex2.tu = TerrainModel[index].tu;
                vertex2.tv = TerrainModel[index].tv;
                vertex2.nx = TerrainModel[index].nx;
                vertex2.ny = TerrainModel[index].ny;
                vertex2.nz = TerrainModel[index].nz;
                index++;

                vertex3.x = TerrainModel[index].x;
                vertex3.y = TerrainModel[index].y;
                vertex3.z = TerrainModel[index].z;
                vertex3.tu = TerrainModel[index].tu;
                vertex3.tv = TerrainModel[index].tv;
                vertex3.nx = TerrainModel[index].nx;
                vertex3.ny = TerrainModel[index].ny;
                vertex3.nz = TerrainModel[index].nz;
                index++;

                /// MAKE SUEW that the tangent nad Binoemals calculated are being sent intot he model after the method below.
                // Calculate the tangent and binormal of that face.
                CalculateTangentBinormal(vertex1, vertex2, vertex3, out tangent, out binormal);

                // Store the tangent and binormal for this face back in the model structure.
                TerrainModel[index - 1].tx = tangent.x;
                TerrainModel[index - 1].ty = tangent.y;
                TerrainModel[index - 1].tz = tangent.z;
                TerrainModel[index - 1].bx = binormal.x;
                TerrainModel[index - 1].by = binormal.y;
                TerrainModel[index - 1].bz = binormal.z;
                TerrainModel[index - 2].tx = tangent.x;
                TerrainModel[index - 2].ty = tangent.y;
                TerrainModel[index - 2].tz = tangent.z;
                TerrainModel[index - 2].bx = binormal.x;
                TerrainModel[index - 2].by = binormal.y;
                TerrainModel[index - 2].bz = binormal.z;
                TerrainModel[index - 3].tx = tangent.x;
                TerrainModel[index - 3].ty = tangent.y;
                TerrainModel[index - 3].tz = tangent.z;
                TerrainModel[index - 3].bx = binormal.x;
                TerrainModel[index - 3].by = binormal.y;
                TerrainModel[index - 3].bz = binormal.z;
            }
        }
        private void CalculateTangentBinormal(DTempVertexType vertex1, DTempVertexType vertex2, DTempVertexType vertex3, out DVertexTempType tangent, out DVertexTempType binormal)
        {
            float[] vector1 = new float[3];
            float[] vector2 = new float[3];
            float[] tuVector = new float[2];
            float[] tvVector = new float[2];

            // Calculate the two vectors for this face.
            vector1[0] = vertex2.x - vertex1.x;
            vector1[1] = vertex2.y - vertex1.y;
            vector1[2] = vertex2.z - vertex1.z;
            vector2[0] = vertex3.x - vertex1.x;
            vector2[1] = vertex3.y - vertex1.y;
            vector2[2] = vertex3.z - vertex1.z;

            // Calculate the tu and tv texture space vectors.
            tuVector[0] = vertex2.tu - vertex1.tu;
            tvVector[0] = vertex2.tv - vertex1.tv;
            tuVector[1] = vertex3.tu - vertex1.tu;
            tvVector[1] = vertex3.tv - vertex1.tv;

            // Calculate the denominator of the tangent/binormal equation.
            float den = 1.0f / (tuVector[0] * tvVector[1] - tuVector[1] * tvVector[0]);

            // Calculate the cross products and multiply by the coefficient to get the tangent and binormal.
            tangent.x = (tvVector[1] * vector1[0] - tvVector[0] * vector2[0]) * den;
            tangent.y = (tvVector[1] * vector1[1] - tvVector[0] * vector2[1]) * den;
            tangent.z = (tvVector[1] * vector1[2] - tvVector[0] * vector2[2]) * den;
            binormal.x = (tuVector[0] * vector2[0] - tuVector[1] * vector1[0]) * den;
            binormal.y = (tuVector[0] * vector2[1] - tuVector[1] * vector1[1]) * den;
            binormal.z = (tuVector[0] * vector2[2] - tuVector[1] * vector1[2]) * den;

            // Calculate the length of this normal.
            float length = (float)Math.Sqrt((tangent.x * tangent.x) + (tangent.y * tangent.y) + (tangent.z * tangent.z));

            // Normalize the normal and then store it
            binormal.x = binormal.x / length;
            binormal.y = binormal.y / length;
            binormal.z = binormal.z / length;
        }
        private void BuildTerrainModel()
        {
            // Set the number of vertices in the model.
            VertexCount = (m_TerrainWidth - 1) * (m_TerrainHeight - 1) * 6;
            // Create the terrain model array.
            TerrainModel = new DModelType[VertexCount];

            // Load the terrain model with the height map terrain data.
            int index = 0;
            for (int j = 0; j < (m_TerrainHeight - 1); j++)
            {
                for (int i = 0; i < (m_TerrainWidth - 1); i++)
                {
                    int index1 = (m_TerrainWidth * j) + i;          // Bottom left.
                    int index2 = (m_TerrainWidth * j) + (i + 1);      // Bottom right.
                    int index3 = (m_TerrainWidth * (j + 1)) + i;      // Upper left.
                    int index4 = (m_TerrainWidth * (j + 1)) + (i + 1);  // Upper right.

                    // Upper left.
                    TerrainModel[index].x = HeightMap[index3].x;
                    TerrainModel[index].y = HeightMap[index3].y;
                    TerrainModel[index].z = HeightMap[index3].z;
                    TerrainModel[index].nx = HeightMap[index3].nx;
                    TerrainModel[index].ny = HeightMap[index3].ny;
                    TerrainModel[index].nz = HeightMap[index3].nz;
                    TerrainModel[index].tu = 0.0f;
                    TerrainModel[index].tv = 0.0f;
                    TerrainModel[index].r = HeightMap[index3].r;
                    TerrainModel[index].g = HeightMap[index3].g;
                    TerrainModel[index].b = HeightMap[index3].b;
                    index++;

                    // Upper right.
                    TerrainModel[index].x = HeightMap[index4].x;
                    TerrainModel[index].y = HeightMap[index4].y;
                    TerrainModel[index].z = HeightMap[index4].z;
                    TerrainModel[index].nx = HeightMap[index4].nx;
                    TerrainModel[index].ny = HeightMap[index4].ny;
                    TerrainModel[index].nz = HeightMap[index4].nz;
                    TerrainModel[index].tu = 1.0f;
                    TerrainModel[index].tv = 0.0f;
                    TerrainModel[index].r = HeightMap[index4].r;
                    TerrainModel[index].g = HeightMap[index4].g;
                    TerrainModel[index].b = HeightMap[index4].b;
                    index++;

                    // Bottom left.
                    TerrainModel[index].x = HeightMap[index1].x;
                    TerrainModel[index].y = HeightMap[index1].y;
                    TerrainModel[index].z = HeightMap[index1].z;
                    TerrainModel[index].nx = HeightMap[index1].nx;
                    TerrainModel[index].ny = HeightMap[index1].ny;
                    TerrainModel[index].nz = HeightMap[index1].nz;
                    TerrainModel[index].tu = 0.0f;
                    TerrainModel[index].tv = 1.0f;
                    TerrainModel[index].r = HeightMap[index1].r;
                    TerrainModel[index].g = HeightMap[index1].g;
                    TerrainModel[index].b = HeightMap[index1].b;
                    index++;

                    // Bottom left.
                    TerrainModel[index].x = HeightMap[index1].x;
                    TerrainModel[index].y = HeightMap[index1].y;
                    TerrainModel[index].z = HeightMap[index1].z;
                    TerrainModel[index].nx = HeightMap[index1].nx;
                    TerrainModel[index].ny = HeightMap[index1].ny;
                    TerrainModel[index].nz = HeightMap[index1].nz;
                    TerrainModel[index].tu = 0.0f;
                    TerrainModel[index].tv = 1.0f;
                    TerrainModel[index].r = HeightMap[index1].r;
                    TerrainModel[index].g = HeightMap[index1].g;
                    TerrainModel[index].b = HeightMap[index1].b;
                    index++;

                    // Upper right.
                    TerrainModel[index].x = HeightMap[index4].x;
                    TerrainModel[index].y = HeightMap[index4].y;
                    TerrainModel[index].z = HeightMap[index4].z;
                    TerrainModel[index].nx = HeightMap[index4].nx;
                    TerrainModel[index].ny = HeightMap[index4].ny;
                    TerrainModel[index].nz = HeightMap[index4].nz;
                    TerrainModel[index].tu = 1.0f;
                    TerrainModel[index].tv = 0.0f;
                    TerrainModel[index].r = HeightMap[index4].r;
                    TerrainModel[index].g = HeightMap[index4].g;
                    TerrainModel[index].b = HeightMap[index4].b;
                    index++;

                    // Bottom right.
                    TerrainModel[index].x = HeightMap[index2].x;
                    TerrainModel[index].y = HeightMap[index2].y;
                    TerrainModel[index].z = HeightMap[index2].z;
                    TerrainModel[index].nx = HeightMap[index2].nx;
                    TerrainModel[index].ny = HeightMap[index2].ny;
                    TerrainModel[index].nz = HeightMap[index2].nz;
                    TerrainModel[index].tu = 1.0f;
                    TerrainModel[index].tv = 1.0f;
                    TerrainModel[index].r = HeightMap[index2].r;
                    TerrainModel[index].g = HeightMap[index2].g;
                    TerrainModel[index].b = HeightMap[index2].b;
                    index++;
                }
            }
        }
        private bool CalculateNormals()
        {
            // Create a temporary array to hold the un-normalized normal vectors.
            int index;
            float length;
            Vector3 vertex1, vertex2, vertex3, vector1, vector2, sum;
            DVertexTempType[] normals = new DVertexTempType[(m_TerrainHeight - 1) * (m_TerrainWidth - 1)];

            // Go through all the faces in the mesh and calculate their normals.
            for (int j = 0; j < (m_TerrainHeight - 1); j++)
            {
                for (int i = 0; i < (m_TerrainWidth - 1); i++)
                {
                    int index1 = (j * m_TerrainHeight) + i;
                    int index2 = (j * m_TerrainHeight) + (i + 1);
                    int index3 = ((j + 1) * m_TerrainHeight) + i;

                    // Get three vertices from the face.
                    vertex1.X = HeightMap[index1].x;
                    vertex1.Y = HeightMap[index1].y;
                    vertex1.Z = HeightMap[index1].z;
                    vertex2.X = HeightMap[index2].x;
                    vertex2.Y = HeightMap[index2].y;
                    vertex2.Z = HeightMap[index2].z;
                    vertex3.X = HeightMap[index3].x;
                    vertex3.Y = HeightMap[index3].y;
                    vertex3.Z = HeightMap[index3].z;

                    // Calculate the two vectors for this face.
                    vector1 = vertex1 - vertex3;
                    vector2 = vertex3 - vertex2;
                    index = (j * (m_TerrainHeight - 1)) + i;

                    // Calculate the cross product of those two vectors to get the un-normalized value for this face normal.
                    Vector3 vecTestCrossProduct = Vector3.Cross(vector1, vector2);
                    normals[index].x = vecTestCrossProduct.X;
                    normals[index].y = vecTestCrossProduct.Y;
                    normals[index].z = vecTestCrossProduct.Z;
                }
            }

            // Now go through all the vertices and take an average of each face normal 	
            // that the vertex touches to get the averaged normal for that vertex.
            for (int j = 0; j < m_TerrainHeight; j++)
            {
                for (int i = 0; i < m_TerrainWidth; i++)
                {
                    // Initialize the sum.
                    sum = Vector3.Zero;

                    // Initialize the count.
                    int count = 9;

                    // Bottom left face.
                    if (((i - 1) >= 0) && ((j - 1) >= 0))
                    {
                        index = ((j - 1) * (m_TerrainHeight - 1)) + (i - 1);

                        sum[0] += normals[index].x;
                        sum[1] += normals[index].y;
                        sum[2] += normals[index].z;
                        count++;
                    }
                    // Bottom right face.
                    if ((i < (m_TerrainWidth - 1)) && ((j - 1) >= 0))
                    {
                        index = ((j - 1) * (m_TerrainHeight - 1)) + i;

                        sum[0] += normals[index].x;
                        sum[1] += normals[index].y;
                        sum[2] += normals[index].z;
                        count++;
                    }
                    // Upper left face.
                    if (((i - 1) >= 0) && (j < (m_TerrainHeight - 1)))
                    {
                        index = (j * (m_TerrainHeight - 1)) + (i - 1);

                        sum[0] += normals[index].x;
                        sum[1] += normals[index].y;
                        sum[2] += normals[index].z;
                        count++;
                    }
                    // Upper right face.
                    if ((i < (m_TerrainWidth - 1)) && (j < (m_TerrainHeight - 1)))
                    {
                        index = (j * (m_TerrainHeight - 1)) + i;

                        sum.X += normals[index].x;
                        sum.Y += normals[index].y;
                        sum.Z += normals[index].z;
                        count++;
                    }

                    // Take the average of the faces touching this vertex.
                    sum.X = (sum.X / (float)count);
                    sum.Y = (sum.Y / (float)count);
                    sum.Z = (sum.Z / (float)count);

                    // Calculate the length of this normal.
                    length = (float)Math.Sqrt((sum.X * sum.X) + (sum.Y * sum.Y) + (sum.Z * sum.Z));

                    // Get an index to the vertex location in the height map array.
                    index = (j * m_TerrainHeight) + i;

                    // Normalize the final shared normal for this vertex and store it in the height map array.
                    DHeightMapType editHeightMap = HeightMap[index];
                    editHeightMap.nx = (sum.X / length);
                    editHeightMap.ny = (sum.Y / length);
                    editHeightMap.nz = (sum.Z / length);
                    HeightMap[index] = editHeightMap;
                }
            }

            // Release the temporary normals.
            normals = null;

            return true;
        }
        private void ReduceHeightMap(float flattenAmount)
        {
            // This actually is not the same execution as teh C++ implementation.
            for (var i = 0; i < HeightMap.Count; i++)
            {
                var temp = HeightMap[i];
                temp.y /= flattenAmount;
                HeightMap[i] = temp;
            }
        }
        private bool LoadHeightMap(string heightMapFileName)
        {
            Bitmap bitmap;

            try
            {
                // Open the height map file in binary.
                bitmap = new Bitmap(GameConfig.DataFilePath + heightMapFileName);
            }
            catch
            {
                return false;
            }

            // Save the dimensions of the terrain.
            m_TerrainWidth = bitmap.Width;
            m_TerrainHeight = bitmap.Height;

            // Create the structure to hold the height map data.
            HeightMap = new List<DHeightMapType>(m_TerrainWidth * m_TerrainHeight);

            // Read the image data into the height map
            for (var j = 0; j < m_TerrainHeight; j++)
                for (var i = 0; i < m_TerrainWidth; i++)
                    HeightMap.Add(new DHeightMapType()
                    {
                        x = i,
                        y = bitmap.GetPixel(i, j).R,
                        z = j
                    });

            bitmap.Dispose();
            bitmap = null;

            return true;
        }
        private bool InitializeBuffers(SharpDX.Direct3D11.Device device)
        {
            try
            {
                // Set the index count to the same as the vertex count.
                IndexCount = VertexCount;

                // Create the vertex array.
                DVertexType[] vertices = new DVertexType[VertexCount];
                // Create the index array.
                int[] indices = new int[IndexCount];

                // Load the vertex and index array with data from the terrain model.
                for (int i = 0; i < VertexCount; i++)
                {
                    vertices[i].position = new Vector3(TerrainModel[i].x, TerrainModel[i].y, TerrainModel[i].z);
                    vertices[i].texture = new Vector2(TerrainModel[i].tu, TerrainModel[i].tv);
                    vertices[i].normal = new Vector3(TerrainModel[i].nx, TerrainModel[i].ny, TerrainModel[i].nz);
                    vertices[i].tangent = new Vector3(TerrainModel[i].tx, TerrainModel[i].ty, TerrainModel[i].tz);
                    vertices[i].binormal = new Vector3(TerrainModel[i].bx, TerrainModel[i].by, TerrainModel[i].bz);
                    vertices[i].color = new Vector4(TerrainModel[i].r, TerrainModel[i].g, TerrainModel[i].b, 1.0f);
                    indices[i] = i;
                }

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
        public void ShutDown()
        {
            // Release the textures.
            ReleaseTextures();

            // Release the vertex and index buffers.
            ShutdownBuffers();

            // Release the terrain model.
            ReleaseTerrainModel();

            // Release the height map data.
            ShutdownHeightMap();
        }
        private void ReleaseTerrainModel()
        {
            if (TerrainModel != null)
                TerrainModel = null;
        }
        private void ReleaseTextures()
        {
            // Release the normal map texture object.
            NormalMapTexture?.Destroy();
            NormalMapTexture = null;
            // Release the color texture object.
            ColorTexture?.Destroy();
            ColorTexture = null;
        }
        private void ShutdownHeightMap()
        {
            // Release the HeightMap Data loaded from the file.
            HeightMap?.Clear();
            HeightMap = null;
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
        public void Render(DeviceContext deviceContext)
        {
            // Put the vertex and index buffers on the graphics pipeline to prepare them for drawing.
            RenderBuffers(deviceContext);
        }
        private void RenderBuffers(DeviceContext deviceContext)
        {
            // Set the vertex buffer to active in the input assembler so it can be rendered.
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<DVertexType>(), 0));
            // Set the index buffer to active in the input assembler so it can be rendered.
            deviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            // Set the type of the primitive that should be rendered from this vertex buffer, in this case triangles.
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }

    }
}

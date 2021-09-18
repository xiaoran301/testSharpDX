using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using WindowsFormsApp1.Foundation;
namespace WindowsFormsApp1.Water
{
    /*
     * 一个游戏场景必有的，input，camera，light，d3drender等
     */
    class Game: IGame
    {
        public InputSystem Input { get; private set; }
        private DDX11 D3D { get; set; }
        public Camera Camera { get; set; }
        public Transform Position { get; set; }
        public Light Light { get; set; }
        public RenderTexture RefractionTexture { get; set; }
        public RenderTexture ReflectionTexture { get; set; }
        public ReflectionShader ReflectionShader { get; set; }

        public Water WaterModel { get; set; }
        public WaterShader WaterShader { get; set; }

        public Terrain TerrainModel { get; set; }
        public TerrainShader TerrainShader { get; set; }



        public void Init(IntPtr windowHandle)
        {
            var configuration = GameConfig.Instance;

            Input = InputSystem.Instance;

            Input.Init(windowHandle);

            D3D = new DDX11();

            if (!D3D.Init( windowHandle))
                return ;

            Position = new Transform();
            Position.SetPosition(280.379f, 24.5225f, 367.018f);
            Position.SetRotation(19.6834f, 222.013f, 0.0f);

            Camera = new Camera();
            Camera.SetPosition(0.0f, 0.0f, -10.0f);
            Camera.RenderBaseViewMatrix();
            Matrix baseViewMatrix = Camera.BaseViewMatrix;

            Light = new Light();
            Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
            Light.Direction = new Vector3(0.5f, -0.75f, 0.25f);

            // 水准备
            RefractionTexture = new RenderTexture();
            if (!RefractionTexture.Init(D3D.Device ))
                Debug.Assert(false);

            ReflectionTexture = new RenderTexture();
            if (!ReflectionTexture.Init(D3D.Device ))
                Debug.Assert(false);

            // Create the reflection shader object.
            ReflectionShader = new ReflectionShader();
            if (!ReflectionShader.Init(D3D.Device, windowHandle))
                Debug.Assert(false);
            // 水
            WaterModel = new Water();
            if (!WaterModel.Init(D3D.Device, "waternormal.bmp", 3.75f, 110.0f))
                Debug.Assert(false);
            WaterShader = new WaterShader();
            if (!WaterShader.Init(D3D.Device, windowHandle))
                Debug.Assert(false);

            TerrainModel = new Terrain();
            if (!TerrainModel.Initialize(D3D.Device, "hm.bmp", "cm.bmp", 20.0f, "dirt04.bmp", "normal01.bmp"))
                Debug.Assert(false);
            TerrainShader = new TerrainShader();
            if (!TerrainShader.Initialize(D3D.Device, windowHandle))
                 Debug.Assert(false);
        }

        public bool UpdateInput()
        {
            return false;
        }
        public void Update(float frameTime)
        {

            if (!HandleInput(frameTime))
                return ;

            // 逻辑

            WaterModel.Update();

            // 水用到的折射和反射纹理要提前准备好 
            if (!RenderRefractionToTexture())
                return ;

            if (!RenderReflectionToTexture())
                return ;

            // 渲染
            Render();
            
        }

        private bool RenderRefractionToTexture()
        {
            // Setup a clipping plane based on the height of the water to clip everything above it to create a refraction.
            Vector4 clipPlane = new Vector4(0.0f, -1.0f, 0.0f, WaterModel.WaterHeight + 0.1f);

            // Set the render target to be the refraction render to texture.
            RefractionTexture.SetRenderTarget(D3D.DeviceContext);

            // Clear the refraction render to texture.
            RefractionTexture.ClearRenderTarget(D3D.DeviceContext, 0.0f, 0.0f, 0.0f, 1.0f);

            // Generate the view matrix based on the camera's position.
            Camera.Render();

            // Get the matrices from the camera and d3d objects.
            Matrix worldMatrix = D3D.WorldMatrix;
            Matrix viewMatrix = Camera.ViewMatrix;
            Matrix projectionMatrix = D3D.ProjectionMatrix;

            //// Render the terrain using the reflection shader and the refraction clip plane to produce the refraction effect.
            TerrainModel.Render(D3D.DeviceContext);
            if (!ReflectionShader.Render(D3D.DeviceContext, TerrainModel.IndexCount, worldMatrix, viewMatrix, projectionMatrix, TerrainModel.ColorTexture.TextureResource, TerrainModel.NormalMapTexture.TextureResource, Light.DiffuseColour, Light.Direction, 2.0f, clipPlane))
                return false;

            // Reset the render target back to the original back buffer and not the render to texture anymore.
            D3D.SetBackBufferRenderTarget();

            // Reset the viewport back to the original.
            D3D.ResetViewPort();

            return true;
        }
        private bool RenderReflectionToTexture()
        {
            // Setup a clipping plane based on the height of the water to clip everything below it.
            Vector4 clipPlane = new Vector4(0.0f, 1.0f, 0.0f, -WaterModel.WaterHeight);

            // Set the render target to be the reflection render to texture.
            ReflectionTexture.SetRenderTarget(D3D.DeviceContext);

            // Clear the reflection render to texture.
            ReflectionTexture.ClearRenderTarget(D3D.DeviceContext, 0.0f, 0.0f, 0.0f, 1.0f);

            // Use the camera to render the reflection and create a reflection view matrix.
            Camera.RenderReflection(WaterModel.WaterHeight);

            // Get the camera reflection view matrix instead of the normal view matrix.
            Matrix reflectionViewMatrix = Camera.ReflectionViewMatrix;

            // Get the world and projection matrices from the d3d object.
            Matrix worldMatrix = D3D.WorldMatrix;
            Matrix projectionMatrix = D3D.ProjectionMatrix;

            // Get the position of the camera.
            Vector3 cameraPosition = Camera.GetPosition();

            // Invert the Y coordinate of the camera around the water plane height for the reflected camera position.
            cameraPosition.Y = -cameraPosition.Y + (WaterModel.WaterHeight * 2.0f);

            // Translate the sky dome and sky plane to be centered around the reflected camera position.
            Matrix.Translation(cameraPosition.X, cameraPosition.Y, cameraPosition.Z, out worldMatrix);

            // Turn off back face culling and the Z buffer.
            D3D.TurnOffCulling();
            D3D.TurnZBufferOff();

            //// Render the sky dome using the reflection view matrix.
            //SkyDome.Render(D3D.DeviceContext);
            //if (!SkyDomeShader.Render(D3D.DeviceContext, SkyDome.IndexCount, worldMatrix, reflectionViewMatrix, projectionMatrix, SkyDome.ApexColour, SkyDome.CenterColour))
            //    return false;

            // Enable back face culling.
            D3D.TurnOnCulling();

            // Enable additive blending so the clouds blend with the sky dome color.
            D3D.EnableSecondBlendState();

            //// Render the sky plane using the sky plane shader.
            //SkyPlane.Render(D3D.DeviceContext);
            //if (!SkyPlaneShader.Render(D3D.DeviceContext, SkyPlane.IndexCount, worldMatrix, reflectionViewMatrix, projectionMatrix, SkyPlane.CloudTexture.TextureResource, SkyPlane.PerturbTexture.TextureResource, SkyPlane.m_Translation, SkyPlane.m_Scale, SkyPlane.m_Brightness))
            //    return false;

            // Turn off blending and enable the Z buffer again.
            D3D.TurnOffAlphaBlending();
            D3D.TurnZBufferOn();

            // Reset the world matrix.
            worldMatrix = D3D.WorldMatrix;

            // Render the terrain using the reflection view matrix and reflection clip plane.
            //TerrainModel.Render(D3D.DeviceContext);
            //if (!ReflectionShader.Render(D3D.DeviceContext, TerrainModel.IndexCount, worldMatrix, reflectionViewMatrix, projectionMatrix, TerrainModel.ColorTexture.TextureResource, TerrainModel.NormalMapTexture.TextureResource, Light.DiffuseColour, Light.Direction, 2.0f, clipPlane))
            //    return false;

            // Reset the render target back to the original back buffer and not the render to texture anymore.
            D3D.SetBackBufferRenderTarget();

            // Reset the viewport back to the original.
            D3D.ResetViewPort();

            return true;
        }

        public bool Render()
        {

            // Clear the scene.
            D3D.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

            // Generate the view matrix based on the camera's position.
            Camera.Render();

            // Get the world, view, projection, ortho, base view and reflection matrices from the camera and Direct3D objects.
            Matrix worldMatrix = D3D.WorldMatrix;
            Matrix viewCameraMatrix = Camera.ViewMatrix;
            Matrix projectionMatrix = D3D.ProjectionMatrix;
            Matrix orthoMatrix = D3D.OrthoMatrix;
            Matrix baseViewMatrix = Camera.BaseViewMatrix;
            Matrix reflectionMatrix = Camera.ReflectionViewMatrix;

            // Get the position of the camera.
            Vector3 cameraPosition = Camera.GetPosition();

            // Translate the sky dome to be centered around the camera position.
            Matrix.Translation(cameraPosition.X, cameraPosition.Y, cameraPosition.Z, out worldMatrix);

            // Turn off back face culling and the Z buffer.
            D3D.TurnOffCulling();
            D3D.TurnZBufferOff();

    
            // Turn back face culling back on.
            D3D.TurnOnCulling();

            // Enable additive blending so the clouds blend with the sky dome color.
            D3D.EnableSecondBlendState();


            // Turn off blending.
            D3D.TurnOffAlphaBlending();

            // Turn the Z buffer back on.
            D3D.TurnZBufferOn();

            // Reset the world matrix.
            worldMatrix = D3D.WorldMatrix;

            // Render the terrain using the terrain shader.
            TerrainModel.Render(D3D.DeviceContext);
            if (!TerrainShader.Render(D3D.DeviceContext, TerrainModel.IndexCount, worldMatrix, viewCameraMatrix, projectionMatrix, TerrainModel.ColorTexture.TextureResource, TerrainModel.NormalMapTexture.TextureResource, Light.DiffuseColour, Light.Direction, 2.0f))
                return false;

            // Translate to the location of the water and render it.
            Matrix.Translation(240.0f, WaterModel.WaterHeight, 250.0f, out worldMatrix);
            WaterModel.Render(D3D.DeviceContext);
            if (!WaterShader.Render(D3D.DeviceContext, WaterModel.IndexCount, worldMatrix, viewCameraMatrix, projectionMatrix, reflectionMatrix, ReflectionTexture.ShaderResourceView, RefractionTexture.ShaderResourceView, WaterModel.Texture.TextureResource, Camera.GetPosition(), WaterModel.NormalMapTiling, WaterModel.WaterTranslation, WaterModel.ReflectRefractScale, WaterModel.RefractionTint, Light.Direction, WaterModel.SpecularShininess))
                return false;

            // Reset the world matrix.
            worldMatrix = D3D.WorldMatrix;

            // Turn off the Z buffer to begin all 2D rendering.
            D3D.TurnZBufferOff();

            // Turn on the alpha blending before rendering the text.
            D3D.TurnOnAlphaBlending();


            // Turn off alpha blending after rendering the text.
            D3D.TurnOffAlphaBlending();

            // Turn the Z buffer back on now that all 2D rendering has completed.
            D3D.TurnZBufferOn();

            // Present the rendered scene to the screen.
            D3D.EndScene();
            return true;

        }
        public void Destroy()
        {

            Position = null;
            Light = null;
            Camera = null;

            D3D?.Destroy();
            D3D = null;

            WaterShader?.Destroy();
            WaterShader = null;

            WaterModel?.Destroy();
            WaterModel = null;

            TerrainModel?.ShutDown();
            TerrainModel = null;

            TerrainShader?.ShutDown();
            TerrainShader = null;
        }
        private bool HandleInput(float frameTime)
        {
            Position.SetFrameTime(frameTime);

            // Handle the input
            bool keydown = Input.IsLeftArrowPressed();
            Position.TurnLeft(keydown);
            keydown = Input.IsRightArrowPressed();
            Position.TurnRight(keydown);
            keydown = Input.IsUpArrowPressed();
            Position.MoveForward(keydown);
            keydown = Input.IsDownArrowPressed();
            Position.MoveBackward(keydown);
            keydown = Input.IsPageUpPressed();
            Position.LookUpward(keydown);
            keydown = Input.IsPageDownPressed();
            Position.LookDownward(keydown);
            keydown = Input.IsAPressed();
            Position.MoveUpward(keydown);
            keydown = Input.IsZPressed();
            Position.MoveDownward(keydown);

            Camera.SetPosition(Position.PositionX, Position.PositionY, Position.PositionZ);
            Camera.SetRotation(Position.RotationX, Position.RotationY, Position.RotationZ);

            return true;
        }
    }
}

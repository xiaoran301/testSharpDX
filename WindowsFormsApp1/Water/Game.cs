using System;
using System.Collections.Generic;
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


            // 渲染
            Render();
            
        }
        public void Render()
        {

        }
        public void Destroy()
        {

            Position = null;
            Light = null;
            Camera = null;

            D3D?.Destroy();
            D3D = null;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1.Foundation
{
    class GameConfig
    {
        private static GameConfig _instance = null;
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        // Static Variables.
        public static bool FullScreen { get; private set; }
        public static bool VerticalSyncEnabled { get; private set; }
        public static float ScreenDepth { get; private set; }
        public static float ScreenNear { get; private set; }
        public static FormBorderStyle BorderStyle { get; private set; }
        public static string VertexShaderProfile = "vs_4_0";
        public static string PixelShaderProfile = "ps_4_0";
        public static string ShaderFilePath { get; private set; }
        public static string DataFilePath { get; private set; }
        public static string ModelFilePath { get; set; }
        public static string FontFilePath { get; private set; }

        public static GameConfig Instance
        {
            get
            {
                if(_instance == null) {
                    _instance = new GameConfig();
                }
                return _instance;
            }
        } 

        public void Init(string title, int width, int height, bool fullScreen, bool vSync)
        {

            FullScreen = fullScreen;
            Title = title;
            VerticalSyncEnabled = vSync;

            if (!FullScreen)
            {
                Width = width;
                Height = height;
            }
            else
            {
                Width = Screen.PrimaryScreen.Bounds.Width;
                Height = Screen.PrimaryScreen.Bounds.Height;
            }
        }

        static GameConfig()
        {
            FullScreen = false;
            VerticalSyncEnabled = false;
            ScreenDepth = 1000.0f;   // 1000.0f
            ScreenNear = 0.1f;      // 0.1f
            BorderStyle = FormBorderStyle.None;

            ShaderFilePath = @"Externals\Shaders\";
            FontFilePath = @"Externals\Font\";
            DataFilePath = @"Externals\Data\";
            ModelFilePath = @"Externals\Models\";
        }
    }
}

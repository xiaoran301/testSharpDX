using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApp1.Foundation;

namespace WindowsFormsApp1.Water
{
   public class GameSystem: AsbGameSystem
    {
        private static GameSystem _instance = null;

        public static GameSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameSystem();
                }
                return _instance;
            }
        }
        public static void Start()
        {
            GameSystem.Instance.Init("test water demo",1280 , 800,true);
            GameSystem.Instance.Run();
                     
        }
        protected override void InitGame(IntPtr windowHandle)
        {
            game = new Game();
            game.Init(windowHandle);
        }
    }
}

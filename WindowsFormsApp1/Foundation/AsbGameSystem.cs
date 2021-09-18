using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Windows;
using WindowsFormsApp1.Foundation;

namespace WindowsFormsApp1.Foundation
{
   public abstract class AsbGameSystem
    {

        public TimeManager Timer { get; private set; }
        private RenderForm renderForm;
        protected IGame game;

        private CheckBox cbReflection;

        abstract protected void InitGame(IntPtr windowHandle);

        public void Init(string title, int width, int height, bool vSync)
        {
            GameConfig.Instance.Init(title,width,height,false,vSync);
            Timer = new TimeManager();
            Timer.Init();
            InitRenderForm(title);

            // 在renderForm后创建game
            InitGame(renderForm.Handle);
        }
  
        
        virtual public bool Update()
        {
            Timer.Update();
            game.Update(Timer.FrameTime);
            return true;
        }
        private void DestroyWindows()
        {
            renderForm?.Dispose();
            renderForm = null;
        }
        public void Destroy()
        {
            DestroyWindows();

            // Release the Timer object
            Timer = null;

            game?.Destroy();
                  }
        protected void Run()
        {
            RenderLoop.Run(renderForm, () =>
            {
                if (!Update())
                    Destroy();
            });

        }
        private void InitRenderForm(string title)
        {
            renderForm = new RenderForm(title)
            {

                ClientSize = new Size(GameConfig.Instance.Width, GameConfig.Instance.Height),
                FormBorderStyle = FormBorderStyle.FixedSingle
            };

            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;

            // 窗体ui
            InitUI();

            renderForm.Show();
            renderForm.Location = new Point((width / 2) - (GameConfig.Instance.Width / 2), (height / 2) - (GameConfig.Instance.Height / 2));
        }
        private void InitUI()
        {
            CheckBox checkBox1 = new CheckBox();
            checkBox1.Appearance = Appearance.Normal;
            checkBox1.AutoCheck = true;
            checkBox1.Text = "反射";
            checkBox1.Click += new EventHandler(CheckBoxReflection_Click);

            renderForm.Controls.Add(checkBox1);
            cbReflection = checkBox1;
        }

        void CheckBoxReflection_Click(object sender, EventArgs e)
        {
            GameConfig.TriggerReflection = cbReflection.Checked;
        }
    }
}

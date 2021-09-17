using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Foundation
{
   public interface IGame
    {
       
        void Init(IntPtr windowHandle);
        bool UpdateInput();
        void Update(float frameTime);
        void Destroy();
    }
}

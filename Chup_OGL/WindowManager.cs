using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Input;

namespace Chup_OGL
{
    public class WindowManager
    {
        private OGLWindow window;
        private Thread renderThread;
        private Thread mainThread;
        private bool running;

        public WindowManager()
        {
            mainThread = new Thread(() =>
            {
                window = new OGLWindow();
                running = true;

                while (running)
                {
                    window.Update();
                    Thread.Sleep(1);
                }
            });
            mainThread.Start();

            while (!running)
                Thread.Sleep(1);
        }

        public void DrawFrame(ulong[] frame)
        {
            uint[] data = new uint[64 * 32];
            for (int y = 0; y < 32; y++)
            {
                ulong line = frame[y];
                for (int x = 0; x < 64; x++)
                {
                    data[y * 64 + 63 - x] = ((line & 1) == 0) ? 0x000000FF : 0xFFFFFFFF;
                    line >>= 1;
                }
            }

            window.data = data;

            window.Context.MakeCurrent(window.WindowInfo);
            window.Draw();
        }

        private Key[] keys = new Key[16]
        {
            Key.X,       // 0
            Key.Number1, // 1
            Key.Number2, // 2
            Key.Number3, // 3
            Key.Q,       // 4
            Key.W,       // 5
            Key.E,       // 6
            Key.A,       // 7
            Key.S,       // 8
            Key.D,       // 9
            Key.Z,       // A
            Key.C,       // B
            Key.Number4, // C
            Key.R,       // D
            Key.F,       // E
            Key.V,       // F
        };

        public bool[] GetKeys()
        {
            var state = Keyboard.GetState();
            bool[] states = new bool[16];

            for (int i = 0; i < 16; i++)
                states[i] = state.IsKeyDown(keys[i]);

            return states;
        }
    }
}

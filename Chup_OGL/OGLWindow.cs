using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Chup_OGL
{
    public class OGLWindow : GameWindow
    {
        private bool ViewportUpdated;

        public uint[] data;
        private int viewTexture;

        public OGLWindow()
            : base(1280, 720)
        {
            Title = "Chup-8";

            VSync = VSyncMode.On;
            Visible = true;
            ViewportUpdated = true;

            viewTexture = GL.GenTexture();

            // Release context for render thread
            Context.MakeCurrent(null);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            ViewportUpdated = true;
        }

        public void Update()
        {
            ProcessEvents();
        }

        public void Draw()
        {
            // Update viewport
            if (ViewportUpdated)
            {
                GL.Viewport(0, 0, Width, Height);

                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0, Width, 0, Height, 0.0, 4.0);

                ViewportUpdated = false;
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.Black);

            // Convert raw data into texture
            GL.BindTexture(TextureTarget.Texture2D, viewTexture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 64, 32, 0, PixelFormat.Rgba, PixelType.UnsignedInt8888, data);

            // Enable blending and textures
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Draw texture
            GL.Color3(Color.White);
            GL.Begin(PrimitiveType.Triangles);

            GL.TexCoord2(0, 1); GL.Vertex2(0, 0);
            GL.TexCoord2(0, 0); GL.Vertex2(0, Height);
            GL.TexCoord2(1, 0); GL.Vertex2(Width, Height);

            GL.TexCoord2(1, 0); GL.Vertex2(Width, Height);
            GL.TexCoord2(1, 1); GL.Vertex2(Width, 0);
            GL.TexCoord2(0, 1); GL.Vertex2(0, 0);

            GL.End();

            // Cleanup for caller
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);

            SwapBuffers();
        }
    }
}

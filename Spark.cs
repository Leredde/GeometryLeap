using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace GeometryLeap
{
    class Spark
    {
        private const double ALPHA_STEP = 10.0;

        private System.Drawing.Color color;

        private double transX = 0.0;
        private double transY = 0.0;
        private double speed = 0.0;
        private double rotate = 0.0;
        private double alpha = 255.0;
        private Stopwatch w;

        public Spark(System.Drawing.Color color, double posX, double posY, double speed, double angle)
        {
            this.color = color;
            this.transX = posX;
            this.transY = posY;
            this.rotate = angle;
            this.speed = speed * 0.5;
            w = new Stopwatch();
        }

        public void Draw()
        {
            if (w.IsRunning)
            {
                transX += Math.Cos((rotate + 90) * Math.PI / 180.0) * speed * w.ElapsedMilliseconds;
                transY += Math.Sin((rotate + 90) * Math.PI / 180.0) * speed * w.ElapsedMilliseconds;
                w.Restart();
            }
            else
            {
                w.Start();
            }


            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();


            GL.Scale(0.02, 0.02, 1);
            GL.Translate(transX, transY, 0.0);
            GL.Rotate(rotate, 0, 0, 1);

            GL.PushMatrix();
            GL.Scale(0.5, 0.5, 1);

            GL.LineWidth(2);

            GL.Begin(PrimitiveType.Lines);
            GL.Color4(color.R, color.G, color.B, (Byte)alpha);

            GL.Vertex2(0.0f, 1.0f);
            GL.Vertex2(0.0f, -1.0f);

            GL.End();

            GL.PopMatrix();
            GL.PopMatrix();

            if (alpha > ALPHA_STEP)
            {
                alpha -= ALPHA_STEP;
            }
            else
            {
                alpha = 0.0;
            }
        }
    }
}

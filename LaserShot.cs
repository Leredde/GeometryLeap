using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace GeometryLeap
{
    class LaserShot
    {
        private const int SPARK_COUNT = 5;
        private const double SPARK_FAN = 170.0; // degrees
        private const double IMPACT_DURATION = 1000.0; // millisec

        private Random rand;

        private System.Drawing.Color color;

        private double transX = 0.0;
        private double transY = 0.0;
        private double speed = 0.0;
        private double rotate = 0.0;
        private Stopwatch w;
        private Stopwatch impactWatch;
        private List<Spark> sparks;
        private bool exploding;

        public bool IsExploding()
        {
            return exploding;
        }

        public LaserShot(System.Drawing.Color color, double posX, double posY, double speed, double angle)
        {
            this.color = color;
            this.transX = posX;
            this.transY = posY;
            this.rotate = angle;
            this.speed = 0.1 + speed;
            w = new Stopwatch();
            impactWatch = new Stopwatch();
            sparks = new List<Spark>();

            rand = new Random(DateTime.Now.Millisecond);
        }

        public void Draw()
        {
            if (w.IsRunning)
            {                
                transX += Math.Cos((rotate+90) * Math.PI / 180.0) * speed * w.ElapsedMilliseconds;
                transY += Math.Sin((rotate+90) * Math.PI / 180.0) * speed * w.ElapsedMilliseconds;
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
            GL.Scale(0.7, 0.7, 1);

            GL.LineWidth(2);

            GL.Begin(PrimitiveType.LineStrip);
            GL.Color3(color);

            GL.Vertex2(-0.5f, -1.0f);
            GL.Vertex2(0.0f, 1.0f);
            GL.Vertex2(0.5f, -1.0f);

            GL.End();

            GL.PopMatrix();
            GL.PopMatrix();
        }

        // bool return deprecated
        public bool DrawImpact()
        {
            bool finished = false;

            exploding = true;

            if (sparks.Count == 0)
            {
                double angle = 0.0;
                for (int i = 0; i < SPARK_COUNT; i++)
                {
                    angle = 180.0 + rotate - SPARK_FAN / 2 + i * SPARK_FAN / SPARK_COUNT + (rand.NextDouble() - 0.5) * SPARK_FAN / 3.0;
                    Spark spark = new Spark(color, transX, transY, speed, angle);
                    sparks.Add(spark);
                    //spark.Draw();
                }
                impactWatch.Start();
            }
            else
            {
                if (impactWatch.ElapsedMilliseconds > IMPACT_DURATION)
                {
                    finished = true;
                    exploding = false;
                    impactWatch.Stop();
                    sparks.Clear();
                }
                else
                {
                    foreach (Spark s in sparks)
                    {
                        s.Draw();
                    }
                }
            }

            return finished;
        }   

        public double PosX()
        {
            return transX;
        }

        public double PosY()
        {
            return transY;
        }
    }
}

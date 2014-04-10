using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Leap;
using System.Diagnostics;
using System.Media;
using System.Windows.Media;

namespace GeometryLeap
{
    class SpaceShip
    {
        private const double SENSITIVITY = 0.3;
        private int handId = 0;
        private double rotate = 0.0;
        private double transX = 0.0;
        private double transY = 0.0;
        private double deltaX = 0;
        private double deltaY = 0;
        private double prevX = 0;
        private double prevY = 0;
        private double fireRate = 0.0;
        private System.Drawing.Color color;
        private System.Drawing.Color laserColor;
        private List<LaserShot> shots;
        private Stopwatch speedWatch;
        private Stopwatch fireRateWatch;
        private SpaceShip oponent;
        private SoundPlayer soundFire;
        private SoundPlayer soundImpact;
        private MediaPlayer soundFire2;
        

        private int _shotsReceived = 0;

        public int ShotsReceived
        {
            get { return _shotsReceived; }
            set { _shotsReceived = value; }
        }

        private bool _visible = false;

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        private const double BORDER = 0.95 * 2 * 25;// get it from actual graphical borders

        public SpaceShip(System.Drawing.Color color, System.Drawing.Color laserColor)
        {
            this.color = color;
            this.laserColor = laserColor;
            this.shots = new List<LaserShot>();
            this.speedWatch = new Stopwatch();
            this.fireRateWatch = new Stopwatch();
            this.fireRate = 50.0; // shots per sec
            soundFire2 = new MediaPlayer();
            soundFire2.Open(new Uri("pack://application:,,,/Resources/fire.wav"));
            //this.soundFire = new SoundPlayer((new Uri("pack://application:,,,/Resources/fire.wav")).LocalPath);
            //this.soundImpact = new SoundPlayer((new Uri("pack://application:,,,/Resources/impact.wav")).LocalPath);

        }

        public double PosX()
        {
            return this.transX;
        }

        public double PosY()
        {
            return this.transY;
        }

        public void SetOponent(SpaceShip oponent)
        {            
            this.oponent = oponent;
        }

        public void Draw()
        {
            for (int i = 0; i < shots.Count; i++)
            {
                LaserShot shot = shots[i];
                if (IsBorderReached(shot.PosX()) || IsBorderReached(shot.PosY()))
                {
                    if (shot.DrawImpact())
                    {
                        shots.Remove(shot);
                        //soundImpact.Play();
                    }
                }
                else
                {
                    if (shot.IsExploding() || IsOponentShot(shot.PosX(), shot.PosY()))
                    {
                        if (shot.DrawImpact())
                        {
                            shots.Remove(shot);
                            this.oponent.ShotsReceived++;
                            //soundImpact.Play();
                        }
                        else 
                        {                             
                        }
                    }
                    else
                    {
                        shot.Draw();
                    }
                }
            }

            if (Visible)
            {             

                GL.PushMatrix();
                GL.Scale(0.02, 0.02, 1);
                GL.Translate(transX, transY, 0.0);

                GL.Rotate(rotate, 0, 0, 1);

                GL.LineWidth(2);

                GL.Begin(PrimitiveType.LineLoop);
                GL.Color3(color);
                GL.Vertex2(0.0f, 1.0f);
                GL.Vertex2(0.5f, -1.0f);
                GL.Vertex2(0.0f, -0.7f);
                GL.Vertex2(-0.5f, -1.0f);

                GL.End();

                GL.PopMatrix();
            }
        }
        
        public void Move(Hand hand)
        {
            if (Visible)
            {
                // Is first time with this hand
                if (hand.Id != this.handId)
                {
                    this.handId = hand.Id;
                    this.prevX = hand.StabilizedPalmPosition.x;
                    this.prevY = hand.StabilizedPalmPosition.y;
                }
                else
                {
                    this.deltaX = (hand.StabilizedPalmPosition.x - this.prevX) * SENSITIVITY;
                    this.deltaY = (hand.StabilizedPalmPosition.y - this.prevY) * SENSITIVITY;

                    this.prevX = hand.StabilizedPalmPosition.x;
                    this.prevY = hand.StabilizedPalmPosition.y;

                    double rotThreshold = 35.0;

                    if (hand.PalmVelocity.Magnitude > rotThreshold)
                    {
                        rotate = 180.0 + hand.PalmVelocity.Roll * 180.0 / (Math.PI);
                    }
                }

                transX += deltaX;
                transY += deltaY;

                BlockToBorders(ref transX);
                BlockToBorders(ref transY);


                double speed = 0.0;
                if (speedWatch.IsRunning)
                {
                    speed = Math.Sqrt(deltaX*deltaX+deltaY*deltaY) / speedWatch.ElapsedMilliseconds;
                    speedWatch.Restart();
                }
                else
                {
                    speedWatch.Start();
                }

                if (hand.Fingers.Count < 2)
                {
                    bool fireOn = false;
                    if (fireRateWatch.IsRunning)
                    {
                        if (fireRateWatch.ElapsedMilliseconds > 1000.0 / fireRate)
                        {
                            fireOn = true;
                            fireRateWatch.Restart();
                        }
                    }
                    else
                    {
                        fireOn = true;
                        fireRateWatch.Start();
                    }

                    if (fireOn)
                    {
                        shots.Add(new LaserShot(laserColor, transX, transY, speed, rotate));
                        soundFire2.Play();
                    }
                }
            }
        }

        // Make border mgt better (special class or something)
        private void BlockToBorders(ref double pos)
        {
            if (pos > BORDER)
            {
                pos = BORDER;
            }
            else if (pos < -BORDER)
            {
                pos = -BORDER;
            }
        }

        private bool IsBorderReached(double pos)
        {
            bool isBorderReached = false;
            if (pos > BORDER)// MAKE THAT BETTER!!
            {
                isBorderReached = true;
            }
            else if (pos < -BORDER)
            {
                isBorderReached = true;
            }
            return isBorderReached;
        }

        private bool IsOponentShot(double posX, double posY)
        {
            bool shotValid = false;
            double threshold = 1.0;

            if (oponent != null)
            {
                if (oponent.Visible)
                {
                    // TO REWORK
                    if (posX < oponent.PosX() + threshold && posX > oponent.PosX() - threshold &&
                        posY < oponent.PosY() + threshold && posY > oponent.PosY() - threshold)
                    {
                        shotValid = true;
                    }
                }
            }

            return shotValid;
        }
    }
}

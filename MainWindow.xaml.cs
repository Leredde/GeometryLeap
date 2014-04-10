using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Leap;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace GeometryLeap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameWindow game;
        private Leap.Controller leap;
        private SpaceShip ship1;
        private int ship1HandId;
        private SpaceShip ship2;
        private int ship2HandId;

        public MainWindow()
        {  
            InitializeComponent();

            try
            {
                leap = new Leap.Controller();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.Print(e.ToString());
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            this.startButton.IsEnabled = false;
            InitializeGame();

            ship1 = new SpaceShip(System.Drawing.Color.GreenYellow, System.Drawing.Color.Red);
            ship2 = new SpaceShip(System.Drawing.Color.DodgerBlue, System.Drawing.Color.Chartreuse); //blueViolet?

            ship1.Visible = false;
            ship2.Visible = false;

            ship1.SetOponent(ship2);
            ship2.SetOponent(ship1);

            game.Run(60.0);
        }

        private void DrawWarZone()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();

            GL.Scale(0.95, 0.95, 1);
            GL.LineWidth((float)1.5);
            GL.Begin(PrimitiveType.LineLoop);
            GL.Color3(System.Drawing.Color.DeepPink);
            GL.Vertex2(1.0f, 1.0f);
            GL.Vertex2(1.0f, -1.0f);
            GL.Vertex2(-1.0f, -1.0f);
            GL.Vertex2(-1.0f, 1.0f);

            GL.End();

            GL.PopMatrix();
        }

        private void DrawScores()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();

            GL.Scale(0.1, 0.1, 1);
            GL.LineWidth((float)1.5);
            
            GL.Color3(System.Drawing.Color.DeepPink);
            GL.Vertex2(1.0f, 1.0f);
            GL.Vertex2(1.0f, -1.0f);
            GL.Vertex2(-1.0f, -1.0f);
            GL.Vertex2(-1.0f, 1.0f);

            GL.End();

            GL.PopMatrix();
        }

        private void DrawAxies()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();

            GL.Scale(0.5, 0.5, 5);
            GL.LineWidth((float)2);

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(System.Drawing.Color.SkyBlue);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 0.0f);
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(System.Drawing.Color.Red);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 1.0f, 0.0f);
            GL.End();

            GL.Begin(PrimitiveType.Lines);
            GL.Color3(System.Drawing.Color.Green);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 1.0f);
            GL.End();

            GL.PopMatrix();
        }


        private void InitializeGame()
        {
            game = new GameWindow(800, 600, new OpenTK.Graphics.GraphicsMode(32, 24, 8, 8));

            game.Load += (sender, e) =>
            {
                // setup settings, load textures, sounds
                game.VSync = VSyncMode.On;
                game.Title = "GeometryLeap";


                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Enable(EnableCap.Blend);
                //game.WindowBorder = WindowBorder.Hidden;
                //game.Height = (int) System.Windows.SystemParameters.PrimaryScreenHeight;// Forms.PrimaryScreen.Bounds;
                //game.Width = (int)System.Windows.SystemParameters.PrimaryScreenWidth;
                //game.X = 0;
                //game.Y = 0;

                //debug only
                //game.

            };

            game.Resize += (sender, e) =>
            {
                GL.Viewport(0, 0, game.Width, game.Height);
            };

            game.UpdateFrame += (sender, e) =>
            {
                // add game logic, input handling
                if (game.Keyboard[OpenTK.Input.Key.Escape])
                {
                    game.Exit();
                }

                // Leap!
                MoveShip();
            };

            game.RenderFrame += (sender, e) =>
            {
                // render graphics
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);

                //DrawAxies();
                ship1.Draw();
                ship2.Draw();
                DrawWarZone();

                game.SwapBuffers();
            };

            game.Closed += (sender, e) =>
            {
                this.startButton.IsEnabled = true;
            };
        }
        
        private void MoveShip()
        {
            Leap.Frame frame = leap.Frame();

            if (!frame.Hands.IsEmpty)
            {
                this.ship1.Visible = true;
                Hand hand1 = frame.Hand(ship1HandId);
                if (hand1.IsValid)
                {
                    this.ship1.Move(hand1);
                }
                else
                {
                    this.ship1HandId = frame.Hands.Leftmost.Id;
                    if (this.ship1HandId == this.ship2HandId)
                    {
                        this.ship1HandId = frame.Hands.Rightmost.Id;// do better
                    }
                }

                if (frame.Hands.Count > 1)
                {
                    this.ship2.Visible = true;
                    Hand hand2 = frame.Hand(ship2HandId);
                    if (hand2.IsValid)
                    {
                        this.ship2.Move(hand2);
                    }
                    else
                    {
                        this.ship2HandId = frame.Hands.Rightmost.Id;
                        if (this.ship2HandId == this.ship1HandId)
                        {
                            this.ship2HandId = frame.Hands.Leftmost.Id;// do better
                        }
                    }
                }
                else
                {
                    this.ship2.Visible = false;

                }
            }
            else
            {
                this.ship1.Visible = false;
            }

            this.label1.Content = ship2.ShotsReceived.ToString();
            this.label2.Content = ship1.ShotsReceived.ToString();

        }
    }
}

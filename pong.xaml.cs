using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static wpfpixpile.Draw;

namespace wpfpixpile
{
    /// <summary>
    /// Interaction logic for pong.xaml
    /// </summary>
    public partial class PongWindow : Window
    {
        static Player player1 = new Player(new Int32Rect(0, 0, 10, 50));
        static Player player2 = new Player(new Int32Rect(0, 0, 10, 50));
        static Int32Rect ball = new Int32Rect(100, 100, 10, 10);
        static int playerSpeed = 6;
        static (int X, int Y) ballSpeed = (4, 4);
        
        static bool player1Win = false;
        static bool player2Win = false;
        static bool player1Auto = true;
        static bool player2Auto = true;
        static int safeZone = 150;
        static Random rnd = new Random();

        private class Player(Int32Rect _playerRect, bool _movingUp = false, bool _movingDown = false, bool _movingLeft = false, bool _movingRight = false)
        {
            public Int32Rect playerRect = _playerRect;
            public bool movingUp = _movingUp;
            public bool movingDown = _movingDown;
            public bool movingLeft = _movingLeft;
            public bool movingRight = _movingRight;
        }

        public PongWindow()
        {
            InitializeComponent();
        }

        private async void PongWindowWindowLoaded(object sender, RoutedEventArgs e)
        {
            Setup(Frame);

            PongWindowWindow.KeyDown += KeyDownListener;
            PongWindowWindow.KeyUp += KeyUpListener;

            player2.playerRect.X = FrameWidth - player2.playerRect.Width;
            ball = new Int32Rect(rnd.Next(safeZone, wb.PixelWidth - safeZone), rnd.Next(safeZone, wb.PixelHeight - safeZone), 10, 10);
            ballSpeed = ((rnd.Next(0, 2) * 2 - 1) * 4, (rnd.Next(0, 2) * 2 - 1) * 4);

            cfps = 60;

            Task.Run(() => UpdateCalculations(CalculateFrame));
            await RenderLoop(RenderFrame);
        }

        public void CalculateFrame()
        {
            void CheckMovement(Player player)
            {
                if (player.movingUp && player.playerRect.Y - playerSpeed >= 0) { player.playerRect.Y -= playerSpeed; }
                if (player.movingDown && player.playerRect.Y + player.playerRect.Height + playerSpeed < FrameHeight) { player.playerRect.Y += playerSpeed; }
                if (player.movingLeft) { player.playerRect.X -= playerSpeed; }
                if (player.movingRight) { player.playerRect.X += playerSpeed; }
            }

            void MoveBall()
            {
                if (ball.X + ball.Width >= FrameWidth)
                {
                    ballSpeed.X = -ballSpeed.X;
                    player1Win = true;
                }
                if (ball.X <= 0)
                {
                    ballSpeed.X = -ballSpeed.X;
                    player2Win = true;
                }

                if (ball.Y + ball.Height >= FrameHeight || ball.Y <= 0)
                {
                    ballSpeed.Y = -ballSpeed.Y;
                }

                if (ball.Y + ball.Height <= player1.playerRect.Y + player1.playerRect.Height && ball.Y >= player1.playerRect.Y && ball.X <= player1.playerRect.X + player1.playerRect.Width ||
                    ball.Y + ball.Height <= player2.playerRect.Y + player2.playerRect.Height && ball.Y >= player2.playerRect.Y && ball.X + ball.Width >= player2.playerRect.X)
                {
                    ballSpeed.X = -ballSpeed.X;
                }

                ball.X += ballSpeed.X;
                ball.Y += ballSpeed.Y;
            }

            void AutoMove(Player player)
            {
                if (player.playerRect.Y - playerSpeed >= 0 && player.playerRect.Y + player.playerRect.Height > ball.Y + player.playerRect.Height / 2) { player.playerRect.Y -= playerSpeed; }
                if (player.playerRect.Y + player.playerRect.Height + playerSpeed < FrameHeight && player.playerRect.Y < ball.Y + ball.Height - player.playerRect.Height / 2) { player.playerRect.Y += playerSpeed; }
            }

            if (player1Auto) { AutoMove(player1); }
            else { CheckMovement(player1); }
            if (player2Auto) { AutoMove(player2); }
            else { CheckMovement(player2); }

            MoveBall();
        }

        public void RenderFrame()
        {
            DrawRectangle(wb, new Int32Rect(0, 0, FrameWidth, FrameHeight), Color.FromRgb(0, 0, 0));
            DrawRectangle(wb, ball, Color.FromRgb(255, 255, 255));
            DrawRectangle(wb, player1.playerRect, Color.FromRgb(255, 0, 0));
            DrawRectangle(wb, player2.playerRect, Color.FromRgb(0, 128, 255));
            if (player1Win) { DrawRectangle(wb, new Int32Rect(20, 20, FrameWidth - 40, FrameHeight - 40), Color.FromRgb(255, 0, 0));
                pauseBool = true;
                player1Win = false;
            }
            if (player2Win) { DrawRectangle(wb, new Int32Rect(20, 20, FrameWidth - 40, FrameHeight - 40), Color.FromRgb(0, 128, 255));
                pauseBool = true;
                player2Win = false;
            }
        }

        private void KeyDownListener(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W) { player1.movingUp = true; }
            if (e.Key == Key.S) { player1.movingDown = true; }
            //if (e.Key == Key.A) { player1.movingLeft = true; }
            //if (e.Key == Key.D) { player1.movingRight = true; }

            if (e.Key == Key.Up) { player2.movingUp = true; }
            if (e.Key == Key.Down) { player2.movingDown = true; }
            /*if (e.Key == Key.Left) { player2.movingLeft = true; }
            if (e.Key == Key.Right) { player2.movingRight = true; }*/

            if (e.Key == Key.F11)
            {
                if (fullScreen) { PongWindowWindow.WindowState = WindowState.Normal; } else { PongWindowWindow.WindowState = WindowState.Maximized; }
                fullScreen = !fullScreen;
            }
            if (e.Key == Key.R)
            {
                player1 = new Player(new Int32Rect(0, 0, 10, 50));
                player2 = new Player(new Int32Rect((int)PongWindowWindow.ActualWidth - player2.playerRect.Width, 0, 10, 50));
                ball = new Int32Rect(rnd.Next(safeZone, wb.PixelWidth - safeZone), rnd.Next(safeZone, wb.PixelHeight - safeZone), 10, 10);
                ballSpeed = ((rnd.Next(0, 2) * 2 - 1) * 4, (rnd.Next(0, 2) * 2 - 1) * 4);
            }
            if (e.Key == Key.Escape) { this.Close(); }
            if (e.Key == Key.Space) { pauseBool = !pauseBool; }
        }

        private void KeyUpListener(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W) { player1.movingUp = false; }
            if (e.Key == Key.S) { player1.movingDown = false; }
            //if (e.Key == Key.A) { player1.movingLeft = false; }
            //if (e.Key == Key.D) { player1.movingRight = false; }

            if (e.Key == Key.Up) { player2.movingUp = false; }
            if (e.Key == Key.Down) { player2.movingDown = false; }
            /*if (e.Key == Key.Left) { player2.movingLeft = false; }
            if (e.Key == Key.Right) { player2.movingRight = false; }*/
        }
    }
}

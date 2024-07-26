using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static wpfpixpile.Draw;

namespace wpfpixpile
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class PongWindow : Window
    {
        static WriteableBitmap wb = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgr32, null);
        static Image Frame = new Image();
        static Player player1 = new Player(new Int32Rect(0, 0, 10, 50));
        static Player player2 = new Player(new Int32Rect(0, 0, 10, 50));
        static Int32Rect ball = new Int32Rect(100, 100, 10, 10);
        static bool Pause = false;
        static int playerSpeed = 6;
        static (int X, int Y) ballSpeed = (4, 4);
        static bool fullScreen = false;

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
            Frame = new Image();
            RenderOptions.SetBitmapScalingMode(Frame, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(Frame, EdgeMode.Aliased);

            PongWindowWindow.Content = Frame;

            wb = new WriteableBitmap(
                (int)PongWindowWindow.ActualWidth, (int)PongWindowWindow.ActualHeight,
                96, 96, PixelFormats.Bgr32, null
            );

            Frame.Source = wb;
            Frame.Stretch = Stretch.Uniform;
            Frame.HorizontalAlignment = HorizontalAlignment.Left;
            Frame.VerticalAlignment = VerticalAlignment.Top;

            PongWindowWindow.KeyDown += KeyDownListener;
            PongWindowWindow.KeyUp += KeyUpListener;

            player2.playerRect.X = (int)PongWindowWindow.ActualWidth - player2.playerRect.Width;

            await GameLoop();
        }

        private async Task GameLoop()
        {
            void CheckMovement(Player player)
            {
                if (player.movingUp && player.playerRect.Y - playerSpeed >= 0) { player.playerRect.Y -= playerSpeed; }
                if (player.movingDown && player.playerRect.Y + player.playerRect.Height + playerSpeed < wb.PixelHeight) { player.playerRect.Y += playerSpeed; }
                if (player.movingLeft) { player.playerRect.X -= playerSpeed; }
                if (player.movingRight) { player.playerRect.X += playerSpeed; }
            }
            void MoveBall()
            {
                if (ball.X + ball.Width >= wb.PixelWidth)
                {
                    ballSpeed.X = -ballSpeed.X;
                    DrawRectangle(wb, new Int32Rect(20, 20, wb.PixelWidth - 40, wb.PixelHeight - 40), Color.FromRgb(255, 0, 0));
                    Pause = true;
                }
                if (ball.X <= 0)
                {
                    ballSpeed.X = -ballSpeed.X;
                    DrawRectangle(wb, new Int32Rect(20, 20, wb.PixelWidth - 40, wb.PixelHeight - 40), Color.FromRgb(0, 128, 255));
                    Pause = true;
                }

                if (ball.Y + ball.Height >= wb.PixelHeight || ball.Y <= 0)
                {
                    ballSpeed.Y = -ballSpeed.Y;
                }

                if (ball.Y + ball.Height <= player1.playerRect.Y + player1.playerRect.Height && ball.Y >= player1.playerRect.Y && ball.X <= player1.playerRect.X + player1.playerRect.Width ||
                    ball.Y + ball.Height <= player2.playerRect.Y + player2.playerRect.Height && ball.Y >= player2.playerRect.Y && ball.X + ball.Width >= player2.playerRect.X)
                {
                    ballSpeed.X = -ballSpeed.X;
                }

                /*if (((ball.Y + ball.Height > player1.playerRect.Y || ball.Y < player1.playerRect.Y + player1.playerRect.Height) && ball.X < player1.playerRect.X + player1.playerRect.Width) ||
                    ((ball.Y + ball.Height > player2.playerRect.Y || ball.Y < player2.playerRect.Y + player2.playerRect.Height) && ball.X + ball.Width > player2.playerRect.X))
                {
                    ballSpeed.Y = -ballSpeed.Y;
                }*/

                ball.X += ballSpeed.X;
                ball.Y += ballSpeed.Y;
            }

            void AutoMove(Player player)
            {
                if (player.playerRect.Y - playerSpeed >= 0 && player.playerRect.Y + player.playerRect.Height > ball.Y + player.playerRect.Height / 2) { player.playerRect.Y -= playerSpeed; }
                if (player.playerRect.Y + player.playerRect.Height + playerSpeed < wb.PixelHeight && player.playerRect.Y < ball.Y + ball.Height - player.playerRect.Height / 2) { player.playerRect.Y += playerSpeed; }
            }

            while (true)
            {
                if (!Pause)
                {
                    DrawRectangle(wb, new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight), Color.FromRgb(0, 0, 0));
                    DrawRectangle(wb, ball, Color.FromRgb(255, 255, 255));
                    DrawRectangle(wb, player1.playerRect, Color.FromRgb(255, 0, 0));
                    //CheckMovement(player1);
                    AutoMove(player1);
                    DrawRectangle(wb, player2.playerRect, Color.FromRgb(0, 128, 255));
                    //CheckMovement(player2);
                    AutoMove(player2);
                    MoveBall();
                    await Task.Delay(1000 / 120);
                }
                else { await Task.Delay(1000 / 120); }
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
                ball = new Int32Rect(100, 100, 10, 10);
            }
            if (e.Key == Key.Escape) { this.Close(); }
            if (e.Key == Key.Space) { Pause = !Pause; }
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

using System.Numerics;
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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_pong_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PongWindow pong = new PongWindow();
            pong.Owner = this;
            pong.Show();
        }

        private void button_shark3d_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Shark3dWindow shark3d = new Shark3dWindow();
            shark3d.Owner = this;
            shark3d.Show();
        }

        private void button_cssysstat_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CsSysStatWindow cssysstat = new CsSysStatWindow();
            cssysstat.Owner = this;
            cssysstat.Show();
        }
    }
}

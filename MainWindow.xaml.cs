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

        private void button_test_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TestWindow test = new TestWindow();
            test.Owner = this;
            test.Show();
        }
    }
}

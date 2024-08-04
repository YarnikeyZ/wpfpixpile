using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using static wpfpixpile.Draw;

namespace wpfpixpile
{
    /// <summary>
    /// Interaction logic for CsSysStat.xaml
    /// </summary>
    public partial class CsSysStatWindow : Window
    {
        static bool DrawnAsLines = true;
        static bool DrawnAsRects = false;

        static int[] PlotPointsBefore = [];
        static int[] PlotPointsNow = [];

        static int PointCount = 200;
        static double PointHeightPercent = 0;
        static double PointDencity = 0;

        static PerformanceCounter cpuCounter = new PerformanceCounter();

        public CsSysStatWindow()
        {
            InitializeComponent();
        }

        private async void CsSysStatWindowWindowLoaded(object sender, RoutedEventArgs e)
        {
            Setup(Frame);

            PlotPointsBefore = new int[PointCount];
            PlotPointsNow = new int[PointCount];

            PointDencity = Frame.ActualWidth / PointCount;
            PointHeightPercent = Frame.ActualHeight / 100;

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            Task.Run(() => UpdateCalculations(CalculateFrame));
            await RenderLoop(RenderFrame);
        }

        public void CalculateFrame()
        {
            Array.Copy(PlotPointsBefore, 1, PlotPointsNow, 0, PlotPointsBefore.Length - 1); // shift to left and start calc
            PlotPointsNow[PlotPointsNow.Length - 1] = (int)cpuCounter.NextValue(); // set last element to next value
            PlotPointsNow.CopyTo(PlotPointsBefore, 0); // place to buffer and end
        }

        public void RenderFrame()
        {
            DrawRectangle(wb, new Int32Rect(0, 0, (int)Frame.ActualWidth, (int)Frame.ActualHeight), PColors.Black);
            DrawRectangle(wb, new Int32Rect(0, 0, (int)Frame.ActualWidth, 1), PColors.Red);
            DrawRectangle(wb, new Int32Rect(0, (int)(PointHeightPercent * 25), (int)Frame.ActualWidth, 1), Color.FromRgb(192, 192, 192));
            DrawRectangle(wb, new Int32Rect(0, (int)(PointHeightPercent * 50), (int)Frame.ActualWidth, 1), Color.FromRgb(128, 128, 128));
            DrawRectangle(wb, new Int32Rect(0, (int)(PointHeightPercent * 75), (int)Frame.ActualWidth, 1), Color.FromRgb(64, 64, 64));

            int pBefore = 0;
            int point = 0;
            for (int i = 0; i < PlotPointsNow.Length; i++)
            {
                if (i != 0)
                {
                    point = (PlotPointsNow[i] <= 0 ? 1 : PlotPointsNow[i]);
                    pb_cpu.Value = point;
                    l_cpu.Content = $"CPU: {point}%";
                    point = (int)(Frame.Height - point * PointHeightPercent);
                }
                else { point = (int)Frame.Height; }


                if (DrawnAsRects)
                {
                    DrawRectangle(
                        wb,
                        new Int32Rect((int)(i * PointDencity), point, (int)PointDencity + 1, (int)Frame.ActualHeight),
                        PColors.White
                    );
                }

                if (DrawnAsLines)
                {
                    DrawLine(
                        wb,
                        new Point((int)((i - 1) * PointDencity), pBefore),
                        new Point((int)(i * PointDencity), point),
                        PColors.White
                    );
                }

                pBefore = point;
            }
        }
    }
}

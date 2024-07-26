using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Reflection;
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
    public partial class TestWindow : Window
    {
        static WriteableBitmap wb = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgr32, null);
        static double[][] zb = new double[wb.PixelWidth][];

        static Image Frame = new Image();
        
        static bool Pause = false;
        static bool frameDone = false;
        static bool runBool = true;

        static bool drawPoligons = false; // F1
        static bool drawVertices = false; // F2
        static bool drawFill = true; // F3 **
        static bool drawOutside = true; // F4
        static bool drawClip = true; // F5
        static bool drawRes = true; // F6
        static bool drawInvertNorm = false; // F7
        static bool drawFullScreen = false; // F11
        static bool drawTakeScreenshot = false; // F12

        public bool sprint = false;
        public bool movingW = false;
        public bool movingS = false;
        public bool movingA = false;
        public bool movingD = false;
        public bool movingZ = false;
        public bool movingX = false;
        public bool movingUp = false;
        public bool movingDown = false;
        public bool movingLeft = false;
        public bool movingRight = false;

        static Int32Rect background;
         
        Color RGBclr = Color.FromRgb(0, 0, 0);
        public static int RGBClrCount = 0;
        Color REDclr = Color.FromRgb(255, 0, 0);
        Color GREENclr = Color.FromRgb(0, 255, 0);
        Color BLACKclr = Color.FromRgb(0, 0, 0);

        public double FrameAspect = 1;

        public Vector3D cameraDirection = new Vector3D(0, 0, -1, 0);
        public Vector3D cameraPos = new Vector3D(0, 0, 0, 1);
        public int cameraSpeed = 1;

        public int RotX = 0;
        public int RotY = 0;

        public static Object3D[] SceneObjects = new Object3D[2];

        string pathToScreenshots = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+"\\Screenshots\\";

        public TestWindow()
        {
            InitializeComponent();
        }

        private async void TestWindowWindowLoaded(object sender, RoutedEventArgs e)
        {
            Frame = new Image();
            RenderOptions.SetBitmapScalingMode(Frame, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(Frame, EdgeMode.Aliased);

            TestWindowWindow.Content = Frame;

            wb = new WriteableBitmap(
                (int)TestWindowWindow.ActualWidth, (int)TestWindowWindow.ActualHeight,
                96, 96, PixelFormats.Bgr32, null
            );

            zb = new double[wb.PixelWidth][];
            for (int x = 0; x < wb.PixelWidth; x++)
            {
                zb[x] = new double[wb.PixelHeight];
            }
            for (int y = 0; y < wb.PixelHeight; y++)
            {
                for (int x = 0; x < wb.PixelWidth; x++)
                {
                    zb[x][y] = double.PositiveInfinity;
                }
            }

            Frame.Source = wb;
            Frame.Stretch = Stretch.UniformToFill;
            Frame.HorizontalAlignment = HorizontalAlignment.Left;
            Frame.VerticalAlignment = VerticalAlignment.Top;

            TestWindowWindow.KeyDown += KeyDownListener;
            TestWindowWindow.KeyUp += KeyUpListener;

            background = new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight);
            FrameAspect = 1;// (double)wb.PixelHeight / (double)wb.PixelWidth;


            runBool = true;
            LoadObjects();

            Task.Run(() => UpdateMovement());
            Task.Run(() => UpdateRGBColor());
            Task.Run(() => UpdateCalculations());
            await RenderLoop();
        }
        
        private void LoadObjects()
        {
            //new Object3D(new Vector3D(0, 0, -400), new Vector3D(100, 100, 100), 0, 0, true, false).ChangeToAxis().PushToScene(ref SceneObjects);

            //new Object3D(new Vector3D(0, 0, -400), new Vector3D(100, 100, 100), 0, 0, false).ChangeToCube(REDclr).PushToScene(ref SceneObjects);
            //new Object3D(new Vector3D(-100, 0, -400), new Vector3D(100, 100, 100), 0, 35, false).ChangeToCube(GREENclr).PushToScene(ref SceneObjects);

            //new Object3D(new Vector3D(0, 0, -400), new Vector3D(100, 100, 100), 0, 0, false).ChangeToPiramid().PushToScene(ref SceneObjects);

            new Object3D(new Vector3D(0, 0, -400), new Vector3D(100, 100, 100), 0, 0, false, true).ChangeToLoadFromObj("C:\\Users\\jrikt\\Desktop\\test.obj").PushToScene(ref SceneObjects);
        }

        private void UpdateRGBColor()
        {
            while (true)
            {
                if (RGBClrCount < 256)
                {
                    RGBclr = MakeColorGradient(0.07, 4, RGBClrCount);
                    RGBClrCount += 1;
                }
                else { RGBClrCount = 0; }
                Thread.Sleep(1000 / 20);
            }
        }

        private void UpdateMovement()
        {
            while (true)
            {
                if (movingW) { cameraPos += cameraDirection * (sprint ? cameraSpeed * 3 : cameraSpeed); }
                if (movingS) { cameraPos -= cameraDirection * (sprint ? cameraSpeed * 3 : cameraSpeed); }
                if (movingA) { cameraPos += (new Vector3D(0, 1, 0) * cameraDirection.Norm()) * (sprint ? cameraSpeed * 3 : cameraSpeed); }
                if (movingD) { cameraPos -= (new Vector3D(0, 1, 0) * cameraDirection.Norm()) * (sprint ? cameraSpeed * 3 : cameraSpeed); }
                if (movingZ) { cameraPos += (new Vector3D(0, 1, 0)) * (sprint ? cameraSpeed * 3 : cameraSpeed); }
                if (movingX) { cameraPos -= (new Vector3D(0, 1, 0)) * (sprint ? cameraSpeed * 3 : cameraSpeed); }
                foreach (var LoadedObject in SceneObjects)
                {
                    if (LoadedObject != null)
                    {
                        if (movingUp) { LoadedObject.RotX += (sprint ? cameraSpeed * 3 : cameraSpeed); }
                        if (movingDown) { LoadedObject.RotX -= (sprint ? cameraSpeed * 3 : cameraSpeed); }
                        if (movingLeft) { LoadedObject.RotY += (sprint ? cameraSpeed * 3 : cameraSpeed); }
                        if (movingRight) { LoadedObject.RotY -= (sprint ? cameraSpeed * 3 : cameraSpeed); }
                    }
                }
                Thread.Sleep(1000 / 20);
            }
        }

        private void UpdateCalculations()
        {
            Draw.Matrix4x4 Look;
            Draw.Matrix4x4 Perspective;
            Draw.Matrix4x4 Mat;
            Vector3D Vertex;
            
            while (runBool)
            {
                if (!Pause && !frameDone)
                {
                    Look = Draw.Matrix4x4.GetLookAt(cameraPos, cameraPos + cameraDirection, new Vector3D(0, 1, 0));
                    Perspective = Draw.Matrix4x4.getPerspectiveProj(90, FrameAspect, -1, -1000);

                    foreach (Object3D LoadedObject in SceneObjects)
                    {
                        if (LoadedObject != null)
                        {
                            LoadedObject.ObjectVertices.CopyTo(LoadedObject.BakedObjectVertices, 0);

                            // Mat prep
                            Mat = Draw.Matrix4x4.GetRotationX(LoadedObject.RotX);
                            Mat = Draw.Matrix4x4.GetRotationY(LoadedObject.RotY) * Mat;
                            Mat = Draw.Matrix4x4.GetScale(LoadedObject.Scale.X, LoadedObject.Scale.Y, LoadedObject.Scale.Z) * Mat;
                            Mat = Draw.Matrix4x4.GetTranslation(LoadedObject.Pos.X, LoadedObject.Pos.Y, LoadedObject.Pos.Z) * Mat;
                            Mat = Look * Mat;
                            Mat = Perspective * Mat;

                            // Vertices prep
                            for (int i = 0; i < LoadedObject.BakedObjectVertices.Length; i++)
                            {
                                Vertex = LoadedObject.ObjectVertices[i] * Mat;
                                Vertex.X = Vertex.X / Vertex.W * 640;
                                Vertex.Y = Vertex.Y / Vertex.W * 360;
                                LoadedObject.BakedObjectVertices[i] = Vertex;
                            }
                        }
                    }
                    frameDone = true;
                    Thread.Sleep(1000 / 20);
                }
                else { Thread.Sleep(1000 / 20); }
            }
        }

        private async Task RenderLoop()
        {
            Vector3D v1;
            Vector3D v2;
            Vector3D v3;
            Vector3D t1;
            Vector3D t2;
            Vector3D normal;
            double res;
            Point p1;
            Point p2;
            Point p3;
            int[] ind;
            Stopwatch sw = Stopwatch.StartNew();
            int TFPS = 1000;
            int RFPS = 0;
            int MFPS = 0;
            int MMFPS = 0;

            while (runBool)
            {
                //Trace.WriteLine($"Second pass = {sw.Elapsed.TotalSeconds}   FPS(R/MR) = {RFPS}/{MFPS}   Faker(0/1) = {!frameDone}   ");
                if (MMFPS != MFPS) { Trace.WriteLine($"{MFPS} ({MFPS - MMFPS})"); MMFPS = MFPS; }
                if (sw.Elapsed.TotalSeconds >= 1) { MFPS = RFPS; RFPS = 0; sw = Stopwatch.StartNew(); }
                ++RFPS;

                if (!Pause && frameDone)
                {
                    DrawRectangle(wb, background, BLACKclr);
                    DrawPixel(wb, new Point(5, 5), drawPoligons ? GREENclr : REDclr);
                    DrawPixel(wb, new Point(5, 18), drawVertices ? GREENclr : REDclr);
                    DrawPixel(wb, new Point(5, 31), drawFill ? GREENclr : REDclr);
                    DrawPixel(wb, new Point(5, 44), drawOutside ? GREENclr : REDclr);
                    DrawPixel(wb, new Point(5, 57), drawClip ? GREENclr : REDclr);
                    DrawPixel(wb, new Point(5, 70), drawRes ? GREENclr : REDclr);
                    DrawPixel(wb, new Point(5, 83), drawInvertNorm ? GREENclr : REDclr);
                    DrawPixel(wb, new Point(5, 96), drawFullScreen ? GREENclr : REDclr);
                    DrawPixel(wb, new Point(5, 109), drawTakeScreenshot ? GREENclr : REDclr);

                    foreach (Object3D LoadedObject in SceneObjects)
                    {
                        if (LoadedObject != null)
                        {
                            for (int i = 0; i < LoadedObject.ObjectIndices.Length; i++)
                            {
                                ind = LoadedObject.ObjectIndices[i];

                                v1 = LoadedObject.BakedObjectVertices[ind[0]];
                                v2 = LoadedObject.BakedObjectVertices[ind[1]];
                                v3 = LoadedObject.BakedObjectVertices[ind[2]];

                                if (!drawClip || Math.Abs(cameraPos.DistanceTo(v1)) > 2 || Math.Abs(cameraPos.DistanceTo(v2)) > 2 || Math.Abs(cameraPos.DistanceTo(v3)) > 2)
                                {
                                    if (drawOutside || !(v1.X + wb.PixelWidth / 2 < 0 || v1.X > wb.PixelWidth ||
                                        v2.X + wb.PixelWidth / 2 < 0 || v2.X > wb.PixelWidth ||
                                        v3.X + wb.PixelWidth / 2 < 0 || v3.X > wb.PixelWidth ||
                                        -(v1.Y - wb.PixelHeight / 2) < 0 || -(v1.Y - wb.PixelHeight / 2) > wb.PixelHeight ||
                                        -(v2.Y - wb.PixelHeight / 2) < 0 || -(v2.Y - wb.PixelHeight / 2) > wb.PixelHeight ||
                                        -(v3.Y - wb.PixelHeight / 2) < 0 || -(v3.Y - wb.PixelHeight / 2) > wb.PixelHeight)
                                    )
                                    {
                                        t1 = v1 - v2;
                                        t2 = v2 - v3;
                                        normal = (t1 * t2).Norm();
                                        res = Vector3D.ScalarProduct(cameraDirection, normal);

                                        if (!drawRes || (!drawInvertNorm && res > 0 || drawInvertNorm && res < 0 || LoadedObject.InFront))
                                        {
                                            p1.X = (int)v1.X; p1.Y = (int)v1.Y;
                                            p2.X = (int)v2.X; p2.Y = (int)v2.Y;
                                            p3.X = (int)v3.X; p3.Y = (int)v3.Y;

                                            if (drawFill)
                                            {
                                                DrawPolygonFill(wb, [p1, p2, p3], LoadedObject.ObjectColors[i], true);
                                            }
                                            if (drawPoligons)
                                            {
                                                DrawLine(wb, p1, p2, LoadedObject.ObjectColors[i], true);
                                                DrawLine(wb, p2, p3, LoadedObject.ObjectColors[i], true);
                                                DrawLine(wb, p1, p3, LoadedObject.ObjectColors[i], true);
                                            }
                                            if (drawVertices)
                                            {
                                                DrawPixel(wb, p1, LoadedObject.ObjectColors[i], true);
                                                DrawPixel(wb, p2, LoadedObject.ObjectColors[i], true);
                                                DrawPixel(wb, p3, LoadedObject.ObjectColors[i], true);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (drawTakeScreenshot)
                    {
                        Pause = true;
                        TakeScreenshot($"{pathToScreenshots}screenshot_{DateTime.Now.ToString().Replace(" ", "_").Replace(".", "_").Replace(":", "_")}.png", wb.Clone());
                        Pause = false;
                        drawTakeScreenshot = false;
                    }

                    frameDone = false;
                    await Task.Delay(1000 / TFPS);
                }
                else { await Task.Delay(1000 / TFPS); }
            }
        }

        private void KeyDownListener(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift) { sprint = true; }
            if (e.Key == Key.W) { movingW = true; }
            if (e.Key == Key.S) { movingS = true; }
            if (e.Key == Key.A) { movingA = true; }
            if (e.Key == Key.D) { movingD = true; }
            if (e.Key == Key.Z) { movingZ = true; }
            if (e.Key == Key.X) { movingX = true; }
            if (e.Key == Key.Up) { movingUp = true; }
            if (e.Key == Key.Down) { movingDown = true; }
            if (e.Key == Key.Left) { movingLeft = true; }
            if (e.Key == Key.Right) { movingRight = true; }


            if (e.Key == Key.F1) { drawPoligons = !drawPoligons; }
            if (e.Key == Key.F2) { drawVertices = !drawVertices; }
            if (e.Key == Key.F3) { drawFill = !drawFill; }
            if (e.Key == Key.F4) { drawOutside = !drawOutside; }
            if (e.Key == Key.F5) { drawClip = !drawClip; }
            if (e.Key == Key.F6) { drawRes = !drawRes; }
            if (e.Key == Key.F7) { drawInvertNorm = !drawInvertNorm; }
            if (e.Key == Key.F11)
            {
                if (drawFullScreen) { TestWindowWindow.WindowState = WindowState.Normal; }
                else { TestWindowWindow.WindowState = WindowState.Maximized; }
                drawFullScreen = !drawFullScreen;
            }
            if (e.Key == Key.F12)
            {
                if (!drawTakeScreenshot)
                {
                    drawTakeScreenshot = true;
                }
            }

            if (e.Key == Key.Escape) { this.Close(); runBool = false; }
            if (e.Key == Key.Space) { Pause = !Pause; }
        }

        private void KeyUpListener(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift) { sprint = false; }
            if (e.Key == Key.W) { movingW = false; }
            if (e.Key == Key.S) { movingS = false; }
            if (e.Key == Key.A) { movingA = false; }
            if (e.Key == Key.D) { movingD = false; }
            if (e.Key == Key.Z) { movingZ = false; }
            if (e.Key == Key.X) { movingX = false; }
            if (e.Key == Key.Up) { movingUp = false; }
            if (e.Key == Key.Down) { movingDown = false; }
            if (e.Key == Key.Left) { movingLeft = false; }
            if (e.Key == Key.Right) { movingRight = false; }
        }
    }
}

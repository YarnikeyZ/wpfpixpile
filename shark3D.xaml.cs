using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using static wpfpixpile.Draw;

namespace wpfpixpile
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Shark3dWindow : Window
    {
        static bool drawPoligons = false; // F1
        static bool drawVertices = false; // F2
        static bool drawFill = true; // F3
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

        public Vector3D cameraDirection = new Vector3D(0, 0, -1, 0);
        public Vector3D cameraPos = new Vector3D(0, 0, 0, 1);
        public int cameraSpeed = 1;

        public int RotX = 0;
        public int RotY = 0;

        public static Object3D[] SceneObjects = new Object3D[2];

        public Shark3dWindow()
        {
            InitializeComponent();
        }

        private async void Shark3dWindowWindowLoaded(object sender, RoutedEventArgs e)
        {
            Setup(Frame);

            Shark3dWindowWindow.KeyDown += KeyDownListener;
            Shark3dWindowWindow.KeyUp += KeyUpListener;

            FrameAspect = 1;// (double)wb.PixelHeight / (double)wb.PixelWidth;
            runBool = true;
            LoadObjects();

            Task.Run(() => UpdateCalculations(CalculateFrame));
            await RenderLoop(RenderFrame);
        }
        
        private void LoadObjects()
        {
            //new Object3D(new Vector3D(0, 0, -400), new Vector3D(100, 100, 100), 0, 0, true, false).ChangeToAxis().PushToScene(ref SceneObjects);

            //new Object3D(new Vector3D(0, 0, -400), new Vector3D(100, 100, 100), 0, 0, false).ChangeToCube(REDclr).PushToScene(ref SceneObjects);
            //new Object3D(new Vector3D(-100, 0, -400), new Vector3D(100, 100, 100), 0, 35, false).ChangeToCube(GREENclr).PushToScene(ref SceneObjects);

            //new Object3D(new Vector3D(0, 0, -400), new Vector3D(100, 100, 100), 0, 0, false).ChangeToPiramid().PushToScene(ref SceneObjects);

            new Object3D(new Vector3D(0, 0, -400), new Vector3D(100, 100, 100), 0, 0, false, true).ChangeToLoadFromObj("C:\\Users\\jrikt\\Desktop\\test.obj").PushToScene(ref SceneObjects);
        }

        public void CalculateFrame()
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

            // Calculating for render
            Matrix4x4 Look = Matrix4x4.GetLookAt(cameraPos, cameraPos + cameraDirection, new Vector3D(0, 1, 0));
            Matrix4x4 Perspective = Matrix4x4.getPerspectiveProj(90, FrameAspect, -1, -1000);

            foreach (Object3D LoadedObject in SceneObjects)
            {
                if (LoadedObject != null)
                {
                    LoadedObject.ObjectVertices.CopyTo(LoadedObject.BakedObjectVertices, 0);

                    // Mat prep
                    Matrix4x4 Mat = Matrix4x4.GetRotationX(LoadedObject.RotX);
                    Mat = Matrix4x4.GetRotationY(LoadedObject.RotY) * Mat;
                    Mat = Matrix4x4.GetScale(LoadedObject.Scale.X, LoadedObject.Scale.Y, LoadedObject.Scale.Z) * Mat;
                    Mat = Matrix4x4.GetTranslation(LoadedObject.Pos.X, LoadedObject.Pos.Y, LoadedObject.Pos.Z) * Mat;
                    Mat = Look * Mat;
                    Mat = Perspective * Mat;

                    // Vertices prep
                    for (int i = 0; i < LoadedObject.BakedObjectVertices.Length; i++)
                    {
                        Vector3D Vertex = LoadedObject.ObjectVertices[i] * Mat;
                        Vertex.X = Vertex.X / Vertex.W * 640;
                        Vertex.Y = Vertex.Y / Vertex.W * 360;
                        LoadedObject.BakedObjectVertices[i] = Vertex;
                    }
                }
            }
        }

        public void RenderFrame()
        {
            DrawRectangle(wb, new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight), PColors.Black);
            DrawPixel(wb, new Point(5, 5), drawPoligons ? PColors.Green : PColors.Red);
            DrawPixel(wb, new Point(5, 18), drawVertices ? PColors.Green : PColors.Red);
            DrawPixel(wb, new Point(5, 31), drawFill ? PColors.Green : PColors.Red);
            DrawPixel(wb, new Point(5, 44), drawOutside ? PColors.Green : PColors.Red);
            DrawPixel(wb, new Point(5, 57), drawClip ? PColors.Green : PColors.Red);
            DrawPixel(wb, new Point(5, 70), drawRes ? PColors.Green : PColors.Red);
            DrawPixel(wb, new Point(5, 83), drawInvertNorm ? PColors.Green : PColors.Red);
            DrawPixel(wb, new Point(5, 96), drawFullScreen ? PColors.Green : PColors.Red);
            DrawPixel(wb, new Point(5, 109), drawTakeScreenshot ? PColors.Green : PColors.Red);

            foreach (Object3D LoadedObject in SceneObjects)
            {
                if (LoadedObject != null)
                {
                    for (int i = 0; i < LoadedObject.ObjectIndices.Length; i++)
                    {
                        int[] ind = LoadedObject.ObjectIndices[i];

                        Vector3D v1 = LoadedObject.BakedObjectVertices[ind[0]];
                        Vector3D v2 = LoadedObject.BakedObjectVertices[ind[1]];
                        Vector3D v3 = LoadedObject.BakedObjectVertices[ind[2]];

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
                                Vector3D t1 = v1 - v2;
                                Vector3D t2 = v2 - v3;
                                Vector3D normal = (t1 * t2).Norm();
                                double res = Vector3D.ScalarProduct(cameraDirection, normal);

                                if (!drawRes || (!drawInvertNorm && res > 0 || drawInvertNorm && res < 0 || LoadedObject.InFront))
                                {
                                    Point p1 = new Point((int)v1.X, (int)v1.Y);
                                    Point p2 = new Point((int)v2.X, (int)v2.Y);
                                    Point p3 = new Point((int)v3.X, (int)v3.Y);

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
                if (drawFullScreen) { Shark3dWindowWindow.WindowState = WindowState.Normal; }
                else { Shark3dWindowWindow.WindowState = WindowState.Maximized; }
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
            if (e.Key == Key.Space) { pauseBool = !pauseBool; }
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

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Math;

namespace wpfpixpile
{
    public class Draw
    {
        // Engine variables
        public static WriteableBitmap wb = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgr32, null);

        public static int fps = 60;
        public static int cfps = 20;

        public static bool runBool = true;
        public static bool frameDone = false;
        public static bool pauseBool = false;
        public static bool drawTakeScreenshot = false;
        public static bool fullScreen = false;

        public static int FrameWidth = 0;
        public static int FrameHeight = 0;
        public static double FrameAspect = 0;

        public static string pathToScreenshots = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Screenshots\\";

        // Engine work
        public static void Setup(Image ImageElement)
        {
            RenderOptions.SetBitmapScalingMode(ImageElement, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(ImageElement, EdgeMode.Aliased);
            wb = new WriteableBitmap(
                (int)ImageElement.ActualWidth, (int)ImageElement.ActualHeight,
                96, 96, PixelFormats.Bgr32, null
            );
            ImageElement.Source = wb;
            FrameWidth = (int)ImageElement.ActualWidth;
            FrameHeight = (int)ImageElement.ActualHeight;
            FrameAspect = (int)ImageElement.ActualWidth / (int)ImageElement.ActualHeight;
            runBool = true;
        }

        public static void UpdateCalculations(Action CalculationFunc)
        {
            while (runBool)
            {
                if (!frameDone && !pauseBool)
                {
                    CalculationFunc();

                    frameDone = true;
                    Thread.Sleep(1000 / cfps);
                }
                else { Thread.Sleep(1000 / cfps); }

            }
        }

        public static async Task RenderLoop(Action RenderFunc)
        {
            while (runBool)
            {
                if (frameDone && !pauseBool)
                {
                    RenderFunc();

                    if (drawTakeScreenshot)
                    {
                        TakeScreenshot($"{pathToScreenshots}screenshot_{DateTime.Now.ToString().Replace(" ", "_").Replace(".", "_").Replace(":", "_")}.png", wb.Clone());
                        drawTakeScreenshot = false;
                    }

                    frameDone = false;
                    await Task.Delay(1000 / fps);
                }
                else { await Task.Delay(1000 / fps); }
            }
        }

        // Rendering
        public static Color MakeColorGradient(double frequency, int phase, double i)
        {
            return Color.FromRgb(
                (byte)(int)(Sin(frequency * i + 0) * 128 + 127),
                (byte)(int)(Sin(frequency * i + phase) * 128 + 127),
                (byte)(int)(Sin(frequency * i + phase * 2) * 128 + 127)
            );
        }

        public static void DrawPixel(WriteableBitmap wb, Point p, Color clr, bool d3 = false)
        {
            if (d3) { p.X += FrameWidth / 2; p.Y = -(p.Y - FrameHeight / 2); }
            if (p.X < 0 || p.X >= FrameWidth || p.Y < 0 || p.Y >= FrameHeight) { return; }

            wb.WritePixels(
                new Int32Rect((int)p.X, (int)p.Y, 1, 1),
                (byte[])[clr.B, clr.G, clr.R, 255],
                FrameWidth * wb.Format.BitsPerPixel / 8,
                0
            );
        }

        public static void DrawRectangle(WriteableBitmap wb, Int32Rect rect, Color clr, bool d3 = false)
        {
            if (rect.Width != 0 && rect.Height != 0)
            {
                if (d3) { rect.X += FrameWidth / 2; rect.Y = -(rect.Y - FrameHeight / 2); }

                Int32Rect orig = new Int32Rect(rect.X, rect.Y, rect.Width, rect.Height);

                while (
                    rect.X < 0 || rect.X > FrameWidth || rect.Y < 0 || rect.Y > FrameHeight ||
                    rect.X + rect.Width > FrameWidth || rect.Y + rect.Height > FrameHeight
                )
                {
                    if (rect.X < 0 && rect.Width > 0) { rect.X++; rect.Width--; }
                    if (rect.X + rect.Width > FrameWidth && rect.Width > 0) { rect.Width--; }
                    if (rect.Width == 0) { break; }

                    if (rect.Y < 0 && rect.Height > 0) { rect.Y++; rect.Height--; }
                    if (rect.Y + rect.Height > FrameHeight && rect.Height > 0) { rect.Height--; }
                    if (rect.Height == 0) { break; }
                };

                int stride = (rect.Width * wb.Format.BitsPerPixel + 7) / 8;
                byte[] buffer = new byte[stride * rect.Height];
                for (int i = 0; i < stride * rect.Height; i += 4)
                {
                    buffer[i + 0] = clr.B;
                    buffer[i + 1] = clr.G;
                    buffer[i + 2] = clr.R;
                    buffer[i + 3] = 255;
                }

                wb.WritePixels(rect, buffer, stride, 0);
            }
        }

        public static void DrawLine(WriteableBitmap wb, Point p1, Point p2, Color clr, bool d3 = false)
        {
            double x1 = p1.X, y1 = p1.Y;
            double x2 = p2.X, y2 = p2.Y;

            double dx = Abs(x2 - x1);
            double dy = Abs(y2 - y1);

            double sx = x1 < x2 ? 1 : -1;
            double sy = y1 < y2 ? 1 : -1;

            double err1 = dx - dy;
            double err2;

            while (true)
            {
                DrawPixel(wb, new Point(x1, y1), clr, d3);

                if (x1 == x2 && y1 == y2) break;

                err2 = 2 * err1;

                if (err2 > -dy)
                {
                    err1 -= dy;
                    x1 += sx;
                }

                if (err2 < dx)
                {
                    err1 += dx;
                    y1 += sy;
                }
            }
        }

        public static void DrawEllipse(WriteableBitmap wb, Int32Rect ellipse, Color clr, bool fill = false)
        {
            int posX = ellipse.X;
            int posY = ellipse.Y;
            int radX = ellipse.Width;
            int radY = ellipse.Height;
            int X = 0;
            int Y = radY;
            double powRadX = radX * radX;
            double powRadY = radY * radY;
            double dX = 2 * powRadY * X;
            double dY = 2 * powRadX * Y;
            double d1;
            double d2;


            if (fill)
            {
                for (int fillY = -radY; fillY <= radY; fillY++)
                {
                    for (int fillX = -radX; fillX <= radX; fillX++)
                    {
                        if (fillX * fillX * radY * radY + fillY * fillY * radX * radX <= radY * radY * radX * radX)
                        {
                            DrawPixel(wb, new Point(posX + fillX, posY + fillY), clr);
                        }
                    }
                }
            }

            d1 = powRadY - powRadX * radY + 0.25f * powRadX;
            while (dX < dY)
            {
                DrawPixel(wb, new Point(posX + X, posY + Y), clr);
                DrawPixel(wb, new Point(posX - X, posY + Y), clr);
                DrawPixel(wb, new Point(posX + X, posY - Y), clr);
                DrawPixel(wb, new Point(posX - X, posY - Y), clr);
                if (d1 < 0)
                {
                    X++;
                    dX += 2 * powRadY;
                    d1 += dX + powRadY;
                }
                else
                {
                    X++;
                    Y--;
                    dX += 2 * powRadY;
                    dY -= 2 * powRadX;
                    d1 += dX - dY + powRadY;
                }
            }

            d2 = powRadY * Pow(X + 0.5f, 2) + powRadX * Pow(Y - 1, 2) - powRadX * powRadY;
            while (Y >= 0)
            {
                DrawPixel(wb, new Point(posX + X, posY + Y), clr);
                DrawPixel(wb, new Point(posX - X, posY + Y), clr);
                DrawPixel(wb, new Point(posX + X, posY - Y), clr);
                DrawPixel(wb, new Point(posX - X, posY - Y), clr);
                if (d2 > 0)
                {
                    Y--;
                    dY -= 2 * powRadX;
                    d2 += powRadX - dY;
                }
                else
                {
                    Y--;
                    X++;
                    dX += 2 * powRadY;
                    dY -= 2 * powRadX;
                    d2 += dX - dY + powRadX;
                }
            }
        }

        public static void TakeScreenshot(string path, BitmapSource image)
        {
            if (path != string.Empty)
            {
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(stream);
                }
            }
        }

        public static void DrawPolygonFill(WriteableBitmap wb, Point[] points, Color clr, bool d3 = false)
        {
            int minY = (int)points.Min(p => p.Y);
            int maxY = (int)points.Max(p => p.Y);

            for (int y = minY; y <= maxY; y++)
            {
                List<int> intersections = new List<int>();

                for (int i = 0; i < points.Length; i++)
                {
                    int j = (i + 1) % points.Length;
                    if ((points[i].Y <= y && points[j].Y > y) || (points[j].Y <= y && points[i].Y > y))
                    {
                        double factor = (double)(y - points[i].Y) / (points[j].Y - points[i].Y);
                        intersections.Add((int)(points[i].X + factor * (points[j].X - points[i].X)));
                    }
                }
                intersections.Sort();
                for (int i = 0; i < intersections.Count; i += 2)
                {
                    DrawRectangle(
                        wb,
                        new Int32Rect(intersections[i], y, Abs(intersections[i + 1]+1 - intersections[i]), 1),
                        clr,
                        d3
                    );
                    //DrawLine(wb, new Point(intersections[i], y), new Point(intersections[i + 1], y), clr, d3);
                    /*for (int x = intersections[i]; x < intersections[i + 1]; x++)
                    {
                        DrawPixel(wb, new Point(x, y), clr, d3);
                    }*/
                }
            }
        }

        public class PColors()
        {
            public static Color AliceBlue = Color.FromRgb(240,248,255);
            public static Color AntiqueWhite = Color.FromRgb(250,235,215);
            public static Color Aqua = Color.FromRgb(0,255,255);
            public static Color Aquamarine = Color.FromRgb(127,255,212);
            public static Color Azure = Color.FromRgb(240,255,255);
            public static Color Beige = Color.FromRgb(245,245,220);
            public static Color Bisque = Color.FromRgb(255,228,196);
            public static Color Black = Color.FromRgb(0,0,0);
            public static Color BlanchedAlmond = Color.FromRgb(255,235,205);
            public static Color Blue = Color.FromRgb(0,0,255);
            public static Color BlueViolet = Color.FromRgb(138,43,226);
            public static Color Brown = Color.FromRgb(165,42,42);
            public static Color BurlyWood = Color.FromRgb(222,184,135);
            public static Color CadetBlue = Color.FromRgb(95,158,160);
            public static Color Chartreuse = Color.FromRgb(127,255,0);
            public static Color Chocolate = Color.FromRgb(210,105,30);
            public static Color Coral = Color.FromRgb(255,127,80);
            public static Color CornflowerBlue = Color.FromRgb(100,149,237);
            public static Color Cornsilk = Color.FromRgb(255,248,220);
            public static Color Crimson = Color.FromRgb(220,20,60);
            public static Color Cyan = Color.FromRgb(0,255,255);
            public static Color DarkBlue = Color.FromRgb(0,0,139);
            public static Color DarkCyan = Color.FromRgb(0,139,139);
            public static Color DarkGoldenRod = Color.FromRgb(184,134,11);
            public static Color DarkGray = Color.FromRgb(169,169,169);
            public static Color DarkGrey = Color.FromRgb(169,169,169);
            public static Color DarkGreen = Color.FromRgb(0,100,0);
            public static Color DarkKhaki = Color.FromRgb(189,183,107);
            public static Color DarkMagenta = Color.FromRgb(139,0,139);
            public static Color DarkOliveGreen = Color.FromRgb(85,107,47);
            public static Color DarkOrange = Color.FromRgb(255,140,0);
            public static Color DarkOrchid = Color.FromRgb(153,50,204);
            public static Color DarkRed = Color.FromRgb(139,0,0);
            public static Color DarkSalmon = Color.FromRgb(233,150,122);
            public static Color DarkSeaGreen = Color.FromRgb(143,188,143);
            public static Color DarkSlateBlue = Color.FromRgb(72,61,139);
            public static Color DarkSlateGray = Color.FromRgb(47,79,79);
            public static Color DarkSlateGrey = Color.FromRgb(47,79,79);
            public static Color DarkTurquoise = Color.FromRgb(0,206,209);
            public static Color DarkViolet = Color.FromRgb(148,0,211);
            public static Color DeepPink = Color.FromRgb(255,20,147);
            public static Color DeepSkyBlue = Color.FromRgb(0,191,255);
            public static Color DimGray = Color.FromRgb(105,105,105);
            public static Color DimGrey = Color.FromRgb(105,105,105);
            public static Color DodgerBlue = Color.FromRgb(30,144,255);
            public static Color FireBrick = Color.FromRgb(178,34,34);
            public static Color FloralWhite = Color.FromRgb(255,250,240);
            public static Color ForestGreen = Color.FromRgb(34,139,34);
            public static Color Fuchsia = Color.FromRgb(255,0,255);
            public static Color Gainsboro = Color.FromRgb(220,220,220);
            public static Color GhostWhite = Color.FromRgb(248,248,255);
            public static Color Gold = Color.FromRgb(255,215,0);
            public static Color GoldenRod = Color.FromRgb(218,165,32);
            public static Color Gray = Color.FromRgb(128,128,128);
            public static Color Grey = Color.FromRgb(128,128,128);
            public static Color Green = Color.FromRgb(0,128,0);
            public static Color GreenYellow = Color.FromRgb(173,255,47);
            public static Color HoneyDew = Color.FromRgb(240,255,240);
            public static Color HotPink = Color.FromRgb(255,105,180);
            public static Color IndianRed = Color.FromRgb(205,92,92);
            public static Color Indigo = Color.FromRgb(75,0,130);
            public static Color Ivory = Color.FromRgb(255,255,240);
            public static Color Khaki = Color.FromRgb(240,230,140);
            public static Color Lavender = Color.FromRgb(230,230,250);
            public static Color LavenderBlush = Color.FromRgb(255,240,245);
            public static Color LawnGreen = Color.FromRgb(124,252,0);
            public static Color LemonChiffon = Color.FromRgb(255,250,205);
            public static Color LightBlue = Color.FromRgb(173,216,230);
            public static Color LightCoral = Color.FromRgb(240,128,128);
            public static Color LightCyan = Color.FromRgb(224,255,255);
            public static Color LightGoldenRodYellow = Color.FromRgb(250,250,210);
            public static Color LightGray = Color.FromRgb(211,211,211);
            public static Color LightGrey = Color.FromRgb(211,211,211);
            public static Color LightGreen = Color.FromRgb(144,238,144);
            public static Color LightPink = Color.FromRgb(255,182,193);
            public static Color LightSalmon = Color.FromRgb(255,160,122);
            public static Color LightSeaGreen = Color.FromRgb(32,178,170);
            public static Color LightSkyBlue = Color.FromRgb(135,206,250);
            public static Color LightSlateGray = Color.FromRgb(119,136,153);
            public static Color LightSlateGrey = Color.FromRgb(119,136,153);
            public static Color LightSteelBlue = Color.FromRgb(176,196,222);
            public static Color LightYellow = Color.FromRgb(255,255,224);
            public static Color Lime = Color.FromRgb(0,255,0);
            public static Color LimeGreen = Color.FromRgb(50,205,50);
            public static Color Linen = Color.FromRgb(250,240,230);
            public static Color Magenta = Color.FromRgb(255,0,255);
            public static Color Maroon = Color.FromRgb(128,0,0);
            public static Color MediumAquaMarine = Color.FromRgb(102,205,170);
            public static Color MediumBlue = Color.FromRgb(0,0,205);
            public static Color MediumOrchid = Color.FromRgb(186,85,211);
            public static Color MediumPurple = Color.FromRgb(147,112,219);
            public static Color MediumSeaGreen = Color.FromRgb(60,179,113);
            public static Color MediumSlateBlue = Color.FromRgb(123,104,238);
            public static Color MediumSpringGreen = Color.FromRgb(0,250,154);
            public static Color MediumTurquoise = Color.FromRgb(72,209,204);
            public static Color MediumVioletRed = Color.FromRgb(199,21,133);
            public static Color MidnightBlue = Color.FromRgb(25,25,112);
            public static Color MintCream = Color.FromRgb(245,255,250);
            public static Color MistyRose = Color.FromRgb(255,228,225);
            public static Color Moccasin = Color.FromRgb(255,228,181);
            public static Color NavajoWhite = Color.FromRgb(255,222,173);
            public static Color Navy = Color.FromRgb(0,0,128);
            public static Color OldLace = Color.FromRgb(253,245,230);
            public static Color Olive = Color.FromRgb(128,128,0);
            public static Color OliveDrab = Color.FromRgb(107,142,35);
            public static Color Orange = Color.FromRgb(255,165,0);
            public static Color OrangeRed = Color.FromRgb(255,69,0);
            public static Color Orchid = Color.FromRgb(218,112,214);
            public static Color PaleGoldenRod = Color.FromRgb(238,232,170);
            public static Color PaleGreen = Color.FromRgb(152,251,152);
            public static Color PaleTurquoise = Color.FromRgb(175,238,238);
            public static Color PaleVioletRed = Color.FromRgb(219,112,147);
            public static Color PapayaWhip = Color.FromRgb(255,239,213);
            public static Color PeachPuff = Color.FromRgb(255,218,185);
            public static Color Peru = Color.FromRgb(205,133,63);
            public static Color Pink = Color.FromRgb(255,192,203);
            public static Color Plum = Color.FromRgb(221,160,221);
            public static Color PowderBlue = Color.FromRgb(176,224,230);
            public static Color Purple = Color.FromRgb(128,0,128);
            public static Color RebeccaPurple = Color.FromRgb(102,51,153);
            public static Color Red = Color.FromRgb(255,0,0);
            public static Color RosyBrown = Color.FromRgb(188,143,143);
            public static Color RoyalBlue = Color.FromRgb(65,105,225);
            public static Color SaddleBrown = Color.FromRgb(139,69,19);
            public static Color Salmon = Color.FromRgb(250,128,114);
            public static Color SandyBrown = Color.FromRgb(244,164,96);
            public static Color SeaGreen = Color.FromRgb(46,139,87);
            public static Color SeaShell = Color.FromRgb(255,245,238);
            public static Color Sienna = Color.FromRgb(160,82,45);
            public static Color Silver = Color.FromRgb(192,192,192);
            public static Color SkyBlue = Color.FromRgb(135,206,235);
            public static Color SlateBlue = Color.FromRgb(106,90,205);
            public static Color SlateGray = Color.FromRgb(112,128,144);
            public static Color SlateGrey = Color.FromRgb(112,128,144);
            public static Color Snow = Color.FromRgb(255,250,250);
            public static Color SpringGreen = Color.FromRgb(0,255,127);
            public static Color SteelBlue = Color.FromRgb(70,130,180);
            public static Color Tan = Color.FromRgb(210,180,140);
            public static Color Teal = Color.FromRgb(0,128,128);
            public static Color Thistle = Color.FromRgb(216,191,216);
            public static Color Tomato = Color.FromRgb(255,99,71);
            public static Color Turquoise = Color.FromRgb(64,224,208);
            public static Color Violet = Color.FromRgb(238,130,238);
            public static Color Wheat = Color.FromRgb(245,222,179);
            public static Color White = Color.FromRgb(255,255,255);
            public static Color WhiteSmoke = Color.FromRgb(245,245,245);
            public static Color Yellow = Color.FromRgb(255,255,0);
            public static Color YellowGreen = Color.FromRgb(154,205,50);
        }

        public class Vector3D(double _X, double _Y, double _Z, double _W = 1)
        {
            public double X { get; set; } = _X;
            public double Y { get; set; } = _Y;
            public double Z { get; set; } = _Z;
            public double W { get; set; } = _W;

            override public string ToString()
            {
                return $"[{X};{Y};{Z};{W}]";
            }

            public double Len() { return Sqrt(X * X + Y * Y + Z * Z); }
            public Vector3D Norm() { double LenMem = Len(); X /= LenMem; Y /= LenMem; Z /= LenMem; return new Vector3D(X, Y, Z); }
            public double DistanceTo(Vector3D p) { return Math.Sqrt(Math.Pow(X - p.X, 2) + Math.Pow(Y - p.Y, 2) + Math.Pow(Z - p.Z, 2)); }

            public static Vector3D operator +(Vector3D left, Vector3D right)
            {
                return new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
            }
            public static Vector3D operator -(Vector3D left, Vector3D right)
            {
                return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
            }
            public static Vector3D operator *(Vector3D left, Vector3D right)
            {
                return new Vector3D(left.Y * right.Z - left.Z * right.Y, left.Z * right.X - left.X * right.Z, left.X * right.Y - left.Y * right.X);
            }
            public static Vector3D operator *(Vector3D left, double right)
            {
                return new Vector3D(left.X * right, left.Y * right, left.Z * right);
            }
            public static Vector3D operator *(Vector3D left, Matrix4x4 right)
            {
                return new Vector3D(
                    right.Matrix[0, 0] * left.X + right.Matrix[0, 1] * left.Y + right.Matrix[0, 2] * left.Z + right.Matrix[0, 3] * left.W,
                    right.Matrix[1, 0] * left.X + right.Matrix[1, 1] * left.Y + right.Matrix[1, 2] * left.Z + right.Matrix[1, 3] * left.W,
                    right.Matrix[2, 0] * left.X + right.Matrix[2, 1] * left.Y + right.Matrix[2, 2] * left.Z + right.Matrix[2, 3] * left.W,
                    right.Matrix[3, 0] * left.X + right.Matrix[3, 1] * left.Y + right.Matrix[3, 2] * left.Z + right.Matrix[3, 3] * left.W
                    );
            }
            public static double ScalarProduct(Vector3D left, Vector3D right)
            {
                return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
            }
        }

        public class Matrix4x4(double[,] _Matrix)
        {
            public double[,] Matrix { get; set; } = _Matrix;

            public static Matrix4x4 GetTranslation(double dx, double dy, double dz)
            {
                return new Matrix4x4(new double[4, 4]
                {
                    { 1, 0, 0, dx },
                    { 0, 1, 0, dy },
                    { 0, 0, 1, dz },
                    { 0, 0, 0, 1 }
                });
            }

            public static Matrix4x4 GetScale(double sx, double sy, double sz)
            {
                return new Matrix4x4(new double[4, 4]
                {
                    { sx, 0, 0, 0 },
                    { 0, sy, 0, 0 },
                    { 0, 0, sz, 0 },
                    { 0, 0, 0, 1 }
                });
            }

            public static Matrix4x4 GetRotationX(double a)
            {
                double r = PI / 180 * a;
                return new Matrix4x4(new double[4, 4]
                {
                    { 1, 0, 0, 0 },
                    { 0, Cos(r), -Sin(r), 0 },
                    { 0, Sin(r), Cos(r), 0 },
                    { 0, 0, 0, 1 }
                });
            }
            public static Matrix4x4 GetRotationY(double a)
            {
                double r = PI / 180 * a;
                return new Matrix4x4(new double[4, 4]
                {
                    { Cos(r), 0, Sin(r), 0 },
                    { 0, 1, 0, 0 },
                    { -Sin(r), 0, Cos(r), 0 },
                    { 0, 0, 0, 1 }
                });
            }
            public static Matrix4x4 GetRotationZ(double a)
            {
                double r = PI / 180 * a;
                return new Matrix4x4(new double[4, 4]
                {
                    { Cos(r), -Sin(r), 0, 0 },
                    { Sin(r), Cos(r), 0, 0 },
                    { 0, 0, 1, 0 },
                    { 0, 0, 0, 1 }
                });
            }

            public static Matrix4x4 GetLookAt(Vector3D eye, Vector3D target, Vector3D up)
            {
                Vector3D vz = (eye - target).Norm();
                Vector3D vx = (up * vz).Norm();
                Vector3D vy = (vz * vx).Norm();

                return Matrix4x4.GetTranslation(-eye.X, -eye.Y, -eye.Z) * new Matrix4x4(new double[,] {
                    { vx.X, vx.Y, vx.Z, 0 },
                    { vy.X, vy.Y, vy.Z, 0 },
                    { vz.X, vz.Y, vz.Z, 0 },
                    { 0, 0, 0, 1}
                });
            }

            public static Matrix4x4 getPerspectiveProj(double fovy, double aspect, double n, double f)
            {
                double radians = PI / 180 * fovy;
                double sx = (1 / Tan(radians / 2)) / aspect;
                double sy = (1 / Tan(radians / 2));
                double sz = (f + n) / (f - n);
                double dz = (-2 * f * n) / (f - n);

                return new Matrix4x4(new double[,] {
                    { sx, 0, 0, 0 },
                    { 0, sy, 0, 0 },
                    { 0, 0, sz, dz },
                    { 0, 0, -1, 0 }
                });
            }

            public static Matrix4x4 operator *(Matrix4x4 left, Matrix4x4 right)
            {
                double[,] MatLeft = left.Matrix;

                double[,] MatRight = right.Matrix;

                double[,] MatMult = new double[4, 4] {
                    { 0, 0, 0, 0 },
                    { 0, 0, 0, 0 },
                    { 0, 0, 0, 0 },
                    { 0, 0, 0, 0 }
                };

                MatMult[0, 0] = MatLeft[0, 0] * MatRight[0, 0] + MatLeft[0, 1] * MatRight[1, 0] + MatLeft[0, 2] * MatRight[2, 0] + MatLeft[0, 3] * MatRight[3, 0];
                MatMult[0, 1] = MatLeft[0, 0] * MatRight[0, 1] + MatLeft[0, 1] * MatRight[1, 1] + MatLeft[0, 2] * MatRight[2, 1] + MatLeft[0, 3] * MatRight[3, 1];
                MatMult[0, 2] = MatLeft[0, 0] * MatRight[0, 2] + MatLeft[0, 1] * MatRight[1, 2] + MatLeft[0, 2] * MatRight[2, 2] + MatLeft[0, 3] * MatRight[3, 2];
                MatMult[0, 3] = MatLeft[0, 0] * MatRight[0, 3] + MatLeft[0, 1] * MatRight[1, 3] + MatLeft[0, 2] * MatRight[2, 3] + MatLeft[0, 3] * MatRight[3, 3];

                MatMult[1, 0] = MatLeft[1, 0] * MatRight[0, 0] + MatLeft[1, 1] * MatRight[1, 0] + MatLeft[1, 2] * MatRight[2, 0] + MatLeft[1, 3] * MatRight[3, 0];
                MatMult[1, 1] = MatLeft[1, 0] * MatRight[0, 1] + MatLeft[1, 1] * MatRight[1, 1] + MatLeft[1, 2] * MatRight[2, 1] + MatLeft[1, 3] * MatRight[3, 1];
                MatMult[1, 2] = MatLeft[1, 0] * MatRight[0, 2] + MatLeft[1, 1] * MatRight[1, 2] + MatLeft[1, 2] * MatRight[2, 2] + MatLeft[1, 3] * MatRight[3, 2];
                MatMult[1, 3] = MatLeft[1, 0] * MatRight[0, 3] + MatLeft[1, 1] * MatRight[1, 3] + MatLeft[1, 2] * MatRight[2, 3] + MatLeft[1, 3] * MatRight[3, 3];

                MatMult[2, 0] = MatLeft[2, 0] * MatRight[0, 0] + MatLeft[2, 1] * MatRight[1, 0] + MatLeft[2, 2] * MatRight[2, 0] + MatLeft[2, 3] * MatRight[3, 0];
                MatMult[2, 1] = MatLeft[2, 0] * MatRight[0, 1] + MatLeft[2, 1] * MatRight[1, 1] + MatLeft[2, 2] * MatRight[2, 1] + MatLeft[2, 3] * MatRight[3, 1];
                MatMult[2, 2] = MatLeft[2, 0] * MatRight[0, 2] + MatLeft[2, 1] * MatRight[1, 2] + MatLeft[2, 2] * MatRight[2, 2] + MatLeft[2, 3] * MatRight[3, 2];
                MatMult[2, 3] = MatLeft[2, 0] * MatRight[0, 3] + MatLeft[2, 1] * MatRight[1, 3] + MatLeft[2, 2] * MatRight[2, 3] + MatLeft[2, 3] * MatRight[3, 3];

                MatMult[3, 0] = MatLeft[3, 0] * MatRight[0, 0] + MatLeft[3, 1] * MatRight[1, 0] + MatLeft[3, 2] * MatRight[2, 0] + MatLeft[3, 3] * MatRight[3, 0];
                MatMult[3, 1] = MatLeft[3, 0] * MatRight[0, 1] + MatLeft[3, 1] * MatRight[1, 1] + MatLeft[3, 2] * MatRight[2, 1] + MatLeft[3, 3] * MatRight[3, 1];
                MatMult[3, 2] = MatLeft[3, 0] * MatRight[0, 2] + MatLeft[3, 1] * MatRight[1, 2] + MatLeft[3, 2] * MatRight[2, 2] + MatLeft[3, 3] * MatRight[3, 2];
                MatMult[3, 3] = MatLeft[3, 0] * MatRight[0, 3] + MatLeft[3, 1] * MatRight[1, 3] + MatLeft[3, 2] * MatRight[2, 3] + MatLeft[3, 3] * MatRight[3, 3];

                return new Matrix4x4(MatMult);
            }

            override public string ToString()
            {
                string str = $"";
                for (int i = 0; i < 4; i++)
                {
                    str += "[";
                    for (int j = 0; j < 4; j++)
                    {
                        str += j != 3 ? $"{Matrix[i, j]}," : $"{Matrix[i, j]}";
                    }
                    str += "]\n";
                }
                str += "]";

                return str;
            }
        }
    }
}

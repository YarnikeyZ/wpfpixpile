using System;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Math;

namespace wpfpixpile
{
    public class Draw
    {
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
            if (d3) { p.X += wb.PixelWidth / 2; p.Y = -(p.Y - wb.PixelHeight / 2); }
            if (p.X < 0 || p.X >= wb.PixelWidth || p.Y < 0 || p.Y >= wb.PixelHeight) { return; }

            wb.WritePixels(
                new Int32Rect((int)p.X, (int)p.Y, 1, 1),
                (byte[])[clr.B, clr.G, clr.R, 255],
                wb.PixelWidth * wb.Format.BitsPerPixel / 8,
                0
            );
        }

        public static void DrawRectangle(WriteableBitmap wb, Int32Rect rect, Color clr, bool d3 = false)
        {
            if (rect.Width != 0 && rect.Height != 0)
            {
                if (d3) { rect.X += wb.PixelWidth / 2; rect.Y = -(rect.Y - wb.PixelHeight / 2); }

                Int32Rect orig = new Int32Rect(rect.X, rect.Y, rect.Width, rect.Height);

                while (
                    rect.X < 0 || rect.X > wb.PixelWidth || rect.Y < 0 || rect.Y > wb.PixelHeight ||
                    rect.X + rect.Width > wb.PixelWidth || rect.Y + rect.Height > wb.PixelHeight
                )
                {
                    if (rect.X < 0 && rect.Width > 0) { rect.X++; rect.Width--; }
                    if (rect.X + rect.Width > wb.PixelWidth && rect.Width > 0) { rect.Width--; }
                    if (rect.Width == 0) { break; }

                    if (rect.Y < 0 && rect.Height > 0) { rect.Y++; rect.Height--; }
                    if (rect.Y + rect.Height > wb.PixelHeight && rect.Height > 0) { rect.Height--; }
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

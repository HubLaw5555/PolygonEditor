using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;

namespace PolygonEditor
{
    public struct PolarPoint
    {
        public double R;
        public double Sin;
        public double Cos;
        public Point BasePoint => new Point(R * Cos, R * Sin);

        public PolarPoint(Point pt)
        {
            R = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
            Sin = pt.Y / R;
            Cos = pt.X / R;
        }
    }

    public struct Line
    {
        public double A, B, C;

        public Line(Point f, Point t)
        {
            double denominator = t.X - f.X;

            A = t.Y - f.Y;
            B = -denominator;
            C = denominator * f.Y - A * f.X;
        }

        public Line OrthogonalOnPoint(Point pt)
        {
            Line line = new Line();
            line.A = -B;
            line.B = A;
            line.C = B * pt.X - A * pt.Y;
            return line;
        }

        public Vector GetLineDirection()
        {
            if(B == 0)
            {
                return new Vector(0, 1);
            }
            Vector v = new Vector(1, 1);
            v.Y *= -A / B;
            v /= v.Length;
            return v;
        }
    }

    public static class LetterConverter
    {
        private const string letters = "ABCDEFGHIJKLMNOPQRSTUWXYZ";
        private static readonly int len = letters.Length;

        public static string Convert(int inputNum)
        {
            if (inputNum < 1)
            {
                return "";
            }

            int nr = (inputNum - 1) / 25;
            string s = letters[(inputNum - 1) % 25].ToString();

            if (nr > 0)
            {
                s += nr.ToString();
            }

            return s;
        }
    }

    public static class Geometry
    {
        public static double Distance(Point l, Point r)
        {
            return Math.Sqrt(Math.Pow((l.X - r.X), 2) + Math.Pow((r.Y - l.Y), 2)); ;
        }

        public static double DistSquared(Point l, Point r)
        {
            return Math.Pow((l.X - r.X), 2) + Math.Pow((r.Y - l.Y), 2);
        }

        // metoda z:
        // https://stackoverflow.com/questions/17692922/check-is-a-point-x-y-is-between-two-points-drawn-on-a-straight-line
        public static bool IsOnLine(Point A, Point B, Point C, double tolerance)
        {
            double minX = Math.Min(A.X, B.X) - tolerance;
            double maxX = Math.Max(A.X, B.X) + tolerance;
            double minY = Math.Min(A.Y, B.Y) - tolerance;
            double maxY = Math.Max(A.Y, B.Y) + tolerance;

            if (C.X >= maxX || C.X <= minX || C.Y <= minY || C.Y >= maxY)
            {
                return false;
            }

            if (A.X == B.X)
            {
                if (Math.Abs(A.X - C.X) >= tolerance)
                {
                    return false;
                }
                return true;
            }

            if (A.Y == B.Y)
            {
                if (Math.Abs(A.Y - C.Y) >= tolerance)
                {
                    return false;
                }
                return true;
            }

            double distFromLine = Math.Abs(((B.X - A.X) * (A.Y - C.Y)) - ((A.X - C.X) * (B.Y - A.Y)))
                / Math.Sqrt((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));

            if (distFromLine >= tolerance)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public static class WriteableBitmapExtender
    {
        public static WriteableBitmap CreateWritableBitmap(int width, int height, Color color)
        {
            WriteableBitmap image = new WriteableBitmap(
                width,
                height,
                96,
                96,
                PixelFormats.Bgr32,
                null);
            image.Fill(color);

            return image;
        }

        public static void Fill(this WriteableBitmap bitmap, Color color)
        {

            try
            {
                bitmap.Lock();

                unsafe
                {
                    IntPtr pBackBuffer = bitmap.BackBuffer;

                    int color_data = color.R << 16; // R
                    color_data |= color.G << 8;   // G
                    color_data |= color.B << 0;   // B
                    for (int i = 0; i < bitmap.PixelWidth; ++i)
                        for(int j = 0; j < bitmap.PixelHeight; ++j)
                        {
                            *((int*)pBackBuffer) = color_data;
                            pBackBuffer += 4;
                        }
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                }

            }
            finally
            {
                bitmap.Unlock();
            }
        }

        public static void DrawPixel(this WriteableBitmap bitmap, int x, int y, Color color)
        {
            try
            {
                bitmap.Lock();

                if (x < 0 || y < 0 || x >= bitmap.PixelWidth || y >= bitmap.PixelHeight) return;


                unsafe
                {
                    IntPtr pBackBuffer = bitmap.BackBuffer;
                    pBackBuffer += y * bitmap.BackBufferStride;
                    pBackBuffer += x * 4;

                    int color_data = color.R << 16; // R
                    color_data |= color.G << 8;   // G
                    color_data |= color.B << 0;   // B

                    *((int*)pBackBuffer) = color_data;
                }

                bitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        public static void DrawRect(this WriteableBitmap bitmap, int x, int y,  int width, int height, Color color)
        {
            for (int i = 0; i < width; ++i)
            {
                for(int j = 0; j < height; ++j)
                {
                    bitmap.DrawPixel(x + i, y + j, color);
                }
            }
        }

        public static void DrawLine(this WriteableBitmap bitmap, Point from, Point to, int size, Color? color = null)
        {
            bitmap.DrawLine((int)from.X, (int)from.Y, (int)to.X, (int)to.Y, size, color);
        }

        // metoda wykorzystująca algorytm z:
        // https://pl.wikipedia.org/wiki/Algorytm_Bresenhama
        public static void DrawLine(this WriteableBitmap bitmap, int x1, int y1, int x2, int y2, int size, Color? color = null)
        {
            if(color == null)
            {
                color = Colors.White;
            }
            Color c = (Color)color;

            int d, dx, dy, ai, bi, xi, yi;
            int x = x1, y = y1;
            if (x1 < x2)
            {
                xi = 1;
                dx = x2 - x1;
            }
            else
            {
                xi = -1;
                dx = x1 - x2;
            }
            if (y1 < y2)
            {
                yi = 1;
                dy = y2 - y1;
            }
            else
            {
                yi = -1;
                dy = y1 - y2;
            }
            bitmap.DrawRect(x, y, size, size, c);

            if (dx > dy)
            {
                ai = (dy - dx) * 2;
                bi = dy * 2;
                d = bi - dx;
                while (x != x2)
                {
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        x += xi;
                    }
                    bitmap.DrawRect(x, y, size, size, c);
                }
            }
            else
            {
                ai = (dx - dy) * 2;
                bi = dx * 2;
                d = bi - dy;
                while (y != y2)
                {
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        y += yi;
                    }
                    bitmap.DrawRect(x, y, size, size, c);
                }
            }
        }
    }

    public class VerticesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string name = values[0].ToString();
            int x = int.Parse(values[1].ToString());
            int y = int.Parse(values[2].ToString());

            return $"Wierzchołek {name}: ({x},{y})";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception();
        }
    }

    public class EdgesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string l = values[0].ToString();
            string r = values[1].ToString();
            int x1 = int.Parse(values[2].ToString());
            int x2 = int.Parse(values[3].ToString());
            int y1 = int.Parse(values[4].ToString());
            int y2 = int.Parse(values[5].ToString());
            string len = string.Format("{0:00.##}", Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)));
            return $"|{l} {r}| =  {len}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception();
        }
    }

    public class RelationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PolygonEdge edge = (PolygonEdge)values[0];
            if (edge is Edge) return null;

            string communicat = edge is FixedLenghtEdge ? "FIX_L" : $"ORTH {(edge as OrtogonalEdge).UniqueNr}";

            return $"|{edge.leftVertex.VertexText}  {edge.rightVertex.VertexText}|   " + communicat;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception();
        }
    }
}

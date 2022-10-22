using System;
using System.Windows;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Runtime.InteropServices;

namespace PolygonEditor
{
    public static class PixelHelper
    {
        [DllImport("gdi32")]
        private static extern int GetPixel(int hdc, int nXPos, int nYPos);

        [DllImport("user32")]
        private static extern int GetWindowDC(int hwnd);

        [DllImport("user32")]
        private static extern int ReleaseDC(int hWnd, int hDC);

        public static SolidColorBrush GetPixelColor(Point point)
        {
            int lDC = GetWindowDC(0);
            int intColor = GetPixel(lDC, (int)point.X, (int)point.Y);

            // Release the DC after getting the Color.
            ReleaseDC(0, lDC);

            byte a = (byte)((intColor >> 0x18) & 0xffL);
            byte b = (byte)((intColor >> 0x10) & 0xffL);
            byte g = (byte)((intColor >> 8) & 0xffL);
            byte r = (byte)(intColor & 0xffL);
            Color color = Color.FromRgb(r, g, b);
            return new SolidColorBrush(color);
        }
    }

    public static class LetterConverter
    {
        private const string letters = "ABCDEFGHIJKLMNOPQRSTUWXYZ";
        private static readonly int len = letters.Length;

        public static string Convert(int inputNum)
        {
            if(inputNum < 1)
            {
                return "";
            }

            int nr = (inputNum - 1) / 25;
            string s = letters[(inputNum - 1) % 25].ToString();
            //inputNum -= nr > 0 ? nr * 25 : 0;
            if(nr > 0)
            {
                s += nr.ToString();
            }
            //while (inputNum > 0)
            //{
            //    s += letters[inputNum % 25 - 1];
            //    inputNum /= 26;
            //}

            //char[] res = s.ToCharArray();
            //Array.Reverse(res);
            //return new String(res);

            return s;
        }
    }

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
    }

    public static class CanvasExtender
    {
        public static unsafe Color GetPixelColor(this WriteableBitmap bitmap, int x, int y)
        {
            var pix = new Color();
            byte[] ColorData = { 0, 0, 0, 0 }; // NOTE, results comes in BGRA order! 
            IntPtr pBackBuffer = bitmap.BackBuffer;
            byte* pBuff = (byte*)pBackBuffer.ToPointer();
            var b = pBuff[4 * x + (y * bitmap.BackBufferStride)];
            var g = pBuff[4 * x + (y * bitmap.BackBufferStride) + 1];
            var r = pBuff[4 * x + (y * bitmap.BackBufferStride) + 2];
            var a = pBuff[4 * x + (y * bitmap.BackBufferStride) + 3];
            pix.R = r;
            pix.G = g;
            pix.B = b;
            pix.A = a;
            return pix;
        }


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

        public static Stack<Point> DrawLine(this WriteableBitmap bitmap, Point from, Point to, int size, Color? color = null)
        {
            return bitmap.DrawLine((int)from.X, (int)from.Y, (int)to.X, (int)to.Y, size, color);
        }

        public static Stack<Point> DrawLine(this WriteableBitmap bitmap, int x1, int y1, int x2, int y2, int size, Color? color = null)
        {
            if(color == null)
            {
                color = Colors.White;
            }
            Color c = (Color)color;
            Stack<Point> stack = new Stack<Point>();

            // zmienne pomocnicze
            int d, dx, dy, ai, bi, xi, yi;
            int x = x1, y = y1;
            // ustalenie kierunku rysowania
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
            // ustalenie kierunku rysowania
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
            // pierwszy piksel
            bitmap.DrawRect(x, y, size, size, c);
            //stack.Push(new Point(x,y));

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    stack.Push(new Point(x + i, y + j));
                }
            }

            // oś wiodąca OX
            if (dx > dy)
            {
                ai = (dy - dx) * 2;
                bi = dy * 2;
                d = bi - dx;
                // pętla po kolejnych x
                while (x != x2)
                {
                    // test współczynnika
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

                    for (int i = 0; i < size; ++i)
                    {
                        for (int j = 0; j < size; ++j)
                        {
                            stack.Push(new Point(x + i, y + j));
                        }
                    }
                }
            }
            // oś wiodąca OY
            else
            {
                ai = (dx - dy) * 2;
                bi = dx * 2;
                d = bi - dy;
                // pętla po kolejnych y
                while (y != y2)
                {
                    // test współczynnika
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

                    for (int i = 0; i < size; ++i)
                    {
                        for (int j = 0; j < size; ++j)
                        {
                            stack.Push(new Point(x + i, y + j));
                        }
                    }
                }
            }

            return stack;
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
            //string len = string.Format("{0:0.##}", Math.Sqrt(Math.Pow((l.X - r.X), 2) + Math.Pow((l.Y - r.Y), 2)));
            return $"|{l} {r}| =  {len}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception();
        }
    }
}

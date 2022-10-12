using System;
using System.Windows;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonEditor
{
    public static class CanvasExtender
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

        public static void DrawLine(this WriteableBitmap bitmap, Point from, Point to, Color? color = null)
        {
            bitmap.DrawLine((int)from.X, (int)from.Y, (int)to.X, (int)to.Y, color);
        }

        public static void DrawLine(this WriteableBitmap bitmap, int x1, int y1, int x2, int y2, Color? color = null)
        {
            if(color == null)
            {
                color = Colors.Black;
            }
            Color c = (Color)color;


            int m_new = 2 * (y2 - y1);
            int slope_error_new = m_new - (x2 - x1);

            for (int x = x1, y = y1; x <= x2; x++)
            {
                bitmap.DrawPixel(x, y, c);

                slope_error_new += m_new;

                if (slope_error_new >= 0) {
                    y++;
                    slope_error_new -= 2 * (x2 - x1);
                }
            }
        }
    }

}

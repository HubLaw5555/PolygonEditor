using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace PolygonEditor
{
    public interface DrawingObject
    {
        void DrawLine(WriteableBitmap bitmap, Point from, Point to, int size, Color? color = null);
        WriteableBitmap Clear(WriteableBitmap bitmap, int width, int height, Color color);
    }

    public class BresenhamLineDrawing: DrawingObject
    {
        public void DrawLine(WriteableBitmap bitmap, Point from, Point to, int size, Color? color = null)
        {
            bitmap.DrawLine(from, to, size, color);
        }

        public WriteableBitmap Clear(WriteableBitmap bitmap, int width, int height, Color color)
        {
            return CanvasExtender.CreateWritableBitmap(width, height, color);
        }
    }

    public class LibraryLineDrawing: DrawingObject
    {
        List<System.Windows.Shapes.Line> lines = new List<System.Windows.Shapes.Line>();

        public void DrawLine(WriteableBitmap bitmap, Point from, Point to, int size, Color? color = null)
        {
            System.Windows.Shapes.Line line = new System.Windows.Shapes.Line();
            lines.Add(line);
            line.Stroke = new SolidColorBrush(color != null ? (Color)color : Colors.Black);
            line.X1 = from.X;
            line.Y1 = from.Y;
            line.X2 = to.X;
            line.Y2 = to.Y;
            line.StrokeThickness = size;
            (Application.Current.MainWindow as MainWindow).canvas.Children.Add(line);
        }

        public WriteableBitmap Clear(WriteableBitmap bitmap, int width, int height, Color color)
        {
            foreach(var line in lines)
            {
                if ((Application.Current.MainWindow as MainWindow).canvas.Children.Contains(line))
                    (Application.Current.MainWindow as MainWindow).canvas.Children.Remove(line);
            }
            lines.Clear();
            return bitmap;
        }
    }
}

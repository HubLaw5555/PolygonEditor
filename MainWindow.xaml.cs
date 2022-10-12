using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PolygonEditor
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel model;
        public MainWindow()
        {
            InitializeComponent();
            model = new MainViewModel();
            DataContext = model;
            canvasImage.Stretch = Stretch.None;
            Loaded += (e, args) =>
            {
                model.PolygonBitmap = CanvasExtender.CreateWritableBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, Colors.White);
                model.PolygonBitmap.DrawLine(20, 50, 120, 150, Colors.Black);
            };
        }
    }
}

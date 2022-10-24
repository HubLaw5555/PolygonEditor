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
using System.ComponentModel;

namespace PolygonEditor
{
    /// <summary>
    /// Logika interakcji dla klasy VisualControl.xaml
    /// </summary>
    public partial class VisualControl : UserControl, INotifyPropertyChanged
    {
        public VisualControl()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
                //OnPropertyChanged("Text");
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(VisualControl));

        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Brush), typeof(VisualControl));

        public int TextSize
        {
            get { return (int)GetValue(TextSizeProperty); }
            set
            {
                SetValue(TextSizeProperty, value);
                //OnPropertyChanged("TextSizeProperty");
            }
        }

        public static readonly DependencyProperty TextSizeProperty = DependencyProperty.Register("TextSize", typeof(int), typeof(VisualControl));


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

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
    /// Logika interakcji dla klasy VertexControl.xaml
    /// </summary>
    public partial class VertexControl : UserControl, INotifyPropertyChanged
    {
        public Vertex vertexOwner;

        public VertexControl(Vertex vertex)
        {
            vertexOwner = vertex;
            InitializeComponent();
            vertexText.Text = LetterConverter.Convert(vertex.VertexNumber);
        }

        public void SetNumber(int nr)
        {
            vertexText.Text = LetterConverter.Convert(nr);
        }

        private void RemoveFromPolygon(object sender, RoutedEventArgs args)
        {
            if (vertexOwner.ownerPolygon.vertices.Count < 4) return;

            (System.Windows.Application.Current.MainWindow as MainWindow).RemoveVertex(vertexOwner);
        }

        ContextMenu BuildMenu()
        {
            ContextMenu theMenu = new ContextMenu();

            MenuItem mia = new MenuItem();
            mia.Header = "Usuń wierzchołek";
            mia.Click += RemoveFromPolygon;

            theMenu.Items.Add(mia);
            return theMenu;
        }

        private void vertexCntrl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow window = System.Windows.Application.Current.MainWindow as MainWindow;
            Polygon polygon = vertexOwner.ownerPolygon;
            if (window.currentPolygon != polygon && window.model.state == States.Edit)
            {
                window.currentPolygon = polygon;
            }
            e.Handled = false;
        }

        private void UserControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ContextMenu = BuildMenu();
            this.ContextMenu.IsOpen = true;
        }

        #region Property Changed
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

    }
}

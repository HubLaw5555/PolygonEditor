using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.ComponentModel;

namespace PolygonEditor
{
    public struct VertexNeighbours
    {
        public PolygonEdge edge;
        public Vertex next;
        public Vertex prev;
    }

    public class Vertex: INotifyPropertyChanged
    {
        int _x;
        public int X
        {
            get { return _x; }
            set
            {
                if(value < 0 || value > (Application.Current.MainWindow as MainWindow).canvas.ActualWidth)
                {
                    return;
                }
                SetField(ref _x, value, "X");
                if (window.canvas.Children.Contains(control))
                {
                    Canvas.SetLeft(control , value - control.Width/2);
                }
                if(this.ownerPolygon != null && this.ownerPolygon.isDone)
                {
                    this.neighbours.edge.ActualiseVisualPosition();
                    this.neighbours.prev.neighbours.edge.ActualiseVisualPosition();
                }
            }
        }

        int _y;
        public int Y
        {
            get { return _y; }
            set
            {
                if (value < 0 || value > (Application.Current.MainWindow as MainWindow).canvas.ActualHeight)
                {
                    return;
                }
                SetField(ref _y, value, "Y");
                if (window.canvas.Children.Contains(control))
                {
                    Canvas.SetTop(control, value - control.Height/2);
                }
                if (this.ownerPolygon != null && this.ownerPolygon.isDone)
                {
                    this.neighbours.edge.ActualiseVisualPosition();
                    this.neighbours.prev.neighbours.edge.ActualiseVisualPosition();
                }
            }
        }

        public Polygon ownerPolygon;
        public VertexNeighbours neighbours;
        public Point Position => new Point(X, Y);

        public VertexControl control;
        MainWindow window;


        public Vertex(double x, double y) : this((int)x, (int)y) { }

        private int _vN;
        public int VertexNumber
        {
            get { return _vN; }
            set { 
                SetField(ref _vN, value, "VertexNumber");
                OnPropertyChanged("VertexText");
            }
        }
        public string VertexText
        {
            get { return LetterConverter.Convert(VertexNumber); }
        }

        public Vertex(int x, int y)
        {
            window = (MainWindow)Application.Current.MainWindow;
            control = new VertexControl(this);
            neighbours = new VertexNeighbours();

            window.canvas.Children.Add(control);
            X = x;
            Y = y;
        }

        public Vertex(Polygon polygon, int x, int y) : this(x, y)
        {
            ownerPolygon = polygon;
            VertexNumber = polygon.vertices.Count + 1;
            control.SetNumber(VertexNumber);
        }

        public void SimpleMoveVertex(Point to)
        {
            X = (int)to.X;
            Y = (int)to.Y;
            ownerPolygon.CalculateBalancePoint();
        }

        public void MoveVertex(Point to)
        {
            SimpleMoveVertex(to);

            List<Vertex> moved = new List<Vertex>();
            moved.Add(this);
            Vertex next = this.neighbours.next;
            PolygonEdge edge = this.neighbours.edge;
            
            while(moved.Count < ownerPolygon.vertices.Count)
            {
                edge.CorrectVertexOnEdge(next);

                moved.Add(next);
                next = moved.Count % 2 == 0 ? moved[moved.Count - 2].neighbours.prev :
                    moved[moved.Count - 2].neighbours.next;
                edge = moved.Count % 2 == 0 ? next.neighbours.edge : next.neighbours.prev.neighbours.edge;
            }
        }

        public Vertex(Point pt) : this((int)pt.X, (int)pt.Y) { }

        public void AddOwner(Polygon polygon)
        {
            ownerPolygon = polygon;
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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                SetField(ref _x, value, "X");
                if (window.canvas.Children.Contains(control))
                {
                    Canvas.SetLeft(control , value - control.Width/2);
                }
            }
        }

        int _y;
        public int Y
        {
            get { return _y; }
            set
            {
                SetField(ref _y, value, "Y");
                if (window.canvas.Children.Contains(control))
                {
                    Canvas.SetTop(control, value - control.Height/2);
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
            //control.VertexNumber = LetterConverter.Convert(VertexNumber);
        }

        public Vertex(Polygon polygon, int x, int y) : this(x, y)
        {
            ownerPolygon = polygon;
            VertexNumber = polygon.vertices.Count + 1;
            control.SetNumber(VertexNumber);
        }

        //public void SetEdge(PolygonEdge edge)
        //{
        //    control.edgeOwner = edge;
        //}



        public void MoveVertex(Point to)
        {
            X = (int)to.X;
            Y = (int)to.Y;
            ownerPolygon.CalculateBalancePoint();
            Vertex next = this.neighbours.next;
            while(next != this)
            {
                next.neighbours.prev.neighbours.edge.VertexMovesEdge(next);
                next.neighbours.edge.VertexMovesEdge(next);
                next = next.neighbours.next;
            }

            //if(!(this.neighbours.edge is Edge))
            //{
            //    this.neighbours.edge.VertexMovesEdge(this, /*source*/vs, pos, to);
            //}
            
            //if(!(this.neighbours.prev.neighbours.edge is Edge))
            //{
            //    this.neighbours.prev.neighbours.edge.VertexMovesEdge(this, /*source*/vs, pos, to);
            //}

            //if (newPos != null)
            //{
            //    Vertex v = this.neighbours.next;
            //    v.MoveVertex(vs, (Point)newPos);
            //}
            //count--;
            //this.neighbours.prev.neighbours.edge.VertexMovesEdge(this, /*source*/ref count, Position, to);
            
            //this.neighbours.SameEdge.VertexMovesEdge(this, source, Position, to);
            //this.neighbours.NextEdge.VertexMovesEdge(this, source, Position, to);
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

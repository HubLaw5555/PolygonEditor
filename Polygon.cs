using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace PolygonEditor
{
    public class Polygon : INotifyPropertyChanged
    {
        private ObservableCollection<Vertex> _verts;
        public ObservableCollection<Vertex> vertices
        {
            get { return _verts; }
            set { SetField(ref _verts, value, "vertices"); }
        }


        private ObservableCollection<PolygonEdge> _edgs;
        public ObservableCollection<PolygonEdge> edges
        {
            get { return _edgs; }
            set { SetField(ref _edgs, value, "edges"); }
        }

        private ObservableCollection<PolygonEdge> _edRel;
        public ObservableCollection<PolygonEdge> edgesWithRelation
        {
            get { return _edRel; }
            set { SetField(ref _edRel, value, "edgesWithRelation"); }
        }

        public bool isDone;

        public Polygon()
        {
            vertices = new ObservableCollection<Vertex>();
            edges = new ObservableCollection<PolygonEdge>();
            edgesWithRelation = new ObservableCollection<PolygonEdge>();
            isDone = false;
        }

        public Point balancePoint { get; set; }

        public void AddVertex(Vertex vertex)
        {
            if (vertices.Count > 0)
            {
                vertex.neighbours.next = vertices.Last();
                vertices.Last().neighbours.prev = vertex;
                PolygonEdge e = AddEdge(vertices.Last(), vertex);
                vertex.neighbours.edge = e;
                //vertices.Last().SetEdge(e);
            }
            vertex.AddOwner(this);
            vertices.Add(vertex);
        }

        public void InsertVertex(Vertex vertex, Vertex prev)
        {
            vertex.VertexNumber = prev.VertexNumber != 1 ? prev.VertexNumber : vertices.Count + 1;
            vertex.control.SetNumber(vertex.VertexNumber);
            vertices.Insert(vertices.IndexOf(prev), vertex);

            PolygonEdge edge = prev.neighbours.edge;
            Vertex next = prev.neighbours.next;

            int index = edges.IndexOf(edge);
            PolygonEdge e = new Edge(next, vertex);
            edges.Insert(index, e);

            edge.leftVertex = vertex;
            prev.neighbours.next = vertex;
            vertex.neighbours.prev = prev;
            vertex.neighbours.edge = e;
            vertex.neighbours.next = next;
            next.neighbours.prev = vertex;
            RenumerateAllVertices(/*prev.neighbours.next*/vertex, vertex.VertexNumber, 0);
        }

        public void ChangeEdges(PolygonEdge from, PolygonEdge to)
        {
            edges.Insert(edges.IndexOf(from), to);
            edges.Remove(from);

            Vertex prev, next;
            prev = next = null;

            if(from.rightVertex.neighbours.edge == from)
            {
                prev = from.rightVertex;
                next = from.leftVertex;
            }
            else
            {
                prev = from.leftVertex;
                next = from.rightVertex;
            }


            prev.neighbours.edge = to;
        }

        public PolygonEdge AddEdge(Vertex l, Vertex r)
        {
            PolygonEdge edge = null;
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                edge = new FixedLenghtEdge(l, r);
            }
            else
            {
                edge = new Edge(l, r);
            }
            edges.Add(edge);
            return edge;
            //DrawLine(l.X, l.Y, r.X, r.Y,2);
        }

        private void DrawLine(int x1, int y1, int x2, int y2, int size, Color? color = null)
        {
            (Application.Current.MainWindow as MainWindow).model.PolygonBitmap.DrawLine(x1, y1, x2, y2, size, color);
        }

        public void Redraw()
        {
            foreach (var edge in edges)
            {
                edge.RedrawLine();
            }
        }

        // direction: 1 -> next, 0 -> prev
        public void RenumerateAllVertices(Vertex v, int number, int direction = 1)
        {
            Vertex next = direction == 1 ? v.neighbours.next : v.neighbours.prev;

            while (next.VertexNumber != 1)
            {
                next.VertexNumber = ++number;
                next.control.SetNumber(next.VertexNumber);
                next = direction == 1 ? next.neighbours.next : next.neighbours.prev;
            }
        }

        public PolygonEdge IsOnEdge(Point point)
        {
            foreach (var edge in edges)
            {
                if(edge.IsPointOnEdge(point) != null)
                {
                    return edge;
                }
            }
            return null;
        }

        public void CalculateBalancePoint()
        {
            double x = 0, y = 0;
            foreach(var v in vertices)
            {
                x += v.X;
                y += v.Y;
            }
            balancePoint = new Point(x / vertices.Count, y / vertices.Count);
        }

        //public void FillNeighbours()
        //{
        //    for(int i = 0; i < edges.Count; ++i)
        //    {
        //        int prev = i == 0 ? edges.Count - 1 : i - 1;
        //        int next = i == edges.Count - 1 ? 0 : i + 1;

        //        PolygonEdge edge = edges[i];
        //        edge.leftVertex.neighbours.SameEdge = edge;
        //        edge.leftVertex.neighbours.SameEdgeV = edge.rightVertex;
        //        edge.leftVertex.neighbours.NextEdge = edges[prev];
        //        edge.leftVertex.neighbours.NextEdgeV = edges[prev].leftVertex;

        //        edge.rightVertex.neighbours.SameEdge = edge;
        //        edge.rightVertex.neighbours.SameEdgeV = edge.leftVertex;
        //        edge.rightVertex.neighbours.NextEdge = edges[next];
        //        edge.rightVertex.neighbours.NextEdgeV = edges[next].rightVertex;
        //    }
        //}

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

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
            //edge.RemoveRelationsIfExists(false);
            RenumerateAllVertices(vertex, vertex.VertexNumber, 0);
        }

        public void ChangeEdges(PolygonEdge from, PolygonEdge to)
        {
            if (edges.Contains(from))
            {
                edges.Insert(edges.IndexOf(from), to);
                edges.Remove(from);
            }

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
            if (Keyboard.IsKeyDown(Key.LeftShift) )
            {
                if( this.edges.Count == 0 || this.edges.Count % 2 == 1 || r != edges.First().leftVertex || edges.Where(e => e is Edge).ToList().Count > 0)
                {
                    edge = new FixedLenghtEdge(l, r);
                }
                else
                {
                    MessageBox.Show("Nie można dodać krawędzi o ustalonej długości. Dodano zwykłą krawędź", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    edge = new Edge(l, r);
                }
            }
            else
            {
                edge = new Edge(l, r);
            }
            edges.Add(edge);
            return edge;
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

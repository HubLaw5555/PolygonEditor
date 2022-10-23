using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PolygonEditor
{
    public abstract class PolygonEdge: INotifyPropertyChanged
    {
        private Vertex _l, _r;
        public Vertex leftVertex
        {
            get { return _l; }
            set { SetField(ref _l, value, "leftVertex"); }
        }


        public Vertex rightVertex
        {
            get { return _r; }
            set { SetField(ref _r, value, "rightVertex"); }
        }

        private Stack<Point> linePoints;


        public PolygonEdge(Vertex l, Vertex r)
        {
            leftVertex = l;
            rightVertex = r;
        }

        protected void DrawLine(int x1, int y1, int x2, int y2, int size, Color? color = null)
        {
            linePoints = (Application.Current.MainWindow as MainWindow).model.PolygonBitmap.DrawLine(x1, y1, x2, y2, size, color);
        }

        protected void DrawPoint(Point pt, Color color)
        {
            (Application.Current.MainWindow as MainWindow).model.PolygonBitmap.DrawPixel((int)pt.X, (int)pt.Y, color);
        }

        protected void DrawPoint(int x, int y, Color color)
        {
            (Application.Current.MainWindow as MainWindow).model.PolygonBitmap.DrawPixel(x, y, color);
        }

        protected void EraseLastLine()
        {
            if(linePoints != null && linePoints.Count > 0)
            {
                do
                {
                    DrawPoint(linePoints.Pop(), Colors.Black);
                } while (linePoints.Count > 0);
            }
        }
        
        protected void DrawEdge(Color color)
        {
            DrawLine(leftVertex.X, leftVertex.Y, rightVertex.X, rightVertex.Y, 2, color);
        }

        public abstract void RedrawLine();
        

        //public abstract void VertexMovesEdge(Vertex v, /*Vertex source*/List<Vertex> vs, Point from, Point to);
        public abstract void CorrectVertexOnEdge(Vertex v /*Vertex source List<Vertex> vs,*/);

        public abstract void Substitute(PolygonEdge edge);

        //public PolygonEdge IsPointOnEdge(Point pos)
        //{
        //    Point left = leftVertex.Position;
        //    Point right = rightVertex.Position;

        //    if((left.X == right.X && left.X == pos.X && pos.Y < Math.Max(left.Y, right.Y) && pos.Y > Math.Min(left.Y, right.Y))
        //        || (left.Y == right.Y && left.Y == pos.Y && pos.X < Math.Max(left.X, right.X) && pos.X > Math.Min(left.X, right.X)))
        //    {

        //        return this;
        //    }
        //    else if(left.X != right.X && left.Y != right.Y)
        //    {
        //        double a1 = (pos.X - left.X) / (right.X - left.X);
        //        double a2 = (pos.Y - left.Y) / (right.Y - left.Y);
        //        return Math.Abs(a1 - a2) < 0.01 ? this : null;
        //    }
        //    return null;

        //}

        public PolygonEdge IsPointOnEdge(Point pos)
        {
            bool isonEdge = Geometry.IsOnLine(leftVertex.Position, rightVertex.Position, pos, 2);
            return isonEdge ? this : null;
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

    public class Edge : PolygonEdge
    {
        public Color color = Colors.White;

        public Edge(Vertex l, Vertex r) : base(l, r) 
        {
            DrawLine(leftVertex.X, leftVertex.Y, rightVertex.X, rightVertex.Y, 2, Colors.White);
        }

        public override void CorrectVertexOnEdge(Vertex v)
        {
            return;
        }

        public override void Substitute(PolygonEdge edge)
        {
            this.leftVertex.ownerPolygon.ChangeEdges(this, edge);
        }

        //public override void VertexMovesEdge(Vertex v, /*Vertex source*/ List<Vertex> vs, Point from, Point to)
        //{
        //    //if(v != leftVertex && v != rightVertex)
        //    //{
        //    //    return;
        //    //}
        //    //v.X = (int)to.X;
        //    //v.Y = (int)to.Y;

        //    Vertex neigh = null;

        //    if (vs.Count == 1)
        //    {
        //        neigh = vs.First().neighbours.next;
        //    }
        //    else if (vs.Contains(vs[vs.Count - 2].neighbours.next))
        //    {
        //        neigh = vs[vs.Count - 2].neighbours.prev;
        //    }
        //    else
        //    {
        //        neigh = vs[vs.Count - 2].neighbours.next;
        //    }

        //}

        public override void RedrawLine()
        {
            //EraseLastLine();
            DrawEdge(color);
        }
    }


    public class FixedLenghtEdge : PolygonEdge
    {
        public Color color = Colors.Red;
        public double length;

        public FixedLenghtEdge(Vertex l, Vertex r) : base(l, r)
        {
            length = Math.Sqrt(Math.Pow((l.X - r.X), 2) + Math.Pow((r.Y - l.Y), 2));
            l.ownerPolygon.edgesWithRelation.Add(this);
            DrawLine(leftVertex.X, leftVertex.Y, rightVertex.X, rightVertex.Y, 2, Colors.Red);
        }

        public FixedLenghtEdge(PolygonEdge e) : base(e.leftVertex, e.rightVertex) { }

        public override void CorrectVertexOnEdge(Vertex v)
        {
            ////   nowy         zrobiony
            ////   v ---------> prev
            ////   actualPos    from -> to
            ////
            ///
            Vertex prev = this.rightVertex == v ? this.leftVertex : this.rightVertex;//v.neighbours.prev; // zrobiony
            Point to = new Point(prev.X, prev.Y); // nowa pozycja zrobionego

            Point actualPos = new Point(v.X, v.Y);

            double d = Geometry.Distance(actualPos, to); // odleglosc od obecnego do nowej pozycji zrobionego
            Vector shift = new Vector(to.X - actualPos.X, to.Y - actualPos.Y); // wektor od pozycji obecnego do nowej zrobionego
            shift *= (d - length) / d;

            actualPos += shift; //  przesuwanie pozycji aktualnego
            v.X = (int)actualPos.X;
            v.Y = (int)actualPos.Y;
        }

        public override void Substitute(PolygonEdge edge)
        {
            this.leftVertex.ownerPolygon.ChangeEdges(this, edge);
        }

        //public override void VertexMovesEdge(Vertex v, /*Vertex source*/List<Vertex> vs, Point from, Point to)
        //{
        //    Vertex vertInEdge = null;

        //    if(vs.Count == 1)
        //    {
        //        vertInEdge = vs.First().neighbours.next;
        //    }
        //    else if(vs.Contains(vs[vs.Count - 2].neighbours.next))
        //    {
        //        vertInEdge = vs[vs.Count - 2].neighbours.prev;
        //    }
        //    else
        //    {
        //        vertInEdge = vs[vs.Count - 2].neighbours.next;
        //    }

        //    //source = v.neighbours.edge == this ? source : neigh;
        //    if (/*neigh != source*/ vs.Count <= v.ownerPolygon.vertices.Count - 1)
        //    {
        //        Point realF = new Point(vertInEdge.X, vertInEdge.Y);
        //        double d = Geometry.Distance(realF, to);
        //        Vector shift = new Vector(to.X - realF.X, to.Y - realF.Y);
        //        shift *= (d - length) / d;

        //        vs.Add(vertInEdge);
        //        //return realF + shift;
        //        vertInEdge.MoveVertex(/*v*/vs, realF + shift);
        //    }
        //    //else if(vs.Count == v.ownerPolygon.vertices.Count - 1)
        //    //{
        //    //    Vertex prev = v.neighbours.prev, next = v.neighbours.next ;
        //    //    Point P1 = new Point(prev.X, prev.Y), P2 = new Point(next.X, next.Y);
        //    //    Line line = new Line(P1, P2);
        //    //    double d = Geometry.Distance(P1, P2);
        //    //    double n = (v.neighbours.prev.neighbours.edge as FixedLenghtEdge).length;
        //    //    double m = (v.neighbours.edge as FixedLenghtEdge).length;

        //    //    double u = (d * d + n * n - m * m) / 2 / d;
        //    //    double h = Math.Sqrt(u * u - n * n);
        //    //    double sqrt = Math.Sqrt(line.A * line.A + line.B * line.B);
        //    //    double dx = u * line.B / sqrt;
        //    //    double dy = -line.A * dx / line.B;
        //    //    Point B = P1 + new Vector(dx, dy);
        //    //    double x = (line.A * (h * sqrt - line.C - line.B * B.Y) + line.B * line.B * B.X) / (line.A * line.A + line.B);
        //    //    double y = line.B * x / line.A + B.Y - line.B * B.X / line.A;
        //    //    //count++;
        //    //    vs.Add(neigh);
        //    //    //return new Point(x, y);
        //    //    neigh.MoveVertex(/*v*/vs, new Point(x,y));
        //    //}
        //    //return null;
        //}

        public override void RedrawLine()
        {
            //EraseLastLine();
            DrawEdge(color);
        }
    }

    public class OrtogonalEdge : PolygonEdge
    {
        public Color color = Colors.Blue;

        public PolygonEdge pairedEdge;

        public OrtogonalEdge(Vertex left, Vertex right, PolygonEdge pair = null): base(left, right)
        {
            left.ownerPolygon.edgesWithRelation.Add(this);
            pairedEdge = pair;

            if (pairedEdge != null)
            {
                CorrectVertexOnEdge(right);
            }

            DrawLine(leftVertex.X, leftVertex.Y, rightVertex.X, rightVertex.Y, 2, Colors.Blue);
        }
        //public OrtogonalEdge(PolygonEdge e) : base(e.leftVertex, e.rightVertex) { }

        public override void CorrectVertexOnEdge(Vertex v)
        {

            //Vertex prev = this.rightVertex == v ? this.leftVertex : this.rightVertex;
            //Point center = new Point((pairedEdge.leftVertex.X + pairedEdge.rightVertex.X) / 2, 
            //    (pairedEdge.leftVertex.Y + pairedEdge.rightVertex.Y) / 2);
            //Vector vec = new Vector(v.Position.X - center.X, v.Position.Y - center.Y);
            //vec /= vec.Length; // wektor jednostkowy w kierunku od krawędzi do punktu

            //Vector x = new Vector(v.X - prev.X, v.Y - prev.Y);
            //Vector shift = ((x * vec) / (vec * vec)) * vec; // rzut na prostą

            //Point actualPos = prev.Position;
            //actualPos += shift;

            //v.X = (int)actualPos.X;
            //v.Y = (int)actualPos.Y;

            Vertex prev = this.rightVertex == v ? this.leftVertex : this.rightVertex;
            Line line = new Line(pairedEdge.leftVertex.Position, pairedEdge.rightVertex.Position);

            Line orthoLine = line.OrthogonalOnPoint(prev.Position);
            Vector vec = orthoLine.GetLineDirection(); //  jesli tu jest line, to rownolegle by byly
            Vector x = new Vector(v.X - prev.X, v.Y - prev.Y);
            Vector shift = ((x * vec) / (vec * vec)) * vec; // rzut na prostą

            Point actualPos = prev.Position;
            actualPos += shift;

            v.X = (int)actualPos.X;
            v.Y = (int)actualPos.Y;
            return;
        }

        public override void Substitute(PolygonEdge edge)
        {
            this.leftVertex.ownerPolygon.ChangeEdges(this, edge);
            pairedEdge.leftVertex.ownerPolygon.ChangeEdges(pairedEdge, new Edge(pairedEdge.leftVertex, pairedEdge.rightVertex));
        }

        //public override void VertexMovesEdge(Vertex v, /*Vertex source*/List<Vertex> vs, Point from, Point to)
        //{
        //    //if (v != leftVertex && v != rightVertex)
        //    //{
        //    //    return;
        //    //}
        //    //v.X = (int)to.X;
        //    //v.Y = (int)to.Y;
        //    //return null;
        //}

        public override void RedrawLine()
        {
            //EraseLastLine();
            DrawEdge(Colors.Blue);
        }
    }
}

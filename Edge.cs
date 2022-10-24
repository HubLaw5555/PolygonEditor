using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.ComponentModel;

namespace PolygonEditor
{
    public abstract class PolygonEdge : INotifyPropertyChanged
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

        public PolygonEdge(Vertex l, Vertex r)
        {
            leftVertex = l;
            rightVertex = r;
        }

        protected VisualControl visual;

        protected void DrawLine(int x1, int y1, int x2, int y2, int size, Color? color = null)
        {
            (Application.Current.MainWindow as MainWindow).DrawLine(x1, y1, x2, y2, size, color);
        }

        protected void DrawEdge(Color color)
        {
            DrawLine(leftVertex.X, leftVertex.Y, rightVertex.X, rightVertex.Y, 2, color);
        }

        public void ActualiseVisualPosition()
        {
            if (visual != null && leftVertex != null && rightVertex != null)
            {
                Canvas.SetTop(visual, (leftVertex.Y + rightVertex.Y) / 2 + 5);
                Canvas.SetLeft(visual, (leftVertex.X + rightVertex.X) / 2 + 5);
            }
        }

        public PolygonEdge IsPointOnEdge(Point pos)
        {
            bool isonEdge = Geometry.IsOnLine(leftVertex.Position, rightVertex.Position, pos, 2);
            return isonEdge ? this : null;
        }

        public abstract void RedrawLine();

        public abstract void CorrectVertexOnEdge(Vertex v);

        public abstract void Substitute(PolygonEdge edge);

        public abstract void RemoveVisualSymbolIfExists();

        public abstract void RemoveRelationsIfExists(bool simple = false);

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

        public override void RemoveVisualSymbolIfExists()
        {
            return;
        }

        public override void RemoveRelationsIfExists(bool simple = false)
        {
            return;
        }

        public override void RedrawLine()
        {
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
            GenerateSymbol();
            ActualiseVisualPosition();
        }

        private void GenerateSymbol()
        {
            visual = new VisualControl();
            visual.Text = "L";
            visual.Color = Brushes.Red;
            visual.TextSize = 15;
            visual.Height = visual.Width = 20;
            (Application.Current.MainWindow as MainWindow).canvas.Children.Add(visual);
        }

        public override void RemoveVisualSymbolIfExists()
        {
            if (visual != null && (Application.Current.MainWindow as MainWindow).canvas.Children.Contains(visual))
            {
                (Application.Current.MainWindow as MainWindow).canvas.Children.Remove(visual);
                visual = null;
            }
        }

        public override void RemoveRelationsIfExists(bool simple = false)
        {
            this.leftVertex.ownerPolygon.edgesWithRelation.Remove(this);
            this.RemoveVisualSymbolIfExists();
            this.leftVertex.ownerPolygon.ChangeEdges(this, new Edge(this.leftVertex, this.rightVertex));
        }

        public override void CorrectVertexOnEdge(Vertex v)
        {
            Vertex prev = this.rightVertex == v ? this.leftVertex : this.rightVertex;
            Point to = new Point(prev.X, prev.Y);

            Point actualPos = new Point(v.X, v.Y);

            double d = Geometry.Distance(actualPos, to); 
            Vector shift = new Vector(to.X - actualPos.X, to.Y - actualPos.Y);
            shift *= (d - length) / d;

            actualPos += shift;
            v.X = (int)actualPos.X;
            v.Y = (int)actualPos.Y;
        }

        public override void Substitute(PolygonEdge edge)
        {
            RemoveRelationsIfExists(true);
            this.leftVertex.ownerPolygon.ChangeEdges(this, edge);
        }

        public override void RedrawLine()
        {
            DrawEdge(color);
        }
    }

    public class OrtogonalEdge : PolygonEdge
    {
        public Color color = Colors.Blue;

        public PolygonEdge pairedEdge;

        private static int _unq = 2;
        public int UniqueNr;

        public OrtogonalEdge(Vertex left, Vertex right, PolygonEdge pair = null) : base(left, right)
        {
            left.ownerPolygon.edgesWithRelation.Add(this);
            pairedEdge = pair;

            if (pairedEdge != null)
            {
                CorrectVertexOnEdge(right);
            }

            UniqueNr = (_unq++) / 2;

            DrawLine(leftVertex.X, leftVertex.Y, rightVertex.X, rightVertex.Y, 2, Colors.Blue);
            GenerateSymbol();
            ActualiseVisualPosition();
        }

        private void GenerateSymbol()
        {
            visual = new VisualControl();
            visual.Text = "⟂" + UniqueNr;
            visual.Color = Brushes.Blue;
            visual.TextSize = 15;
            visual.Height = visual.Width = 20;
            (Application.Current.MainWindow as MainWindow).canvas.Children.Add(visual);
        }



        public override void RemoveVisualSymbolIfExists()
        {
            if (visual != null && (Application.Current.MainWindow as MainWindow).canvas.Children.Contains(visual))
            {
                (Application.Current.MainWindow as MainWindow).canvas.Children.Remove(visual);
            }
        }

        public override void RemoveRelationsIfExists(bool simple = false)
        {
            this.leftVertex.ownerPolygon.edgesWithRelation.Remove(this);
            this.RemoveVisualSymbolIfExists();
            this.leftVertex.ownerPolygon.ChangeEdges(this, new Edge(this.leftVertex, this.rightVertex));

            if (simple != true)
            {
                pairedEdge.leftVertex.ownerPolygon.edgesWithRelation.Remove(pairedEdge);
                pairedEdge.RemoveVisualSymbolIfExists();
                pairedEdge.leftVertex.ownerPolygon.ChangeEdges(pairedEdge, new Edge(pairedEdge.leftVertex, pairedEdge.rightVertex));
            }

        }

        public override void CorrectVertexOnEdge(Vertex v)
        {
            Vertex prev = this.rightVertex == v ? this.leftVertex : this.rightVertex;
            Line line = new Line(pairedEdge.leftVertex.Position, pairedEdge.rightVertex.Position);

            Line orthoLine = line.OrthogonalOnPoint(prev.Position);
            Vector vec = orthoLine.GetLineDirection();
            Vector x = new Vector(v.X - prev.X, v.Y - prev.Y);
            Vector shift = ((x * vec) / (vec * vec)) * vec;

            Point actualPos = prev.Position;
            actualPos += shift;

            v.X = (int)actualPos.X;
            v.Y = (int)actualPos.Y;
        }

        public override void Substitute(PolygonEdge edge)
        {
            RemoveRelationsIfExists(true);
            this.leftVertex.ownerPolygon.ChangeEdges(this, edge);
            pairedEdge.leftVertex.ownerPolygon.ChangeEdges(pairedEdge, new Edge(pairedEdge.leftVertex, pairedEdge.rightVertex));
        }

        public override void RedrawLine()
        {
            DrawEdge(Colors.Blue);
        }
    }
}

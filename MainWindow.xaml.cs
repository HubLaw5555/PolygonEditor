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
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public readonly static Color backColor = Color.FromRgb(52, 52, 52);
        public MainViewModel model;

        public List<Polygon> polygons;

        private Polygon _polygon;
        public Polygon currentPolygon
        {
            get { return _polygon; }
            set { SetField(ref _polygon, value, "currentPolygon"); }
        }

        public MainWindow()
        {
            InitializeComponent();
            model = new MainViewModel();
            DataContext = model;
            canvasImage.Stretch = Stretch.None;
            currentPolygon = new Polygon();
            polygons = new List<Polygon>();
            polygons.Add(currentPolygon);
            Loaded += (e, args) =>
            {
                model.PolygonBitmap = CanvasExtender.CreateWritableBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, backColor);
                //model.PolygonBitmap.DrawLine(20, 50, 120, 150, Colors.White);
            };
        }

        private void canvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PolygonEdge edge = null;
            Point point = e.GetPosition(canvas);

            if(model.state == States.Edit && (edge = IsOnAnyPolygonEdge(point)) != null)
            {
                ContextMenu menu = new ContextMenu();
                MenuItem item = new MenuItem();
                item.Header = "Dodaj wierzchołek na środku";
                currentPolygon = edge.leftVertex.ownerPolygon;
                item.Click += (ee, args) =>
                {
                    Vertex v = new Vertex(currentPolygon, (int)((edge.leftVertex.X + edge.rightVertex.X)/2),
                         (int)((edge.leftVertex.Y + edge.rightVertex.Y) / 2));
                    Vertex prev = edge.leftVertex.neighbours.edge == edge ? edge.leftVertex : edge.rightVertex;

                    edge.Substitute(new Edge(edge.leftVertex, edge.rightVertex));
                    currentPolygon.InsertVertex(v, prev);
                    RedrawCanvas();
                };

                menu.Items.Add(item);
                menu.IsOpen = true;
                //canvas.Children.Add(menu);
                //Canvas.SetTop(menu, point.Y);
                //Canvas.SetLeft(menu, point.X);

            }
            else if (!(e.Source is VertexControl))
            {
                if (model.state == States.Edit)
                {
                    currentPolygon = new Polygon();
                    polygons.Add(currentPolygon);
                }

                model.state = States.PolygonAdd;

                Vertex v = new Vertex(currentPolygon, (int)point.X, (int)point.Y);
                currentPolygon.AddVertex(v);
                e.Handled = true;
            }
            else
            {
                VertexControl vertex = e.Source as VertexControl;
                if (model.state == States.PolygonAdd && vertex == currentPolygon.vertices.First().control)
                {
                    vertex.vertexOwner.neighbours.next = currentPolygon.vertices.Last();
                    currentPolygon.vertices.Last().neighbours.prev = vertex.vertexOwner;
                    currentPolygon.AddEdge(currentPolygon.vertices.Last(), currentPolygon.vertices.First());
                    vertex.vertexOwner.neighbours.edge = currentPolygon.edges.Last();
                    currentPolygon.isDone = true;
                    currentPolygon.CalculateBalancePoint();
                    model.state = States.Edit;
                    //currentPolygon.FillNeighbours();
                    //currentPolygon = null;
                    e.Handled = true;
                }
            }
        }

        private struct DragData
        {
            public Point? from, to;
            public object movableObj;
        }

        DragData VertexDragData = new DragData();

        private PolygonEdge lastOrthoEdge = null;

        private PolygonEdge IsOnAnyPolygonEdge(Point point)
        {
            if (polygons == null) return null;

            foreach (var pol in polygons)
            {
                PolygonEdge edge = pol.IsOnEdge(point);
                if (edge != null) return edge;
            }
            return null;
        }

        public Polygon IsOnPolygonCenter(Point point)
        {
            if (polygons == null) return null;

            foreach (var pol in polygons)
            {
                if (Math.Abs(point.X - pol.balancePoint.X) < 10 && Math.Abs(point.Y - pol.balancePoint.Y) < 10)
                {
                    return pol;
                }
            }
            return null;
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (model.state == States.PolygonAdd)
            //{
            //    VertexDragData.movableObj = null;
            //    return;
            //}

            PolygonEdge edge = null;
            Polygon polygon = null;
            Point point = e.GetPosition(this.canvas);

            if(model.state == States.OrthogonalOne && (edge = IsOnAnyPolygonEdge(point)) != null)
            {
                lastOrthoEdge = new OrtogonalEdge(edge.leftVertex, edge.rightVertex);
                edge.leftVertex.ownerPolygon.ChangeEdges(edge, lastOrthoEdge);
                model.state = States.OrthogonalTwo;
            }
            else if(model.state == States.OrthogonalTwo && (edge = IsOnAnyPolygonEdge(point)) != null && lastOrthoEdge != null)
            {
                PolygonEdge ortho = new OrtogonalEdge(edge.leftVertex, edge.rightVertex, lastOrthoEdge);
                (lastOrthoEdge as OrtogonalEdge).pairedEdge = ortho;
                edge.leftVertex.ownerPolygon.ChangeEdges(edge, ortho);
                model.state = States.Edit;
                RedrawCanvas();
            }
            else if (model.state == States.Edit && e.Source is VertexControl)
            {
                VertexDragData.movableObj = e.Source as VertexControl;
                if ((VertexDragData.movableObj as VertexControl).vertexOwner.ownerPolygon.isDone != true)
                {
                    VertexDragData.movableObj = null;
                    return;
                }

                VertexDragData.from = point;
                VertexDragData.to = point;
            }
            else if (model.state == States.Edit && (edge = IsOnAnyPolygonEdge(point)) != null)
            {
                VertexDragData.movableObj = edge;
                currentPolygon = edge.leftVertex.ownerPolygon;

                if (edge.leftVertex.ownerPolygon.isDone != true)
                {
                    VertexDragData.movableObj = null;
                    return;
                }
                VertexDragData.from = point;
                VertexDragData.to = point;
            }
            else if (model.state == States.Edit && (polygon = IsOnPolygonCenter(point)) != null)
            {
                VertexDragData.movableObj = polygon;
                currentPolygon = polygon;
                if (polygon.isDone != true)
                {
                    VertexDragData.movableObj = null;
                    return;
                }
                VertexDragData.from = point;
                VertexDragData.to = point;
            }
            else
            {
                VertexDragData.movableObj = null;
                VertexDragData.from = null;
                VertexDragData.to = null;
            }
        }

        private void RemovePolygon_Click(object sender, RoutedEventArgs args)
        {
            if(currentPolygon != null)
            {
                currentPolygon.edges.Clear();
                foreach(var v in currentPolygon.vertices)
                {
                    canvas.Children.Remove(v.control);
                }
                currentPolygon.vertices.Clear();
                polygons.Remove(currentPolygon);
                RedrawCanvas();
            }
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (VertexDragData.movableObj != null)
            {
                Point to = e.GetPosition(this.canvas);

                VertexDragData.movableObj = null;
                VertexDragData.from = null;
                VertexDragData.to = null;
            }
        }

        private void RedrawCanvas()
        {
            model.PolygonBitmap = CanvasExtender.CreateWritableBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, backColor);
            foreach (var polygon in polygons)
            {
                polygon.Redraw();
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(this.canvas);
            if (VertexDragData.movableObj != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.SetCursor(Cursors.Arrow);
                VertexDragData.from = VertexDragData.to;
                VertexDragData.to = point;
                if (VertexDragData.from != null && VertexDragData.to != null)
                {
                    if (VertexDragData.movableObj is VertexControl)
                    {
                        Vertex v = (VertexDragData.movableObj as VertexControl).vertexOwner;
                        //var vs = new List<Vertex>();
                        //vs.Add(v);
                        v.MoveVertex(/*v*/(Point)VertexDragData.to);
                    }
                    else if (VertexDragData.movableObj is PolygonEdge)
                    {
                        Vertex left = (VertexDragData.movableObj as PolygonEdge).leftVertex;
                        Vertex right = (VertexDragData.movableObj as PolygonEdge).rightVertex;
                        Vector vec = new Vector(((Point)VertexDragData.to).X - ((Point)VertexDragData.from).X,
                            ((Point)VertexDragData.to).Y - ((Point)VertexDragData.from).Y);
                        left.MoveVertex(left.Position + vec);
                        right.MoveVertex(right.Position + vec);
                    }
                    else if(VertexDragData.movableObj is Polygon)
                    {
                        Polygon polygon = VertexDragData.movableObj as Polygon;
                        Vector vec = new Vector(((Point)VertexDragData.to).X - ((Point)VertexDragData.from).X,
                            ((Point)VertexDragData.to).Y - ((Point)VertexDragData.from).Y);
                        foreach(var v in polygon.vertices)
                        {
                            v.MoveVertex(v.Position + vec);
                        }
                    }
                    //int count = 0;
                    //v.neighbours.SameEdge.MoveVertex(v,(Point)VertexDragData.from, (Point)VertexDragData.to);
                    RedrawCanvas();
                }
            }
            else if (IsOnPolygonCenter(point) != null)
            {
                Mouse.SetCursor(Cursors.SizeAll);
            }
            else
            {
                Mouse.SetCursor(Cursors.Arrow);
            }
        }

        public void RemoveVertex(Vertex v)
        {
            PolygonEdge e = new Edge(v.neighbours.prev, v.neighbours.next);
            var edgs = v.ownerPolygon.edges.Where(ee => ee.leftVertex == v || ee.rightVertex == v);
            int index = int.MaxValue;
            List<PolygonEdge> listEdgs = edgs.ToList();


            foreach (var edg in listEdgs)
            {
                index = Math.Min(index, v.ownerPolygon.edges.IndexOf(edg));
                v.ownerPolygon.edges.Remove(edg);
            }
            v.ownerPolygon.edges.Insert(index, e);
            v.ownerPolygon.vertices.Remove(v);

            Vertex prev = v.neighbours.prev;
            Vertex next = v.neighbours.next;

            prev.neighbours.next = next;
            prev.neighbours.edge = e;
            next.neighbours.prev = prev;

            int number = next.VertexNumber == v.ownerPolygon.vertices.Count + 1 ? 0 : next.VertexNumber;
            v.ownerPolygon.RenumerateAllVertices(next, number, 0);
            this.canvas.Children.Remove(v.control);
            RedrawCanvas();
        }

        //public void AddVertexLeft(Vertex v)
        //{
        //    int index = v.ownerPolygon.vertices.IndexOf(v);
        //}

        //public void AddVertexRight(Vertex v)
        //{
        //    int index = v.ownerPolygon.vertices.IndexOf(v);
        //}

        private void ButtonRemoveRelation_Click(object sender, RoutedEventArgs e)
        {
            if(model.state == States.Edit && currentPolygon != null)
            {
                PolygonEdge edge = ((Button)sender).DataContext as PolygonEdge;
                if(edge != null)
                {
                    currentPolygon.edgesWithRelation.Remove(edge);
                    currentPolygon.ChangeEdges(edge, new Edge(edge.leftVertex, edge.rightVertex));
                }
            }
        }

        private void OrthogonalStart_Click(object sender, RoutedEventArgs e)
        {
            if(model.state == States.Edit)
            {
                model.state = States.OrthogonalOne;
            }
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

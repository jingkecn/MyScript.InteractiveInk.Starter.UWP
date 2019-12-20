using Windows.Foundation;
using Windows.UI.Xaml.Media;
using MyScript.IInk.Graphics;
using Point = Windows.Foundation.Point;

namespace MyScript.InteractiveInk.Views.Shapes
{
    public sealed partial class Path : IPath
    {
        public Path()
        {
            InitializeComponent();
        }

        public void MoveTo(float x, float y)
        {
            PathFigure.StartPoint = new Point(x, y);
        }

        public void LineTo(float x, float y)
        {
            PathFigure.Segments.Add(new LineSegment {Point = new Point(x, y)});
        }

        public void CurveTo(float x1, float y1, float x2, float y2, float x, float y)
        {
            PathFigure.Segments.Add(new BezierSegment
            {
                Point1 = new Point(x1, y1), Point2 = new Point(x2, y2), Point3 = new Point(x, y)
            });
        }

        public void QuadTo(float x1, float y1, float x, float y)
        {
            PathFigure.Segments.Add(new QuadraticBezierSegment {Point1 = new Point(x1, y1), Point2 = new Point(x, y)});
        }

        public void ArcTo(float rx, float ry, float phi, bool fA, bool fS, float x, float y)
        {
            PathFigure.Segments.Add(new ArcSegment
            {
                IsLargeArc = fA,
                Point = new Point(x, y),
                RotationAngle = phi,
                Size = Size.Empty, // TODO: size required, why are there different radius for x and y?
                SweepDirection = fS ? SweepDirection.Clockwise : SweepDirection.Counterclockwise
            });
        }

        public void ClosePath()
        {
            PathFigure.IsClosed = true;
        }

        public uint UnsupportedOperations { get; } = (uint)PathOperation.ARC_OPS;
    }
}

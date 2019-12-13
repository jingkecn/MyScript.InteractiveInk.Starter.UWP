using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace MyScript.InteractiveInk.Services.Ink
{
    public partial class InkSelectionService
    {
        private readonly InkCanvas _inkCanvas;
        private readonly Canvas _selectionCanvas;
        private readonly InkStrokeService _strokeService;
        private Rect _selectionRect = Rect.Empty;

        public InkSelectionService(InkCanvas inkCanvas, Canvas selectionCanvas, InkStrokeService strokeService)
        {
            Initialize(_inkCanvas = inkCanvas);
            _selectionCanvas = selectionCanvas;
            _strokeService = strokeService;
        }

        private Rectangle SelectionRectangle
        {
            get
            {
                var rectangle = _selectionCanvas.Children.OfType<Rectangle>()
                    .SingleOrDefault(child => child.Name == nameof(SelectionRectangle));
                if (rectangle == null)
                {
                    _selectionCanvas.Children.Add(rectangle = new Rectangle
                    {
                        Name = nameof(SelectionRectangle),
                        Stroke = new SolidColorBrush(Colors.Gray),
                        StrokeDashCap = PenLineCap.Round,
                        StrokeDashArray = new DoubleCollection {2, 2},
                        StrokeThickness = 2
                    });
                }

                return rectangle;
            }
        }

        public void Clear()
        {
            _selectionRect = Rect.Empty;
            _selectionCanvas.Children.Clear();
        }

        public bool Contains(Point position)
        {
            return !_selectionRect.IsEmpty && RectHelper.Contains(_selectionRect, position);
        }

        public void Update(Rect rect)
        {
            _selectionRect = rect;
            SelectionRectangle.Height = rect.Height;
            SelectionRectangle.Width = rect.Width;
            Canvas.SetLeft(SelectionRectangle, rect.Left);
            Canvas.SetTop(SelectionRectangle, rect.Top);
        }
    }

    public partial class InkSelectionService : IDisposable
    {
        private Point _fromPosition;

        public void Dispose()
        {
            Dispose(_inkCanvas);
        }

        private void Dispose(InkCanvas canvas)
        {
            canvas.ManipulationStarted += Canvas_ManipulationStarted;
            canvas.ManipulationDelta += Canvas_ManipulationDelta;
            canvas.ManipulationCompleted += Canvas_ManipulationCompleted;
        }

        private void Initialize(InkCanvas canvas)
        {
            canvas.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            canvas.ManipulationStarted += Canvas_ManipulationStarted;
            canvas.ManipulationDelta += Canvas_ManipulationDelta;
            canvas.ManipulationCompleted += Canvas_ManipulationCompleted;
        }

        private void Move(Point offset)
        {
            _selectionRect.X = Canvas.GetLeft(SelectionRectangle) + offset.X;
            _selectionRect.Y = Canvas.GetTop(SelectionRectangle) + offset.Y;
            Canvas.SetLeft(SelectionRectangle, _selectionRect.X);
            Canvas.SetTop(SelectionRectangle, _selectionRect.Y);
        }

        private void Canvas_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (_selectionRect.IsEmpty)
            {
                return;
            }

            _fromPosition = e.Position;
        }

        private void Canvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (_selectionRect.IsEmpty)
            {
                return;
            }

            Move(new Point {X = e.Delta.Translation.X, Y = e.Delta.Translation.Y});
            _strokeService.Move(_fromPosition, _fromPosition = e.Position);
        }

        private void Canvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (_selectionRect.IsEmpty)
            {
                return;
            }

            _strokeService.Move(_fromPosition, e.Position);
        }
    }
}

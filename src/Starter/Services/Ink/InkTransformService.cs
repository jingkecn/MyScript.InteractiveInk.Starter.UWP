using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace MyScript.InteractiveInk.Services.Ink
{
    public class InkTransformService
    {
        private readonly Canvas _drawingCanvas;
        private readonly InkAnalyzer _inkAnalyzer = new InkAnalyzer();
        private readonly InkStrokeService _inkStrokeService;

        public InkTransformService(Canvas drawingCanvas, InkStrokeService inkStrokeService)
        {
            _drawingCanvas = drawingCanvas;
            _inkStrokeService = inkStrokeService;
        }

        public IEnumerable<UIElement> Elements => _drawingCanvas.Children;

        public async Task<InkTransformResult> TransformAsync()
        {
            var result = new InkTransformResult(_drawingCanvas);
            var strokes = _inkStrokeService.Strokes.ToImmutableList();
            var selected = _inkStrokeService.SelectedStrokes.ToImmutableList();
            if (selected.Any())
            {
                strokes = selected;
            }

            if (strokes.Any())
            {
                _inkAnalyzer.ClearDataForAllStrokes();
                _inkAnalyzer.AddDataForStrokes(strokes);
                var analysis = await _inkAnalyzer.AnalyzeAsync();
                switch (analysis.Status)
                {
                    case InkAnalysisStatus.Updated:
                        var words = AnalyzeWords();
                        var shapes = AnalyzeShapes();
                        // Generate result
                        result.Strokes.AddRange(strokes);
                        result.Elements.AddRange(words);
                        result.Elements.AddRange(shapes);
                        break;
                    case InkAnalysisStatus.Unchanged:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return result;
        }

        #region Drawing Text

        private UIElement DrawText(string recognition, Rect rect)
        {
            var text = new TextBlock
            {
                Text = recognition, FontSize = rect.Height, Foreground = new SolidColorBrush(Colors.Black)
            };
            Canvas.SetLeft(text, rect.Left);
            Canvas.SetTop(text, rect.Top);
            _drawingCanvas.Children.Add(text);
            return text;
        }

        #endregion

        #region UI Elements

        public void Add(params UIElement[] elements)
        {
            elements.ToImmutableList().ForEach(element => _drawingCanvas.Children.Add(element));
        }

        public void Clear()
        {
            _drawingCanvas.Children.Clear();
        }

        public void Remove(params UIElement[] elements)
        {
            elements.ToImmutableList().ForEach(element =>
            {
                if (_drawingCanvas.Children.Contains(element))
                {
                    _drawingCanvas.Children.Remove(element);
                }
            });
        }

        #endregion

        #region Analyzing

        private IEnumerable<UIElement> AnalyzeWords()
        {
            var nodes = _inkAnalyzer.AnalysisRoot.FindNodes(InkAnalysisNodeKind.InkWord).OfType<InkAnalysisInkWord>();
            foreach (var node in nodes)
            {
                var text = DrawText(node.RecognizedText, node.BoundingRect);
                var ids = node.GetStrokeIds();
                _inkStrokeService.Remove(ids);
                _inkAnalyzer.RemoveDataForStrokes(ids);
                yield return text;
            }
        }

        private IEnumerable<UIElement> AnalyzeShapes()
        {
            var nodes = _inkAnalyzer.AnalysisRoot.FindNodes(InkAnalysisNodeKind.InkDrawing)
                .OfType<InkAnalysisInkDrawing>();
            foreach (var node in nodes)
            {
                var ids = node.GetStrokeIds();
                switch (node.DrawingKind)
                {
                    case InkAnalysisDrawingKind.Drawing:
                        break;
                    case InkAnalysisDrawingKind.Circle:
                    case InkAnalysisDrawingKind.Ellipse:
                        yield return DrawEllipse(node);
                        break;
                    case InkAnalysisDrawingKind.Triangle:
                    case InkAnalysisDrawingKind.IsoscelesTriangle:
                    case InkAnalysisDrawingKind.EquilateralTriangle:
                    case InkAnalysisDrawingKind.RightTriangle:
                    case InkAnalysisDrawingKind.Quadrilateral:
                    case InkAnalysisDrawingKind.Rectangle:
                    case InkAnalysisDrawingKind.Square:
                    case InkAnalysisDrawingKind.Diamond:
                    case InkAnalysisDrawingKind.Trapezoid:
                    case InkAnalysisDrawingKind.Parallelogram:
                    case InkAnalysisDrawingKind.Pentagon:
                    case InkAnalysisDrawingKind.Hexagon:
                        yield return DrawPolygon(node);
                        _inkStrokeService.Remove(ids);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _inkAnalyzer.RemoveDataForStrokes(ids);
            }
        }

        #endregion

        #region Drawing Shapes

        private UIElement DrawEllipse(InkAnalysisInkDrawing shape)
        {
            var ellipse = new Ellipse {Stroke = new SolidColorBrush(Colors.DimGray), StrokeThickness = 2};
            // Size
            var left = shape.Points[0];
            var top = shape.Points[1];
            var right = shape.Points[2];
            var bottom = shape.Points[3];
            ellipse.Width = Math.Sqrt(Math.Pow(left.X - right.X, 2) + Math.Pow(left.Y - right.Y, 2));
            ellipse.Height = Math.Sqrt(Math.Pow(top.X - bottom.X, 2) + Math.Pow(top.Y - bottom.Y, 2));
            // Transformation
            var transform = new TransformGroup();
            // Translation
            var translation = new TranslateTransform {X = shape.Center.X, Y = shape.Center.Y};
            transform.Children.Add(translation);
            // Rotation
            var angle = Math.Atan2(right.Y - left.Y, right.X - left.X);
            var rotation = new RotateTransform {Angle = angle * 180 / Math.PI};
            transform.Children.Add(rotation);
            // Rendering
            ellipse.RenderTransform = transform;
            _drawingCanvas.Children.Add(ellipse);
            return ellipse;
        }

        private UIElement DrawPolygon(InkAnalysisInkDrawing shape)
        {
            var polygon = new Polygon {Stroke = new SolidColorBrush(Colors.DimGray), StrokeThickness = 2};
            shape.Points.ToImmutableList().ForEach(point => polygon.Points?.Add(point));
            _drawingCanvas.Children.Add(polygon);
            return polygon;
        }

        #endregion
    }
}

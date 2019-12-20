using System;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using MyScript.IInk.Graphics;
using MyScript.InteractiveInk.Extensions;
using Color = Windows.UI.Color;
using FillRule = Windows.UI.Xaml.Media.FillRule;
using Path = MyScript.InteractiveInk.Views.Shapes.Path;
using Rectangle = Windows.UI.Xaml.Shapes.Rectangle;
using Transform = MyScript.IInk.Graphics.Transform;

namespace MyScript.InteractiveInk.Views.Controls
{
    internal sealed partial class DrawingCanvas
    {
        public DrawingCanvas()
        {
            InitializeComponent();
        }

        private void Draw(Shape shape)
        {
            shape.Fill = new SolidColorBrush(FillColor);
            shape.RenderTransform = Transform.ToPlatformTransform();
            shape.Stroke = new SolidColorBrush(StrokeColor);
            shape.StrokeDashArray = StrokeDashArray;
            shape.StrokeDashOffset = StrokeDashOffset;
            shape.StrokeDashCap = shape.StrokeEndLineCap = shape.StrokeStartLineCap = StrokeLineCap;
            shape.StrokeLineJoin = StrokeLineJoin;
            shape.StrokeMiterLimit = StrokeMiterLimit;
            shape.StrokeThickness = StrokeThickness;
            Children.Add(shape);
        }

        private void Draw(TextBlock text)
        {
            text.FontFamily = new FontFamily(FontFamily);
            text.FontSize = FontSize;
            text.FontStyle = FontStyle;
            text.FontWeight = FontWeight;
            text.LineHeight = LineHeight;
            text.RenderTransform = Transform.ToPlatformTransform();
            Typography.SetVariants(text, FontVariants);
            Children.Add(text);
        }
    }

    internal sealed partial class DrawingCanvas : ICanvas
    {
        private Color FillColor { get; set; }
        private FillRule FillRule { get; set; }
        private string FontFamily { get; set; }
        private double FontSize { get; set; }
        private FontStyle FontStyle { get; set; }
        private FontVariants FontVariants { get; set; }
        private FontWeight FontWeight { get; set; }
        private double LineHeight { get; set; }
        private Color StrokeColor { get; set; }
        private DoubleCollection StrokeDashArray { get; set; }
        private double StrokeDashOffset { get; set; }
        private PenLineCap StrokeLineCap { get; set; }
        private PenLineJoin StrokeLineJoin { get; set; }
        private double StrokeMiterLimit { get; set; }
        private double StrokeThickness { get; set; }

        public void StartGroup(string id, float x, float y, float width, float height, bool clipContent)
        {
            if (!clipContent)
            {
                return;
            }

            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var rectangle = new Rectangle {Name = id, Height = height, Width = width};
                SetLeft(rectangle, x);
                SetTop(rectangle, y);
                Draw(rectangle);
            }).AsTask();
        }

        public void EndGroup(string id)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => Children.Remove<FrameworkElement>(element => element.Name == id)).AsTask();
        }

        public void StartItem(string id)
        {
            //throw new NotImplementedException();
        }

        public void EndItem(string id)
        {
            //throw new NotImplementedException();
        }

        #region Rendering

        public IPath CreatePath()
        {
            return new Path();
        }

        public void DrawPath(IPath path)
        {
            if (!(path is Path target))
            {
                return;
            }

            if (target.Data is PathGeometry geometry)
            {
                geometry.FillRule = FillRule;
            }

            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Draw(target)).AsTask();
        }

        public void DrawRectangle(float x, float y, float width, float height)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var rectangle = new Rectangle {Height = height, Width = width};
                SetLeft(rectangle, x);
                SetTop(rectangle, y);
                Draw(rectangle);
            }).AsTask();
        }

        public void DrawLine(float x1, float y1, float x2, float y2)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => Draw(new Line {X1 = x1, Y1 = y1, X2 = x2, Y2 = y2})).AsTask();
        }

        public void DrawObject(string url, string mimeType, float x, float y, float width, float height)
        {
            throw new NotImplementedException();
        }

        public void DrawText(string label, float x, float y, float minX, float minY, float maxX, float maxY)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var text = new TextBlock {Text = label};
                SetLeft(text, Math.Max(minX, Math.Min(x, maxX)));
                SetTop(text, Math.Max(minY, Math.Min(y, maxY)));
                Draw(text);
            }).AsTask();
        }

        public Transform Transform { get; set; } = new Transform(1, 0, 0, 0, 1, 0);

        #endregion

        #region Styles

        public void SetStrokeColor(IInk.Graphics.Color color)
        {
            StrokeColor = color.ToPlatformColor();
        }

        public void SetStrokeWidth(float width)
        {
            StrokeThickness = width / 2;
        }

        public void SetStrokeLineCap(LineCap lineCap)
        {
            StrokeLineCap = lineCap switch
            {
                LineCap.BUTT => PenLineCap.Flat,
                LineCap.ROUND => PenLineCap.Round,
                LineCap.SQUARE => PenLineCap.Square,
                _ => throw new ArgumentOutOfRangeException(nameof(lineCap), lineCap, null)
            };
        }

        public void SetStrokeLineJoin(LineJoin lineJoin)
        {
            StrokeLineJoin = lineJoin switch
            {
                LineJoin.MITER => PenLineJoin.Miter,
                LineJoin.ROUND => PenLineJoin.Round,
                LineJoin.BEVEL => PenLineJoin.Bevel,
                _ => throw new ArgumentOutOfRangeException(nameof(lineJoin), lineJoin, null)
            };
        }

        public void SetStrokeMiterLimit(float limit)
        {
            StrokeMiterLimit = limit;
        }

        public void SetStrokeDashArray(float[] array)
        {
            if (array == null)
            {
                StrokeDashArray = null;
                return;
            }

            var collection = new DoubleCollection();
            foreach (var value in array)
            {
                collection.Add(value);
            }

            StrokeDashArray = collection;
        }

        public void SetStrokeDashOffset(float offset)
        {
            StrokeDashOffset = offset;
        }

        public void SetFillColor(IInk.Graphics.Color color)
        {
            FillColor = color.ToPlatformColor();
        }

        public void SetFillRule(IInk.Graphics.FillRule rule)
        {
            FillRule = rule switch
            {
                IInk.Graphics.FillRule.NONZERO => FillRule.Nonzero,
                IInk.Graphics.FillRule.EVENODD => FillRule.EvenOdd,
                _ => throw new ArgumentOutOfRangeException(nameof(rule), rule, null)
            };
        }

        public void SetFontProperties(string family, float lineHeight, float size, string style, string variant,
            int weight)
        {
            FontFamily = family;
            FontSize = size;
            FontStyle = Enum.Parse<FontStyle>(style, true);
            FontVariants = Enum.Parse<FontVariants>(variant, true);
            FontWeight = weight switch
            {
                var value when value >= 700 => FontWeights.Bold,
                var value when value < 400 => FontWeights.Light,
                _ => FontWeights.Normal
            };
            LineHeight = lineHeight;
        }

        #endregion
    }
}

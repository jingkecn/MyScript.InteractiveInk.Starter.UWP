using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using MyScript.IInk.Graphics;
using MyScript.InteractiveInk.UI.Extensions;
using Color = Windows.UI.Color;

namespace MyScript.InteractiveInk.UI.Commands
{
    public sealed partial class Canvas
    {
        public CanvasDrawingSession DrawingSession { get; set; }
    }

    /// <summary>
    ///     Implements <see cref="ICanvas" />.
    ///     <inheritdoc cref="ICanvas" />
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public sealed partial class Canvas : ICanvas
    {
        private CanvasActiveLayer ActiveLayer { get; set; }
        private Dictionary<string, Rect> Layers { get; } = new Dictionary<string, Rect>();

        public void StartGroup(string id, float x, float y, float width, float height, bool clipContent)
        {
            if (!clipContent)
            {
                return;
            }

            ActiveLayer?.Dispose();
            ActiveLayer = DrawingSession?.CreateLayer(1, Layers[id] = new Rect(x, y, width, height));
        }

        public void EndGroup(string id)
        {
            if (!Layers.ContainsKey(id))
            {
                return;
            }

            Layers.Remove(id, out var rect);
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            ActiveLayer?.Dispose();
            ActiveLayer = DrawingSession?.CreateLayer(1, rect);
        }

        public void StartItem(string id)
        {
        }

        public void EndItem(string id)
        {
        }

        #region Rendering

        public IPath CreatePath()
        {
            return new Path {PathBuilder = new CanvasPathBuilder(DrawingSession?.Device)};
        }

        public void DrawPath(IPath path)
        {
            if (!(path is Path target) || !(target.Geometry is { } geometry))
            {
                return;
            }

            DrawingSession?.DrawGeometry(geometry, StrokeColor, StrokeThickness, StrokeStyle);
            DrawingSession?.FillGeometry(geometry, FillColor);
        }

        public void DrawRectangle(float x, float y, float width, float height)
        {
            DrawingSession?.DrawRectangle(x, y, width, height, StrokeColor, StrokeThickness, StrokeStyle);
            DrawingSession?.FillRectangle(x, y, width, height, FillColor);
        }

        public void DrawLine(float x1, float y1, float x2, float y2)
        {
            DrawingSession?.DrawLine(x1, y1, x2, y2, StrokeColor, StrokeThickness, StrokeStyle);
        }

        public void DrawObject(string url, string mimeType, float x, float y, float width, float height)
        {
            throw new NotImplementedException();
        }

        public void DrawText(string label, float x, float y, float minX, float minY, float maxX, float maxY)
        {
            DrawingSession?.DrawText(label, Math.Max(minX, Math.Min(x, maxX)),
                Math.Max(minY, Math.Min(y, maxY)) - TextBaseLine, FillColor, TextFormat);
        }

        public Transform Transform
        {
            get => _transform ??= Matrix3x2.Identity.ToNative();
            set
            {
                _transform = value;
                if (DrawingSession == null)
                {
                    return;
                }

                DrawingSession.Transform = _transform.ToPlatform();
            }
        }

        private Transform _transform;

        #endregion

        #region Styles

        private Color FillColor { get; set; } = Colors.Black;
        private Color StrokeColor { get; set; } = Colors.Black;
        private CanvasStrokeStyle StrokeStyle { get; } = new CanvasStrokeStyle();
        private float StrokeThickness { get; set; } = 1;
        private float TextBaseLine { get; set; } = 1;
        private CanvasTextFormat TextFormat { get; } = new CanvasTextFormat();

        public void SetStrokeColor(IInk.Graphics.Color color)
        {
            StrokeColor = color.ToPlatform();
        }

        public void SetStrokeWidth(float width)
        {
            StrokeThickness = width;
        }

        public void SetStrokeLineCap(LineCap lineCap)
        {
            StrokeStyle.DashCap = lineCap switch
            {
                LineCap.BUTT => StrokeStyle.EndCap = StrokeStyle.StartCap = CanvasCapStyle.Flat,
                LineCap.ROUND => StrokeStyle.EndCap = StrokeStyle.StartCap = CanvasCapStyle.Round,
                LineCap.SQUARE => StrokeStyle.EndCap = StrokeStyle.StartCap = CanvasCapStyle.Square,
                _ => throw new ArgumentOutOfRangeException(nameof(lineCap), lineCap, null)
            };
        }

        public void SetStrokeLineJoin(LineJoin lineJoin)
        {
            StrokeStyle.LineJoin = lineJoin switch
            {
                LineJoin.MITER => CanvasLineJoin.Miter,
                LineJoin.ROUND => CanvasLineJoin.Round,
                LineJoin.BEVEL => CanvasLineJoin.Bevel,
                _ => throw new ArgumentOutOfRangeException(nameof(lineJoin), lineJoin, null)
            };
        }

        public void SetStrokeMiterLimit(float limit)
        {
            StrokeStyle.MiterLimit = limit;
        }

        public void SetStrokeDashArray(float[] array)
        {
            StrokeStyle.CustomDashStyle = array;
        }

        public void SetStrokeDashOffset(float offset)
        {
            StrokeStyle.DashOffset = offset;
        }

        public void SetFillColor(IInk.Graphics.Color color)
        {
            FillColor = color.ToPlatform();
        }

        public void SetFillRule(FillRule rule)
        {
            //throw new NotImplementedException();
        }

        public void SetFontProperties(string family, float lineHeight, float size, string style, string variant,
            int weight)
        {
            if (DrawingSession == null)
            {
                return;
            }

            TextFormat.FontFamily = family;
            TextFormat.FontSize = size;
            TextFormat.FontStyle = Enum.Parse<FontStyle>(style, true);
            TextFormat.FontWeight =
                weight >= 700 ? FontWeights.Bold : weight < 400 ? FontWeights.Light : FontWeights.Normal;
            using var layout =
                new CanvasTextLayout(DrawingSession?.Device, "k", TextFormat, float.MaxValue, float.MaxValue);
            TextBaseLine = layout.LineMetrics.FirstOrDefault().Baseline;
        }

        #endregion
    }

    /// <summary>
    ///     Implements <see cref="IDisposable" />.
    ///     <inheritdoc cref="IDisposable" />
    /// </summary>
    public sealed partial class Canvas : IDisposable
    {
        public void Dispose()
        {
            DrawingSession?.Flush();
            ActiveLayer?.Dispose();
            DrawingSession?.Dispose();
            DrawingSession = null;
        }
    }
}

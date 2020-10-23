using System;
using System.Numerics;
using Windows.Foundation;
using MyScript.IInk;
using MyScript.IInk.Graphics;
using MyScript.InteractiveInk.Annotations;
using Point = MyScript.IInk.Graphics.Point;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static partial class RendererExtensions
    {
        public static void Draw([NotNull] this Renderer source, Rect rect, LayerType layers, [NotNull] ICanvas canvas)
        {
            var (x, y, width, height) = ((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

            if (layers.HasFlag(LayerType.BACKGROUND))
            {
                source.DrawBackground(x, y, width, height, canvas);
            }

            if (layers.HasFlag(LayerType.MODEL))
            {
                source.DrawModel(x, y, width, height, canvas);
            }

            if (layers.HasFlag(LayerType.TEMPORARY))
            {
                source.DrawTemporaryItems(x, y, width, height, canvas);
            }

            if (layers.HasFlag(LayerType.CAPTURE))
            {
                source.DrawCaptureStrokes(x, y, width, height, canvas);
            }
        }
    }

    public static partial class RendererExtensions
    {
        public static void ChangeViewAt([NotNull] this Renderer source, Point position, Point translation, float scale,
            [CanBeNull] IRenderTarget target = null, [CanBeNull] Action<Point> clamp = null)
        {
            source.Scroll(translation, clamp: clamp);
            source.ZoomAt(position, scale);
            target?.Invalidate(source, LayerType.LayerType_ALL);
        }

        public static void Scroll([NotNull] this Renderer source, Point translation,
            [CanBeNull] IRenderTarget target = null, [CanBeNull] Action<Point> clamp = null)
        {
            var offset = new Point(source.ViewOffset.X, source.ViewOffset.Y);
            offset.X -= translation.X;
            offset.Y -= translation.Y;
            source.ScrollTo(offset, target, clamp);
        }

        public static void ScrollTo([NotNull] this Renderer source, Point offset,
            [CanBeNull] IRenderTarget target = null, [CanBeNull] Action<Point> clamp = null)
        {
            clamp?.Invoke(offset);
            source.ViewOffset = offset;
            target?.Invalidate(source, LayerType.LayerType_ALL);
        }
    }

    public static partial class RendererExtensions
    {
        public static Transform ToNative(this Matrix3x2 source)
        {
            return new Transform(
                source.M11, source.M21, source.M31,
                source.M12, source.M22, source.M32);
        }

        public static Matrix3x2 ToPlatform(this Transform source)
        {
            return new Matrix3x2(
                (float)source.XX, (float)source.XY,
                (float)source.YX, (float)source.YY,
                (float)source.TX, (float)source.TY);
        }
    }
}

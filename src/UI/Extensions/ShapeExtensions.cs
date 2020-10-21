using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using MyScript.IInk.Graphics;
using MyScript.InteractiveInk.Annotations;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static class ShapeExtensions
    {
        public static Rect Clamp(this Rect source, [NotNull] FrameworkElement element)
        {
            return RectHelper.Intersect(LayoutInformation.GetLayoutSlot(element), source);
        }

        [NotNull]
        public static Rectangle ToNative(this Rect source)
        {
            return new Rectangle((float)source.X, (float)source.Y,
                (float)source.Width, (float)source.Height);
        }
    }
}
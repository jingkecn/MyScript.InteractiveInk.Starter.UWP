using Windows.Foundation;
using MyScript.IInk.Graphics;

namespace MyScript.InteractiveInk.Extensions
{
    public static class RectangleExtensions
    {
        public static Rectangle ToNative(this Rect source)
        {
            return new Rectangle((float)source.X, (float)source.Y,
                (float)source.Width, (float)source.Height);
        }
    }
}

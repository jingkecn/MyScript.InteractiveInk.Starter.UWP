using Windows.UI.Xaml.Media;
using Transform = MyScript.IInk.Graphics.Transform;

namespace MyScript.InteractiveInk.Extensions
{
    public static class TransformExtensions
    {
        public static MatrixTransform ToPlatformTransform(this Transform source)
        {
            return new MatrixTransform
            {
                Matrix = new Matrix(
                    source.XX, source.XY,
                    source.YX, source.YY,
                    source.TX, source.TY)
            };
        }
    }
}

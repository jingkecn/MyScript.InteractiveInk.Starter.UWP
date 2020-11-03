using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static class ContentPartExtensions
    {
        [CanBeNull]
        public static ContentPart GetNext([NotNull] this ContentPart source)
        {
            if (!(source.Package is {} package))
            {
                return null;
            }

            var index = package.IndexOfPart(source);
            return index >= 0 && index < package.PartCount ? package.GetPart(++index) : null;
        }

        [CanBeNull]
        public static ContentPart GetPrevious([NotNull] this ContentPart source)
        {
            if (!(source.Package is { } package))
            {
                return null;
            }

            var index = package.IndexOfPart(source);
            return index > 0 && index < package.PartCount ? package.GetPart(--index) : null;
        }

        public static bool HasNext([NotNull] this ContentPart source)
        {
            var package = source.Package;
            return package.IndexOfPart(source) != package.PartCount - 1;
        }

        public static bool HasPrevious([NotNull] this ContentPart source)
        {
            var package = source.Package;
            return package.IndexOfPart(source) != 0;
        }
    }
}

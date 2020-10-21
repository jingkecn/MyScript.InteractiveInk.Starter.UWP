using System;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas.Text;
using MyScript.IInk.Graphics;
using MyScript.InteractiveInk.Annotations;
using Color = Windows.UI.Color;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static partial class StyleExtensions
    {
        public static Color ToPlatform(this IInk.Graphics.Color source)
        {
            return Color.FromArgb(
                Convert.ToByte(source.A),
                Convert.ToByte(source.R),
                Convert.ToByte(source.G),
                Convert.ToByte(source.B));
        }

        public static string ToHex(this IInk.Graphics.Color source)
        {
            return $"#{source.R:X2}{source.G:X2}{source.B:X2}{source.A:X2}";
        }

        public static IInk.Graphics.Color ToNative(this Color source)
        {
            return new IInk.Graphics.Color(source.R, source.G, source.B, source.A);
        }
    }

    public static partial class StyleExtensions
    {
        [NotNull]
        public static CanvasTextFormat GetTextFormat([NotNull] this CanvasTextLayout source, int characterIndex)
        {
            return new CanvasTextFormat
            {
                FontFamily = source.GetFontFamily(characterIndex),
                FontSize = source.GetFontSize(characterIndex),
                FontStretch = source.GetFontStretch(characterIndex),
                FontStyle = source.GetFontStyle(characterIndex),
                FontWeight = source.GetFontWeight(characterIndex)
            };
        }

        public static void SetTextFormat([NotNull] this CanvasTextLayout source, int characterIndex, int characterCount,
            [NotNull] CanvasTextFormat format)
        {
            source.SetFontFamily(characterIndex, characterCount, format.FontFamily);
            source.SetFontSize(characterIndex, characterCount, format.FontSize);
            source.SetFontStretch(characterIndex, characterCount, format.FontStretch);
            source.SetFontStyle(characterIndex, characterCount, format.FontStyle);
            source.SetFontWeight(characterIndex, characterCount, format.FontWeight);
        }

        [NotNull]
        public static CanvasTextFormat ToCanvasTextFormat([NotNull] this Style source, float dpi)
        {
            return new CanvasTextFormat
            {
                FontFamily = source.FontFamily,
                FontSize = source.FontSize.FromMillimeterToPixel(dpi),
                FontStyle = Enum.Parse<FontStyle>(source.FontStyle, true),
                FontWeight = source.FontWeight switch
                {
                    var value when value >= 700 => FontWeights.Bold,
                    var value when value < 400 => FontWeights.Light,
                    _ => FontWeights.Normal
                }
            };
        }
    }
}

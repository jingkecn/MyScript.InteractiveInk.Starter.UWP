using System;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas.Text;
using MyScript.IInk.Graphics;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static class StyleExtensions
    {
        public static CanvasTextFormat ToCanvasTextFormat(this Style source, float dpi)
        {
            return new CanvasTextFormat
            {
                FontFamily = source.FontFamily,
                FontSize = source.FontSize.FromMillimeterToPixel(dpi),
                FontStyle = Enum.Parse<FontStyle>(source.FontStyle, true),
                FontWeight = source.FontWeight >= 700 ? FontWeights.Bold :
                    source.FontWeight <= 400 ? FontWeights.Light : FontWeights.Normal
            };
        }
    }
}

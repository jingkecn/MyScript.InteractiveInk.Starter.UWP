using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using MyScript.IInk.Graphics;
using MyScript.IInk.Text;
using MyScript.InteractiveInk.Extensions;

namespace MyScript.InteractiveInk.Services
{
    // ReSharper disable once RedundantExtendsListEntry
    public partial class FontMetricsService : IFontMetricsProvider
    {
        public Rectangle[] GetCharacterBoundingBoxes(Text text, TextSpan[] spans)
        {
            return GetGlyphMetrics(text, spans).Select(metrics => metrics.BoundingBox).ToArray();
        }

        public float GetFontSizePx(Style style)
        {
            return style.FontSize;
        }
    }

    public partial class FontMetricsService : IFontMetricsProvider2
    {
        public bool SupportsGlyphMetrics()
        {
            return true;
        }

        public GlyphMetrics[] GetGlyphMetrics(Text text, TextSpan[] spans)
        {
            var metrics = new List<GlyphMetrics>();
            var layout = new RichEditBox {IsReadOnly = true};
            layout.Document.SetText(TextSetOptions.None, text.Label);
            spans.ToImmutableList().ForEach(span =>
            {
                var style = span.Style;
                var selection = layout.Document.Selection;
                selection.SetRange(span.BeginPosition, span.EndPosition);
                var format = selection.CharacterFormat;
                format.Name = style.FontFamily;
                format.FontStyle = Enum.Parse<FontStyle>(style.FontStyle, true);
                format.Size = style.FontSize;
                format.Weight = style.FontWeight;
                selection.GetRect(PointOptions.None, out var rect, out _);
                metrics.Add(new GlyphMetrics(rect.ToNative(), 0, 0));
            });
            return metrics.ToArray();
        }
    }
}

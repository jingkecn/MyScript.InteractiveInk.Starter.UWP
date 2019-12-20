using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.Toolkit.Helpers;
using MyScript.IInk;
using MyScript.IInk.Graphics;

namespace MyScript.InteractiveInk.Extensions
{
    public static class RendererExtensions
    {
        public static void Draw(this Renderer source, Rect rect, LayerType layers,
            Dictionary<LayerType, ICanvas> canvas)
        {
            var (x, y, width, height) = ((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            var style = Singleton<Style>.Instance;
            style.SetChangeFlags((uint)StyleFlag.StyleFlag_ALL);

            if (layers.HasFlag(LayerType.BACKGROUND))
            {
                style.ApplyTo(canvas[LayerType.BACKGROUND]);
                source.DrawBackground(x, y, width, height, canvas[LayerType.BACKGROUND]);
            }

            if (layers.HasFlag(LayerType.MODEL))
            {
                style.ApplyTo(canvas[LayerType.MODEL]);
                source.DrawModel(x, y, width, height, canvas[LayerType.MODEL]);
            }

            if (layers.HasFlag(LayerType.TEMPORARY))
            {
                style.ApplyTo(canvas[LayerType.TEMPORARY]);
                source.DrawTemporaryItems(x, y, width, height, canvas[LayerType.TEMPORARY]);
            }

            // ReSharper disable once InvertIf
            if (layers.HasFlag(LayerType.CAPTURE))
            {
                style.ApplyTo(canvas[LayerType.CAPTURE]);
                source.DrawCaptureStrokes(x, y, width, height, canvas[LayerType.CAPTURE]);
            }
        }
    }
}

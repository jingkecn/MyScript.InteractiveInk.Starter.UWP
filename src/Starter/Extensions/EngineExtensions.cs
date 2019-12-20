using System;
using Windows.Graphics.Display;
using MyScript.IInk;

namespace MyScript.InteractiveInk.Extensions
{
    public static class EngineExtensions
    {
        public static Renderer CreateRenderer(this Engine source, IRenderTarget target)
        {
            var info = DisplayInformation.GetForCurrentView();
            var (dpiX, dpiY, scale) = (info.RawDpiX, info.RawDpiY, (float)info.RawPixelsPerViewPixel);

            if (!(Math.Abs(scale) > 0))
            {
                scale = (float)(Convert.ToDouble(info.ResolutionScale) / 100);
            }

            if (scale > 0)
            {
                dpiX /= scale;
                dpiY /= scale;
            }

            if (!(Math.Abs(dpiX) > 0) || !(Math.Abs(dpiY) > 0))
            {
                dpiX = dpiY = 96;
            }

            return source.CreateRenderer(dpiX, dpiY, target);
        }
    }
}

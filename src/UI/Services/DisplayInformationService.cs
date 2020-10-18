using System;
using System.Numerics;
using Windows.Graphics.Display;

namespace MyScript.InteractiveInk.UI.Services
{
    public static class DisplayInformationService
    {
        private static Vector2? _dpi2;
        public static float Dpi => _dpi2.HasValue ? (_dpi2.Value.X + _dpi2.Value.Y) / 2.0f : 96;
        public static Vector2 Dpi2 => _dpi2 ??= GetDpi();

        private static Vector2 GetDpi()
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

            return new Vector2(dpiX, dpiY);
        }
    }
}

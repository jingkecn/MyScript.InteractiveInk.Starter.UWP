using System;
using Windows.Devices.Input;
using Windows.UI.Xaml.Controls;
using MyScript.IInk;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static class EnumExtensions
    {
        public static PointerType ToNative(this PointerDeviceType source, PointerType? predominance = null)
        {
            return source switch
            {
                PointerDeviceType.Touch => PointerType.TOUCH,
                PointerDeviceType.Pen => predominance ?? PointerType.PEN,
                PointerDeviceType.Mouse => predominance ?? PointerType.TOUCH,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };
        }

        public static PointerType ToNative(this InkToolbarTool source)
        {
            return source switch
            {
                InkToolbarTool.BallpointPen => PointerType.PEN,
                InkToolbarTool.Pencil => PointerType.PEN,
                InkToolbarTool.Highlighter => PointerType.PEN,
                InkToolbarTool.Eraser => PointerType.ERASER,
                InkToolbarTool.CustomPen => PointerType.PEN,
                InkToolbarTool.CustomTool => PointerType.TOUCH,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };
        }
    }
}

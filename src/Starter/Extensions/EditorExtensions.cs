using System;
using System.Diagnostics;
using Windows.Devices.Input;
using Windows.UI.Input;
using MyScript.IInk;

namespace MyScript.InteractiveInk.Extensions
{
    public static partial class EditorExtensions
    {
        private static readonly string Tag = typeof(EditorExtensions).Name;

        public static void PointerCancel(this Editor source, PointerPoint point)
        {
            Debug.WriteLine($"---------- {Tag}.{nameof(PointerCancel)} ----------");
            Debug.WriteLine($"{nameof(point)}: ({point.PointerId}, {point.Position}, {point.Timestamp})");
            source.PointerCancel((int)point.PointerId);
        }

        public static void PointerDown(this Editor source, PointerPoint point)
        {
            Debug.WriteLine($"---------- {Tag}.{nameof(PointerDown)} ----------");
            Debug.WriteLine($"{nameof(point)}: ({point.PointerId}, {point.Position}, {point.Timestamp})");
            source.PointerDown((float)point.Position.X, (float)point.Position.Y,
                (long)point.Timestamp, point.Properties.Pressure,
                point.PointerDevice.PointerDeviceType.ToNative(), (int)point.PointerId);
        }

        public static void PointerMove(this Editor source, PointerPoint point)
        {
            if (!point.IsInContact)
            {
                return;
            }

            Debug.WriteLine($"---------- {Tag}.{nameof(PointerMove)} ----------");
            Debug.WriteLine($"{nameof(point)}: ({point.PointerId}, {point.Position}, {point.Timestamp})");
            source.PointerMove((float)point.Position.X, (float)point.Position.Y,
                (long)point.Timestamp, point.Properties.Pressure,
                point.PointerDevice.PointerDeviceType.ToNative(), (int)point.PointerId);
        }

        public static void PointerUp(this Editor source, PointerPoint point)
        {
            Debug.WriteLine($"---------- {Tag}.{nameof(PointerUp)} ----------");
            Debug.WriteLine($"{nameof(point)}: ({point.PointerId}, {point.Position}, {point.Timestamp})");
            source.PointerUp((float)point.Position.X, (float)point.Position.Y,
                (long)point.Timestamp, point.Properties.Pressure,
                point.PointerDevice.PointerDeviceType.ToNative(), (int)point.PointerId);
        }
    }

    public static partial class EditorExtensions
    {
        public static PointerType ToNative(this PointerDeviceType source)
        {
            return source switch
            {
                PointerDeviceType.Touch => PointerType.TOUCH,
                PointerDeviceType.Pen => PointerType.PEN,
                PointerDeviceType.Mouse => PointerType.TOUCH,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };
        }
    }
}

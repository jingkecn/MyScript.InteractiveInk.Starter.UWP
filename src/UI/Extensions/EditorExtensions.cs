using System;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Input;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static partial class EditorExtensions
    {
        public static void Typeset(this Editor source, [CanBeNull] ContentBlock block = null)
        {
            var states = source.GetSupportedTargetConversionStates(block);
            if (!states.Any())
            {
                return;
            }

            source.Convert(block, states.First());
        }

        public static void Typeset(this Editor source, float x, float y)
        {
            source.Typeset(source.HitBlock(x, y));
        }

        public static void Typeset(this Editor source, Point position)
        {
            source.Typeset((float)position.X, (float)position.Y);
        }
    }

    public static partial class EditorExtensions
    {
        public static void WaitForIdleAndClear(this Editor source)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            source.Clear();
        }

        public static void WaitForIdleAndRedo(this Editor source)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            source.Redo();
        }

        public static void WaitForIdleAndTypeset(this Editor source, [CanBeNull] ContentBlock block = null)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            source.Typeset(block);
        }

        public static void WaitForIdleAndTypeset(this Editor source, float x, float y)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            source.Typeset(x, y);
        }

        public static void WaitForIdleAndTypeset(this Editor source, Point position)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            source.Typeset(position);
        }

        public static void WaitForIdleAndUndo(this Editor source)
        {
            if (!source.IsIdle())
            {
                source.WaitForIdle();
            }

            source.Undo();
        }
    }

    /// <summary>
    ///     Sends pointer events.
    ///     Please be careful with the timestamp: the editor accepts the pointer timestamp in milliseconds, whereas the UWP
    ///     pointer timestamp is in microseconds, therefore, a conversion between these two units is a must.
    ///     Otherwise, you would encounter the following error when handling pointer events on the text document part:
    ///     - ink rejected: stroke is too long.
    ///     This is because, on the text document part, the interactive-ink SDK checks the interval time between the first
    ///     point (pointer down) and the last point (pointer up / cancel) of a single stroke, and raises the error if the
    ///     interval time is too high (> 15s).
    /// </summary>
    public static partial class EditorExtensions
    {
        public static void PointerCancel(this Editor source, PointerPoint point)
        {
            Debug.WriteLine($"---------- {nameof(EditorExtensions)}.{nameof(PointerCancel)} ----------");
            Debug.WriteLine($"{nameof(point)}: ({nameof(point.PointerId)}={point.PointerId};" +
                            $"{nameof(point.Position)}={point.Position};" +
                            $"{nameof(point.Timestamp)}={point.Timestamp.FromMicrosecondsToMilliseconds()})");
            source.PointerCancel((int)point.PointerId);
        }

        public static void PointerDown(this Editor source, PointerPoint point)
        {
            Debug.WriteLine($"---------- {nameof(EditorExtensions)}.{nameof(PointerDown)} ----------");
            Debug.WriteLine($"{nameof(point)}: ({nameof(point.PointerId)}={point.PointerId};" +
                            $"{nameof(point.Position)}={point.Position};" +
                            $"{nameof(point.Timestamp)}={point.Timestamp.FromMicrosecondsToMilliseconds()})");
            source.PointerDown((float)point.Position.X, (float)point.Position.Y,
                (long)point.Timestamp.FromMicrosecondsToMilliseconds(), point.Properties.Pressure,
                point.PointerDevice.PointerDeviceType.ToNative(), (int)point.PointerId);
        }

        public static void PointerMove(this Editor source, PointerPoint point)
        {
            if (!point.IsInContact)
            {
                return;
            }

            Debug.WriteLine($"---------- {nameof(EditorExtensions)}.{nameof(PointerMove)} ----------");
            Debug.WriteLine($"{nameof(point)}: ({nameof(point.PointerId)}={point.PointerId};" +
                            $"{nameof(point.Position)}={point.Position};" +
                            $"{nameof(point.Timestamp)}={point.Timestamp.FromMicrosecondsToMilliseconds()})");
            source.PointerMove((float)point.Position.X, (float)point.Position.Y,
                (long)point.Timestamp.FromMicrosecondsToMilliseconds(), point.Properties.Pressure,
                point.PointerDevice.PointerDeviceType.ToNative(), (int)point.PointerId);
        }

        public static void PointerUp(this Editor source, PointerPoint point)
        {
            Debug.WriteLine($"---------- {nameof(EditorExtensions)}.{nameof(PointerUp)} ----------");
            Debug.WriteLine($"{nameof(point)}: ({nameof(point.PointerId)}={point.PointerId};" +
                            $"{nameof(point.Position)}={point.Position};" +
                            $"{nameof(point.Timestamp)}={point.Timestamp.FromMicrosecondsToMilliseconds()})");
            source.PointerUp((float)point.Position.X, (float)point.Position.Y,
                (long)point.Timestamp.FromMicrosecondsToMilliseconds(), point.Properties.Pressure,
                point.PointerDevice.PointerDeviceType.ToNative(), (int)point.PointerId);
        }
    }

    public static partial class EditorExtensions
    {
        public static PointerType ToNative(this PointerDeviceType source)
        {
            switch (source)
            {
                case PointerDeviceType.Touch:
                    return PointerType.TOUCH;
                case PointerDeviceType.Pen:
                    return PointerType.PEN;
                case PointerDeviceType.Mouse:
                    return PointerType.TOUCH;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }
    }
}

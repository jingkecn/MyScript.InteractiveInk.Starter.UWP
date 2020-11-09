using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Windows.Devices.Input;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.Geometry;
using MyScript.IInk;
using MyScript.IInk.Graphics;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Common.Enumerations;

namespace MyScript.InteractiveInk.Extensions
{
    public static partial class EnumExtensions
    {
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

    public static partial class EnumExtensions
    {
        public static CanvasFilledRegionDetermination ToPlatform(this FillRule source)
        {
            return source switch
            {
                FillRule.NONZERO => CanvasFilledRegionDetermination.Winding,
                FillRule.EVENODD => CanvasFilledRegionDetermination.Alternate,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };
        }
    }

    public static partial class EnumExtensions
    {
        public static CanvasCapStyle ToPlatform(this LineCap source)
        {
            return source switch
            {
                LineCap.BUTT => CanvasCapStyle.Flat,
                LineCap.ROUND => CanvasCapStyle.Round,
                LineCap.SQUARE => CanvasCapStyle.Square,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };
        }

        public static CanvasLineJoin ToPlatform(this LineJoin source)
        {
            return source switch
            {
                LineJoin.MITER => CanvasLineJoin.Miter,
                LineJoin.ROUND => CanvasLineJoin.Round,
                LineJoin.BEVEL => CanvasLineJoin.Bevel,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };
        }
    }

    public static partial class EnumExtensions
    {
        public static string ToNative(this ContentType source)
        {
            var type = typeof(ContentType);
            var name = Enum.GetName(type, source);
            return type.GetField(name).GetCustomAttributes<DisplayNameAttribute>().First().DisplayName;
        }

        public static ContentType ToPlatformContentType([NotNull] this string source)
        {
            if (source == ContentType.Diagram.ToNative())
            {
                return ContentType.Diagram;
            }

            if (source == ContentType.Math.ToNative())
            {
                return ContentType.Math;
            }

            if (source == ContentType.RawContent.ToNative())
            {
                return ContentType.RawContent;
            }

            if (source == ContentType.Text.ToNative())
            {
                return ContentType.Text;
            }

            if (source == ContentType.TextDocument.ToNative())
            {
                return ContentType.TextDocument;
            }

            throw new ArgumentOutOfRangeException(nameof(source), source, null);
        }
    }

    public static partial class EnumExtensions
    {
        public static PointerType ToNative(this PointerDeviceType source, PointerType? predominance = null)
        {
            return source switch
            {
                PointerDeviceType.Touch => predominance ?? PointerType.TOUCH,
                PointerDeviceType.Pen => predominance ?? PointerType.PEN,
                PointerDeviceType.Mouse => predominance ?? PointerType.TOUCH,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
            };
        }
    }
}

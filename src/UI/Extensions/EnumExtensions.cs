using System;
using Windows.Devices.Input;
using MyScript.IInk;

namespace MyScript.InteractiveInk.UI.Extensions
{
    public static class EnumExtensions
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

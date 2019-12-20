using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyScript.InteractiveInk.Extensions
{
    public static class UiExtensions
    {
        public static void Remove<T>(this UIElementCollection source, Predicate<T> predicate) where T : UIElement
        {
            var items = source.OfType<T>().Where(item => predicate(item));
            foreach (var item in items)
            {
                source.Remove(item);
            }
        }
    }
}

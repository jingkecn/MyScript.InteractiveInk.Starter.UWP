using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using MyScript.IInk;
using MyScript.IInk.Graphics;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Extensions;
using Point = Windows.Foundation.Point;

namespace MyScript.InteractiveInk.Views.Controls
{
    public sealed partial class InteractiveInkCanvas
    {
        public static readonly DependencyProperty EditorProperty =
            DependencyProperty.Register("Editor", typeof(Editor), typeof(InteractiveInkCanvas),
                new PropertyMetadata(default(Editor)));

        public InteractiveInkCanvas()
        {
            InitializeComponent();
        }

        public Editor Editor
        {
            get => GetValue(EditorProperty) as Editor;
            set => SetValue(EditorProperty, value);
        }
    }

    public sealed partial class InteractiveInkCanvas : IDisposable
    {
        public void Dispose()
        {
        }

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private void Initialize([NotNull] Editor editor)
        {
            editor.SetViewSize((int)ActualWidth, (int)ActualHeight);
        }
    }

    public sealed partial class InteractiveInkCanvas : IRenderTarget
    {
        public void Invalidate(Renderer renderer, int x, int y, int width, int height, LayerType layers)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => Invalidate(renderer, Clamp(x, y, width, height), layers)).AsTask();
        }

        public void Invalidate(Renderer renderer, LayerType layers)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => Invalidate(renderer, LayoutInformation.GetLayoutSlot(this), layers)).AsTask();
        }

        private void Invalidate([NotNull] Renderer renderer, Rect rect, LayerType layers)
        {
            renderer?.Draw(rect, layers,
                new Dictionary<LayerType, ICanvas>
                {
                    {LayerType.BACKGROUND, BackgroundLayer},
                    {LayerType.MODEL, ModelLayer},
                    {LayerType.TEMPORARY, TemporaryLayer},
                    {LayerType.CAPTURE, CaptureLayer}
                });
        }

        private Rect Clamp(int x, int y, int width, int height)
        {
            var left = Math.Max(0, x);
            var top = Math.Max(0, y);
            var right = Math.Min(x + width, ActualWidth);
            var bottom = Math.Min(y + height, ActualHeight);
            return new Rect(new Point(left, top), new Point(right, bottom));
        }
    }

    public sealed partial class InteractiveInkCanvas
    {
        private void CaptureLayer_OnPointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            Editor.PointerCancel(e.GetCurrentPoint(element));
            e.Handled = true;
        }

        private void CaptureLayer_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            Editor.PointerMove(e.GetCurrentPoint(element));
            e.Handled = true;
        }

        private void CaptureLayer_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            Editor.PointerDown(e.GetCurrentPoint(element));
            e.Handled = true;
        }

        private void CaptureLayer_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            Editor.PointerUp(e.GetCurrentPoint(element));
        }
    }

    public sealed partial class InteractiveInkCanvas
    {
        private void InteractiveInkCanvas_OnLoaded(object sender, RoutedEventArgs e)
        {
            Initialize(Editor);
        }

        private void InteractiveInkCanvas_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Editor.SetViewSize((int)e.NewSize.Width, (int)e.NewSize.Height);
            foreach (var element in Children.OfType<FrameworkElement>())
            {
                element.Width = e.NewSize.Width;
                element.Height = e.NewSize.Height;
            }
        }

        private void InteractiveInkCanvas_OnUnloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }
    }
}

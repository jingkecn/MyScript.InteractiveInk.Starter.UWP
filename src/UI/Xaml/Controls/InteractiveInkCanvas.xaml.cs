using System;
using System.Diagnostics;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.UI.Commands;
using MyScript.InteractiveInk.UI.Extensions;

namespace MyScript.InteractiveInk.UI.Xaml.Controls
{
    public sealed partial class InteractiveInkCanvas
    {
        public static readonly DependencyProperty EditorProperty =
            DependencyProperty.Register("Editor", typeof(Editor), typeof(InteractiveInkCanvas),
                new PropertyMetadata(default(Editor)));

        public static readonly DependencyProperty PredominantInputProperty =
            DependencyProperty.Register("PredominantInput", typeof(PointerType), typeof(InteractiveInkCanvas),
                new PropertyMetadata(default(PointerType)));

        public InteractiveInkCanvas()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     <inheritdoc cref="Editor" />
        /// </summary>
        [CanBeNull]
        public Editor Editor
        {
            get => GetValue(EditorProperty) as Editor;
            set => SetValue(EditorProperty, value);
        }

        /// <summary>
        ///     <inheritdoc cref="PointerType" />
        /// </summary>
        public PointerType PredominantInput
        {
            get => (PointerType)GetValue(PredominantInputProperty);
            set => SetValue(PredominantInputProperty, value);
        }

        /// <summary>
        ///     <inheritdoc cref="Renderer" />
        /// </summary>
        [CanBeNull]
        public Renderer Renderer => Editor?.Renderer;
    }

    /// <summary>
    ///     Implements <see cref="IRenderTarget" />.
    ///     <inheritdoc cref="IRenderTarget" />
    /// </summary>
    public sealed partial class InteractiveInkCanvas : IRenderTarget
    {
        public void Invalidate(Renderer renderer, int x, int y, int width, int height, LayerType layers)
        {
            var region = new Rect(x, y, width, height);
            if (region.IsEmpty)
            {
                return;
            }

            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Invalidate(region.Clamp(this), layers)).AsTask();
        }

        public void Invalidate(Renderer renderer, LayerType layers)
        {
            Invalidate(renderer, 0, 0, (int)ActualWidth, (int)ActualHeight, layers);
        }

        private void Invalidate(Rect region, LayerType layers)
        {
            if (region.IsEmpty)
            {
                return;
            }

            if (layers.HasFlag(LayerType.BACKGROUND))
            {
                BackgroundLayer.Invalidate(region.Clamp(BackgroundLayer));
            }

            if (layers.HasFlag(LayerType.CAPTURE))
            {
                CaptureLayer.Invalidate(region.Clamp(CaptureLayer));
            }

            if (layers.HasFlag(LayerType.MODEL))
            {
                ModelLayer.Invalidate(region.Clamp(ModelLayer));
            }

            if (layers.HasFlag(LayerType.TEMPORARY))
            {
                TemporaryLayer.Invalidate(region.Clamp(TemporaryLayer));
            }
        }
    }

    /// <summary>
    ///     Handles gestures.
    /// </summary>
    public sealed partial class InteractiveInkCanvas
    {
        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (!(sender is UIElement element) || e.PointerDeviceType == PointerDeviceType.Pen)
            {
                return;
            }

            var position = e.GetPosition(element);
            Editor?.Typeset(position);
        }
    }

    /// <summary>
    ///     Handles manipulations.
    /// </summary>
    public sealed partial class InteractiveInkCanvas
    {
        private const GestureSettings DefaultSettings =
            GestureSettings.ManipulationTranslateInertia |
            GestureSettings.ManipulationTranslateY;

        private GestureRecognizer _gestureRecognizer;

        private GestureRecognizer GestureRecognizer =>
            _gestureRecognizer ??= new GestureRecognizer {GestureSettings = DefaultSettings};

        private void OnManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            if (args.PointerDeviceType == PointerDeviceType.Pen || !Editor.IsScrollAllowed())
            {
                return;
            }

            Debug.WriteLine($"{nameof(InteractiveInkCanvas)}.{nameof(OnManipulationUpdated)}");
            Renderer?.ChangeViewAt(args.Position.ToNative(), args.Delta.Translation.ToNative(), args.Delta.Scale, this,
                offset => Editor?.ClampViewOffset(offset));
        }
    }

    /// <summary>
    ///     Handles pointer events.
    /// </summary>
    public sealed partial class InteractiveInkCanvas
    {
        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            element.CapturePointer(e.Pointer);
            var point = e.GetCurrentPoint(element);
            if (PredominantInput != PointerType.ERASER)
            {
                PredominantInput = point.PointerDevice.PointerDeviceType.ToNative();
            }

            GestureRecognizer.ProcessDownEvent(point);
            Editor?.PointerDown(point, PredominantInput);
            e.Handled = true;
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            var point = e.GetCurrentPoint(element);
            var points = e.GetIntermediatePoints(element);
            GestureRecognizer.ProcessMoveEvents(points);
            Editor?.PointerMove(point, PredominantInput);
            e.Handled = true;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            var point = e.GetCurrentPoint(element);
            GestureRecognizer.ProcessUpEvent(point);
            Editor?.PointerUp(point, PredominantInput);
            element.ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }

        private void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            if (!(sender is UIElement element))
            {
                return;
            }

            GestureRecognizer.CompleteGesture();
            var point = e.GetCurrentPoint(element);
            Editor?.PointerCancel(point);
            element.ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }
    }

    /// <summary>
    ///     Handles regional invalidation events.
    /// </summary>
    public sealed partial class InteractiveInkCanvas
    {
        private void OnRegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            var layer = sender.Name switch
            {
                nameof(BackgroundLayer) => LayerType.BACKGROUND,
                nameof(CaptureLayer) => LayerType.CAPTURE,
                nameof(ModelLayer) => LayerType.MODEL,
                nameof(TemporaryLayer) => LayerType.TEMPORARY,
                _ => LayerType.LayerType_ALL
            };

            foreach (var region in args.InvalidatedRegions)
            {
                if (region.IsEmpty)
                {
                    continue;
                }

                var clamped = region.Clamp(sender);
                using var canvas = new Canvas {DrawingSession = sender.CreateDrawingSession(clamped)};
                Renderer?.Draw(clamped, layer, canvas);
            }
        }
    }

    /// <summary>
    ///     Handles lifecycle events.
    /// </summary>
    public sealed partial class InteractiveInkCanvas
    {
        private void OnLoaded(object sender, RoutedEventArgs _)
        {
            Editor?.SetViewSize((int)ActualWidth, (int)ActualHeight);
            GestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Editor?.SetViewSize((int)e.NewSize.Width, (int)e.NewSize.Height);
        }

        private void OnUnloaded(object sender, RoutedEventArgs _)
        {
            GestureRecognizer.CompleteGesture();
            GestureRecognizer.ManipulationUpdated -= OnManipulationUpdated;
            BackgroundLayer.RemoveFromVisualTree();
            CaptureLayer.RemoveFromVisualTree();
            ModelLayer.RemoveFromVisualTree();
            TemporaryLayer.RemoveFromVisualTree();
        }
    }
}

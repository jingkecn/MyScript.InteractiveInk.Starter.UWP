using System.Collections.Generic;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MyScript.InteractiveInk.Services.Ink
{
    public class InkTransformResult
    {
        public InkTransformResult(Canvas drawingCanvas)
        {
            DrawingCanvas = drawingCanvas;
        }

        public Canvas DrawingCanvas { get; }
        public List<UIElement> Elements { get; } = new List<UIElement>();
        public List<InkStroke> Strokes { get; } = new List<InkStroke>();
    }
}

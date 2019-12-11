using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;

namespace MyScript.InteractiveInk.Services.Ink.UndoRedo
{
    public class ClearAllUndoRedoOperation : StrokesUndoRedoOperation
    {
        private readonly List<UIElement> _elements;
        private readonly InkTransformService _transformService;

        public ClearAllUndoRedoOperation(InkStrokeService strokeService, IEnumerable<InkStroke> strokes,
            InkTransformService transformService, IEnumerable<UIElement> elements) : base(strokeService, strokes)
        {
            _elements = elements.ToList();
            _transformService = transformService;
        }

        public override void ExecuteRedo()
        {
            StrokeService.Remove(Strokes.ToArray());
            _transformService.Remove(_elements.ToArray());
        }

        public override void ExecuteUndo()
        {
            StrokeService.Add(Strokes.ToArray());
            _transformService.Add(_elements.ToArray());
        }
    }
}

using System.Collections.Immutable;
using MyScript.InteractiveInk.Events;

namespace MyScript.InteractiveInk.Services.Ink.UndoRedo
{
    public class TransformUndoRedoOperation : UndoRedoOperation
    {
        private readonly InkTransformResult _transformResult;

        public TransformUndoRedoOperation(InkStrokeService strokeService, InkTransformResult transformResult)
            : base(strokeService)
        {
            _transformResult = transformResult;
        }

        public override void ExecuteRedo()
        {
            // Remove strokes
            StrokeService.Remove(_transformResult.Strokes.ToArray());
            // Add text and shapes
            _transformResult.Elements.ToImmutableList().ForEach(_transformResult.DrawingCanvas.Children.Add);
        }

        public override void ExecuteUndo()
        {
            // Remove text and shapes
            _transformResult.Elements.ToImmutableList().ForEach(element =>
            {
                if (_transformResult.DrawingCanvas.Children.Contains(element))
                {
                    _transformResult.DrawingCanvas.Children.Remove(element);
                }
            });
            // Add strokes
            StrokeService.Add(_transformResult.Strokes.ToArray());
        }

        protected override void OnAddStroke(object sender, AddStrokeEventArgs e)
        {
            if (e.NewStroke == null)
            {
                return;
            }

            var changes = _transformResult.Strokes.RemoveAll(stroke => stroke.Id == e.OldStroke?.Id);
            if (changes > 0)
            {
                _transformResult.Strokes.Add(e.NewStroke);
            }
        }
    }
}

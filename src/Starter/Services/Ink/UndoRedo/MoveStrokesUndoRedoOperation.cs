using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace MyScript.InteractiveInk.Services.Ink.UndoRedo
{
    public class MoveStrokesUndoRedoOperation : StrokesUndoRedoOperation
    {
        private readonly Point _fromPosition;
        private readonly Point _toPosition;

        public MoveStrokesUndoRedoOperation(InkStrokeService strokeService, IEnumerable<InkStroke> strokes,
            Point fromPosition, Point toPosition) : base(strokeService, strokes)
        {
            _fromPosition = fromPosition;
            _toPosition = toPosition;
        }

        public override void ExecuteRedo()
        {
            StrokeService.Select(Strokes.ToArray());
            StrokeService.Move(_fromPosition, _toPosition);
        }

        public override void ExecuteUndo()
        {
            StrokeService.Select(Strokes.ToArray());
            StrokeService.Move(_toPosition, _fromPosition);
        }
    }
}

using System.Collections.Generic;
using Windows.UI.Input.Inking;

namespace MyScript.InteractiveInk.Services.Ink.UndoRedo
{
    public class RemoveStrokesUndoRedoOperation : StrokesUndoRedoOperation
    {
        public RemoveStrokesUndoRedoOperation(InkStrokeService strokeService, IEnumerable<InkStroke> strokes)
            : base(strokeService, strokes)
        {
        }

        public override void ExecuteRedo()
        {
            StrokeService.Remove(Strokes.ToArray());
        }

        public override void ExecuteUndo()
        {
            StrokeService.Add(Strokes.ToArray());
        }
    }
}

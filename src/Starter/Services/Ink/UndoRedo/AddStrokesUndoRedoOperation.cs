using System.Collections.Generic;
using Windows.UI.Input.Inking;

namespace MyScript.InteractiveInk.Services.Ink.UndoRedo
{
    public class AddStrokesUndoRedoOperation : StrokesUndoRedoOperation
    {
        public AddStrokesUndoRedoOperation(InkStrokeService strokeService, IEnumerable<InkStroke> strokes)
            : base(strokeService, strokes)
        {
        }

        public override void ExecuteRedo()
        {
            StrokeService.Add(Strokes.ToArray());
        }

        public override void ExecuteUndo()
        {
            StrokeService.Remove(Strokes.ToArray());
        }
    }
}

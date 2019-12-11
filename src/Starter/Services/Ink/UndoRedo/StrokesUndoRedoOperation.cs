using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;
using MyScript.InteractiveInk.Events;

namespace MyScript.InteractiveInk.Services.Ink.UndoRedo
{
    public abstract class StrokesUndoRedoOperation : UndoRedoOperation
    {
        protected readonly List<InkStroke> Strokes;

        protected StrokesUndoRedoOperation(InkStrokeService strokeService, IEnumerable<InkStroke> strokes)
            : base(strokeService)
        {
            Strokes = strokes.ToList();
        }

        protected sealed override void OnAddStroke(object sender, AddStrokeEventArgs e)
        {
            if (e.NewStroke == null)
            {
                return;
            }

            var changes = Strokes.RemoveAll(stroke => stroke.Id == e.OldStroke?.Id);
            if (changes > 0)
            {
                Strokes.Add(e.NewStroke);
            }
        }
    }
}

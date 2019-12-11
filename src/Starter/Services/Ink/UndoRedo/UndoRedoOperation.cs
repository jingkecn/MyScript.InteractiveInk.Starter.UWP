using System;
using MyScript.InteractiveInk.Events;

namespace MyScript.InteractiveInk.Services.Ink.UndoRedo
{
    public abstract partial class UndoRedoOperation
    {
        protected readonly InkStrokeService StrokeService;

        protected UndoRedoOperation(InkStrokeService strokeService)
        {
            StrokeService = strokeService;
            StrokeService.AddStroke += OnAddStroke;
        }

        protected abstract void OnAddStroke(object sender, AddStrokeEventArgs e);
    }

    public abstract partial class UndoRedoOperation : IDisposable
    {
        public virtual void Dispose()
        {
            StrokeService.AddStroke -= OnAddStroke;
        }
    }

    public abstract partial class UndoRedoOperation : IUndoRedoOperation
    {
        public abstract void ExecuteRedo();
        public abstract void ExecuteUndo();
    }
}

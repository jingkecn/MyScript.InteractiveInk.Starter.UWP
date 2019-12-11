using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Events;
using MyScript.InteractiveInk.Services.Ink.UndoRedo;

namespace MyScript.InteractiveInk.Services.Ink
{
    public partial class InkUndoRedoService
    {
        private readonly Stack<IUndoRedoOperation> _redoStack = new Stack<IUndoRedoOperation>();
        private readonly InkStrokeService _strokeService;
        private readonly Stack<IUndoRedoOperation> _undoStack = new Stack<IUndoRedoOperation>();

        public InkUndoRedoService(InkStrokeService strokeService)
        {
            _strokeService = strokeService;
            _strokeService.CutStrokes += OnCutStrokes;
            _strokeService.MoveStrokes += OnMoveStrokes;
            _strokeService.PasteStrokes += OnPasteStrokes;
            _strokeService.StrokesCollected += OnStrokesCollected;
            _strokeService.StrokesErased += OnStrokesErased;
        }

        public bool CanRedo => _redoStack.Any();
        public bool CanUndo => _undoStack.Any();

        public void Add([NotNull] IUndoRedoOperation operation)
        {
            _redoStack.Clear();
            _undoStack.Push(operation);
            OnAddOperation(this, EventArgs.Empty);
        }

        #region Execution

        public void Redo()
        {
            if (!CanRedo)
            {
                return;
            }

            _strokeService.MoveStrokes -= OnMoveStrokes;
            var operation = _redoStack.Pop();
            operation.ExecuteRedo();
            _undoStack.Push(operation);
            _strokeService.MoveStrokes += OnMoveStrokes;
            OnExecuteRedo(this, EventArgs.Empty);
        }

        public void Undo()
        {
            if (!CanUndo)
            {
                return;
            }

            _strokeService.MoveStrokes -= OnMoveStrokes;
            var operation = _undoStack.Pop();
            operation.ExecuteUndo();
            _redoStack.Push(operation);
            _strokeService.MoveStrokes += OnMoveStrokes;
            OnExecuteUndo(this, EventArgs.Empty);
        }

        #endregion
    }

    public partial class InkUndoRedoService : IDisposable
    {
        public void Dispose()
        {
            _strokeService.CutStrokes -= OnCutStrokes;
            _strokeService.MoveStrokes -= OnMoveStrokes;
            _strokeService.PasteStrokes -= OnPasteStrokes;
            _strokeService.StrokesCollected -= OnStrokesCollected;
            _strokeService.StrokesErased -= OnStrokesErased;
            _redoStack.Clear();
            _undoStack.Clear();
        }
    }

    public partial class InkUndoRedoService
    {
        private void OnCutStrokes(object sender, TransferStrokesEventArgs e)
        {
            Add(new RemoveStrokesUndoRedoOperation(_strokeService, e.Strokes));
        }

        private void OnMoveStrokes(object sender, MoveStrokesEventArgs e)
        {
            Add(new MoveStrokesUndoRedoOperation(_strokeService, e.Strokes, e.FromPosition, e.ToPosition));
        }

        private void OnPasteStrokes(object sender, TransferStrokesEventArgs e)
        {
            Add(new AddStrokesUndoRedoOperation(_strokeService, e.Strokes));
        }

        private void OnStrokesCollected(object sender, InkStrokesCollectedEventArgs e)
        {
            Add(new AddStrokesUndoRedoOperation(_strokeService, e.Strokes));
        }

        private void OnStrokesErased(object sender, InkStrokesErasedEventArgs e)
        {
            Add(new RemoveStrokesUndoRedoOperation(_strokeService, e.Strokes));
        }
    }

    public partial class InkUndoRedoService
    {
        public event EventHandler<EventArgs> AddOperation;
        public event EventHandler<EventArgs> ExecuteRedo;
        public event EventHandler<EventArgs> ExecuteUndo;

        protected virtual void OnAddOperation(object sender, EventArgs args)
        {
            AddOperation?.Invoke(sender, args);
        }

        protected virtual void OnExecuteRedo(object sender, EventArgs args)
        {
            ExecuteRedo?.Invoke(sender, args);
        }

        protected virtual void OnExecuteUndo(object sender, EventArgs args)
        {
            ExecuteUndo?.Invoke(sender, args);
        }
    }
}

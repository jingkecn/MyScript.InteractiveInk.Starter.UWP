using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using MyScript.InteractiveInk.Events;

namespace MyScript.InteractiveInk.Services
{
    public partial class InkStrokeService
    {
        private readonly InkStrokeContainer _inkStrokeContainer;

        public InkStrokeService(InkCanvas canvas)
        {
            var presenter = canvas.InkPresenter;
            presenter.StrokesErased += OnStrokesErased;
            presenter.StrokesCollected += OnStrokesCollected;
            _inkStrokeContainer = presenter.StrokeContainer;
        }

        public IEnumerable<InkStroke> SelectedStrokes => Strokes.Where(stroke => stroke.Selected);

        private Rect SelectionRect =>
            _inkStrokeContainer.GetStrokes().Where(stroke => stroke.Selected).Aggregate(Rect.Empty, (rect, stroke) =>
            {
                rect.Union(stroke.BoundingRect);
                return rect;
            });

        public IEnumerable<InkStroke> Strokes => _inkStrokeContainer.GetStrokes();

        #region Add, Move & Clear

        public InkStroke Add(InkStroke stroke)
        {
            var strokeToAdd = stroke.Clone();
            _inkStrokeContainer.AddStroke(strokeToAdd);
            OnAddStroke(this, new AddStrokeEventArgs {NewStroke = strokeToAdd, OldStroke = stroke});
            return strokeToAdd;
        }

        public void Clear()
        {
            _inkStrokeContainer.Clear();
            OnClearStrokes(this, EventArgs.Empty);
        }

        public void Move(Point from, Point to)
        {
            var x = to.X - from.X;
            var y = to.Y - from.Y;
            var matrix = Matrix3x2.CreateTranslation((float)x, (float)y);
            if (matrix.IsIdentity)
            {
                return;
            }

            var strokesToMove = SelectedStrokes.ToImmutableList();
            strokesToMove.ForEach(stroke => stroke.PointTransform *= matrix);
            OnMoveStrokes(this, new MoveStrokesEventArgs {From = from, To = to, Strokes = strokesToMove});
        }

        public bool Remove(InkStroke stroke)
        {
            var strokes = _inkStrokeContainer.GetStrokes();
            var strokeToRemove = strokes.SingleOrDefault(s => s.Id == stroke.Id);
            if (strokeToRemove == null)
            {
                return false;
            }

            ClearSelection();

            // Select the target strokes and remove it.
            strokeToRemove.Selected = true;
            _inkStrokeContainer.DeleteSelected();

            OnRemoveStroke(this, new RemoveStrokeEventArgs {RemovedStroke = strokeToRemove});

            return true;
        }

        public bool Remove(IEnumerable<uint> ids)
        {
            var enumerable = ids.ToImmutableList();
            enumerable.Select(id => _inkStrokeContainer.GetStrokeById(id)).ToImmutableList()
                .ForEach(item => Remove(item));
            return _inkStrokeContainer.GetStrokes().Any(stroke => enumerable.Contains(stroke.Id));
        }

        #endregion

        #region Selection

        public void ClearSelection()
        {
            _inkStrokeContainer.GetStrokes().ToImmutableList().ForEach(stroke => stroke.Selected = false);
            OnSelectStrokes(this, EventArgs.Empty);
        }

        public Rect Select(params InkStroke[] strokes)
        {
            ClearSelection();
            strokes.ToImmutableList().ForEach(stroke => stroke.Selected = true);
            OnSelectStrokes(this, EventArgs.Empty);
            return SelectionRect;
        }

        public Rect Select(IInkAnalysisNode node)
        {
            var ids = node.Kind == InkAnalysisNodeKind.Paragraph &&
                      node.Children.FirstOrDefault()?.Kind == InkAnalysisNodeKind.ListItem
                ? node.GetStrokeIds().ToHashSet().ToList()
                : node.GetStrokeIds();
            var strokes = ids.Select(id => _inkStrokeContainer.GetStrokeById(id));
            return Select(strokes.ToArray());
        }

        public Rect Select(PointCollection points)
        {
            ClearSelection();
            var rect = _inkStrokeContainer.SelectWithPolyLine(points);
            OnSelectStrokes(this, EventArgs.Empty);
            return rect;
        }

        #endregion

        #region Copy, Cut &  Paste

        public Rect Copy()
        {
            _inkStrokeContainer.CopySelectedToClipboard();
            OnCopyStrokes(this,
                new TransferStrokesEventArgs
                {
                    Strokes = _inkStrokeContainer.GetStrokes().Where(stroke => stroke.Selected)
                });
            return SelectionRect;
        }

        public Rect Cut()
        {
            var rect = Copy();
            var strokesToRemove = SelectedStrokes.ToImmutableList();
            strokesToRemove.ForEach(stroke => Remove(stroke));
            OnCutStrokes(this, new TransferStrokesEventArgs {Strokes = strokesToRemove});
            return rect;
        }

        public Rect Paste(Point position)
        {
            var rect = Rect.Empty;
            if (!_inkStrokeContainer.CanPasteFromClipboard())
            {
                return rect;
            }

            var preset = _inkStrokeContainer.GetStrokes().Select(stroke => stroke.Id);
            rect = _inkStrokeContainer.PasteFromClipboard(position);
            var strokes = _inkStrokeContainer.GetStrokes().Where(stroke => !preset.Contains(stroke.Id));
            OnPasteStrokes(this, new TransferStrokesEventArgs {Strokes = strokes});
            return rect;
        }

        #endregion
    }

    public partial class InkStrokeService
    {
        #region Actions

        public event EventHandler<AddStrokeEventArgs> AddStroke;
        public event EventHandler<EventArgs> ClearStrokes;
        public event EventHandler<TransferStrokesEventArgs> CopyStrokes;
        public event EventHandler<TransferStrokesEventArgs> CutStrokes;
        public event EventHandler<MoveStrokesEventArgs> MoveStrokes;
        public event EventHandler<TransferStrokesEventArgs> PasteStrokes;
        public event EventHandler<RemoveStrokeEventArgs> RemoveStroke;
        public event EventHandler<EventArgs> SelectStrokes;

        protected virtual void OnAddStroke(object sender, AddStrokeEventArgs e)
        {
            AddStroke?.Invoke(sender, e);
        }

        protected virtual void OnClearStrokes(object sender, EventArgs e)
        {
            ClearStrokes?.Invoke(sender, e);
        }

        protected virtual void OnCopyStrokes(object sender, TransferStrokesEventArgs e)
        {
            CopyStrokes?.Invoke(sender, e);
        }

        protected virtual void OnCutStrokes(object sender, TransferStrokesEventArgs e)
        {
            CutStrokes?.Invoke(sender, e);
        }

        protected virtual void OnMoveStrokes(object sender, MoveStrokesEventArgs e)
        {
            MoveStrokes?.Invoke(sender, e);
        }

        protected virtual void OnPasteStrokes(object sender, TransferStrokesEventArgs e)
        {
            PasteStrokes?.Invoke(sender, e);
        }

        protected virtual void OnRemoveStroke(object sender, RemoveStrokeEventArgs e)
        {
            RemoveStroke?.Invoke(sender, e);
        }

        protected virtual void OnSelectStrokes(object sender, EventArgs e)
        {
            SelectStrokes?.Invoke(sender, e);
        }

        #endregion

        #region Notifications

        public event EventHandler<InkStrokesCollectedEventArgs> StrokesCollected;
        public event EventHandler<InkStrokesErasedEventArgs> StrokesErased;

        protected virtual void OnStrokesCollected(object sender, InkStrokesCollectedEventArgs e)
        {
            StrokesCollected?.Invoke(sender, e);
        }

        protected virtual void OnStrokesErased(object sender, InkStrokesErasedEventArgs e)
        {
            StrokesErased?.Invoke(sender, e);
        }

        #endregion
    }
}

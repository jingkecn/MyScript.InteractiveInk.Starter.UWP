using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace MyScript.InteractiveInk.Services.Ink
{
    public partial class InkLassoSelectionService
    {
        private readonly InkCanvas _inkCanvas;
        private readonly Canvas _selectionCanvas;
        private readonly InkSelectionService _selectionService;
        private readonly InkStrokeService _strokeService;

        public InkLassoSelectionService(InkCanvas inkCanvas, Canvas selectionCanvas, InkStrokeService strokeService,
            InkSelectionService selectionService)
        {
            Initialize(_inkCanvas = inkCanvas);
            _selectionCanvas = selectionCanvas;
            _strokeService = strokeService;
            _selectionService = selectionService;
        }

        public void Start()
        {
            var presenter = _inkCanvas.InkPresenter;
            presenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;
            var input = presenter.UnprocessedInput;
            input.PointerPressed += OnInputPointerPressed;
            input.PointerMoved += OnInputPointerMoved;
            input.PointerReleased += OnInputPointerReleased;
        }

        public void Stop()
        {
            var presenter = _inkCanvas.InkPresenter;
            var input = presenter.UnprocessedInput;
            input.PointerPressed -= OnInputPointerPressed;
            input.PointerMoved -= OnInputPointerMoved;
            input.PointerReleased -= OnInputPointerReleased;
        }
    }

    public partial class InkLassoSelectionService : IDisposable
    {
        public void Dispose()
        {
            var presenter = _inkCanvas.InkPresenter;
            presenter.StrokeInput.StrokeStarted -= OnInputStrokeStarted;
            presenter.StrokesErased -= OnInkPresenterStrokesErased;
            _selectionService?.Dispose();
        }

        private void Initialize(InkCanvas canvas)
        {
            var presenter = canvas.InkPresenter;
            presenter.StrokeInput.StrokeStarted += OnInputStrokeStarted;
            presenter.StrokesErased += OnInkPresenterStrokesErased;
        }

        public void ClearSelection()
        {
            _strokeService.ClearSelection();
            _selectionService.Clear();
        }
    }

    public partial class InkLassoSelectionService
    {
        private Polyline _lasso;
        private bool _lassoEnabled;

        #region Ink Presenter

        private void OnInkPresenterStrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            Stop();
            if (args.Strokes.All(stroke => !stroke.Selected))
            {
                return;
            }

            ClearSelection();
        }

        #endregion

        #region Stroke Input

        private void OnInputStrokeStarted(InkStrokeInput sender, PointerEventArgs args)
        {
            Stop();
        }

        #endregion

        #region Unprocessed Input

        private void OnInputPointerPressed(InkUnprocessedInput sender, PointerEventArgs args)
        {
            var position = args.CurrentPoint.Position;
            if (_selectionService.Contains(position))
            {
                return;
            }

            _lasso = new Polyline
            {
                Stroke = new SolidColorBrush(Colors.DodgerBlue),
                StrokeDashArray = new DoubleCollection {5, 2},
                StrokeThickness = 1
            };

            _lasso.Points?.Add(args.CurrentPoint.RawPosition);
            _selectionCanvas.Children.Add(_lasso);
            _lassoEnabled = true;
        }

        private void OnInputPointerMoved(InkUnprocessedInput sender, PointerEventArgs args)
        {
            if (!_lassoEnabled)
            {
                return;
            }

            _lasso.Points?.Add(args.CurrentPoint.RawPosition);
        }

        private void OnInputPointerReleased(InkUnprocessedInput sender, PointerEventArgs args)
        {
            _lasso.Points?.Add(args.CurrentPoint.RawPosition);
            var rect = _strokeService.Select(_lasso.Points);
            _lassoEnabled = false;
            _selectionCanvas.Children.Remove(_lasso);
            _selectionService.Update(rect);
        }

        #endregion
    }
}

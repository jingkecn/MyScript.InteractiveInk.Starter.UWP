using System;
using System.Linq;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MyScript.InteractiveInk.Services.Ink
{
    public partial class InkNodeSelectionService
    {
        private readonly InkAnalyzerService _analyzerService;
        private readonly InkCanvas _inkCanvas;
        private readonly InkSelectionService _selectionService;
        private readonly InkStrokeService _strokeService;
        private IInkAnalysisNode _selectedNode;

        public InkNodeSelectionService(InkCanvas inkCanvas, InkStrokeService strokeService,
            InkSelectionService selectionService, InkAnalyzerService analyzerService)
        {
            Initialize(_inkCanvas = inkCanvas);
            _strokeService = strokeService;
            _selectionService = selectionService;
            _analyzerService = analyzerService;
        }

        public void ClearSelection()
        {
            _selectedNode = null;
            _strokeService.ClearSelection();
            _selectionService.Clear();
        }

        public void ExpandSelection()
        {
            if (_selectedNode == null)
            {
                return;
            }

            _selectedNode = _selectedNode.Parent;
            ShowOrHideSelection(_selectedNode);
        }

        private void ShowOrHideSelection(IInkAnalysisNode node)
        {
            if (node == null)
            {
                ClearSelection();
                return;
            }

            _selectionService.Update(_strokeService.Select(node));
        }
    }

    public partial class InkNodeSelectionService : IDisposable
    {
        public void Dispose()
        {
            Dispose(_inkCanvas);
        }

        public void Dispose(InkCanvas canvas)
        {
            canvas.Tapped -= InkCanvas_Tapped;
            canvas.DoubleTapped -= InkCanvas_DoubleTapped;
            canvas.InkPresenter.StrokesErased -= InkPresenter_StrokesErased;
        }

        private void Initialize(InkCanvas canvas)
        {
            canvas.Tapped += InkCanvas_Tapped;
            canvas.DoubleTapped += InkCanvas_DoubleTapped;
            canvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;
        }
    }

    public partial class InkNodeSelectionService
    {
        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            if (args.Strokes.All(stroke => !stroke.Selected))
            {
                return;
            }

            ClearSelection();
        }

        private void InkCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowOrHideSelection(_selectedNode = _analyzerService.FindHitNode(e.GetPosition(_inkCanvas)));
        }

        private void InkCanvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var position = e.GetPosition(_inkCanvas);
            if (_selectedNode == null || !RectHelper.Contains(_selectedNode.BoundingRect, position))
            {
                return;
            }

            ExpandSelection();
        }
    }
}

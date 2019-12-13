using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MyScript.InteractiveInk.Events;

namespace MyScript.InteractiveInk.Services.Ink
{
    public partial class InkAnalyzerService
    {
        private readonly InkCanvas _inkCanvas;
        private readonly InkStrokeService _strokeService;
        private readonly DispatcherTimer _timer;
        private InkAnalyzer _inkAnalyzer;

        public InkAnalyzerService(InkCanvas inkCanvas, InkStrokeService strokeService)
        {
            Initialize(_inkCanvas = inkCanvas);
            Initialize(_strokeService = strokeService);
            Initialize(_timer = new DispatcherTimer());
        }

        public InkAnalyzer InkAnalyzer => _inkAnalyzer ??= new InkAnalyzer();

        private async Task AnalyzeAsync(bool clean = false)
        {
            _timer.Stop();
            if (InkAnalyzer.IsAnalyzing)
            {
                _timer.Start();
                return;
            }

            if (clean)
            {
                InkAnalyzer.ClearDataForAllStrokes();
                InkAnalyzer.AddDataForStrokes(_strokeService.Strokes);
            }

            await InkAnalyzer.AnalyzeAsync();
        }

        public IInkAnalysisNode FindHitNode(Point position)
        {
            return FindHitNode(position, InkAnalysisNodeKind.InkWord) ??
                   FindHitNode(position, InkAnalysisNodeKind.InkBullet) ??
                   FindHitNode(position, InkAnalysisNodeKind.InkDrawing);
        }

        private IInkAnalysisNode FindHitNode(Point position, InkAnalysisNodeKind kind)
        {
            return InkAnalyzer.AnalysisRoot.FindNodes(kind)
                .FirstOrDefault(node => RectHelper.Contains(node.BoundingRect, position));
        }
    }

    public partial class InkAnalyzerService
    {
        private void AddStrokes(params InkStroke[] strokes)
        {
            _timer.Stop();
            InkAnalyzer.AddDataForStrokes(strokes);
            _timer.Start();
        }

        private void RemoveStrokes(params InkStroke[] strokes)
        {
            _timer.Stop();
            InkAnalyzer.RemoveDataForStrokes(strokes.Select(stroke => stroke.Id));
            _timer.Start();
        }

        private void ReplaceStrokes(params InkStroke[] strokes)
        {
            strokes.ToImmutableList().ForEach(stroke => InkAnalyzer.ReplaceDataForStroke(stroke));
        }
    }

    public partial class InkAnalyzerService : IDisposable
    {
        public void Dispose()
        {
            Dispose(_inkCanvas);
            Dispose(_strokeService);
            Dispose(_timer);
        }

        private void Dispose(InkCanvas canvas)
        {
            canvas.InkPresenter.StrokeInput.StrokeStarted -= StrokeInput_StrokeStarted;
            canvas.InkPresenter.StrokesErased -= InkPresenter_StrokesErased;
            canvas.InkPresenter.StrokesCollected -= InkPresenter_StrokesCollected;
        }

        private void Dispose(InkStrokeService service)
        {
            service.AddStroke -= StrokeService_AddStroke;
            service.MoveStrokes -= StrokeService_MoveStrokes;
            service.RemoveStroke -= StrokeService_RemoveStroke;
            service.CutStrokes -= StrokeService_CutStrokes;
            service.PasteStrokes -= StrokeService_PasteStrokes;
            service.ClearStrokes -= StrokeService_ClearStrokes;
        }

        private void Dispose(DispatcherTimer timer)
        {
            timer.Tick -= Timer_Tick;
        }

        private void Initialize(InkCanvas canvas)
        {
            canvas.InkPresenter.StrokeInput.StrokeStarted += StrokeInput_StrokeStarted;
            canvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;
            canvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
        }

        private void Initialize(InkStrokeService service)
        {
            service.AddStroke += StrokeService_AddStroke;
            service.MoveStrokes += StrokeService_MoveStrokes;
            service.RemoveStroke += StrokeService_RemoveStroke;
            service.CutStrokes += StrokeService_CutStrokes;
            service.PasteStrokes += StrokeService_PasteStrokes;
            service.ClearStrokes += StrokeService_ClearStrokes;
        }

        private void Initialize(DispatcherTimer timer)
        {
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(400);
        }

        private async void Timer_Tick(object sender, object e)
        {
            await AnalyzeAsync();
        }
    }

    public partial class InkAnalyzerService
    {
        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            AddStrokes(args.Strokes.ToArray());
        }

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            RemoveStrokes(args.Strokes.ToArray());
        }

        private void StrokeInput_StrokeStarted(InkStrokeInput sender, PointerEventArgs args)
        {
            _timer.Stop();
        }
    }

    public partial class InkAnalyzerService
    {
        private void StrokeService_ClearStrokes(object sender, EventArgs e)
        {
            _timer.Stop();
            InkAnalyzer.ClearDataForAllStrokes();
        }

        private void StrokeService_PasteStrokes(object sender, TransferStrokesEventArgs e)
        {
            AddStrokes(e.Strokes.ToArray());
        }

        private void StrokeService_CutStrokes(object sender, TransferStrokesEventArgs e)
        {
            RemoveStrokes(e.Strokes.ToArray());
        }

        private void StrokeService_RemoveStroke(object sender, RemoveStrokeEventArgs e)
        {
            RemoveStrokes(e.RemovedStroke);
        }

        private async void StrokeService_MoveStrokes(object sender, MoveStrokesEventArgs e)
        {
            ReplaceStrokes(e.Strokes.ToArray());
            await AnalyzeAsync(true);
        }

        private void StrokeService_AddStroke(object sender, AddStrokeEventArgs e)
        {
            AddStrokes(e.NewStroke);
        }
    }
}

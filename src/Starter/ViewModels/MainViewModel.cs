using System;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using MyScript.InteractiveInk.Common;
using MyScript.InteractiveInk.Services.Ink;
using MyScript.InteractiveInk.Services.Ink.UndoRedo;

namespace MyScript.InteractiveInk.ViewModels
{
    public partial class MainViewModel
    {
        private bool _canClear;
        private bool _canRedo;
        private bool _canTypeset;
        private bool _canUndo;

        public bool CanClear
        {
            get => _canClear;
            set => Set(ref _canClear, value, nameof(CanClear));
        }

        public bool CanRedo
        {
            get => _canRedo;
            set => Set(ref _canRedo, value, nameof(CanRedo));
        }

        public bool CanTypeset
        {
            get => _canTypeset;
            set => Set(ref _canTypeset, value, nameof(CanTypeset));
        }

        public bool CanUndo
        {
            get => _canUndo;
            set => Set(ref _canUndo, value, nameof(CanUndo));
        }
    }

    public partial class MainViewModel
    {
        private bool _enableLassoSelection;
        private bool _enableMouse;
        private bool _enablePen;
        private bool _enableTouch;

        public bool EnableLassoSelection
        {
            get => _enableLassoSelection;
            set => Set(ref _enableLassoSelection, value, nameof(EnableLassoSelection));
        }

        public bool EnableMouse
        {
            get => _enableMouse;
            set => Set(ref _enableMouse, value, nameof(EnableMouse));
        }

        public bool EnablePen
        {
            get => _enablePen;
            set => Set(ref _enablePen, value, nameof(EnablePen));
        }

        public bool EnableTouch
        {
            get => _enableTouch;
            set => Set(ref _enableTouch, value, nameof(EnableTouch));
        }
    }

    public partial class MainViewModel : Observable
    {
        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            Configure(InkInputDeviceService, propertyName);
            Configure(InkLassoSelectionService, propertyName);
        }

        private void Configure(InkInputDeviceService service, string propertyName)
        {
            var (input, enabled) = propertyName switch
            {
                nameof(EnableMouse) => (CoreInputDeviceTypes.Mouse, EnableMouse),
                nameof(EnablePen) => (CoreInputDeviceTypes.Pen, EnablePen),
                nameof(EnableTouch) => (CoreInputDeviceTypes.Touch, EnableTouch),
                _ => (CoreInputDeviceTypes.None, false)
            };
            if (input == CoreInputDeviceTypes.None)
            {
                return;
            }

            service?.Enable(input, enabled);
        }

        private void Configure(InkLassoSelectionService service, string propertyName)
        {
            if (propertyName != nameof(EnableLassoSelection))
            {
                return;
            }

            if (EnableLassoSelection)
            {
                service.Start();
            }
            else
            {
                service.Stop();
            }
        }
    }

    public partial class MainViewModel : IDisposable
    {
        private InkInputDeviceService InkInputDeviceService { get; set; }
        private InkLassoSelectionService InkLassoSelectionService { get; set; }
        private InkStrokeService InkStrokeService { get; set; }
        private InkTransformService InkTransformService { get; set; }
        private InkUndoRedoService InkUndoRedoService { get; set; }

        public void Dispose()
        {
            InkInputDeviceService?.Dispose();
            InkStrokeService?.Dispose();
            InkUndoRedoService?.Dispose();
        }

        public void Initialize(InkCanvas inkCanvas, Canvas drawingCanvas, Canvas selectionCanvas)
        {
            // Prerequisites
            Initialize(InkInputDeviceService = new InkInputDeviceService(inkCanvas));
            Initialize(InkStrokeService = new InkStrokeService(inkCanvas));
            // Initializations
            InkLassoSelectionService = new InkLassoSelectionService(InkStrokeService, inkCanvas, selectionCanvas);
            InkTransformService = new InkTransformService(drawingCanvas, InkStrokeService);
            Initialize(InkUndoRedoService = new InkUndoRedoService(InkStrokeService));
            Initialize(inkCanvas);
        }

        private void Initialize(InkInputDeviceService service)
        {
            service.PenDetected += (sender, args) => EnableTouch = false;
        }

        private void Initialize(InkStrokeService service)
        {
            service.ClearStrokes += (sender, args) => InitializeCommands();
        }

        private void Initialize(InkUndoRedoService service)
        {
            service.AddOperation += (sender, args) => InitializeCommands();
            service.ExecuteRedo += (sender, args) => InitializeCommands();
            service.ExecuteUndo += (sender, args) => InitializeCommands();
        }

        private void Initialize(InkCanvas inkCanvas)
        {
            var presenter = inkCanvas.InkPresenter;
            var input = presenter.InputDeviceTypes;
            EnableMouse = (input & CoreInputDeviceTypes.Mouse) != 0;
            EnablePen = (input & CoreInputDeviceTypes.Pen) != 0;
            EnableTouch = (input & CoreInputDeviceTypes.Touch) != 0;
        }

        private void InitializeCommands()
        {
            CanClear = InkStrokeService?.Strokes?.Any() == true || InkTransformService?.Elements?.Any() == true;
            CanRedo = InkUndoRedoService?.CanRedo == true;
            CanTypeset = InkStrokeService?.Strokes?.Any() == true;
            CanUndo = InkUndoRedoService?.CanUndo == true;
        }
    }

    public partial class MainViewModel
    {
        private void ClearSelection()
        {
            InkLassoSelectionService.ClearSelection();
        }

        #region Typeset

        private ICommand _typesetCommand;

        public ICommand TypesetCommand =>
            _typesetCommand ??= new RelayCommand(OnExecuteTypesetCommand);

        private async void OnExecuteTypesetCommand()
        {
            var result = await InkTransformService.TransformAsync();
            if (!result.Elements.Any())
            {
                return;
            }

            ClearSelection();
            InkUndoRedoService.Add(new TransformUndoRedoOperation(InkStrokeService, result));
        }

        #endregion

        #region Redo & Undo

        private ICommand _redoCommand;
        private ICommand _undoCommand;

        public ICommand RedoCommand =>
            _redoCommand ??= new RelayCommand(OnExecuteRedoCommand);

        public ICommand UndoCommand =>
            _undoCommand ??= new RelayCommand(OnExecuteUndoCommand);

        private void OnExecuteRedoCommand()
        {
            ClearSelection();
            InkUndoRedoService.Redo();
        }

        private void OnExecuteUndoCommand()
        {
            ClearSelection();
            InkUndoRedoService.Undo();
        }

        #endregion

        #region Ink Manipulation

        private ICommand _clearAllCommand;

        public ICommand ClearAllCommand => _clearAllCommand ??= new RelayCommand(OnExecuteClearAllCommand);

        private void OnExecuteClearAllCommand()
        {
            var strokes = InkStrokeService.Strokes.ToImmutableList();
            var elements = InkTransformService.Elements.ToImmutableList();
            ClearSelection();
            InkStrokeService.Clear();
            InkTransformService.Clear();
            InkUndoRedoService.Add(
                new ClearAllUndoRedoOperation(InkStrokeService, strokes, InkTransformService, elements));
        }

        #endregion
    }
}

using System.Linq;
using System.Windows.Input;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using MyScript.InteractiveInk.Common;
using MyScript.InteractiveInk.Services.Ink;
using MyScript.InteractiveInk.Services.Ink.UndoRedo;

namespace MyScript.InteractiveInk.ViewModels
{
    public partial class MainViewModel : Observable
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

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
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

            InkInputDeviceService.Enable(input, enabled);
        }

        private void Initialize(InkCanvas inkCanvas)
        {
            var presenter = inkCanvas.InkPresenter;
            var input = presenter.InputDeviceTypes;
            EnableMouse = (input & CoreInputDeviceTypes.Mouse) != 0;
            EnablePen = (input & CoreInputDeviceTypes.Pen) != 0;
            EnableTouch = (input & CoreInputDeviceTypes.Touch) != 0;
        }
    }

    public partial class MainViewModel
    {
        private InkInputDeviceService InkInputDeviceService { get; set; }
        private InkStrokeService InkStrokeService { get; set; }
        private InkTransformService InkTransformService { get; set; }
        private InkUndoRedoService InkUndoRedoService { get; set; }

        public void Initialize(InkCanvas inkCanvas, Canvas drawingCanvas)
        {
            InkInputDeviceService = new InkInputDeviceService(inkCanvas);
            InkStrokeService = new InkStrokeService(inkCanvas);
            InkTransformService = new InkTransformService(drawingCanvas, InkStrokeService);
            InkUndoRedoService = new InkUndoRedoService(InkStrokeService);
            Initialize(inkCanvas);
            Initialize(InkInputDeviceService);
        }

        private void Initialize(InkInputDeviceService service)
        {
            service.PenDetected += (sender, args) => EnableTouch = false;
        }
    }

    public partial class MainViewModel
    {
        #region Typeset

        private ICommand _typesetCommand;

        public ICommand TypesetCommand => _typesetCommand ??= new RelayCommand(OnExecuteTypesetCommand);

        private async void OnExecuteTypesetCommand()
        {
            var result = await InkTransformService.TransformAsync();
            if (result.Elements.Any())
            {
                InkUndoRedoService.Add(new TransformUndoRedoOperation(InkStrokeService, result));
            }
        }

        #endregion

        #region Redo & Undo

        private ICommand _redoCommand;
        private ICommand _undoCommand;

        public ICommand RedoCommand => _redoCommand ??= new RelayCommand(OnExecuteRedoCommand);
        public ICommand UndoCommand => _undoCommand ??= new RelayCommand(OnExecuteUndoCommand);

        private void OnExecuteRedoCommand()
        {
            InkUndoRedoService.Redo();
        }

        private void OnExecuteUndoCommand()
        {
            InkUndoRedoService.Undo();
        }

        #endregion

        #region Ink Manipulation

        private ICommand _clearAllCommand;
        public ICommand ClearAllCommand => _clearAllCommand ??= new RelayCommand(OnExecuteClearAllCommand);

        private void OnExecuteClearAllCommand()
        {
            InkStrokeService.Clear();
            InkTransformService.Clear();
        }

        #endregion
    }
}

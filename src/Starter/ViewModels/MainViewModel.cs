using System.Linq;
using System.Windows.Input;
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
    }

    public partial class MainViewModel
    {
        private InkStrokeService InkStrokeService { get; set; }
        private InkTransformService InkTransformService { get; set; }
        private InkUndoRedoService InkUndoRedoService { get; set; }

        public void Initialize(InkCanvas inkCanvas, Canvas drawingCanvas)
        {
            InkStrokeService = new InkStrokeService(inkCanvas);
            InkTransformService = new InkTransformService(drawingCanvas, InkStrokeService);
            InkUndoRedoService = new InkUndoRedoService(InkStrokeService);
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

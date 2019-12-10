using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using MyScript.InteractiveInk.Common;
using MyScript.InteractiveInk.Services;

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

        public void Initialize(InkCanvas inkCanvas)
        {
            InkStrokeService = new InkStrokeService(inkCanvas);
        }
    }

    public partial class MainViewModel
    {
        #region Typeset

        private ICommand _typesetCommand;

        public ICommand TypesetCommand => _typesetCommand ??= new RelayCommand(OnExecuteTypesetCommand);

        private void OnExecuteTypesetCommand()
        {
        }

        #endregion

        #region Redo & Undo

        private ICommand _redoCommand;
        private ICommand _undoCommand;

        public ICommand RedoCommand => _redoCommand ??= new RelayCommand(OnExecuteRedoCommand);
        public ICommand UndoCommand => _undoCommand ??= new RelayCommand(OnExecuteUndoCommand);

        private void OnExecuteRedoCommand()
        {
        }

        private void OnExecuteUndoCommand()
        {
        }

        #endregion

        #region Ink Manipulation

        private ICommand _clearAllCommand;
        public ICommand ClearAllCommand => _clearAllCommand ??= new RelayCommand(OnExecuteClearAllCommand);

        private void OnExecuteClearAllCommand()
        {
        }

        #endregion
    }
}

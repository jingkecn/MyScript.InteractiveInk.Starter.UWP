using System.Windows.Input;
using MyScript.IInk;
using MyScript.InteractiveInk.Common.Helpers;
using MyScript.InteractiveInk.UI.Extensions;
using MyScript.InteractiveInk.ViewModels;

namespace MyScript.InteractiveInk.Views.Controls
{
    public sealed partial class MainCommandBar
    {
        private MainViewModel _viewModel;

        public MainCommandBar()
        {
            InitializeComponent();
        }

        private Editor Editor => ViewModel.Editor;
        private MainViewModel ViewModel => _viewModel ??= DataContext as MainViewModel;

        #region Commands

        private ICommand _redoCommand;
        private ICommand _typesetCommand;
        private ICommand _undoCommand;
        private ICommand _saveCommand;
        private ICommand _saveAsCommand;
        private ICommand _openCommand;

        private ICommand OpenCommand =>
            _openCommand ??= new RelayCommand(async _ => await Editor.WaitForIdleAndOpenAsync());

        private ICommand RedoCommand => _redoCommand ??= new RelayCommand(_ => Editor.WaitForIdleAndRedo());
        private ICommand SaveCommand => _saveCommand ??= new RelayCommand(_ => Editor.WaitForIdleAndSave());

        private ICommand SaveAsCommand =>
            _saveAsCommand ??= new RelayCommand(async _ => await Editor.WaitForIdleAndSaveAsAsync());

        private ICommand TypesetCommand => _typesetCommand ??= new RelayCommand(_ => Editor.WaitForIdleAndTypeset());
        private ICommand UndoCommand => _undoCommand ??= new RelayCommand(_ => Editor.WaitForIdleAndUndo());

        #endregion
    }
}

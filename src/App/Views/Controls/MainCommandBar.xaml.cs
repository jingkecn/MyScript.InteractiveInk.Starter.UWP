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
        private MainViewModel ViewModel => _viewModel ?? (_viewModel = DataContext as MainViewModel);

        #region Commands

        private ICommand _clearAllCommand;
        private ICommand _redoCommand;
        private ICommand _typesetCommand;
        private ICommand _undoCommand;

        private ICommand ClearAllCommand =>
            _clearAllCommand ?? (_clearAllCommand = new RelayCommand(_ => Editor.WaitForIdleAndClear()));

        private ICommand RedoCommand =>
            _redoCommand ?? (_redoCommand = new RelayCommand(_ => Editor.WaitForIdleAndRedo()));

        private ICommand TypesetCommand =>
            _typesetCommand ?? (_typesetCommand = new RelayCommand(_ => Editor.WaitForIdleAndTypeset()));

        private ICommand UndoCommand =>
            _undoCommand ?? (_undoCommand = new RelayCommand(_ => Editor.WaitForIdleAndUndo()));

        #endregion
    }
}

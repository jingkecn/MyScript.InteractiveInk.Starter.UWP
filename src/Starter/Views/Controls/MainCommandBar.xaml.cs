using System;
using Windows.UI.Xaml.Input;
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

        private MainViewModel ViewModel => _viewModel ??= DataContext as MainViewModel;

        private void ClearAllCommand_OnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void RedoCommand_OnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void TypesetCommand_OnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void UndoCommand_OnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}

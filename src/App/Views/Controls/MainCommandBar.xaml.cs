using MyScript.InteractiveInk.ViewModels;

namespace MyScript.InteractiveInk.Views.Controls
{
    public sealed partial class MainCommandBar
    {
        private MainCommandsViewModel _viewModel;

        public MainCommandBar()
        {
            InitializeComponent();
        }

        private MainCommandsViewModel ViewModel => _viewModel ??= DataContext as MainCommandsViewModel;
    }
}

using MyScript.InteractiveInk.ViewModels;

namespace MyScript.InteractiveInk.Views.Controls
{
    public sealed partial class MainMenuBar
    {
        private MainCommandsViewModel _viewModel;

        public MainMenuBar()
        {
            InitializeComponent();
        }

        private MainCommandsViewModel ViewModel => _viewModel ??= DataContext as MainCommandsViewModel;
    }
}

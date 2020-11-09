using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Common.Commands;
using MyScript.InteractiveInk.Common.Enumerations;
using MyScript.InteractiveInk.Common.ViewModels;
using MyScript.InteractiveInk.Extensions;

namespace MyScript.InteractiveInk.ViewModels
{
    public partial class MainCommandsViewModel
    {
        private ContentCommandsViewModel _contentCommands;
        private EditingCommandsViewModel _editingCommands;
        private PageCommandsViewModel _pageCommands;

        public ContentCommandsViewModel ContentCommands
        {
            get => _contentCommands;
            set => Set(ref _contentCommands, value, nameof(ContentCommands));
        }

        public EditingCommandsViewModel EditingCommands
        {
            get => _editingCommands;
            set => Set(ref _editingCommands, value, nameof(EditingCommands));
        }

        public PageCommandsViewModel PageCommands
        {
            get => _pageCommands;
            set => Set(ref _pageCommands, value, nameof(PageCommands));
        }
    }

    public partial class MainCommandsViewModel : IPackageCommands
    {
        private ICommand _commandCreatePackage;
        private ICommand _commandOpenPackage;
        private ICommand _commandSavePackage;

        public ICommand CommandCreatePackage =>
            _commandCreatePackage ??= new RelayCommand<ContentType>(OnCreatePackage);

        public ICommand CommandOpenPackage =>
            _commandOpenPackage ??= new RelayCommand(async _ => Package = await Editor.WaitForIdleAndOpenAsync());

        public ICommand CommandSavePackage => _commandSavePackage ??=
            new RelayCommand<bool>(async saveAsNew => await Editor.WaitForIdleAndSaveAsync(saveAsNew));

        private void OnCreatePackage(ContentType type)
        {
            var folder = ApplicationData.Current.LocalCacheFolder;
            var name = $"{Path.GetRandomFileName()}.iink";
            var path = Path.Combine(folder.Path, name);
            PageCommands.Initialize(Package = Editor.Open(path, type));
        }
    }

    public partial class MainCommandsViewModel : ObservableAsync
    {
        [CanBeNull] public override CoreDispatcher Dispatcher { get; set; }
        [CanBeNull] public Editor Editor { get; set; }
        [CanBeNull] public ContentPackage Package { get; set; }

        public void Initialize([NotNull] Editor editor)
        {
            Editor?.RemoveListener(this);
            Editor = editor;
            Editor.AddListener(this);
            ContentCommands?.Initialize(Editor);
            EditingCommands?.Initialize(Editor);
            PageCommands?.Initialize(Editor);
        }

        public void Initialize(Point position)
        {
            ContentCommands?.Initialize(position);
        }

        public void Initialize([NotNull] IRenderTarget target)
        {
            ContentCommands?.Initialize(target);
            PageCommands?.Initialize(target);
        }
    }

    public partial class MainCommandsViewModel : IDisposable
    {
        public void Dispose()
        {
            ContentCommands?.Dispose();
            EditingCommands?.Dispose();
            PageCommands?.Dispose();
            Editor?.RemoveListener(this);
        }
    }

    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public partial class MainCommandsViewModel : IEditorListener
    {
        public async void PartChanged(Editor editor)
        {
            ApplicationView.GetForCurrentView().Title = string.Empty;
            if (!(editor.Part is { } part) || !(part.Package is {} package) ||
                !(await package.GetAssociatedFile() is {} file))
            {
                return;
            }

            ApplicationView.GetForCurrentView().Title = $"{file.Path} - {part.Type}";
        }

        public void ContentChanged(Editor editor, string[] blockIds)
        {
        }

        public void OnError(Editor editor, string blockId, string message)
        {
        }
    }
}

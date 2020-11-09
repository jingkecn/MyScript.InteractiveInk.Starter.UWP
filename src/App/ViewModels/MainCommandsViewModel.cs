using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Common.Helpers;
using MyScript.InteractiveInk.Common.ViewModels;
using MyScript.InteractiveInk.UI.Enumerations;
using MyScript.InteractiveInk.UI.Extensions;

namespace MyScript.InteractiveInk.ViewModels
{
    public partial class MainCommandsViewModel
    {
        private bool _canRedo;
        private bool _canUndo;

        public bool CanRedo
        {
            get => _canRedo;
            set => Set(ref _canRedo, value, nameof(CanRedo));
        }

        public bool CanUndo
        {
            get => _canUndo;
            set => Set(ref _canUndo, value, nameof(CanUndo));
        }
    }

    public partial class MainCommandsViewModel
    {
        private bool _canAddBlock;
        private bool _canRemoveBlock;

        public bool CanAddBlock
        {
            get => _canAddBlock;
            set => Set(ref _canAddBlock, value, nameof(CanAddBlock));
        }

        public bool CanRemoveBlock
        {
            get => _canRemoveBlock;
            set => Set(ref _canRemoveBlock, value, nameof(CanRemoveBlock));
        }
    }

    public partial class MainCommandsViewModel
    {
        private bool _hasNextPage = true;
        private bool _hasPreviousPage = true;
        private bool _isDocumentPage;

        public bool HasNextPage
        {
            get => _hasNextPage;
            set => Set(ref _hasNextPage, value, nameof(HasNextPage));
        }

        public bool HasPreviousPage
        {
            get => _hasPreviousPage;
            set => Set(ref _hasPreviousPage, value, nameof(HasPreviousPage));
        }

        public bool IsDocumentPage
        {
            get => _isDocumentPage;
            set => Set(ref _isDocumentPage, value, nameof(IsDocumentPage));
        }
    }

    public partial class MainCommandsViewModel
    {
        private ICommand _redoCommand;
        private ICommand _typesetCommand;
        private ICommand _undoCommand;

        public ICommand RedoCommand => _redoCommand ??= new RelayCommand(_ => Editor.WaitForIdleAndRedo());
        public ICommand TypesetCommand => _typesetCommand ??= new RelayCommand(_ => Editor.WaitForIdleAndTypeset());
        public ICommand UndoCommand => _undoCommand ??= new RelayCommand(_ => Editor.WaitForIdleAndUndo());
    }

    public partial class MainCommandsViewModel
    {
        private ICommand _addBlockCommand;
        private ICommand _appendBlockCommand;
        private ICommand _removeBlockCommand;

        public ICommand AddBlockCommand => _addBlockCommand ??=
            new RelayCommand<ContentType>(type => Editor?.AddBlockAt(ContextPosition, type));

        public ICommand AppendBlockCommand => _appendBlockCommand ??=
            new RelayCommand<ContentType>(type => Editor?.AppendBlock(type, target: RenderTarget));

        public ICommand RemoveBlockCommand =>
            _removeBlockCommand ??= new RelayCommand(_ => Editor?.RemoveBlockAt(ContextPosition));
    }

    public partial class MainCommandsViewModel
    {
        private ICommand _addPageCommand;
        private ICommand _goToNextPageCommand;
        private ICommand _goToPreviousPageCommand;

        public ICommand AddPageCommand => _addPageCommand ??= new RelayCommand<ContentType>(OnAddPageCommandExecuted);
        public ICommand GoToNextPageCommand => _goToNextPageCommand ??= new RelayCommand(_ => Editor?.GoToNextPage());

        public ICommand GoToPreviousPageCommand =>
            _goToPreviousPageCommand ??= new RelayCommand(_ => Editor?.GoToPreviousPage());

        private void OnAddPageCommandExecuted(ContentType type)
        {
            if (!(Editor is { } editor) || !(Package is { } package))
            {
                return;
            }

            editor.Part = package.CreatePart(type.ToNative());
        }
    }

    public partial class MainCommandsViewModel
    {
        private ICommand _createCommand;
        private ICommand _openCommand;
        private ICommand _saveAsCommand;
        private ICommand _saveCommand;

        public ICommand CreateCommand => _createCommand ??= new RelayCommand<ContentType>(OnCreateCommandExecuted);

        public ICommand OpenCommand =>
            _openCommand ??= new RelayCommand(async _ => Package = await Editor.WaitForIdleAndOpenAsync());

        public ICommand SaveCommand =>
            _saveCommand ??= new RelayCommand(async _ => await Editor.WaitForIdleAndSaveAsync());

        public ICommand SaveAsCommand =>
            _saveAsCommand ??= new RelayCommand(async _ => await Editor.WaitForIdleAndSaveAsync(true));

        private void OnCreateCommandExecuted(ContentType type)
        {
            var folder = ApplicationData.Current.LocalCacheFolder;
            var name = $"{Path.GetRandomFileName()}.iink";
            var path = Path.Combine(folder.Path, name);
            Package = Editor.Open(path, type);
        }
    }

    public partial class MainCommandsViewModel : ObservableAsync
    {
        public Point ContextPosition { get; set; }
        [CanBeNull] public override CoreDispatcher Dispatcher { get; set; }
        [CanBeNull] public Editor Editor { get; set; }
        [CanBeNull] public ContentPackage Package { get; set; }
        [CanBeNull] public IRenderTarget RenderTarget { get; set; }

        public void Initialize([NotNull] Editor editor)
        {
            Editor = editor;
            Editor.AddListener(this);
        }

        public void Initialize([NotNull] IRenderTarget target)
        {
            RenderTarget = target;
        }

        public void Initialize(Point position)
        {
            ContextPosition = position;
            var block = Editor?.GetBlockAt(position);
            CanAddBlock = block == null || block.IsContainer();
            CanRemoveBlock = block != null && Selections != null && Selections.Contains(block.Id);
        }
    }

    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public partial class MainCommandsViewModel : IEditorListener
    {
        public async void PartChanged(Editor editor)
        {
            CanAddBlock = editor.SupportedAddBlockTypes?.Any() ?? false;
            CanRedo = editor.CanRedo();
            CanUndo = editor.CanUndo();
            HasNextPage = editor.HasNextPage();
            HasPreviousPage = editor.HasPreviousPage();
            IsDocumentPage = editor.Part?.Type == ContentType.TextDocument.ToNative();
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
            CanRedo = editor.CanRedo();
            CanUndo = editor.CanUndo();
        }

        public void OnError(Editor editor, string blockId, string message)
        {
            Dispatcher?.RunAsync(CoreDispatcherPriority.Normal,
                async () => await new MessageDialog(message, blockId).ShowAsync())?.AsTask();
        }
    }

    public partial class MainCommandsViewModel : IEditorListener2
    {
        private IEnumerable<string> Selections { get; set; }

        public void SelectionChanged(Editor editor, string[] blockIds)
        {
            Selections = blockIds;
        }

        public void ActiveBlockChanged(Editor editor, string blockId)
        {
        }
    }
}

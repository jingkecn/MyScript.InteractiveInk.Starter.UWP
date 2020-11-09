using System;
using System.Windows.Input;
using Windows.UI.Core;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Common.Commands;
using MyScript.InteractiveInk.Common.Enumerations;
using MyScript.InteractiveInk.Common.ViewModels;
using MyScript.InteractiveInk.Extensions;

namespace MyScript.InteractiveInk.ViewModels
{
    public partial class PageCommandsViewModel
    {
        private bool _canRemovePage;
        private bool _hasNextPage = true;
        private bool _hasPreviousPage = true;

        public bool CanRemovePage
        {
            get => _canRemovePage;
            set => Set(ref _canRemovePage, value, nameof(CanRemovePage));
        }

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
    }

    public partial class PageCommandsViewModel : ObservableAsync
    {
        [CanBeNull] public override CoreDispatcher Dispatcher { get; set; }
        [CanBeNull] private Editor Editor { get; set; }
        [CanBeNull] private ContentPackage Package { get; set; }
        [CanBeNull] private IRenderTarget RenderTarget { get; set; }

        public void Initialize([NotNull] Editor editor)
        {
            Editor?.RemoveListener(this);
            Editor = editor;
            Editor.AddListener(this);
        }

        public void Initialize([NotNull] ContentPackage package)
        {
            Package = package;
        }

        public void Initialize([NotNull] IRenderTarget target)
        {
            RenderTarget = target;
        }
    }

    public partial class PageCommandsViewModel : IDisposable
    {
        public void Dispose()
        {
            Editor?.RemoveListener(this);
        }
    }

    public partial class PageCommandsViewModel : IEditorListener
    {
        public void PartChanged(Editor editor)
        {
            CanRemovePage = editor.Part != null;
            HasNextPage = editor.HasNextPage();
            HasPreviousPage = editor.HasPreviousPage();
        }

        public void ContentChanged(Editor editor, string[] blockIds)
        {
        }

        public void OnError(Editor editor, string blockId, string message)
        {
        }
    }

    public partial class PageCommandsViewModel : IPageCommands
    {
        private ICommand _commandAddPage;
        private ICommand _commandGoToNextPage;
        private ICommand _commandGoToPreviousPage;
        private ICommand _commandRemovePage;

        public ICommand CommandGoToNextPage => _commandGoToNextPage ??= new RelayCommand(_ => Editor?.GoToNextPage());

        public ICommand CommandGoToPreviousPage =>
            _commandGoToPreviousPage ??= new RelayCommand(_ => Editor?.GoToPreviousPage());

        public ICommand CommandAddPage => _commandAddPage ??= new RelayCommand<ContentType>(OnAddPage);
        public ICommand CommandRemovePage => _commandRemovePage ??= new RelayCommand(OnRemovePage);

        private void OnAddPage(ContentType type)
        {
            if (!(Editor is { } editor) || !(Package is { } package))
            {
                return;
            }

            editor.Part = package.CreatePart(type.ToNative());
        }

        private void OnRemovePage(object _)
        {
            if (!(Editor is { } editor) || !(editor.Part is {} part) || !(Package is { } package))
            {
                return;
            }

            package.RemovePart(part);
            part.Dispose();
            var count = package.PartCount;
            editor.Part = count == 0 ? null : package.GetPart(count - 1);
            if (!(editor.Renderer is {} renderer))
            {
                return;
            }

            RenderTarget?.Invalidate(renderer, LayerType.LayerType_ALL);
        }
    }
}

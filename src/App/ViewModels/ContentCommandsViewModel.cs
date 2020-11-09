using System;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Core;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Common.Commands;
using MyScript.InteractiveInk.Common.Enumerations;
using MyScript.InteractiveInk.Common.ViewModels;
using MyScript.InteractiveInk.Extensions;

namespace MyScript.InteractiveInk.ViewModels
{
    public partial class ContentCommandsViewModel
    {
        private bool _canAddBlock;
        private bool _canAppendBlock;
        private bool _canRemoveBlock;

        public bool CanAddBlock
        {
            get => _canAddBlock;
            set => Set(ref _canAddBlock, value, nameof(CanAddBlock));
        }

        public bool CanAppendBlock
        {
            get => _canAppendBlock;
            set => Set(ref _canAppendBlock, value, nameof(CanAppendBlock));
        }

        public bool CanRemoveBlock
        {
            get => _canRemoveBlock;
            set => Set(ref _canRemoveBlock, value, nameof(CanRemoveBlock));
        }
    }

    public partial class ContentCommandsViewModel : ObservableAsync
    {
        public Point ContextPosition { get; set; }
        [CanBeNull] public override CoreDispatcher Dispatcher { get; set; }
        [CanBeNull] public Editor Editor { get; set; }
        [CanBeNull] public IRenderTarget RenderTarget { get; set; }

        public void Initialize([NotNull] Editor editor)
        {
            Editor?.RemoveListener(this);
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
            CanAddBlock = IsInsideDocument && (block == null || block.IsContainer());
            CanRemoveBlock = block != null && !block.IsContainer();
        }
    }

    public partial class ContentCommandsViewModel : IContentCommands
    {
        private ICommand _commandAddContent;
        private ICommand _commandAppendContent;
        private ICommand _commandRemoveContent;

        public ICommand CommandAddContent => _commandAddContent ??=
            new RelayCommand<ContentType>(type => Editor?.AddBlockAt(ContextPosition, type));

        public ICommand CommandAppendContent => _commandAppendContent ??=
            new RelayCommand<ContentType>(type => Editor?.AppendBlock(type, target: RenderTarget));

        public ICommand CommandRemoveContent =>
            _commandRemoveContent ??= new RelayCommand(_ => Editor?.RemoveBlockAt(ContextPosition));
    }

    public partial class ContentCommandsViewModel : IDisposable
    {
        public void Dispose()
        {
            Editor?.RemoveListener(this);
        }
    }

    public partial class ContentCommandsViewModel : IEditorListener
    {
        private bool IsInsideDocument { get; set; }

        public void PartChanged(Editor editor)
        {
            IsInsideDocument = editor.Part?.Type?.ToPlatformContentType() == ContentType.TextDocument;
            CanAppendBlock = IsInsideDocument && (editor.SupportedAddBlockTypes?.Any() ?? false);
        }

        public void ContentChanged(Editor editor, string[] blockIds)
        {
        }

        public void OnError(Editor editor, string blockId, string message)
        {
        }
    }
}

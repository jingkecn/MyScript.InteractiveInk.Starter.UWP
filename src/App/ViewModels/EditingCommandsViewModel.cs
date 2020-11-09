using System;
using System.Windows.Input;
using Windows.UI.Core;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Common.Commands;
using MyScript.InteractiveInk.Common.ViewModels;
using MyScript.InteractiveInk.Extensions;

namespace MyScript.InteractiveInk.ViewModels
{
    public partial class EditingCommandsViewModel
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

    public partial class EditingCommandsViewModel : ObservableAsync
    {
        [CanBeNull] public override CoreDispatcher Dispatcher { get; set; }
        [CanBeNull] private Editor Editor { get; set; }

        public void Initialize([NotNull] Editor editor)
        {
            Editor?.RemoveListener(this);
            Editor = editor;
            Editor.AddListener(this);
        }
    }

    public partial class EditingCommandsViewModel : IDisposable
    {
        public void Dispose()
        {
            Editor?.RemoveListener(this);
        }
    }

    public partial class EditingCommandsViewModel : IEditingCommands
    {
        private ICommand _commandRedo;
        private ICommand _commandTypeset;
        private ICommand _commandUndo;

        public ICommand CommandTypeset => _commandTypeset ??= new RelayCommand(_ => Editor?.WaitForIdleAndTypeset());
        public ICommand CommandRedo => _commandRedo ??= new RelayCommand(_ => Editor?.WaitForIdleAndRedo());
        public ICommand CommandUndo => _commandUndo ??= new RelayCommand(_ => Editor?.WaitForIdleAndUndo());
    }

    public partial class EditingCommandsViewModel : IEditorListener
    {
        public void PartChanged([NotNull] Editor editor)
        {
            CanRedo = editor.CanRedo();
            CanUndo = editor.CanUndo();
        }

        public void ContentChanged([NotNull] Editor editor, string[] blockIds)
        {
            CanRedo = editor.CanRedo();
            CanUndo = editor.CanUndo();
        }

        public void OnError([NotNull] Editor editor, string blockId, string message)
        {
        }
    }
}

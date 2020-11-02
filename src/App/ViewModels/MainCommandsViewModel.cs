using System;
using System.IO;
using System.Windows.Input;
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
        private ICommand _addCommand;

        private ICommand _redoCommand;

        private ICommand _typesetCommand;
        private ICommand _undoCommand;

        public ICommand AddCommand => _addCommand ??= new RelayCommand<PartType>(OnAddCommandExecuted);


        public ICommand RedoCommand => _redoCommand ??= new RelayCommand(_ => Editor.WaitForIdleAndRedo());


        public ICommand TypesetCommand => _typesetCommand ??= new RelayCommand(_ => Editor.WaitForIdleAndTypeset());
        public ICommand UndoCommand => _undoCommand ??= new RelayCommand(_ => Editor.WaitForIdleAndUndo());

        private void OnAddCommandExecuted(PartType type)
        {
            if (!(Editor is { } editor) || !(Package is {} package))
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

        public ICommand CreateCommand => _createCommand ??= new RelayCommand<PartType>(OnCreateCommandExecuted);

        public ICommand OpenCommand =>
            _openCommand ??= new RelayCommand(async _ => Package = await Editor.WaitForIdleAndOpenAsync());

        public ICommand SaveCommand =>
            _saveCommand ??= new RelayCommand(async _ => await Editor.WaitForIdleAndSaveAsync());

        public ICommand SaveAsCommand =>
            _saveAsCommand ??= new RelayCommand(async _ => await Editor.WaitForIdleAndSaveAsync(true));

        private void OnCreateCommandExecuted(PartType type)
        {
            var folder = ApplicationData.Current.LocalCacheFolder;
            var name = $"{Path.GetRandomFileName()}.iink";
            var path = Path.Combine(folder.Path, name);
            Package = Editor.Open(path, type);
        }
    }

    public partial class MainCommandsViewModel : ObservableAsync
    {
        [CanBeNull] public override CoreDispatcher Dispatcher { get; set; }
        [CanBeNull] public Editor Editor { get; set; }
        [CanBeNull] public ContentPackage Package { get; set; }

        public void Initialize([NotNull] Editor editor)
        {
            Editor = editor;
            Editor.AddListener(this);
        }
    }

    public partial class MainCommandsViewModel : IEditorListener
    {
        public async void PartChanged(Editor editor)
        {
            CanRedo = editor.CanRedo();
            CanUndo = editor.CanUndo();
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
}

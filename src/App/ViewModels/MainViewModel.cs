using System;
using System.Numerics;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Common.Enumerations;
using MyScript.InteractiveInk.Common.ViewModels;
using MyScript.InteractiveInk.Extensions;
using MyScript.InteractiveInk.UI.Services;

namespace MyScript.InteractiveInk.ViewModels
{
    public sealed partial class MainViewModel : ObservableAsync
    {
        private MainCommandsViewModel _commands;
        private Editor _editor;

        public MainCommandsViewModel Commands
        {
            get => _commands;
            set => Set(ref _commands, value, nameof(Commands));
        }

        public Editor Editor
        {
            get => _editor;
            set => Set(ref _editor, value, nameof(Editor));
        }

        public override CoreDispatcher Dispatcher { get; set; }

        public void Initialize([NotNull] CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            Commands = new MainCommandsViewModel
            {
                Dispatcher = dispatcher,
                ContentCommands = new ContentCommandsViewModel {Dispatcher = dispatcher},
                EditingCommands = new EditingCommandsViewModel {Dispatcher = dispatcher},
                PageCommands = new PageCommandsViewModel {Dispatcher = dispatcher}
            };
        }
    }

    public sealed partial class MainViewModel : IDisposable
    {
        private static DisplayInformation Display => DisplayInformation.GetForCurrentView();
        private static Vector2 Dpi => Display.GetDpi();

        public void Dispose()
        {
            Commands?.Dispose();
            Editor?.RemoveListener(this);
        }

        public void Initialize([NotNull] IRenderTarget target)
        {
            Editor?.RemoveListener(this);
            var engine = ((App)Application.Current).Engine;
            Commands.Initialize(Editor = engine.CreateEditor(engine.CreateRenderer(Dpi.X, Dpi.Y, target)));
            Editor.SetFontMetricsProvider(new FontMetricsService(Dpi));
            Commands.Initialize(target);
            Commands.CommandCreatePackage.Execute(ContentType.TextDocument);
            Editor.AddListener(this);
        }
    }

    public sealed partial class MainViewModel : IEditorListener
    {
        public void PartChanged(Editor editor)
        {
        }

        public void ContentChanged(Editor editor, string[] blockIds)
        {
        }

        public void OnError(Editor editor, string blockId, string message)
        {
            Dispatcher?.RunAsync(CoreDispatcherPriority.Normal,
                async () => await new MessageDialog(message, blockId).ShowAsync())?.AsTask();
        }
    }
}

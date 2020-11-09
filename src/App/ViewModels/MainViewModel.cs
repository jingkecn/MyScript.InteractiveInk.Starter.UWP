using System;
using System.Numerics;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Common.ViewModels;
using MyScript.InteractiveInk.UI.Enumerations;
using MyScript.InteractiveInk.UI.Extensions;
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
            Commands = new MainCommandsViewModel {Dispatcher = dispatcher};
        }
    }

    public sealed partial class MainViewModel : IDisposable
    {
        private static DisplayInformation Display => DisplayInformation.GetForCurrentView();
        private static Vector2 Dpi => Display.GetDpi();

        public void Dispose()
        {
            Editor?.Dispose();
        }

        public void Initialize([NotNull] IRenderTarget target)
        {
            var engine = ((App)Application.Current).Engine;
            Commands.Initialize(Editor = engine.CreateEditor(engine.CreateRenderer(Dpi.X, Dpi.Y, target)));
            Commands.Initialize(target);
            Editor.SetFontMetricsProvider(new FontMetricsService(Dpi));
            Commands.CreateCommand.Execute(ContentType.TextDocument);
        }
    }
}

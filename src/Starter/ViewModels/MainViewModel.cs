using System;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Microsoft.Toolkit.Helpers;
using MyScript.IInk;
using MyScript.InteractiveInk.Annotations;
using MyScript.InteractiveInk.Common;
using MyScript.InteractiveInk.Extensions;
using MyScript.InteractiveInk.Services;

namespace MyScript.InteractiveInk.ViewModels
{
    public sealed partial class MainViewModel : Observable
    {
        private Editor _editor;

        public Editor Editor
        {
            get => _editor;
            set => Set(ref _editor, value, nameof(Editor));
        }
    }

    public sealed partial class MainViewModel : IDisposable
    {
        public void Dispose()
        {
        }

        public void Initialize([NotNull] IRenderTarget target)
        {
            var engine = ((App)Application.Current).Engine;
            Initialize(Editor = engine.CreateEditor(engine.CreateRenderer(target)));
        }

        public static void Initialize([NotNull] Editor editor)
        {
            editor.SetFontMetricsProvider(Singleton<FontMetricsService>.Instance);
            var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, $"{Path.GetRandomFileName()}.iink");
            editor.Part = editor.Engine.CreatePackage(path).CreatePart("Diagram");
        }
    }
}

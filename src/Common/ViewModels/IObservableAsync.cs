using Windows.UI.Core;

namespace MyScript.InteractiveInk.Common.ViewModels
{
    public interface IObservableAsync : IObservable
    {
        CoreDispatcher Dispatcher { get; }
    }
}

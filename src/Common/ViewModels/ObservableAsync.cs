using System.Runtime.CompilerServices;
using Windows.UI.Core;
using MyScript.InteractiveInk.Annotations;

namespace MyScript.InteractiveInk.Common.ViewModels
{
    public abstract partial class ObservableAsync : Observable
    {
        [NotifyPropertyChangedInvocator]
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () => base.OnPropertyChanged(propertyName));
        }
    }

    public abstract partial class ObservableAsync : IObservableAsync
    {
        public abstract CoreDispatcher Dispatcher { get; set; }
    }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using MyScript.InteractiveInk.Annotations;

namespace MyScript.InteractiveInk.ViewModels
{
    /// <summary>
    ///     Sets a property and raise <code cref="PropertyChanged">PropertyChanged</code> event.
    /// </summary>
    public partial class Observable
    {
        public void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(value, storage))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }
    }

    /// <summary>
    ///     Implements <code cref="INotifyPropertyChanged">INotifyPropertyChanged</code>.
    /// </summary>
    public partial class Observable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

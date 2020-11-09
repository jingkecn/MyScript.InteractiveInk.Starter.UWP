using System.Windows.Input;

namespace MyScript.InteractiveInk.Common.Commands
{
    public partial interface IPageCommands
    {
        ICommand CommandAddPage { get; }
        ICommand CommandRemovePage { get; }
    }

    public partial interface IPageCommands
    {
        ICommand CommandGoToNextPage { get; }
        ICommand CommandGoToPreviousPage { get; }
    }
}

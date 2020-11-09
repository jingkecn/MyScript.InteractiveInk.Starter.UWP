using System.Windows.Input;

namespace MyScript.InteractiveInk.Common.Commands
{
    public partial interface IEditingCommands
    {
        ICommand CommandRedo { get; }
        ICommand CommandUndo { get; }
    }

    public partial interface IEditingCommands
    {
        ICommand CommandTypeset { get; }
    }
}

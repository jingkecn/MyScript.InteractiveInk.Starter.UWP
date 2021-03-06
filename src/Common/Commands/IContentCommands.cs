using System.Windows.Input;

namespace MyScript.InteractiveInk.Common.Commands
{
    public interface IContentCommands
    {
        ICommand CommandAddContent { get; }
        ICommand CommandAddImage { get; }
        ICommand CommandAppendContent { get; }
        ICommand CommandRemoveContent { get; }
    }
}

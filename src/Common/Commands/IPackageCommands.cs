using System.Windows.Input;

namespace MyScript.InteractiveInk.Common.Commands
{
    public interface IPackageCommands
    {
        ICommand CommandCreatePackage { get; }
        ICommand CommandOpenPackage { get; }
        ICommand CommandSavePackage { get; }
    }
}

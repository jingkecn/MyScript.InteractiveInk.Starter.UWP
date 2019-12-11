namespace MyScript.InteractiveInk.Services.Ink.UndoRedo
{
    public interface IUndoRedoOperation
    {
        void ExecuteRedo();
        void ExecuteUndo();
    }
}

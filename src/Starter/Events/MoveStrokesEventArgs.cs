using Windows.Foundation;

namespace MyScript.InteractiveInk.Events
{
    public class MoveStrokesEventArgs : TransferStrokesEventArgs
    {
        public Point FromPosition { get; set; }
        public Point ToPosition { get; set; }
    }
}

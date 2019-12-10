using Windows.Foundation;

namespace MyScript.InteractiveInk.Events
{
    public class MoveStrokesEventArgs : TransferStrokesEventArgs
    {
        public Point From { get; set; }
        public Point To { get; set; }
    }
}

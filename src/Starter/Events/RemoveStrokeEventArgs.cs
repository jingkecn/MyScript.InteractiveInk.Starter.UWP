using System;
using Windows.UI.Input.Inking;

namespace MyScript.InteractiveInk.Events
{
    public class RemoveStrokeEventArgs : EventArgs
    {
        public InkStroke RemovedStroke { get; set; }
    }
}

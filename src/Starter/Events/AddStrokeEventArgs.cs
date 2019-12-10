using System;
using Windows.UI.Input.Inking;

namespace MyScript.InteractiveInk.Events
{
    public class AddStrokeEventArgs : EventArgs
    {
        public InkStroke NewStroke { get; set; }
        public InkStroke OldStroke { get; set; }
    }
}

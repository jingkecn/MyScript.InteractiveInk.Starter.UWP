using System;
using System.Collections.Generic;
using Windows.UI.Input.Inking;

namespace MyScript.InteractiveInk.Events
{
    public class TransferStrokesEventArgs : EventArgs
    {
        public IEnumerable<InkStroke> Strokes { get; set; }
    }
}

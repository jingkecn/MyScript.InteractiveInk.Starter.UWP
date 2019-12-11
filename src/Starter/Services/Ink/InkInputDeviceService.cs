using System;
using Windows.Devices.Input;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;

namespace MyScript.InteractiveInk.Services.Ink
{
    public partial class InkInputDeviceService
    {
        private readonly InkCanvas _inkCanvas;

        public InkInputDeviceService(InkCanvas inkCanvas)
        {
            _inkCanvas = inkCanvas;
            var presenter = _inkCanvas.InkPresenter;
            presenter.UnprocessedInput.PointerEntered += UnprocessedInput_PointerEntered;
        }

        private void UnprocessedInput_PointerEntered(InkUnprocessedInput sender, PointerEventArgs args)
        {
            switch (args.CurrentPoint.PointerDevice.PointerDeviceType)
            {
                case PointerDeviceType.Pen:
                    OnPenDetected(this, EventArgs.Empty);
                    break;
                case PointerDeviceType.Touch:
                case PointerDeviceType.Mouse:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Enable(CoreInputDeviceTypes types, bool enabled)
        {
            var presenter = _inkCanvas.InkPresenter;
            var current = presenter.InputDeviceTypes;
            presenter.InputDeviceTypes = enabled ? current | types : current & ~types;
        }
    }

    public partial class InkInputDeviceService
    {
        public event EventHandler<EventArgs> PenDetected;

        protected virtual void OnPenDetected(object sender, EventArgs args)
        {
            PenDetected?.Invoke(sender, args);
        }
    }
}

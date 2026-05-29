using System;

namespace DiveCat.God.UI.Popups
{
    public interface IPopup
    {
        PopupState State { get; }
        void Open(Action onComplete = null);
        void Close(Action onComplete = null);
    }

    public enum PopupState
    {
        Closed,
        Opening,
        Opened,
        Closing
    }
}

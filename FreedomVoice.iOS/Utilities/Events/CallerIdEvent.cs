using System;
using FreedomVoice.iOS.Entities;

namespace FreedomVoice.iOS.Utilities.Events
{
    public static class CallerIdEvent
    {
        public static event EventHandler CallerIdChanged;

        public static void OnCallerIdChangedEvent(CallerIdEventArgs args)
        {
            EventHandler evt = CallerIdChanged;
            evt?.Invoke(null, args);
        }
    }

    public class CallerIdEventArgs : EventArgs
    {
        public PresentationNumber SelectedPresentationNumber { get; private set; }       

        public CallerIdEventArgs(PresentationNumber selectedPresentationNumber)
        {
            SelectedPresentationNumber = selectedPresentationNumber;            
        }
    }
}
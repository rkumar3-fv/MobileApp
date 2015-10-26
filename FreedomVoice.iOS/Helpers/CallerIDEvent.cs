using FreedomVoice.iOS.Entities;
using System;

namespace FreedomVoice.iOS.Helpers
{
    public static class CallerIDEvent
    {
        public static event EventHandler CallerIDChanged;

        public static void OnCallerIDChangedEvent(CallerIDEventArgs args)
        {
            EventHandler evt = CallerIDChanged;
            evt?.Invoke(null, args);
        }
    }

    public class CallerIDEventArgs : EventArgs
    {
        public PresentationNumber SelectedPresentationNumber { get; private set; }       

        public CallerIDEventArgs(PresentationNumber selectedPresentationNumber)
        {
            SelectedPresentationNumber = selectedPresentationNumber;            
        }
    }
}
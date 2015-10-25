using FreedomVoice.iOS.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreedomVoice.iOS.Helpers
{
    public static class CallerIDEvent
    {
        public static event EventHandler CallerIDChanged;

        public static void OnCallerIDChangedEvent(CallerIDEventArgs args)
        {
            EventHandler evt = CallerIDChanged;
            if (evt != null)
                evt(null, args);
        }
    }

    public class CallerIDEventArgs : EventArgs
    {
        public PresentationNumber SelectedPresentationNumber { get; private set; }       

        public CallerIDEventArgs(PresentationNumber SelectedPresentationNumber)
        {
            this.SelectedPresentationNumber = SelectedPresentationNumber;            
        }
    }
}

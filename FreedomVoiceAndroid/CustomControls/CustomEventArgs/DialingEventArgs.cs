using System;

namespace com.FreedomVoice.MobileApp.Android.CustomControls.CustomEventArgs
{
    /// <summary>
    /// Dialing event arguments
    /// </summary>
    public class DialingEventArgs : EventArgs
    {
        /// <summary>
        /// Is call ended flag
        /// </summary>
        public bool IsCallEnded { get; }

        /// <summary>
        /// Calling number
        /// </summary>
        public string Number { get; }

        public DialingEventArgs(bool isCallEnded, string number)
        {
            IsCallEnded = isCallEnded;
            Number = number;
        }
    }
}
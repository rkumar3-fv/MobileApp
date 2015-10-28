using Android.Telephony;
using Android.Util;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    /// <summary>
    /// CallState event delegate
    /// </summary>
    /// <param name="sender">CallState helper</param>
    /// <param name="args">Dialing event arguments</param>
    public delegate void DialingEvent(object sender, DialingEventArgs args);

    /// <summary>
    /// Call state monitoring helper
    /// </summary>
    public class CallStateHelper : PhoneStateListener
    {
        /// <summary>
        /// Dailing event
        /// </summary>
        public event DialingEvent CallEvent;

        private string _number;

        /// <summary>
        /// Call state
        /// </summary>
        public bool IsCalled { get; private set; }

        public override void OnCallStateChanged(CallState state, string incomingNumber)
        {
            switch (state)
            {
                case CallState.Ringing:
                    Log.Debug(App.AppPackage, "CALL FROM number: " + incomingNumber);
                    _number = incomingNumber;
                    IsCalled = true;
                    break;
                case CallState.Offhook:
                    Log.Debug(App.AppPackage, "START DIALING TO number: "+ incomingNumber);
                    _number = incomingNumber;
                    IsCalled = true;
                    break;
                case CallState.Idle:
                    Log.Debug(App.AppPackage, "STOP TALKING WITH number: " + _number);
                    if (IsCalled)
                    {
                        CallEvent?.Invoke(this, new DialingEventArgs(IsCalled, _number));
                        IsCalled = false;
                        _number = "";
                    }
                    break;
            }
        }
    }
}
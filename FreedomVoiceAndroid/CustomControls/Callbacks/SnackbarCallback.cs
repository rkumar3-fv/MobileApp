using System;
using Android.Support.Design.Widget;

namespace com.FreedomVoice.MobileApp.Android.CustomControls.Callbacks
{
    public delegate void SnackbarCallbackEvent(object sender, EventArgs args);

    /// <summary>
    /// Custom snackbar callback
    /// </summary>
    public class SnackbarCallback : Snackbar.Callback
    {
        /// <summary>
        /// Snackbar dismiss event
        /// </summary>
        public event SnackbarCallbackEvent SnackbarEvent;

        public override void OnDismissed(Snackbar snackbar, int evt)
        {
            base.OnDismissed(snackbar, evt);
            SnackbarEvent?.Invoke(this, new EventArgs());
        }
    }
}
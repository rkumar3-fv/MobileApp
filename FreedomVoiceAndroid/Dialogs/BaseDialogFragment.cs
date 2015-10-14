using System;
using Android.App;
using Android.OS;
using Android.Widget;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    public delegate void DialogEventHandler(object sender, DialogEventArgs args);

    public abstract class BaseDialogFragment : DialogFragment
    {
        protected Button OkButton;
        protected Button CancelButton;

        /// <summary>
        /// Dialog result event
        /// </summary>
        public event DialogEventHandler DialogEvent;

        public override void OnStart()
        {
            base.OnStart();
            OkButton.Click += OkButtonOnClick;
            CancelButton.Click += CancelButtonOnClick;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            var dialog = base.OnCreateDialog(savedInstanceState);
            dialog.RequestWindowFeature(StyleNoTitle);
            return dialog;
        }

        /// <summary>
        /// Apply button clicked
        /// </summary>
        private void CancelButtonOnClick(object sender, EventArgs eventArgs)
        {
            DialogEvent?.Invoke(this, new DialogEventArgs(DialogResult.Cancel));
            Dismiss();
        }

        /// <summary>
        /// Cancel button clicked
        /// </summary>
        private void OkButtonOnClick(object sender, EventArgs eventArgs)
        {
            DialogEvent?.Invoke(this, new DialogEventArgs(DialogResult.Ok));
            Dismiss();
        }
    }
}
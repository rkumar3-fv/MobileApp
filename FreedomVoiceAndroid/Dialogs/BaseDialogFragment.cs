using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
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
            if (OkButton != null)
                OkButton.Click += OkButtonOnClick;
            if (CancelButton != null)
                CancelButton.Click += CancelButtonOnClick;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            var dialog = base.OnCreateDialog(savedInstanceState);
            dialog.RequestWindowFeature(StyleNoTitle);
            dialog.SetCanceledOnTouchOutside(false);
            dialog.SetOnKeyListener(new BaseDialogKeyListener());
            return dialog;
        }

        /// <summary>
        /// Apply button clicked
        /// </summary>
        private void CancelButtonOnClick(object sender, EventArgs eventArgs)
        {
            Dismiss();
            DialogEvent?.Invoke(this, new DialogEventArgs(DialogResult.Cancel));
        }

        /// <summary>
        /// Cancel button clicked
        /// </summary>
        private void OkButtonOnClick(object sender, EventArgs eventArgs)
        {
            Dismiss();
            DialogEvent?.Invoke(this, new DialogEventArgs(DialogResult.Ok));
        }

        private class BaseDialogKeyListener : Java.Lang.Object, IDialogInterfaceOnKeyListener
        {
            public bool OnKey(IDialogInterface dialog, Keycode keyCode, KeyEvent e)
            {
                if (keyCode != Keycode.Back) return false;
                dialog.Cancel();
                return true;
            }
        }
    }
}
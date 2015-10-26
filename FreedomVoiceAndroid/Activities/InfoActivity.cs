using System;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Abstract activity with info & one confirmation button
    /// </summary>
    public abstract class InfoActivity : LogoutActivity
    {
        protected Button ActionButton;

        protected override void OnStart()
        {
            base.OnStart();
            ActionButton.Click += ActionButtonOnClick;
        }

        /// <summary>
        /// Action button click result
        /// </summary>
        protected abstract void ActionButtonOnClick(object sender, EventArgs eventArgs);
    }
}
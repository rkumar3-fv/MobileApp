using System;
using Android.Telephony;
using Android.Util;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using FreedomVoice.Core.Utils;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Message with audio content details
    /// </summary>
    public abstract class SoundActivity : MessageDetailsActivity
    {
        protected Button SpeakerButton;
        protected Button CallBackButton;

        protected override void OnStart()
        {
            base.OnStart();
            SpeakerButton.Click += SpeakerButtonOnClick;
            CallBackButton.Click += CallBackButtonOnClick;
        }

        protected override void OnResume()
        {
            base.OnResume();
            MessageStamp.Text = DataFormatUtils.ToDuration(Msg.Length);
        }

        /// <summary>
        /// Changing output sound device
        /// </summary>
        private void SpeakerButtonOnClick(object sender, EventArgs eventArgs)
        {

        }

        /// <summary>
        /// Call back action
        /// </summary>
        private void CallBackButtonOnClick(object sender, EventArgs eventArgs)
        {
            if ((Helper.PhoneNumber == null) || (Helper.PhoneNumber.Length == 0))
            {
                var noCellularDialog = new NoCellularDialogFragment();
                noCellularDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgCellular_title));
            }
            else
            {
                var normalizedNumber = PhoneNumberUtils.NormalizeNumber(Msg.FromNumber);
                Log.Debug(App.AppPackage, $"REDIAL: dial to {DataFormatUtils.ToPhoneNumber(normalizedNumber)}");
                Helper.Call(normalizedNumber);
            }
        }
    }
}
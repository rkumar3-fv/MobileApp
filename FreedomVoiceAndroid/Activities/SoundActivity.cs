using System;
using Android.Content;
using Android.Util;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Utils;
using Uri = Android.Net.Uri;

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
            var callIntent = new Intent(Intent.ActionCall, Uri.Parse("tel:" + Msg.FromNumber));
            Log.Debug(App.AppPackage, $"ACTIVITY {GetType().Name} CREATES CALL to {GetString(Resource.String.ActivityInactive_customerNumber)}");
            StartActivity(callIntent);
        }
    }
}
using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// 911 disclaimer info activity
    /// </summary>
    [Activity(
        Label = "@string/ActivityDisclaimer_title",
        Theme = "@style/AppThemeActionBar",
        ScreenOrientation = ScreenOrientation.Portrait,
        NoHistory = true)]
    public class DisclaimerActivity : InfoActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_disclaimer);
            RootLayout = FindViewById(Resource.Id.disclaimerActivity_root);
            ActionButton = FindViewById<Button>(Resource.Id.disclaimerActivity_agreeButton);
        }

        /// <summary>
        /// Agree button click action
        /// </summary>
        protected override void ActionButtonOnClick(object sender, EventArgs eventArgs)
        {
            var intent = new Intent(this, typeof (ContentActivity));
            Helper.DisclaimerApplied();
            StartActivity(intent);
        }
    }
}
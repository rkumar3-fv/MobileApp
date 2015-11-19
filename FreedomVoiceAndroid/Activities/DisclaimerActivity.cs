using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// 911 disclaimer info activity
    /// </summary>
    [Activity(
        Label = "@string/ActivityDisclaimer_title",
        Theme = "@style/AppThemeActionBar",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.StateAlwaysHidden, 
        NoHistory = true)]
    public class DisclaimerActivity : InfoActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_disclaimer);
            RootLayout = FindViewById(Resource.Id.disclaimerActivity_root);
            ActionButton = FindViewById<CardView>(Resource.Id.disclaimerActivity_agreeButton);
        }

        /// <summary>
        /// Agree button click action
        /// </summary>
        protected override void ActionButtonOnClick(object sender, EventArgs eventArgs)
        {
            var intent = new Intent(this, typeof (ContentActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            Helper.DisclaimerApplied();
            StartActivity(intent);
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }
    }
}
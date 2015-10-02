using System;
using Android.App;
using Android.OS;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    [Activity(
        Label = "@string/ActivityDisclaimer_title",
        Theme = "@style/AppThemeActionBar",
        NoHistory = true)]
    public class DisclaimerActivity : InfoActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_disclaimer);
            ActionButton = FindViewById<Button>(Resource.Id.disclaimerActivity_agreeButton);

            SupportActionBar.SetTitle(Resource.String.ActivityDisclaimer_title);
        }

        /// <summary>
        /// Agree button click action
        /// </summary>
        protected override void ActionButtonOnClick(object sender, EventArgs eventArgs)
        {
            
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {

        }
    }
}
using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Set phone number activity w/ back button
    /// </summary>
    [Activity(
        Label = "@string/ActivityNumber_title",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.AdjustResize,
        Theme = "@style/AppThemeActionBar",
        NoHistory = true)]
    public class SetNumberActivityWithBack : SetNumberActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_action_back);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            SkipButton.Visibility = ViewStates.Gone;
        }

        protected new void ApplyButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (SetupNumber())
                BaseBackPressed();
        }

        protected new void SkipButtonOnClick(object sender, EventArgs eventArgs)
        {
            BaseBackPressed();
        }

        public override void OnBackPressed()
        {
            BaseBackPressed();
        }
    }
}
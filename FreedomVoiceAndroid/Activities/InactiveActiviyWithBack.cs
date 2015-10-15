using Android.App;
using Android.Content.PM;
using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Inactive phone number status info w/ back button
    /// </summary>
    [Activity(
        Label = "@string/ActivityInactive_title",
        ScreenOrientation = ScreenOrientation.Portrait,
        Theme = "@style/AppThemeActionBar")]
    public class InactiveActiviyWithBack : InactiveActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_action_back);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }

        public override void OnBackPressed()
        {
            BaseBackPressed();
        }
    }
}
using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Base message details activity
    /// </summary>
    public abstract class MessageDetailsActivity : LogoutActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_action_back);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }
    }
}
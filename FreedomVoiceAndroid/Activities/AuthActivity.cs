using Android.App;
using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Authorization activity
    /// </summary>
    [Activity(
        Label = "@string/ApplicationTitle", 
        MainLauncher = true, 
        Icon = "@mipmap/ic_launcher", 
        Theme = "@style/AppThemeActionBar",
        NoHistory = true)]
    public class AuthActivity : BaseActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_auth);
        }
    }
}


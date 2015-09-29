using Android.App;
using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Main pplication screen
    /// </summary>
    [Activity(
        Label = "@string/ApplicationTitle",
        Icon = "@drawable/ic_launcher")]
    class ContentActivity : BaseActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_content);
        }
    }
}
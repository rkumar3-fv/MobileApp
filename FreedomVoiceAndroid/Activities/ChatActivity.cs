using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Fragments;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    [Activity(
        Label = "@string/ApplicationTitle",
        Icon = "@mipmap/ic_launcher",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.AdjustResize,
        Theme = "@style/AuthAppTheme")]
    public class ChatActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.act_chat);
            
            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.container, new ConversationDetailFragment())
                .Commit();
        }
    }
}
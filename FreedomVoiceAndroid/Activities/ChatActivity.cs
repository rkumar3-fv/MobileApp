using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
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
        private static string EXTRA_CONVERSATION_ID = "EXTRA_CONVERSATION_ID";
        private static string EXTRA_CONVERSATION_PHONE = "EXTRA_CONVERSATION_PHONE";
        public static void Start(Activity context, long conversationId, string phone)
        {
            var intent = new Intent(context, typeof(ChatActivity));
            intent.PutExtra(EXTRA_CONVERSATION_ID, conversationId);
            intent.PutExtra(EXTRA_CONVERSATION_PHONE, phone);
            context.StartActivity(intent);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.act_chat);
            var toolbar = FindViewById<Toolbar>(Resource.Id.chatActivity_toolbar);
            SetSupportActionBar(toolbar);
            toolbar.NavigationClick += (sender, args) => OnBackPressed();
            var conversationId = Intent.GetLongExtra(EXTRA_CONVERSATION_ID, 0);
            var phone = Intent.GetStringExtra(EXTRA_CONVERSATION_PHONE);
            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.chatActivity_container, ConversationDetailFragment.NewInstance(conversationId, phone))
                .Commit();
        }
    }
}
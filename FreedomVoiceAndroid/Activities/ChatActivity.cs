using Android.App;
using Android.Content;
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
        private static string EXTRA_CONVERSATION_ID = "EXTRA_CONVERSATION_ID";
        public static void Start(Activity context, int conversationId)
        {
            var intent = new Intent(context, typeof(ChatActivity));
            intent.PutExtra(EXTRA_CONVERSATION_ID, conversationId);
            context.StartActivity(intent);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.act_chat);

            var conversationId = Intent.GetIntExtra(EXTRA_CONVERSATION_ID, 0);
            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.chatActivity_container, ConversationDetailFragment.newInstance(conversationId))
                .Commit();
        }
    }
}
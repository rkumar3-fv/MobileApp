using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using com.FreedomVoice.MobileApp.Android.Fragments;
using Fragment = Android.Support.V4.App.Fragment;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    [Activity(
        Label = "@string/ApplicationTitle",
        Icon = "@mipmap/ic_launcher",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.AdjustResize,
        LaunchMode = LaunchMode.SingleTask,
        Theme = "@style/AuthAppTheme")]
    public class ChatActivity : BaseActivity
    {
        private const string ExtraScreenValueNewChat = "EXTRA_NEW_CHAT";
        private const string ExtraScreenValueChat = "EXTRA_CHAT";
        private const string ExtraScreen = "EXTRA_SCREEN";
        private const string ExtraConversationId = "EXTRA_CONVERSATION_ID";
        private const string ExtraConversationPhone = "EXTRA_CONVERSATION_PHONE";
        private const string ExtraMyPresentationPhone = "EXTRA_MY_PRESENTATION_PHONE";

        public static Intent OpenChat(Context context, long conversationId, string phone, string myPresentationPhone)
        {
            var intent = new Intent(context, typeof(ChatActivity));
            intent.PutExtra(ExtraScreen, ExtraScreenValueChat);
            intent.PutExtra(ExtraConversationId, conversationId);
            intent.PutExtra(ExtraConversationPhone, phone);
            intent.PutExtra(ExtraMyPresentationPhone, myPresentationPhone);
            return intent;
        }

        public static void StartNewChat(Activity context, string phone)
        {
            var intent = new Intent(context, typeof(ChatActivity));
            intent.PutExtra(ExtraScreen, ExtraScreenValueNewChat);
            intent.PutExtra(ExtraConversationPhone, phone);
            context.StartActivity(intent);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.act_chat);
            var toolbar = FindViewById<Toolbar>(Resource.Id.chatActivity_toolbar);
            SetSupportActionBar(toolbar);
            toolbar.NavigationClick += (sender, args) => OnBackPressed();
            var conversationId = Intent.GetLongExtra(ExtraConversationId, 0);
            var phone = Intent.GetStringExtra(ExtraConversationPhone);
            var myPresentationPhone = Intent.GetStringExtra(ExtraMyPresentationPhone);
            
            if (myPresentationPhone != null)
                Helper.TrySetCurrentPresentationNumber(myPresentationPhone);

            var screenKey = Intent.GetStringExtra(ExtraScreen);
            Fragment fragment;
            if (screenKey == ExtraScreenValueChat)
            {
                fragment = ConversationDetailFragment.NewInstance(conversationId, phone);
            }
            else
            {
                fragment = NewConversationDetailFragment.NewInstance(phone);
            }

            SupportFragmentManager.BeginTransaction()
                .Replace(Resource.Id.chatActivity_container, fragment)
                .Commit();
        }

        public override void OnBackPressed()
        {
            HideKeyboard();   
            base.OnBackPressed();
        }
        
        private void HideKeyboard()
        {
            var imm = (InputMethodManager) GetSystemService(Context.InputMethodService);
            var focusView = CurrentFocus?.WindowToken;
            if (focusView != null)
                imm.HideSoftInputFromWindow(focusView, HideSoftInputFlags.None);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            Intent = intent;
            Recreate();
        }
    }
}
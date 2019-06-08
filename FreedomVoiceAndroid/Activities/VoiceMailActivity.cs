using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Message with voicemail content
    /// </summary>
    [Activity(
        Label = "@string/ActivityVoice_title",
        Theme = "@style/AppThemeActionBar",
        LaunchMode = LaunchMode.SingleTask, 
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class VoiceMailActivity : SoundActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_voicemail);
            RootLayout = FindViewById(Resource.Id.voicemailActivity_root);
            LogoView = FindViewById<ImageView>(Resource.Id.voicemailActivity_logo);
            SenderText = FindViewById<TextView>(Resource.Id.voicemailActivity_senderText);
            MessageDate = FindViewById<TextView>(Resource.Id.voicemailActivity_dateText);
            MessageStamp = FindViewById<TextView>(Resource.Id.voicemailActivity_stampText);
            CallBackButton = FindViewById<Button>(Resource.Id.voicemailActivity_callbackButton);
            SmsButton = FindViewById<Button>(Resource.Id.voicemailActivity_smsButton);
            SpeakerButton = FindViewById<ToggleButton>(Resource.Id.voicemailActivity_speakerButton);
            Progress = FindViewById<ProgressBar>(Resource.Id.voicemailActivity_progress);
            PlayerButton = FindViewById<ImageButton>(Resource.Id.voicemailActivity_playerButton);
            StartTextView = FindViewById<TextView>(Resource.Id.voicemailActivity_playerStartText);
            EndTextView = FindViewById<TextView>(Resource.Id.voicemailActivity_playerEndText);
            PlayerSeek = FindViewById<SeekBar>(Resource.Id.voiceMailActivity_playerSeek);
            TouchLayout = FindViewById<RelativeLayout>(Resource.Id.voicemailActivity_touchArea);
            SupportActionBar.SetTitle(Resource.String.ActivityVoice_title);
        }

        protected override void OnStart()
        {
            base.OnStart();
            LogoView?.SetImageResource(Resource.Drawable.logo_voicemail);
        }
    }
}
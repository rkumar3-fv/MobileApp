using Android.App;
using Android.OS;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Message with voicemail content
    /// </summary>
    [Activity(
        Label = "@string/ActivityVoice_title",
        Theme = "@style/AppThemeActionBar")]
    public class VoiceMailActivity : SoundActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_voicemail);
            RemoveButton = FindViewById<ImageButton>(Resource.Id.voicemailActivity_deleteButton);
            SenderText = FindViewById<TextView>(Resource.Id.voicemailActivity_senderText);
            MessageDate = FindViewById<TextView>(Resource.Id.voicemailActivity_dateText);
            MessageStamp = FindViewById<TextView>(Resource.Id.voicemailActivity_stampText);
            CallBackButton = FindViewById<Button>(Resource.Id.voicemailActivity_callbackButton);
            SpeakerButton = FindViewById<Button>(Resource.Id.voicemailActivity_speakerButton);
            SupportActionBar.SetTitle(Resource.String.ActivityVoice_title);
        }
    }
}
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Message with record content
    /// </summary>
    [Activity(
        Label = "@string/ActivityRecord_title",
        Theme = "@style/AppThemeActionBar",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class VoiceRecordActivity : SoundActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_record);
            RootLayout = FindViewById(Resource.Id.recordActivity_root);
            RemoveButton = FindViewById<ImageButton>(Resource.Id.recordActivity_deleteButton);
            SenderText = FindViewById<TextView>(Resource.Id.recordActivity_senderText);
            MessageDate = FindViewById<TextView>(Resource.Id.recordActivity_dateText);
            MessageStamp = FindViewById<TextView>(Resource.Id.recordActivity_stampText);
            CallBackButton = FindViewById<Button>(Resource.Id.recordActivity_callbackButton);
            SpeakerButton = FindViewById<Button>(Resource.Id.recordActivity_speakerButton);
            SupportActionBar.SetTitle(Resource.String.ActivityRecord_title);
        }
    }
}
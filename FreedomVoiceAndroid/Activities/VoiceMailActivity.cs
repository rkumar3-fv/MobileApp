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
            SupportActionBar.SetTitle(Resource.String.ActivityVoice_title);
        }
    }
}
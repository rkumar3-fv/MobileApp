using Android.App;
using Android.OS;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Message with record content
    /// </summary>
    [Activity(
        Label = "@string/ActivityRecord_title",
        Theme = "@style/AppThemeActionBar")]
    public class VoiceRecordActivity : SoundActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_record);
            RemoveButton = FindViewById<ImageButton>(Resource.Id.recordActivity_deleteButton);
            SupportActionBar.SetTitle(Resource.String.ActivityRecord_title);
        }
    }
}
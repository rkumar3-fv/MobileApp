using Android.App;
using Android.OS;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Message with text content details
    /// </summary>
    [Activity(
        Label = "@string/ActivityFax_title",
        Theme = "@style/AppThemeActionBar")]
    public class FaxActivity : MessageDetailsActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_fax);
            RemoveButton = FindViewById<ImageButton>(Resource.Id.faxActivity_deleteButton);
            SenderText = FindViewById<TextView>(Resource.Id.faxActivity_senderText);
            MessageDate = FindViewById<TextView>(Resource.Id.faxActivity_dateText);
            MessageStamp = FindViewById<TextView>(Resource.Id.faxActivity_stampText);
            SupportActionBar.SetTitle(Resource.String.ActivityFax_title);
        }

        protected override void OnResume()
        {
            base.OnResume();
            MessageStamp.Text = Msg.Length == 1 ? GetString(Resource.String.FragmentMessages_onePage) : $"{Msg.Length} {GetString(Resource.String.FragmentMessages_morePage)}";
        }
    }
}
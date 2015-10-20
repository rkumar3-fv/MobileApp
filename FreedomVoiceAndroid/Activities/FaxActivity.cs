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
            SupportActionBar.SetTitle(Resource.String.ActivityFax_title);
        }
    }
}
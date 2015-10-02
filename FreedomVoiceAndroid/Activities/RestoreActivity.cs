using Android.App;
using Android.OS;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    [Activity(
        Label = "@string/ActivityRestore_title",
        Theme = "@style/AppThemeActionBar")]
    public class RestoreActivity : BaseActivity
    {
        private EditText _emailText;
        private Button _restoreButton;
        private TextView _resultLabel;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_restore);
            _emailText = FindViewById<EditText>(Resource.Id.restoreActivity_emailField);
            _restoreButton = FindViewById<Button>(Resource.Id.restoreActivity_sendButton);
            _resultLabel = FindViewById<TextView>(Resource.Id.restoreActivity_resultText);

            SupportActionBar.SetIcon(Resource.Drawable.ic_action_back);
            SupportActionBar.SetTitle(Resource.String.ActivityRestore_title);
        }
        
        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {

        }
    }
}
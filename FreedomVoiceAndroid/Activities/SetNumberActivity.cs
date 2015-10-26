using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Set phone number activity w/o back button
    /// </summary>
    [Activity(
        Label = "@string/ActivityNumber_title",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.AdjustResize,
        Theme = "@style/AppThemeActionBar")]
    public class SetNumberActivity : LogoutActivity
    {
        private Button _applyButton;
        private Button _skipButton;
        private EditText _phoneText;
        private TextView _phoneErrorText;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_phone);
            _phoneText = FindViewById<EditText>(Resource.Id.phoneActivity_phoneField);
            _applyButton = FindViewById<Button>(Resource.Id.phoneActivity_applyButton);
            _skipButton = FindViewById<Button>(Resource.Id.phoneActivity_skipButton);
            _phoneErrorText = FindViewById<TextView>(Resource.Id.phoneActivity_resultText);

            SupportActionBar.SetTitle(Resource.String.ActivityNumber_title);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_action_logout:
                    var logoutDialog = new LogoutDialogFragment();
                    logoutDialog.DialogEvent += OnDialogEvent;
                    logoutDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgLogout_title));
                    return true;
                case global::Android.Resource.Id.Home:
                    BaseBackPressed();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }

        protected void BaseBackPressed()
        {
            base.OnBackPressed();
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {

        }
    }
}
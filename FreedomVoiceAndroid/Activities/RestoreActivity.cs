using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;
using com.FreedomVoice.MobileApp.Android.Utils;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Password restoration activity
    /// </summary>
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
            _restoreButton.Click += RestoreButtonOnClick;

            SupportActionBar.SetTitle(Resource.String.ActivityRestore_title);
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_action_back);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }

        /// <summary>
        /// Restore button click action
        /// </summary>
        private void RestoreButtonOnClick(object sender, EventArgs e)
        {
            if (_emailText.Length() > 5)
                if (DataValidationUtils.IsEmailValid(_emailText.Text))
                {
                    _resultLabel.Text = "";
                    WaitingActions.Add(Helper.RestorePassword(_emailText.Text));
                    _restoreButton.Enabled = false;
                    return;
                }
             _resultLabel.Text = GetString(Resource.String.ActivityRestore_badResponse);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {

        }
    }
}
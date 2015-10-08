using System;
using Android.App;
using Android.Content;
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
        /// <summary>
        /// Debug data set
        /// REMOVE BEFORE RELEASE!
        /// </summary>
        private void DebugData()
        {
            //TODO: REMOVE
            _emailText.Text = "freedomvoice.user1.267055@gmail.com";
        }

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

        protected override void OnResume()
        {
            base.OnResume();
            DebugData();
        }

        /// <summary>
        /// Restore button click action
        /// </summary>
        private void RestoreButtonOnClick(object sender, EventArgs e)
        {
            if (_emailText.Length() > 5)
                if (DataValidationUtils.IsEmailValid(_emailText.Text))
                {
                    if (_resultLabel.Visibility == ViewStates.Visible)
                        _resultLabel.Visibility = ViewStates.Invisible;
                    _restoreButton.Enabled = false;
                    Helper.RestorePassword(_emailText.Text);
                    return;
                }
            if (_resultLabel.Visibility != ViewStates.Visible)
                _resultLabel.Visibility = ViewStates.Visible;
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

        /// <summary>
        /// Good restoration result action
        /// </summary>
        private void RestorationSuccessfull()
        {
            Toast.MakeText(this, Resource.String.ActivityRestore_goodResponse, ToastLength.Long).Show();
            var intent = new Intent(this, typeof(AuthActivity));
            StartActivity(intent);
        }
    }
}
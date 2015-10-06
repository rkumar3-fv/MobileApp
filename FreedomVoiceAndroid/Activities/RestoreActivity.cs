using System;
using System.Net.Mail;
using Android.App;
using Android.OS;
using Android.Views;
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
                if (IsEmailValid(_emailText.Text))
                {
                    _resultLabel.Text = "";
                    _waitingActions.Add(_helper.RestorePassword(_emailText.Text));
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

        /// <summary>
        /// E-Mail format checking
        /// </summary>
        /// <param name="email">entered e-mail</param>
        /// <returns>validation result</returns>
        private bool IsEmailValid(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
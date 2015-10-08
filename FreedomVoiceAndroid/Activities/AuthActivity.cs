using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Authorization activity
    /// </summary>
    [Activity(
        MainLauncher = true,
        Label = "@string/ApplicationTitle",
        Icon = "@mipmap/ic_launcher", 
        Theme = "@style/AppTheme")]
    public class AuthActivity : BaseActivity
    {
        private Button _authButton;
        private Button _forgotButton;
        private EditText _loginText;
        private EditText _passwordText;
        private TextView _errorText;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_auth);
            _authButton = FindViewById<Button>(Resource.Id.authActivity_loginButton);
            _forgotButton = FindViewById<Button>(Resource.Id.authActivity_restoreButton);
            _loginText = FindViewById<EditText>(Resource.Id.authActivity_loginField);
            _passwordText = FindViewById<EditText>(Resource.Id.authActivity_passwordField);
            _errorText = FindViewById<TextView>(Resource.Id.authActivity_errorText);

            _authButton.Click += AuthButtonOnClick;
            _forgotButton.Click += ForgotButtonOnClick;
        }

        /// <summary>
        /// Authorization action running
        /// </summary>
        private void AuthButtonOnClick(object sender, EventArgs eventArgs)
        {
            if ((_loginText.Length() > 0) && (_passwordText.Length() > 0))
            {
                if (_errorText.Visibility == ViewStates.Visible)
                    _errorText.Visibility = ViewStates.Invisible;
                _authButton.Enabled = false;
                Helper.Authorize(_loginText.Text, _passwordText.Text);
            }
            else
                if (_errorText.Visibility != ViewStates.Visible)
                    _errorText.Visibility = ViewStates.Visible;
        }

        /// <summary>
        /// Restore password in browser
        /// </summary>
        private void ForgotButtonOnClick(object sender, EventArgs e)
        {            
            var intent = new Intent(this, typeof(RestoreActivity));
            StartActivity(intent);
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="args">Result args</param>
        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }
    }
}


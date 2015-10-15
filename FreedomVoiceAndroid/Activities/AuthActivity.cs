using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
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
        WindowSoftInputMode = SoftInput.AdjustResize,
        Theme = "@style/AuthAppTheme")]
    public class AuthActivity : BaseActivity
    {
        private void DebugOnly()
        {
            _loginText.Text = "freedomvoice.adm.267055@gmail.com";
            _passwordText.Text = "adm654654";
        }

        private Button _authButton;
        private Button _forgotButton;
        private EditText _loginText;
        private EditText _passwordText;
        private TextView _errorTextLogin;
        private TextView _errorTextPassword;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_auth);
            _authButton = FindViewById<Button>(Resource.Id.authActivity_loginButton);
            _forgotButton = FindViewById<Button>(Resource.Id.authActivity_forgotButton);
            _loginText = FindViewById<EditText>(Resource.Id.authActivity_loginField);
            _passwordText = FindViewById<EditText>(Resource.Id.authActivity_passwordField);
            _errorTextLogin = FindViewById<TextView>(Resource.Id.authActivity_loginError);
            _errorTextPassword = FindViewById<TextView>(Resource.Id.authActivity_errorText);

            _authButton.Click += AuthButtonOnClick;
            _forgotButton.Click += ForgotButtonOnClick;
        }

        protected override void OnResume()
        {
            base.OnResume();
            DebugOnly();
        }

        /// <summary>
        /// Authorization action running
        /// </summary>
        private void AuthButtonOnClick(object sender, EventArgs eventArgs)
        {
            HideErrors();
            if (_loginText.Text.Length == 0)
            {
                _errorTextLogin.Text = GetString(Resource.String.ActivityAuth_badLogin);
                return;
            }
            if (_passwordText.Text.Length == 0)
            {
                _errorTextPassword.Text = GetString(Resource.String.ActivityAuth_badPassword);
                return;
            }
            var res = Helper.Authorize(_loginText.Text, _passwordText.Text);
            if (res == -1)
                Helper.GetAccounts();
        }

        private void HideErrors()
        {
            if (_errorTextLogin.Text.Length > 0)
                _errorTextLogin.Text = "";
            if (_errorTextPassword.Text.Length > 0)
                _errorTextPassword.Text = "";
        }

        /// <summary>
        /// Restore password in browser
        /// </summary>
        private void ForgotButtonOnClick(object sender, EventArgs e)
        {
            Log.Debug(App.AppPackage, "RESTORE LAUNCHED");
            var intent = new Intent(this, typeof(RestoreActivity));
            StartActivity(intent);
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="args">Result args</param>
        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.AuthLoginError:
                        _errorTextLogin.Text = GetString(Resource.String.ActivityAuth_incorrectLogin);
                        return;
                    case ActionsHelperEventArgs.AuthPasswdError:
                        _errorTextPassword.Text = GetString(Resource.String.ActivityAuth_incorrectPassword);
                        return;
                }
            }
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }
    }
}


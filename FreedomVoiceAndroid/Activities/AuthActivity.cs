using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Analytics;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;
using Java.Util;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Authorization activity
    /// </summary>
    [Activity(
        MainLauncher = true,
        Label = "@string/ApplicationTitle",
        Icon = "@mipmap/ic_launcher",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.AdjustResize,
        Theme = "@style/AuthAppTheme")]
    public class AuthActivity : BaseActivity
    {
        private void DebugOnly()
        {
            //_loginText.Text = "freedomvoice.adm.267055@gmail.com";
            //_passwordText.Text = "adm654654";
        }

        private Color _errorColor;
        private Button _authButton;
        private Button _forgotButton;
        private EditText _loginText;
        private EditText _passwordText;
        private TextView _errorTextLogin;
        private TextView _errorTextPassword;
        private ProgressBar _progressLogin;

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
            _progressLogin = FindViewById<ProgressBar>(Resource.Id.authActivity_progress);

            _authButton.Click += AuthButtonOnClick;
            _forgotButton.Click += ForgotButtonOnClick;
            _errorColor = new Color(ContextCompat.GetColor(this, Resource.Color.textColorError));

            var pInfo = PackageManager.GetPackageInfo(PackageName, 0);
            Appl.AnalyticsTracker.SetAppName(GetString(Resource.String.ApplicationName));
            Appl.AnalyticsTracker.SetAppVersion($"{pInfo.VersionCode} ({pInfo.VersionName})");
            Appl.AnalyticsTracker.SetLanguage(Locale.Default.Language);
            Appl.AnalyticsTracker.SetScreenResolution(Resources.DisplayMetrics.WidthPixels, Resources.DisplayMetrics.HeightPixels);
        }

        protected override void OnResume()
        {
            base.OnResume();
            DebugOnly();
            if (Helper.IsLoggedIn)
                Helper.GetAccounts();
            Appl.AnalyticsTracker.SetScreenName($"Activity {GetType().Name}");
            Appl.AnalyticsTracker.Send(new HitBuilders.ScreenViewBuilder().Build());
        }

        /// <summary>
        /// Authorization action running
        /// </summary>
        private void AuthButtonOnClick(object sender, EventArgs eventArgs)
        {
            HideErrors();
            if (_loginText.Text.Length == 0)
            {
                _loginText.Background.SetColorFilter(_errorColor, PorterDuff.Mode.SrcAtop);
                _errorTextLogin.Text = GetString(Resource.String.ActivityAuth_badLogin);
                return;
            }
            if (_passwordText.Text.Length == 0)
            {
                _passwordText.Background.SetColorFilter(_errorColor, PorterDuff.Mode.SrcAtop);
                _errorTextPassword.Text = GetString(Resource.String.ActivityAuth_badPassword);
                return;
            }
            var res = Helper.Authorize(_loginText.Text, _passwordText.Text);
            if (res == -1)
                Helper.GetAccounts();
            else
            {
                if (_progressLogin.Visibility == ViewStates.Invisible)
                    _progressLogin.Visibility = ViewStates.Visible;
                if (_authButton.Text.Length != 0)
                    _authButton.Text = "";
            }
        }

        private void HideErrors()
        {
            if (_errorTextLogin.Text.Length > 0)
            {
                _loginText.Background.ClearColorFilter();
                _errorTextLogin.Text = "";
            }
            if (_errorTextPassword.Text.Length > 0)
            {
                _passwordText.Background.ClearColorFilter();
                _errorTextPassword.Text = "";
            }
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
                if (_progressLogin.Visibility == ViewStates.Visible)
                    _progressLogin.Visibility = ViewStates.Invisible;
                if (_authButton.Text.Length == 0)
                    _authButton.Text = GetString(Resource.String.ActivityAuth_authButton);
                switch (code)
                {
                    case ActionsHelperEventArgs.ConnectionLostError:
                        Toast.MakeText(this, Resource.String.Snack_connectionLost, ToastLength.Long).Show();
                        return;
                    case ActionsHelperEventArgs.AuthLoginError:
                        _loginText.Background.SetColorFilter(_errorColor, PorterDuff.Mode.SrcAtop);
                        _errorTextLogin.Text = GetString(Resource.String.ActivityAuth_incorrectLogin);
                        return;
                    case ActionsHelperEventArgs.AuthPasswdError:
                        _passwordText.Background.SetColorFilter(_errorColor, PorterDuff.Mode.SrcAtop);
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


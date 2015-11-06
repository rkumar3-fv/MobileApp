using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Analytics;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
#if DEBUG
using Android.Util;
#endif
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;
using com.FreedomVoice.MobileApp.Android.Utils;
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
#if DEBUG
        private void DebugOnly()
        {
            _loginText.Text = "freedomvoice.adm.267055@gmail.com";
            _passwordText.Text = "adm654654";
        }
#endif
        private Color _errorColor;
        private Button _authButton;
        private Button _forgotButton;
        private EditText _loginText;
        private EditText _passwordText;
        private TextView _errorText;
        private TextView _errorTextLogin;
        private TextView _errorTextPassword;
        private ProgressBar _progressLogin;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_auth);
            RootLayout = FindViewById(Resource.Id.authActivity_root);
            _authButton = FindViewById<Button>(Resource.Id.authActivity_loginButton);
            _forgotButton = FindViewById<Button>(Resource.Id.authActivity_forgotButton);
            _loginText = FindViewById<EditText>(Resource.Id.authActivity_loginField);
            _passwordText = FindViewById<EditText>(Resource.Id.authActivity_passwordField);
            _errorTextLogin = FindViewById<TextView>(Resource.Id.authActivity_loginError);
            _errorTextPassword = FindViewById<TextView>(Resource.Id.authActivity_passwordError);
            _errorText = FindViewById<TextView>(Resource.Id.authActivity_errorText);
            _progressLogin = FindViewById<ProgressBar>(Resource.Id.authActivity_progress);

            _loginText.FocusChange += FieldOnFocusChange;
            _passwordText.FocusChange += FieldOnFocusChange;
            _authButton.Click += AuthButtonOnClick;
            _forgotButton.Click += ForgotButtonOnClick;
            _errorColor = new Color(ContextCompat.GetColor(this, Resource.Color.textColorError));
            var progressColor = new Color(ContextCompat.GetColor(this, Resource.Color.colorProgressWhite));
            _progressLogin.IndeterminateDrawable?.SetColorFilter(progressColor, PorterDuff.Mode.SrcIn);
            _progressLogin.ProgressDrawable?.SetColorFilter(progressColor, PorterDuff.Mode.SrcIn);

            var pInfo = PackageManager.GetPackageInfo(PackageName, 0);
            Appl.AnalyticsTracker.SetAppName(GetString(Resource.String.ApplicationName));
            Appl.AnalyticsTracker.SetAppVersion($"{pInfo.VersionCode} ({pInfo.VersionName})");
            Appl.AnalyticsTracker.SetLanguage(Locale.Default.Language);
            Appl.AnalyticsTracker.SetScreenResolution(Resources.DisplayMetrics.WidthPixels, Resources.DisplayMetrics.HeightPixels);

#if TRACE
#if !DEBUG
            HockeyApp.UpdateManager.Register(this, App.HockeyAppKey);
#endif
#endif
        }

        private void FieldOnFocusChange(object sender, View.FocusChangeEventArgs focusChangeEventArgs)
        {
            if ((sender != _loginText) && (sender != _passwordText)) return;
            if (focusChangeEventArgs.HasFocus)
                HideErrors();
        }

        protected override void OnResume()
        {
            base.OnResume();
#if DEBUG
            DebugOnly();
#endif
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
            if ((_loginText.Text.Length == 0)||(!DataValidationUtils.IsEmailValid(_loginText.Text)))
            {
                _loginText.Background.SetColorFilter(_errorColor, PorterDuff.Mode.SrcAtop);
                _errorTextLogin.Visibility = ViewStates.Visible;
                return;
            }
            if (_passwordText.Text.Length == 0)
            {
                _passwordText.Background.SetColorFilter(_errorColor, PorterDuff.Mode.SrcAtop);
                _errorTextPassword.Visibility = ViewStates.Visible;
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
            if (_errorTextLogin.Visibility == ViewStates.Visible)
            {
                _loginText.Background.ClearColorFilter();
                _errorTextLogin.Visibility = ViewStates.Invisible;
            }
            if (_errorTextPassword.Visibility == ViewStates.Visible)
            {
                _passwordText.Background.ClearColorFilter();
                _errorTextPassword.Visibility = ViewStates.Invisible;
            }
            if (_errorText.Visibility == ViewStates.Visible)
                _errorText.Visibility = ViewStates.Invisible;
        }

        /// <summary>
        /// Restore password in browser
        /// </summary>
        private void ForgotButtonOnClick(object sender, EventArgs e)
        {
#if DEBUG
            Log.Debug(App.AppPackage, "RESTORE LAUNCHED");
#endif
            var intent = new Intent(this, typeof(RestoreActivity));
            if ((_loginText.Text.Length > 0) && (DataValidationUtils.IsEmailValid(_loginText.Text)))
                intent.PutExtra(RestoreActivity.EmailField, _loginText.Text);
            StartActivity(intent);
        }

        /// <summary>
        /// Helper event callback action
        /// </summary>
        /// <param name="args">Result args</param>
        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            base.OnHelperEvent(args);
            foreach (var code in args.Codes)
            {
                if (_progressLogin.Visibility == ViewStates.Visible)
                    _progressLogin.Visibility = ViewStates.Invisible;
                if (_authButton.Text.Length == 0)
                    _authButton.Text = GetString(Resource.String.ActivityAuth_authButton);
                switch (code)
                {
                    case ActionsHelperEventArgs.AuthLoginError:
                    case ActionsHelperEventArgs.AuthPasswdError:
                        _errorText.Visibility = ViewStates.Visible;
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


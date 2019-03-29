using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
#if DEBUG
using Android.Util;
#endif
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;
using FreedomVoice.Core.Utils;
using Java.Interop;
using Java.Lang;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Authorization activity
    /// </summary>
    [Activity(
        Label = "@string/ApplicationTitle",
        Icon = "@mipmap/ic_launcher",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.AdjustResize,
        Theme = "@style/AuthAppTheme")]
    public class AuthActivity : BaseActivity
    {
        private Color _errorColor;
        private CardView _authButton;
        private TextView _authLabel;
        private Button _forgotButton;
        private EditText _loginText;
        private EditText _passwordText;
        private TextView _errorText;
        private ProgressBar _progressLogin;

        protected override void OnCreate(Bundle bundle)
        {
            if ((Intent.Flags & ActivityFlags.BroughtToFront) != 0)
            {
                Finish();
                return;
            }
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_auth);
            RootLayout = FindViewById(Resource.Id.authActivity_root);
            _authButton = FindViewById<CardView>(Resource.Id.authActivity_loginButton);
            _authLabel = FindViewById<TextView>(Resource.Id.authActivity_loginLabel);
            _forgotButton = FindViewById<Button>(Resource.Id.authActivity_forgotButton);
            _loginText = FindViewById<EditText>(Resource.Id.authActivity_loginField);
            _passwordText = FindViewById<EditText>(Resource.Id.authActivity_passwordField);
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

//#if DEBUG
            FillInLoginData();
//#endif
        }

        /// <summary>
        /// Only for test purposes, will be removed later
        /// </summary>
        private void FillInLoginData()
        {
            _loginText.Text = "freedomvoice.adm.267055@gmail.com";
            _passwordText.Text = "adm654654";
        }

#if !DEBUG


        protected override void OnResume()
        {
            base.OnResume();

            if (!Appl.ApplicationHelper.IsInsigthsOn)
                Appl.ApplicationHelper.InitInsights();
        }
#endif

        private void FieldOnFocusChange(object sender, View.FocusChangeEventArgs focusChangeEventArgs)
        {
            if ((sender != _loginText) && (sender != _passwordText)) return;
            if (focusChangeEventArgs.HasFocus)
                HideErrors();
        }

        /// <summary>
        /// Authorization action running
        /// </summary>
        private void AuthButtonOnClick(object sender, EventArgs eventArgs)
        {
            HideErrors();
            var imm = GetSystemService(InputMethodService).JavaCast<InputMethodManager>();
            if (RootLayout != null)
                imm?.HideSoftInputFromWindow(RootLayout.WindowToken, 0);

            if (Appl.ApplicationHelper.IsAirplaneModeOn())
            {
                AirplaneDialog();
                return;
            }

            if (!Appl.ApplicationHelper.IsInternetConnected())
            {
                if (RootLayout != null)
                    try
                    {
                        Snackbar.Make(RootLayout, Resource.String.Snack_noInternet, Snackbar.LengthLong).Show();
                    }
                    catch (RuntimeException)
                    {
#if DEBUG
                        Log.Debug(App.AppPackage, "SNACKBAR creation failed. Please, REBUILD APP.");
#else
                        Appl.ApplicationHelper.Reports?.Log("SNACKBAR creation failed. Please, REBUILD APP.");
#endif
                        Toast.MakeText(this, Resource.String.Snack_noInternet, ToastLength.Long).Show();
                    }
                    
                else
                    Toast.MakeText(this, Resource.String.Snack_noInternet, ToastLength.Long).Show();
                return;
            }

            if ((_loginText.Text.Trim().Length < 6)||(!DataFormatUtils.IsValidEmail(_loginText.Text.Trim())))
            {
                _loginText.Background.SetColorFilter(_errorColor, PorterDuff.Mode.SrcAtop);
                _errorText.Text = GetString(Resource.String.ActivityAuth_badLogin);
                _errorText.Visibility = ViewStates.Visible;
                return;
            }
            if (_passwordText.Text.Length == 0)
            {
                _passwordText.Background.SetColorFilter(_errorColor, PorterDuff.Mode.SrcAtop);
                _errorText.Text = GetString(Resource.String.ActivityAuth_badPassword);
                _errorText.Visibility = ViewStates.Visible;
                return;
            }
            var res = Helper.Authorize(_loginText.Text.Trim(), _passwordText.Text);
            if (res == -1)
                Helper.GetAccounts();
            else
            {
                if (_progressLogin.Visibility == ViewStates.Invisible)
                    _progressLogin.Visibility = ViewStates.Visible;
                if (_authLabel.Visibility == ViewStates.Visible)
                    _authLabel.Visibility = ViewStates.Invisible;
            }
        }

        private void HideErrors()
        {
            _loginText.Background.ClearColorFilter();
            _passwordText.Background.ClearColorFilter();
            if (_errorText.Text.Length != 0)
            {
                _errorText.Text = "";
                _errorText.Visibility = ViewStates.Invisible;
            }
        }

        /// <summary>
        /// Restore password in browser
        /// </summary>
        private void ForgotButtonOnClick(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(RestoreActivity));
            if ((_loginText.Text.Length > 0) && DataFormatUtils.IsValidEmail(_loginText.Text))
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
                if (_authLabel.Visibility == ViewStates.Invisible)
                    _authLabel.Visibility = ViewStates.Visible;
                switch (code)
                {
                    case ActionsHelperEventArgs.AuthLoginError:
                        _errorText.Text = GetString(Resource.String.ActivityAuth_badLogin);
                        _errorText.Visibility = ViewStates.Visible;
                        return;
                    case ActionsHelperEventArgs.AuthPasswdError:
                        _errorText.Text = GetString(Resource.String.ActivityAuth_incorrectError);
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


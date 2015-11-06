using System;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Helpers;
using com.FreedomVoice.MobileApp.Android.Utils;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Set phone number activity w/o back button
    /// </summary>
    [Activity(
        Label = "@string/ActivityNumber_title",
        ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.AdjustResize,
        Theme = "@style/AppThemeActionBar",
        NoHistory = true)]
    public class SetNumberActivity : LogoutActivity
    {
        private Color _errorColor;
        private Button _applyButton;
        protected Button SkipButton;
        private EditText _phoneText;
        private TextView _phoneErrorText;
        private ProgressBar _applyProgressBar;
        private ProgressBar _skipProgressBar;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_phone);
            RootLayout = FindViewById(Resource.Id.phoneActivity_root);
            _phoneText = FindViewById<EditText>(Resource.Id.phoneActivity_phoneField);
            _applyProgressBar = FindViewById<ProgressBar>(Resource.Id.phoneActivity_applyProgress);
            _skipProgressBar = FindViewById<ProgressBar>(Resource.Id.phoneActivity_skipProgress);
            _applyButton = FindViewById<Button>(Resource.Id.phoneActivity_applyButton);
            _applyButton.Click += ApplyButtonOnClick;
            SkipButton = FindViewById<Button>(Resource.Id.phoneActivity_skipButton);
            _phoneErrorText = FindViewById<TextView>(Resource.Id.phoneActivity_resultText);
            SkipButton.Click += SkipButtonOnClick;
            SupportActionBar.SetTitle(Resource.String.ActivityNumber_title);

            _errorColor = new Color(ContextCompat.GetColor(this, Resource.Color.textColorError));
            var applyBarColor = new Color(ContextCompat.GetColor(this, Resource.Color.colorProgressWhite));
            var skipBarColor = new Color(ContextCompat.GetColor(this, Resource.Color.colorProgressBlue));
            _applyProgressBar.IndeterminateDrawable?.SetColorFilter(applyBarColor, PorterDuff.Mode.SrcIn);
            _applyProgressBar.ProgressDrawable?.SetColorFilter(applyBarColor, PorterDuff.Mode.SrcIn);
            _skipProgressBar.IndeterminateDrawable?.SetColorFilter(skipBarColor, PorterDuff.Mode.SrcIn);
            _skipProgressBar.ProgressDrawable?.SetColorFilter(skipBarColor, PorterDuff.Mode.SrcIn);
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (RootLayout == null) return;
            var imm = GetSystemService(InputMethodService).JavaCast<InputMethodManager>();
            imm.HideSoftInputFromWindow(RootLayout.WindowToken, 0);
        }

        protected void SkipButtonOnClick(object sender, EventArgs eventArgs)
        {
            
            if (_skipProgressBar.Visibility == ViewStates.Invisible)
                _skipProgressBar.Visibility = ViewStates.Visible;
            if (SkipButton.Text.Length != 0)
                SkipButton.Text = "";
            Helper.GetAccounts();
        }

        protected void ApplyButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (!SetupNumber()) return;
            if (_applyProgressBar.Visibility == ViewStates.Invisible)
                _applyProgressBar.Visibility = ViewStates.Visible;
            if (_applyButton.Text.Length != 0)
                _applyButton.Text = "";
            Helper.GetAccounts();
        }

        /// <summary>
        /// Checking and set up phone number
        /// </summary>
        /// <returns></returns>
        protected bool SetupNumber()
        {
#if DEBUG
            if ((_phoneText.Text.Length >9)&&(_phoneText.Text.Length<13))
#else
            int phoneInt;
            if ((_phoneText.Text.Length == 10)&&(int.TryParse(_phoneText.Text, out phoneInt)))
#endif
            {
                if (DataValidationUtils.IsPhoneValid(_phoneText.Text) != "")
                {
                    Helper.SaveNewNumber(_phoneText.Text);
                    _phoneText.Background.ClearColorFilter();
                    _phoneErrorText.Visibility = ViewStates.Invisible;
                    return true;
                }
            }
            _phoneText.Background.SetColorFilter(_errorColor, PorterDuff.Mode.SrcAtop);
            _phoneErrorText.Visibility = ViewStates.Visible;
            return false;
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

        protected override void OnResume()
        {
            base.OnResume();
            if (string.IsNullOrEmpty(Helper.PhoneNumber)) return;
            _phoneText.Text = Helper.PhoneNumber;
            _phoneText.SetSelection(_phoneText.Text.Length);
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            base.OnHelperEvent(args);
            if (_applyProgressBar.Visibility == ViewStates.Visible)
                _applyProgressBar.Visibility = ViewStates.Invisible;
            if (_skipProgressBar.Visibility == ViewStates.Visible)
                _skipProgressBar.Visibility = ViewStates.Invisible;
            if (_applyButton.Text.Length == 0)
                _applyButton.Text = GetString(Resource.String.ActivityNumber_applyButton);
            if ((SkipButton.Text.Length == 0)&&(SkipButton.Visibility == ViewStates.Visible))
                _applyButton.Text = GetString(Resource.String.ActivityNumber_cancelButton);
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }

        protected void BaseBackPressed()
        {
            base.OnBackPressed();
        }
    }
}
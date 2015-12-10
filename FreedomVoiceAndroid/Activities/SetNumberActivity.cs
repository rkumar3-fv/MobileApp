using System;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
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
        Theme = "@style/AppThemeActionBar")]
    public class SetNumberActivity : LogoutActivity
    {
        private Color _errorColor;
        private CardView _applyButton;
        private TextView _applyLabel;
        protected CardView SkipButton;
        private TextView _skipLabel;
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
            _phoneText.FocusChange += PhoneTextOnFocusChange;
            _applyProgressBar = FindViewById<ProgressBar>(Resource.Id.phoneActivity_applyProgress);
            _applyLabel = FindViewById<TextView>(Resource.Id.phoneActivity_applyLabel);
            _applyButton = FindViewById<CardView>(Resource.Id.phoneActivity_applyButton);
            _applyButton.Click += ApplyButtonOnClick;
            SkipButton = FindViewById<CardView>(Resource.Id.phoneActivity_skipButton);
            _skipLabel = FindViewById<TextView>(Resource.Id.phoneActivity_skipLabel);
            _skipProgressBar = FindViewById<ProgressBar>(Resource.Id.phoneActivity_skipProgress);
            _phoneErrorText = FindViewById<TextView>(Resource.Id.phoneActivity_resultText);
            SkipButton.Click += SkipButtonOnClick;
            SupportActionBar.SetTitle(Resource.String.ActivityNumber_title);
            _errorColor = new Color(ContextCompat.GetColor(this, Resource.Color.textColorErrorDarkLine));
            var applyBarColor = new Color(ContextCompat.GetColor(this, Resource.Color.colorProgressWhite));
            var skipBarColor = new Color(ContextCompat.GetColor(this, Resource.Color.colorProgressBlue));
            _applyProgressBar.IndeterminateDrawable?.SetColorFilter(applyBarColor, PorterDuff.Mode.SrcIn);
            _applyProgressBar.ProgressDrawable?.SetColorFilter(applyBarColor, PorterDuff.Mode.SrcIn);
            _skipProgressBar.IndeterminateDrawable?.SetColorFilter(skipBarColor, PorterDuff.Mode.SrcIn);
            _skipProgressBar.ProgressDrawable?.SetColorFilter(skipBarColor, PorterDuff.Mode.SrcIn);
        }

        private void PhoneTextOnFocusChange(object sender, View.FocusChangeEventArgs focusChangeEventArgs)
        {
            if (!focusChangeEventArgs.HasFocus) return;
            _phoneText.Background.ClearColorFilter();
            _phoneErrorText.Visibility = ViewStates.Invisible;
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
            if (SkipButton.Visibility != ViewStates.Visible) return;
            _phoneText.Background.ClearColorFilter();
            _phoneErrorText.Visibility = ViewStates.Invisible;
            if (_skipProgressBar.Visibility == ViewStates.Invisible)
                _skipProgressBar.Visibility = ViewStates.Visible;
            if (_skipLabel.Visibility == ViewStates.Visible)
                _skipLabel.Visibility = ViewStates.Invisible;
            if (_applyButton.Enabled)
                _applyButton.Enabled = false;
            Helper.GetAccounts();
        }

        protected virtual void ApplyButtonOnClick(object sender, EventArgs eventArgs)
        {
            _phoneText.Background.ClearColorFilter();
            _phoneErrorText.Visibility = ViewStates.Invisible;
            if (!SetupNumber()) return;
            if (_applyProgressBar.Visibility == ViewStates.Invisible)
                _applyProgressBar.Visibility = ViewStates.Visible;
            if (_applyLabel.Visibility == ViewStates.Visible)
                _applyLabel.Visibility = ViewStates.Invisible;
            if ((SkipButton.Visibility == ViewStates.Visible)&&(SkipButton.Enabled))
                SkipButton.Enabled = false;
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
            long phoneDigit;
            if ((_phoneText.Text.Length == 10) && (long.TryParse(_phoneText.Text, out phoneDigit)))
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
                    bool hasRecents;
                    if ((Helper.RecentsDictionary == null) || (Helper.RecentsDictionary.Count == 0))
                        hasRecents = false;
                    else
                        hasRecents = true;
                    var logoutDialog = new LogoutDialogFragment(hasRecents);
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
            var phone = Appl.ApplicationHelper.GetMyPhoneNumber();
            if (string.IsNullOrEmpty(phone)) return;
            _phoneText.Text = phone;
            _phoneText.SetSelection(_phoneText.Text.Length);
            if (!Appl.ApplicationHelper.CheckReadPhoneState())
            { 
                var snackPerm = Snackbar.Make(RootLayout, Resource.String.Snack_noStatePermission, Snackbar.LengthLong);
                snackPerm.SetAction(Resource.String.Snack_noPhonePermissionAction, OnSetStatePermission);
                snackPerm.SetActionTextColor(ContextCompat.GetColor(this, Resource.Color.colorUndoList));
                snackPerm.Show();
            }
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            base.OnHelperEvent(args);
            if (_applyProgressBar.Visibility == ViewStates.Visible)
                _applyProgressBar.Visibility = ViewStates.Invisible;
            if (_skipProgressBar.Visibility == ViewStates.Visible)
                _skipProgressBar.Visibility = ViewStates.Invisible;
            if (_applyLabel.Visibility == ViewStates.Invisible)
                _applyLabel.Visibility = ViewStates.Visible;
            if ((_skipLabel.Visibility == ViewStates.Invisible)&&(SkipButton.Visibility == ViewStates.Visible))
                _skipLabel.Visibility = ViewStates.Visible;
            if ((SkipButton.Visibility == ViewStates.Visible)&&(!SkipButton.Enabled))
                SkipButton.Enabled = true;
            if (!_applyButton.Enabled)
                _applyButton.Enabled = true;
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
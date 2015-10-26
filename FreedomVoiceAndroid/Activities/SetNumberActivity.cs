using System;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Helpers;

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
        private Button _applyButton;
        private Button _skipButton;
        private EditText _phoneText;
        private TextView _phoneErrorText;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_phone);
            _phoneText = FindViewById<EditText>(Resource.Id.phoneActivity_phoneField);
            _applyButton = FindViewById<Button>(Resource.Id.phoneActivity_applyButton);
            _applyButton.Click += ApplyButtonOnClick;
            _skipButton = FindViewById<Button>(Resource.Id.phoneActivity_skipButton);
            _phoneErrorText = FindViewById<TextView>(Resource.Id.phoneActivity_resultText);
            _skipButton.Click += SkipButtonOnClick;
            SupportActionBar.SetTitle(Resource.String.ActivityNumber_title);

            _errorColor = new Color(ContextCompat.GetColor(this, Resource.Color.textColorError));
        }

        protected void SkipButtonOnClick(object sender, EventArgs eventArgs)
        {
            Helper.GetAccounts();
        }

        protected void ApplyButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (SetupNumber())
                Helper.GetAccounts();
        }

        /// <summary>
        /// Checking and set up phone number
        /// </summary>
        /// <returns></returns>
        protected bool SetupNumber()
        {
            if (_phoneText.Text.Length == 10)
            {
                Helper.SaveNewNumber(_phoneText.Text);
                _phoneText.Background.ClearColorFilter();
                _phoneErrorText.Visibility = ViewStates.Invisible;
                return true;
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
            if (!string.IsNullOrEmpty(Helper.PhoneNumber))
            {
                _phoneText.Text = Helper.PhoneNumber;
                _phoneText.SetSelection(_phoneText.Text.Length);
            }
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }

        protected void BaseBackPressed()
        {
            base.OnBackPressed();
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {

        }
    }
}
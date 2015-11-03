using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Helpers;
using com.FreedomVoice.MobileApp.Android.Utils;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Password restoration activity
    /// </summary>
    [Activity(
        Label = "@string/ActivityRestore_title",
        ScreenOrientation = ScreenOrientation.Portrait,
        Theme = "@style/AppThemeActionBar")]
    public class RestoreActivity : BaseActivity
    {
        public const string EmailField = "EMailField";
        private Color _errorColor;
        private EditText _emailText;
        private Button _restoreButton;
        private TextView _resultLabel;
        private ProgressBar _progressSend;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var eMail = Intent.GetStringExtra(EmailField) ?? "";
            SetContentView(Resource.Layout.act_restore);
            RootLayout = FindViewById(Resource.Id.restoreActivity_root);
            _emailText = FindViewById<EditText>(Resource.Id.restoreActivity_emailField);
            _emailText.Text = eMail;
            _restoreButton = FindViewById<Button>(Resource.Id.restoreActivity_sendButton);
            _resultLabel = FindViewById<TextView>(Resource.Id.restoreActivity_resultText);
            _progressSend = FindViewById<ProgressBar>(Resource.Id.restoreActivity_progress);
            _restoreButton.Click += RestoreButtonOnClick;

            SupportActionBar.SetTitle(Resource.String.ActivityRestore_title);
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_action_back);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            _errorColor = new Color(ContextCompat.GetColor(this, Resource.Color.textColorError));
        }

        /// <summary>
        /// Restore button click action
        /// </summary>
        private void RestoreButtonOnClick(object sender, EventArgs e)
        {
            if (_emailText.Length() > 5)
                if (DataValidationUtils.IsEmailValid(_emailText.Text))
                {
                    _emailText.Background.ClearColorFilter();
                    if (_progressSend.Visibility == ViewStates.Invisible)
                        _progressSend.Visibility = ViewStates.Visible;
                    if (_restoreButton.Text.Length > 0)
                        _restoreButton.Text = "";
                    if (_resultLabel.Text.Length > 0)
                        _resultLabel.Text = "";
                    Helper.RestorePassword(_emailText.Text);
                    return;
                }
            _resultLabel.Text = GetString(Resource.String.ActivityRestore_badEmail);
            _emailText.Background.SetColorFilter(_errorColor, PorterDuff.Mode.SrcAtop);
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
            base.OnHelperEvent(args);
            foreach (var code in args.Codes)
            {
                if (_progressSend.Visibility == ViewStates.Visible)
                    _progressSend.Visibility = ViewStates.Invisible;
                if (_restoreButton.Text.Length == 0)
                    _restoreButton.Text = GetString(Resource.String.ActivityRestore_sendButton);
                switch (code)
                {
                    case ActionsHelperEventArgs.RestoreError:
                        _resultLabel.Text = GetString(Resource.String.ActivityRestore_badEmail);
                        return;
                    case ActionsHelperEventArgs.RestoreWrongEmail:
                        _resultLabel.Text = GetString(Resource.String.ActivityRestore_unregistered);
                        return;
                    case ActionsHelperEventArgs.RestoreOk:
                        RestorationSuccessfull();
                        return;
                }
            }
        }

        /// <summary>
        /// Good restoration result action
        /// </summary>
        private void RestorationSuccessfull()
        {
            var logoutDialog = new RestoreDialogFragment();
            logoutDialog.DialogEvent += OnDialogEvent;
            logoutDialog.Show(SupportFragmentManager, GetString(Resource.String.DlgLogout_title));
        }

        /// <summary>
        /// Dialog actions handler
        /// </summary>
        private void OnDialogEvent(object sender, DialogEventArgs args)
        {
            switch (sender.GetType().Name)
            {
                case "RestoreDialogFragment":
                    if (args.Result == DialogResult.Ok)
                    {
                        var intent = new Intent(this, typeof (AuthActivity));
                        StartActivity(intent);
                    }
                    break;
            }
        }
    }
}
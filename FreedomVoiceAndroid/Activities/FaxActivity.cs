using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
#if DEBUG
using Android.Util;
#endif
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;
using Java.Lang;
using Uri = Android.Net.Uri;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Message with text content details
    /// </summary>
    [Activity(
        Label = "@string/ActivityFax_title",
        Theme = "@style/AppThemeActionBar",
        LaunchMode = LaunchMode.SingleTask, 
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class FaxActivity : MessageDetailsActivity
    {
        private Button _openFaxButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.act_fax);
            RootLayout = FindViewById(Resource.Id.faxActivity_root);
            LogoView = FindViewById<ImageView>(Resource.Id.faxActivity_logo);
            SenderText = FindViewById<TextView>(Resource.Id.faxActivity_senderText);
            MessageDate = FindViewById<TextView>(Resource.Id.faxActivity_dateText);
            MessageStamp = FindViewById<TextView>(Resource.Id.faxActivity_stampText);
            Progress = FindViewById<ProgressBar>(Resource.Id.faxActivity_progress);
            _openFaxButton = FindViewById<Button>(Resource.Id.faxActivity_openFax);
            _openFaxButton.Click += OpenFaxButtonOnClick;
            SupportActionBar.SetTitle(Resource.String.ActivityFax_title);
        }

        protected override void OnStart()
        {
            base.OnStart();
            LogoView?.SetImageResource(Resource.Drawable.logo_fax);
            if (Msg.Length < 1)
                _openFaxButton.Activated = false;
        }

        /// <summary>
        /// Open fax action
        /// </summary>
        private void OpenFaxButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (Appl.ApplicationHelper.CheckFilesPermissions())
                AttachmentId = Appl.ApplicationHelper.AttachmentsHelper.LoadAttachment(Msg);
            else
            {
                try
                {
                    var snackPerm = Snackbar.Make(RootLayout, Resource.String.Snack_noStoragePermission, Snackbar.LengthLong);
                    snackPerm.SetAction(Resource.String.Snack_noPhonePermissionAction, OnSetStoragePermission);
                    snackPerm.SetActionTextColor(ContextCompat.GetColor(this, Resource.Color.colorUndoList));
                    snackPerm.Show();
                }
                catch (RuntimeException)
                {
#if DEBUG
                    Log.Debug(App.AppPackage, "SNACKBAR creation failed. Please, REBUILD APP.");
#else
                    Appl.ApplicationHelper.Reports?.Log("SNACKBAR creation failed. Please, REBUILD APP.");
#endif
                    Toast.MakeText(this, Resource.String.Snack_noStoragePermission, ToastLength.Short).Show();
                }
            }
        }

        protected override void AttachmentsHelperOnFinishLoading(object sender, AttachmentHelperEventArgs<string> args)
        {
            if (args.Id != Msg.Id) return;
            base.AttachmentsHelperOnFinishLoading(sender, args);
            if (!_openFaxButton.Activated)
                _openFaxButton.Activated = true;
            var intent = new Intent(Intent.ActionView);
            var file = new Java.IO.File(args.Result);
            file.SetReadable(true);
            intent.SetDataAndType(Uri.FromFile(file), "application/pdf");
            intent.SetFlags(ActivityFlags.NoHistory);
            JavaSystem.Gc();
            try
            {
                StartActivity(intent);
            }
            catch (ActivityNotFoundException)
            {
                try
                {
                    var snakPdf = Snackbar.Make(RootLayout, Resource.String.Snack_pdfError, Snackbar.LengthLong);
                    snakPdf.SetAction(Resource.String.Snack_pdfGet, OnGetReaderClick);
                    snakPdf.SetActionTextColor(ContextCompat.GetColor(this, Resource.Color.colorUndoList));
                    snakPdf.Show();
                }
                catch (RuntimeException)
                {
#if DEBUG
                    Log.Debug(App.AppPackage, "SNACKBAR creation failed. Please, REBUILD APP.");
#else
                    Appl.ApplicationHelper.Reports?.Log("SNACKBAR creation failed. Please, REBUILD APP.");
#endif
                    Toast.MakeText(this, Resource.String.Snack_pdfError, ToastLength.Short).Show();
                }
            }
        }

        private void OnGetReaderClick(View view)
        {
            try
            {
                StartActivity(new Intent(Intent.ActionView, Uri.Parse("market://details?id=" + GetString(Resource.String.Extra_pdfReaderPath))));
            }
            catch (ActivityNotFoundException)
            {
                try
                {
                    Snackbar.Make(RootLayout, Resource.String.Snack_noPlayMarket, Snackbar.LengthLong).Show();
                }
                catch (RuntimeException)
                {
#if DEBUG
                    Log.Debug(App.AppPackage, "SNACKBAR creation failed. Please, REBUILD APP.");
#else
                    Appl.ApplicationHelper.Reports?.Log("SNACKBAR creation failed. Please, REBUILD APP.");
#endif
                    Toast.MakeText(this, Resource.String.Snack_pdfError, ToastLength.Short).Show();
                }
                
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            _openFaxButton.Activated = Msg.Length != 0;
            MessageStamp.Text = Msg.Length == 1 ? GetString(Resource.String.FragmentMessages_onePage) : $"{Msg.Length} {GetString(Resource.String.FragmentMessages_morePage)}";
        }

        protected override void AttachmentsHelperOnProgressLoading(object sender, AttachmentHelperEventArgs<int> args)
        {
            if (Msg.Id != args.Id) return;
            base.AttachmentsHelperOnProgressLoading(sender, args);
            if (_openFaxButton.Activated)
                _openFaxButton.Activated = false;
        }

        protected override void AttachmentsHelperOnFailLoadingEvent(object sender, AttachmentHelperEventArgs<bool> args)
        {
            if (Msg.Id != args.Id) return;
            base.AttachmentsHelperOnFailLoadingEvent(sender, args);
            if (!_openFaxButton.Activated)
                _openFaxButton.Activated = true;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case StoragePermissionRequestId:
                    if (grantResults[0] == Permission.Granted)
                        AttachmentId = Appl.ApplicationHelper.AttachmentsHelper.LoadAttachment(Msg);
                    break;
                default:
                    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                    break;
            }
        }
    }
}
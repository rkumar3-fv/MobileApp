using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
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
            RemoveButton = FindViewById<ImageButton>(Resource.Id.faxActivity_deleteButton);
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
            if (Msg.Length < 1)
                _openFaxButton.Activated = false;
        }

        /// <summary>
        /// Open fax action
        /// </summary>
        private void OpenFaxButtonOnClick(object sender, EventArgs eventArgs)
        {
            AttachmentId = Appl.ApplicationHelper.AttachmentsHelper.LoadAttachment(Msg);
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
                var snakPdf = Snackbar.Make(RootLayout, Resource.String.Snack_pdfError, Snackbar.LengthLong);
                snakPdf.SetAction(Resource.String.Snack_pdfGet, OnUndoClick);
                snakPdf.SetActionTextColor(ContextCompat.GetColor(this, Resource.Color.colorUndoList));
                snakPdf.Show();
            }
        }

        private void OnUndoClick(View view)
        {
            try
            {
                StartActivity(new Intent(Intent.ActionView, Uri.Parse("market://details?id=" + GetString(Resource.String.Extra_pdfReaderPath))));
            }
            catch (ActivityNotFoundException)
            {
                Snackbar.Make(RootLayout, Resource.String.Snack_noPlayMarket, Snackbar.LengthLong).Show();
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
    }
}
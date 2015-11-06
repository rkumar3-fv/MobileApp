using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;
using Uri = Android.Net.Uri;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Message with text content details
    /// </summary>
    [Activity(
        Label = "@string/ActivityFax_title",
        Theme = "@style/AppThemeActionBar",
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
            AttachmentId = AppHelper.Instance(this).AttachmentsHelper.LoadFaxAttachment(Msg);
        }

        private void AttachmentsHelperOnFinishLoading(object sender, AttachmentHelperEventArgs<string> args)
        {
            if (args.Id == Msg.Id)
            {
                var intent = new Intent(Intent.ActionView);
                var file = new Java.IO.File(args.Result);
                file.SetReadable(true);
                intent.SetDataAndType(Uri.FromFile(file), "application/pdf");
                intent.SetFlags(ActivityFlags.NoHistory);
                try
                {
                    StartActivityForResult(intent, 1);
                }
                catch (ActivityNotFoundException)
                {
                    try
                    {
                        StartActivity(new Intent(Intent.ActionView, Uri.Parse("market://details?id=" + GetString(Resource.String.Extra_pdfReaderPath))));
                    }
                    catch (ActivityNotFoundException)
                    {
                        StartActivity(new Intent(Intent.ActionView, Uri.Parse("http://play.google.com/store/apps/details?id=" + GetString(Resource.String.Extra_pdfReaderPath))));
                        throw;
                    }
                    throw;
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            _openFaxButton.Activated = Msg.Length != 0;
            MessageStamp.Text = Msg.Length == 1 ? GetString(Resource.String.FragmentMessages_onePage) : $"{Msg.Length} {GetString(Resource.String.FragmentMessages_morePage)}";
            AppHelper.Instance(this).AttachmentsHelper.OnFinish += AttachmentsHelperOnFinishLoading;
        }

        protected override void OnPause()
        {
            base.OnPause();
            AppHelper.Instance(this).AttachmentsHelper.OnFinish -= AttachmentsHelperOnFinishLoading;
        }
    }
}
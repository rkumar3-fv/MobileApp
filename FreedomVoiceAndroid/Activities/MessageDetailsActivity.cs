using System;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Internal.View;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;
using com.FreedomVoice.MobileApp.Android.Utils;
using FreedomVoice.Core.Utils;
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Base message details activity
    /// </summary>
    public abstract class MessageDetailsActivity : OperationActivity
    {
        public const string MessageExtraTag = "MessageExtra";
        private ContactsHelper _contactsHelper;
        protected Message Msg;
        protected TextView SenderText;
        protected TextView MessageDate;
        protected TextView MessageStamp;
        protected ImageButton RemoveButton;
        protected ProgressBar Progress;
        protected long AttachmentId;
        private Color _lightProgress;
        private Color _darkProgress;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _contactsHelper = ContactsHelper.Instance(this);
            var extra = (Message)Intent.GetParcelableExtra(MessageExtraTag);
            if (extra != null)
                Msg = extra;
            else
                Msg = Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList[Helper.SelectedMessage];
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_action_back);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            _lightProgress = new Color(ContextCompat.GetColor(this, Resource.Color.colorProgressBackground));
            _darkProgress = new Color(ContextCompat.GetColor(this, Resource.Color.colorProgressOrange));
        }

        protected override void OnStart()
        {
            base.OnStart();
            RemoveButton.Click += RemoveButtonOnClick;
            
            Progress.IndeterminateDrawable?.SetColorFilter(_darkProgress, PorterDuff.Mode.SrcIn);
            Progress.ProgressDrawable?.SetColorFilter(_lightProgress, PorterDuff.Mode.SrcIn);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if ((Helper.SelectedExtension != -1)&&(Helper.SelectedFolder != -1)&&(Helper.SelectedMessage != -1))
            {
                if (
                    Msg.Equals(
                        Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList
                            .Count <= Helper.SelectedMessage))
                    OnBackPressed();
                if (
                    !Msg.Equals(
                        Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList[
                            Helper.SelectedMessage]))
                    OnBackPressed();
            }
            else
            {
                //TODO: change after message refactoring
                RemoveButton.Visibility = ViewStates.Invisible;
            }
            string text;
            _contactsHelper.GetName(Msg.FromNumber, out text);
            SenderText.Text = Msg.FromName.Length > 1 ? Msg.FromName : text;
            MessageDate.Text = DataFormatUtils.ToFormattedDate(GetString(Resource.String.Timestamp_yesterday), Msg.MessageDate);
            AppHelper.Instance(this).AttachmentsHelper.OnFinish += AttachmentsHelperOnFinishLoading;
            AppHelper.Instance(this).AttachmentsHelper.OnProgressLoading += AttachmentsHelperOnProgressLoading;
            AppHelper.Instance(this).AttachmentsHelper.FailLoadingEvent += AttachmentsHelperOnFailLoadingEvent;
        }

        protected override void OnPause()
        {
            base.OnPause();
            AppHelper.Instance(this).AttachmentsHelper.OnFinish -= AttachmentsHelperOnFinishLoading;
            AppHelper.Instance(this).AttachmentsHelper.OnProgressLoading -= AttachmentsHelperOnProgressLoading;
            AppHelper.Instance(this).AttachmentsHelper.FailLoadingEvent -= AttachmentsHelperOnFailLoadingEvent;
        }

        protected virtual void AttachmentsHelperOnFinishLoading(object sender, AttachmentHelperEventArgs<string> args)
        {
            if (Progress.Visibility == ViewStates.Visible)
                Progress.Visibility = ViewStates.Invisible;
        }

        protected virtual void AttachmentsHelperOnProgressLoading(object sender, AttachmentHelperEventArgs<int> args)
        {
            if (Progress.Visibility == ViewStates.Invisible)
                Progress.Visibility = ViewStates.Visible;
        }

        protected virtual void AttachmentsHelperOnFailLoadingEvent(object sender, AttachmentHelperEventArgs<bool> args)
        {
            if (Progress.Visibility == ViewStates.Visible)
                Progress.Visibility = ViewStates.Invisible;
        }

        /// <summary>
        /// Remove message request
        /// </summary>
        protected virtual void RemoveButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].FolderName ==
                    GetString(Resource.String.FragmentMessages_folderTrash))
                Helper.DeleteMessage(Helper.SelectedMessage);
            else
                Helper.RemoveMessage(Helper.SelectedMessage);
            Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].TotalMailsCount--;
            if (Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList[Helper.SelectedMessage].Unread)
            {
                Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MailsCount--;
                Helper.ExtensionsList[Helper.SelectedExtension].MailsCount--;
            }
            Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList.RemoveAt(Helper.SelectedMessage);
        }

        public override void OnBackPressed()
        {
            if (Helper.SelectedMessage != -1)
                Helper.GetPrevious();
            base.OnBackPressed();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = new SupportMenuInflater(this);
            inflater.Inflate(Resource.Menu.menu_content, menu);
            return true;
        }

        /// <summary>
        /// Remove message response
        /// </summary>
        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            base.OnHelperEvent(args);
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.MsgMessagesUpdated:
                        OnBackPressed();
                        break;
                }
            }
        }
    }
}
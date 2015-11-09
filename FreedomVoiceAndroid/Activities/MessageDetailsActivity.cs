using System;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.Internal.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.CustomControls.Callbacks;
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
        private int _remove;
        private Snackbar _snakForRemoving;
        private SnackbarCallback _snackCallback;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _contactsHelper = ContactsHelper.Instance(this);
            var extra = (Message)Intent.GetParcelableExtra(MessageExtraTag);
            Msg = extra ?? Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList[Helper.SelectedMessage];
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_action_back);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            _snackCallback = new SnackbarCallback();
            _snackCallback.SnackbarEvent += OnSnackbarDissmiss;
            _lightProgress = new Color(ContextCompat.GetColor(this, Resource.Color.colorProgressBackground));
            _darkProgress = new Color(ContextCompat.GetColor(this, Resource.Color.colorProgressOrange));
            _remove = -1;
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

        private void OnUndoClick(View view)
        {
            _remove = -1;
        }

        private void OnSnackbarDissmiss(object sender, EventArgs args)
        {
            RemoveAction();
        }

        private void RemoveAction()
        {
            if (_remove != -1)
            {
                if (Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].FolderName == GetString(Resource.String.FragmentMessages_folderTrash))
                    Helper.DeleteMessage(_remove);
                else
                    Helper.RemoveMessage(_remove);
                Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList.RemoveAt(_remove);
#if DEBUG
                Log.Debug(App.AppPackage, $"REMOVE message {Helper.SelectedMessage}");
#endif
            }
#if DEBUG
            else
                Log.Debug(App.AppPackage, $"UNDO removing message {Helper.SelectedMessage}");
#endif
            _remove = -1;
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
            if (!args.Result)
                Snackbar.Make(RootLayout, Resource.String.Snack_loadingError, Snackbar.LengthLong).Show();
        }

        /// <summary>
        /// Remove message request
        /// </summary>
        protected virtual void RemoveButtonOnClick(object sender, EventArgs eventArgs)
        {
            _snakForRemoving = Snackbar.Make(RootLayout, Resource.String.FragmentMessages_remove, Snackbar.LengthLong);
            _snakForRemoving.SetAction(Resource.String.FragmentMessages_removeUndo, OnUndoClick);
            _snakForRemoving.SetActionTextColor(ContextCompat.GetColor(this, Resource.Color.colorUndoList));
            _snakForRemoving.SetCallback(_snackCallback);
            _remove = Helper.SelectedMessage;
            _snakForRemoving.Show();
        }

        public override void OnBackPressed()
        {
            if ((_snakForRemoving != null) && (_snakForRemoving.IsShown))
            {
                RemoveAction();
                _snakForRemoving.Dismiss();
            }
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
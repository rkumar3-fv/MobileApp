using System;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.Internal.View;
#if DEBUG
using Android.Util;
#endif
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
        protected int MarkForRemove;
        private Snackbar _snakForRemoving;
        private SnackbarCallback _snackCallback;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _contactsHelper = ContactsHelper.Instance(this);
            var extra = (Message)Intent.GetParcelableExtra(MessageExtraTag);
            if (extra != null)
                Msg = extra;
            else if ((Helper.SelectedExtension != -1)&&(Helper.SelectedFolder != -1)&&(Helper.SelectedMessage != -1))
                Msg = Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList[Helper.SelectedMessage];
            else
                base.OnBackPressed();
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_action_back);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
            _snackCallback = new SnackbarCallback();
            _snackCallback.SnackbarEvent += OnSnackbarDissmiss;
            _lightProgress = new Color(ContextCompat.GetColor(this, Resource.Color.colorProgressBackground));
            _darkProgress = new Color(ContextCompat.GetColor(this, Resource.Color.colorProgressOrange));
            MarkForRemove = -1;
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
            RemoveButton.Visibility = ViewStates.Invisible;
            if ((Helper.SelectedExtension != -1)&&(Helper.SelectedFolder != -1)&&(Helper.SelectedMessage != -1))
            {
                if (!(Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList.Count > Helper.SelectedMessage))
                    OnBackPressed();
                if (!Msg.Equals(Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList[Helper.SelectedMessage]))
                    OnBackPressed();
            }

            string text;
            _contactsHelper.GetName(Msg.FromNumber, out text);
            SenderText.Text = Msg.FromName.Length > 1 ? Msg.FromName : text;
            MessageDate.Text = DataFormatUtils.ToFormattedDate(GetString(Resource.String.Timestamp_yesterday), Msg.MessageDate);
            Appl.ApplicationHelper.AttachmentsHelper.OnFinish += AttachmentsHelperOnFinishLoading;
            Appl.ApplicationHelper.AttachmentsHelper.OnProgressLoading += AttachmentsHelperOnProgressLoading;
            Appl.ApplicationHelper.AttachmentsHelper.FailLoadingEvent += AttachmentsHelperOnFailLoadingEvent;
        }

        protected override void OnPause()
        {
            base.OnPause();
            Appl.ApplicationHelper.AttachmentsHelper.OnFinish -= AttachmentsHelperOnFinishLoading;
            Appl.ApplicationHelper.AttachmentsHelper.OnProgressLoading -= AttachmentsHelperOnProgressLoading;
            Appl.ApplicationHelper.AttachmentsHelper.FailLoadingEvent -= AttachmentsHelperOnFailLoadingEvent;
            MarkForRemove = -1;
        }

        private void OnUndoClick(View view)
        {
            MarkForRemove = -1;
        }

        private void OnSnackbarDissmiss(object sender, EventArgs args)
        {
            RemoveAction();
        }

        private void RemoveAction()
        {
            if (Helper.SelectedMessage == -1) return;
            if (Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList == null) return;
            if (!(Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList.Count > Helper.SelectedMessage)) return;
            if (Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList[Helper.SelectedMessage] == null) return;
            if (!Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList[Helper.SelectedMessage].Equals(Msg)) return;
            if (Appl.ApplicationHelper.AttachmentsHelper.IsInProcess(Msg.Id)) return;
            if (MarkForRemove != -1)
            {
                if (Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].FolderName == GetString(Resource.String.FragmentMessages_folderTrash))
                    Helper.DeleteMessage(MarkForRemove);
                else
                    Helper.RemoveMessage(MarkForRemove);
                Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList.RemoveAt(MarkForRemove);
#if DEBUG
                Log.Debug(App.AppPackage, $"REMOVE message {Helper.SelectedMessage}");
#endif
            }
#if DEBUG
            else
                Log.Debug(App.AppPackage, $"UNDO removing message {Helper.SelectedMessage}");
#endif
            MarkForRemove = -1;
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
            RemoveSnack();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_action_remove:
                    RemoveSnack();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private void RemoveSnack()
        {
            _snakForRemoving = Snackbar.Make(RootLayout, Resource.String.FragmentMessages_remove, 10000);
            _snakForRemoving.SetAction(Resource.String.FragmentMessages_removeUndo, OnUndoClick);
            _snakForRemoving.SetActionTextColor(ContextCompat.GetColor(this, Resource.Color.colorUndoList));
            _snakForRemoving.SetCallback(_snackCallback);
            MarkForRemove = Helper.SelectedMessage;
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
            if ((Helper.SelectedExtension != -1) && (Helper.SelectedFolder != -1) && (Helper.SelectedMessage != -1))
                inflater.Inflate(Resource.Menu.menu_details, menu);
            else
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
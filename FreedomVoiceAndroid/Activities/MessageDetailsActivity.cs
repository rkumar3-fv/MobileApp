using System;
using Android.OS;
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
        private ContactsHelper _contactsHelper;
        protected Message Msg;
        protected TextView SenderText;
        protected TextView MessageDate;
        protected TextView MessageStamp;
        protected ImageButton RemoveButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _contactsHelper = ContactsHelper.Instance(this);
            Msg = Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList[Helper.SelectedMessage];
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_action_back);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }

        protected override void OnStart()
        {
            base.OnStart();
            RemoveButton.Click += RemoveButtonOnClick;
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (Msg.Equals(Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList.Count<=Helper.SelectedMessage))
                OnBackPressed();
            if (!Msg.Equals(Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList[Helper.SelectedMessage]))
                OnBackPressed();
            SenderText.Text = Msg.FromName.Length > 1 ? Msg.FromName : _contactsHelper.GetName(Msg.FromNumber);
            MessageDate.Text = DataFormatUtils.ToFormattedDate(GetString(Resource.String.Timestamp_yesterday), Msg.MessageDate);
        }

        /// <summary>
        /// Remove message request
        /// </summary>
        private void RemoveButtonOnClick(object sender, EventArgs eventArgs)
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
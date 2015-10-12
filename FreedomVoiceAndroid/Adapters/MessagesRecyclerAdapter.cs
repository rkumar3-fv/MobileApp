using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Entities;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    /// <summary>
    /// Messages recycler view adapter
    /// </summary>
    public class MessagesRecyclerAdapter : RecyclerView.Adapter
    {
        private const int CodeExtension = 0;
        private const int CodeFolder = 1;
        private const int CodeMessage = 2;

        private readonly Context _context;
        private List<MessageItem> _currentContent;

        public MessagesRecyclerAdapter(Context context) : this (new List<MessageItem>(), context)
        {}

        public MessagesRecyclerAdapter(List<MessageItem> currentContent, Context context)
        {
            _context = context;
            _currentContent = currentContent;
        }

        /// <summary>
        /// Messages content list
        /// </summary>
        public List<MessageItem> CurrentContent
        {
            get { return _currentContent; }
            set
            {
                _currentContent = value;
                NotifyDataSetChanged();
            }
        }

        /// <summary>
        /// Get messages item by position
        /// </summary>
        /// <param name="position">position number</param>
        /// <returns>extension entity</returns>
        public MessageItem GetContentItem(int position)
        {
            return (_currentContent.Count < position) ? null : _currentContent[position];
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var contentItem = _currentContent[position];
            if (contentItem is Extension)
            {
                var viewHolder = holder as ExtensionViewHolder;
                var extension = contentItem as Extension;
                if (viewHolder == null) return;
                if (extension.ExtensionName.Length > 0)
                    viewHolder.ExtensionName.Text = $"{extension.Id} - {extension.ExtensionName}";
                if (extension.MailsCount == 0) return;
                viewHolder.ExtensionInfo.Text = extension.MailsCount.ToString();
                viewHolder.InfoLayout.Visibility = ViewStates.Visible;
            }
            else if (contentItem is Folder)
            {
                var viewHolder = holder as FoldersViewHolder;
                var folder = contentItem as Folder;
                if (viewHolder == null) return;
                viewHolder.FoldersName.Text = folder.FolderName;
                if (folder.MailsCount != 0)
                    viewHolder.FoldersInfo.Text = folder.MailsCount.ToString();
                //TODO: icons
            }
            else
            {
                var viewHolder = holder as MessagesViewHolder;
                var message = contentItem as Message;
                if ((viewHolder == null)||(message == null)) return;
                viewHolder.MessageDate.Text = message.MessageDate;
                viewHolder.MessageStamp.Text = message.Length.ToString();
                viewHolder.MessageFrom.Text = (message.FromName.Length > 0) ? message.FromName : message.FromNumber;
                //TODO:icons
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch (viewType)
            {
                case CodeExtension:
                    return new ExtensionViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_extension, parent, false));
                case CodeFolder:
                    return new FoldersViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_folder, parent, false));
                default:
                    return new MessagesViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_message, parent, false));
            }
        }

        public override int GetItemViewType(int position)
        {
            if (_currentContent[position] is Extension)
                return CodeExtension;
            if (_currentContent[position] is Folder)
                return CodeFolder;
            return CodeMessage;
        }

        /// <summary>
        /// Content list length
        /// </summary>
        public override int ItemCount => _currentContent?.Count ?? 0;

        /// <summary>
        /// Extension selection viewholder
        /// </summary>
        private class ExtensionViewHolder : RecyclerView.ViewHolder
        {
            public LinearLayout InfoLayout { get; }

            /// <summary>
            /// Extension name field
            /// </summary>
            public TextView ExtensionName { get; }

            /// <summary>
            /// Extension information field
            /// </summary>
            public TextView ExtensionInfo { get; }

            public ExtensionViewHolder(View itemView) : base(itemView)
            {
                ExtensionName = itemView.FindViewById<TextView>(Resource.Id.itemExt_title);
                ExtensionInfo = itemView.FindViewById<TextView>(Resource.Id.itemExt_info);
                InfoLayout = itemView.FindViewById<LinearLayout>(Resource.Id.itemExt_back);
            }
        }

        /// <summary>
        /// Folder selection viewholder
        /// </summary>
        private class FoldersViewHolder : RecyclerView.ViewHolder
        {
            /// <summary>
            /// Folder icon
            /// </summary>
            public ImageView FoldersIcon { get; }

            /// <summary>
            /// Folder name field
            /// </summary>
            public TextView FoldersName { get; }

            /// <summary>
            /// Folder information field
            /// </summary>
            public TextView FoldersInfo { get; }

            public FoldersViewHolder(View itemView) : base(itemView)
            {
                FoldersIcon = itemView.FindViewById<ImageView>(Resource.Id.itemFolder_icon);
                FoldersName = itemView.FindViewById<TextView>(Resource.Id.itemFolder_title);
                FoldersInfo = itemView.FindViewById<TextView>(Resource.Id.itemFolder_info);
            }
        }

        /// <summary>
        /// Message viewholder
        /// </summary>
        private class MessagesViewHolder : RecyclerView.ViewHolder
        {
            /// <summary>
            /// Message icon
            /// </summary>
            public ImageView MessageIcon { get; }

            /// <summary>
            /// Message stamp
            /// </summary>
            public TextView MessageStamp { get; }

            /// <summary>
            /// Message recepient field
            /// </summary>
            public TextView MessageFrom { get; }

            /// <summary>
            /// Message date field
            /// </summary>
            public TextView MessageDate { get; }

            public MessagesViewHolder(View itemView) : base(itemView)
            {
                MessageIcon = itemView.FindViewById<ImageView>(Resource.Id.itemMessage_messageIcon);
                MessageStamp = itemView.FindViewById<TextView>(Resource.Id.itemMessage_messageStamp);
                MessageDate = itemView.FindViewById<TextView>(Resource.Id.itemMessage_messageDate);
                MessageFrom = itemView.FindViewById<TextView>(Resource.Id.itemMessage_messageFrom);
            }
        }
    }
}
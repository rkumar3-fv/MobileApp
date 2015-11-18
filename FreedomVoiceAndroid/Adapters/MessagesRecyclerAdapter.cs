using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Entities;
using com.FreedomVoice.MobileApp.Android.Utils;
using FreedomVoice.Core.Utils;

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

        /// <summary>
        /// Item short click event
        /// </summary>
        public event EventHandler<int> ItemClick;

        private void OnClick(int position)
        {
            if ((_currentContent != null)&&(position<_currentContent.Count))
                ItemClick?.Invoke(this, position);
        }

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
                if (_currentContent.Equals(value)) return;
                _currentContent = value;
                NotifyDataSetChanged();
            }
        }

        /// <summary>
        /// Clean view content
        /// </summary>
        public void Clean()
        {
            _currentContent.Clear();
            NotifyDataSetChanged();
        }

        /// <summary>
        /// Remove item
        /// </summary>
        /// <param name="index">item index</param>
        public void RemoveItem(int index)
        {
            _currentContent.RemoveAt(index);
            NotifyItemRemoved(index);
        }

        /// <summary>
        /// Insert item
        /// </summary>
        /// <param name="item">inserted item</param>
        /// <param name="index">item index</param>
        public void InsertItem(MessageItem item, int index)
        {
            _currentContent.Insert(index, item);
            NotifyItemInserted(index);
        }

        /// <summary>
        /// Get messages item by position
        /// </summary>
        /// <param name="position">position number</param>
        /// <returns>message item entity</returns>
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
                if (extension.MailsCount > 0)
                {
                    viewHolder.ExtensionInfo.Text = extension.MailsCount > 99 ? "99+" : extension.MailsCount.ToString();
                    viewHolder.InfoLayout.Visibility = ViewStates.Visible;
                }
                else
                {
                    viewHolder.ExtensionInfo.Text = "";
                    viewHolder.InfoLayout.Visibility = ViewStates.Invisible;
                }
            }
            else if (contentItem is Folder)
            {
                var viewHolder = holder as FoldersViewHolder;
                var folder = contentItem as Folder;
                if (viewHolder == null) return;
                viewHolder.FoldersName.Text = folder.FolderName;
                
                    if (folder.MailsCount > 0)
                    {
                        viewHolder.FoldersInfo.Text = folder.MailsCount > 99 ? "99+" : folder.MailsCount.ToString();
                        viewHolder.InfoLayout.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        viewHolder.FoldersInfo.Text = "";
                        viewHolder.InfoLayout.Visibility = ViewStates.Invisible;
                    }

                if (folder.FolderName == _context.GetString(Resource.String.FragmentMessages_folderNew))
                    viewHolder.FoldersIcon.SetImageResource(Resource.Drawable.ic_new_folder);
                if (folder.FolderName == _context.GetString(Resource.String.FragmentMessages_folderSent))
                    viewHolder.FoldersIcon.SetImageResource(Resource.Drawable.ic_send_folder);
                else if (folder.FolderName == _context.GetString(Resource.String.FragmentMessages_folderTrash))
                    viewHolder.FoldersIcon.SetImageResource(Resource.Drawable.ic_trash_folder);
                else if (folder.FolderName == _context.GetString(Resource.String.FragmentMessages_folderSaved))
                    viewHolder.FoldersIcon.SetImageResource(Resource.Drawable.ic_save_folder);
                else
                    viewHolder.FoldersIcon.SetImageResource(Resource.Drawable.ic_other_folder);
            }
            else
            {
                var viewHolder = holder as MessagesViewHolder;
                var message = contentItem as Message;
                if ((viewHolder == null)||(message == null)) return;
                viewHolder.MessageDate.Text = DataFormatUtils.ToFormattedDate(_context.GetString(Resource.String.Timestamp_yesterday), message.MessageDate);
                if (message.FromName.Length > 1)
                    viewHolder.MessageFrom.Text = message.FromName;
                else
                {
                    string text;
                    ContactsHelper.Instance(_context).GetName(message.FromNumber, out text);
                    viewHolder.MessageFrom.Text = text;
                }
                switch (message.MessageType)
                {
                    case Message.TypeFax:
                        viewHolder.MessageIcon.SetImageResource(message.Unread
                            ? Resource.Drawable.ic_msg_fax_unread
                            : Resource.Drawable.ic_msg_fax);
                        viewHolder.MessageStamp.Text = message.Length == 1 ? 
                            _context.GetString(Resource.String.FragmentMessages_onePage) : $"{message.Length} {_context.GetString(Resource.String.FragmentMessages_morePage)}";
                        break;
                    case Message.TypeRec:
                        viewHolder.MessageIcon.SetImageResource(message.Unread
                            ? Resource.Drawable.ic_msg_callrec_unread
                            : Resource.Drawable.ic_msg_callrec);
                        viewHolder.MessageStamp.Text = DataFormatUtils.ToDuration(message.Length);
                        break;
                    case Message.TypeVoice:
                        viewHolder.MessageIcon.SetImageResource(message.Unread
                            ? Resource.Drawable.ic_msg_voicemail_unread
                            : Resource.Drawable.ic_msg_voicemail);
                        viewHolder.MessageStamp.Text = DataFormatUtils.ToDuration(message.Length);
                        break;
                }
                if (message.Unread)
                {
                    viewHolder.MessageFrom.SetTypeface(null, TypefaceStyle.Bold);
                    viewHolder.MessageDate.SetTypeface(null, TypefaceStyle.Bold);
                    viewHolder.MessageDate.SetTextColor(ContextCompat.GetColorStateList(_context, Resource.Color.textColorPrimary));
                }
                else
                {
                    viewHolder.MessageFrom.SetTypeface(null, TypefaceStyle.Normal);
                    viewHolder.MessageDate.SetTypeface(null, TypefaceStyle.Normal);
                    viewHolder.MessageDate.SetTextColor(ContextCompat.GetColorStateList(_context, Resource.Color.messageIndicatorText));
                }
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch (viewType)
            {
                case CodeExtension:
                    return new ExtensionViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_extension, parent, false), OnClick);
                case CodeFolder:
                    return new FoldersViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_folder, parent, false), OnClick);
                default:
                    return new MessagesViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_message, parent, false), OnClick);
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
            /// <summary>
            /// Colored layout for messages counter
            /// </summary>
            public LinearLayout InfoLayout { get; }

            /// <summary>
            /// Extension name field
            /// </summary>
            public TextView ExtensionName { get; }

            /// <summary>
            /// Extension information field
            /// </summary>
            public TextView ExtensionInfo { get; }

            public ExtensionViewHolder(View itemView, Action<int> listener) : base(itemView)
            {
                ExtensionName = itemView.FindViewById<TextView>(Resource.Id.itemExt_title);
                ExtensionInfo = itemView.FindViewById<TextView>(Resource.Id.itemExt_info);
                InfoLayout = itemView.FindViewById<LinearLayout>(Resource.Id.itemExt_back);
                itemView.Click += (sender, e) => { if (sender != null) listener(AdapterPosition); };
            }
        }

        /// <summary>
        /// Folder selection viewholder
        /// </summary>
        private class FoldersViewHolder : RecyclerView.ViewHolder
        {
            /// <summary>
            /// Colored layout for messages counter
            /// </summary>
            public LinearLayout InfoLayout { get; }

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

            public FoldersViewHolder(View itemView, Action<int> listener) : base(itemView)
            {
                FoldersIcon = itemView.FindViewById<ImageView>(Resource.Id.itemFolder_icon);
                FoldersName = itemView.FindViewById<TextView>(Resource.Id.itemFolder_title);
                FoldersInfo = itemView.FindViewById<TextView>(Resource.Id.itemFolder_info);
                InfoLayout = itemView.FindViewById<LinearLayout>(Resource.Id.itemFolder_back);
                itemView.Click += (sender, e) => { if (sender != null) listener(AdapterPosition); };
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

            public MessagesViewHolder(View itemView, Action<int> listener) : base(itemView)
            {
                MessageIcon = itemView.FindViewById<ImageView>(Resource.Id.itemMessage_messageIcon);
                MessageStamp = itemView.FindViewById<TextView>(Resource.Id.itemMessage_messageStamp);
                MessageDate = itemView.FindViewById<TextView>(Resource.Id.itemMessage_messageDate);
                MessageFrom = itemView.FindViewById<TextView>(Resource.Id.itemMessage_messageFrom);
                itemView.Click += (sender, e) => { if (sender != null) listener(AdapterPosition); };
            }
        }
    }
}
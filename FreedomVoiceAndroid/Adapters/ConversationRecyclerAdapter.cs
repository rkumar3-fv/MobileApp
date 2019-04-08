using System;
using System.Collections.Generic;
using System.Linq;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Entities;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.DAL.DbEntities;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    public class ConversationRecyclerAdapter : RecyclerView.Adapter
    {
        private List<Conversation> _items = new List<Conversation>();
        private readonly EventHandler<Conversation> _itemClickEventHandler;

        public ConversationRecyclerAdapter(EventHandler<Conversation> itemClickEventHandler)
        {
            this._itemClickEventHandler = itemClickEventHandler;
        }

        public override int ItemCount => _items.Count;

        public void Update(List<Conversation> items)
        {
            _items = items ?? new List<Conversation>();
            NotifyDataSetChanged();
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as AccountVh;
            var item = _items[position];

            vh.UserName.SetText(item.CollocutorPhone.PhoneNumber, TextView.BufferType.Normal);
            var message = item.Messages.FirstOrDefault();
            vh.LastMessage.SetText(message.Text, TextView.BufferType.Normal);
            vh.LastMessageDate.SetText(message.SentAt.ToString(), TextView.BufferType.Normal);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_conversation, parent, false);
            var vh = new AccountVh(view);
            vh.Container.Click += (sender, args) => _itemClickEventHandler(this, _items[vh.AdapterPosition]);
            return vh;
        }

        private class AccountVh : RecyclerView.ViewHolder
        {
            public ViewGroup Container;
            public TextView UserName;
            public TextView LastMessage;
            public TextView LastMessageDate;

            public AccountVh(View itemView) : base(itemView)
            {
                Container = itemView.FindViewById<ViewGroup>(Resource.Id.itemConversation_container);
                UserName = itemView.FindViewById<TextView>(Resource.Id.itemConversation_userName);
                LastMessage = itemView.FindViewById<TextView>(Resource.Id.itemConversation_lastMessage);
                LastMessageDate = itemView.FindViewById<TextView>(Resource.Id.itemConversation_lastMessageDate);
            }
        }
    }
}
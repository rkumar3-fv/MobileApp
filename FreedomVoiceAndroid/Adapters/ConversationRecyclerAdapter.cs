using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Entities;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    public class ConversationRecyclerAdapter : RecyclerView.Adapter
    {
        private List<Account> _accounts = new List<Account>();
        private readonly EventHandler<Account> _itemClickEventHandler;

        public ConversationRecyclerAdapter(EventHandler<Account> itemClickEventHandler)
        {
            this._itemClickEventHandler = itemClickEventHandler;
        }

        public void Update(List<Account> items)
        {
            _accounts = items ?? new List<Account>();
            NotifyDataSetChanged();
        }

        public override int ItemCount => _accounts.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as AccountVh;
            var item = _accounts[position];

            vh.UserName.SetText(item.AccountName, TextView.BufferType.Normal);
            vh.LastMessage.SetText(item.PresentationNumber, TextView.BufferType.Normal);
            vh.LastMessageDate.SetText(item.AccountName, TextView.BufferType.Normal);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_conversation, parent, false);
            var vh = new AccountVh(view);
            vh.Container.Click += (sender, args) => _itemClickEventHandler(this, _accounts[vh.AdapterPosition]);
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
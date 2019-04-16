using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
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
        private List<ConversationViewModel> _items = new List<ConversationViewModel>();
        private readonly EventHandler<ConversationViewModel> _itemClickEventHandler;

        public ConversationRecyclerAdapter(EventHandler<ConversationViewModel> itemClickEventHandler)
        {
            _itemClickEventHandler = itemClickEventHandler;
        }

        public override int ItemCount => _items.Count;

        public void Update(List<ConversationViewModel> items)
        {
            _items = items ?? new List<ConversationViewModel>();
            NotifyDataSetChanged();
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as AccountVh;
            var item = _items[position];

            vh.UserName.SetText(item.Collocutor, TextView.BufferType.Normal);
            vh.UserName.SetTypeface(Typeface.Default, item.IsNew ? TypefaceStyle.Bold : TypefaceStyle.Normal );
            vh.LastMessage.SetText(item.LastMessage, TextView.BufferType.Normal);
            vh.LastMessage.SetTypeface(Typeface.Default, item.IsNew ? TypefaceStyle.Bold : TypefaceStyle.Normal );
            vh.LastMessageDate.SetText(item.Date, TextView.BufferType.Normal);
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
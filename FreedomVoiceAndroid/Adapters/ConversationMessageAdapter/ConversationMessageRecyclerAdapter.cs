using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FreedomVoice.Entities;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    public class ConversationMessageRecyclerAdapter : RecyclerView.Adapter
    {
        private readonly int _myPhoneId;
        private List<ConversationMessageRecyclerItem> _items = new List<ConversationMessageRecyclerItem>();

        public ConversationMessageRecyclerAdapter(int myPhoneId)
        {
            _myPhoneId = myPhoneId;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            switch (holder)
            {
                case ConversationMessageVh messageVh:
                {
                    var item = _items[position] as ConversationMessageItem;
                    messageVh.Bind(item.Message, _myPhoneId);
                    break;
                }
                case ConversationMessageDateVh dateVh:
                {
                    var date = _items[position] as ConversationMessageDate;
                    dateVh.Bind(date.DateTime);
                    break;
                }
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(viewType, parent, false);

            switch (viewType)
            {
                case Resource.Layout.frag_conversation_message_item:
                    return new ConversationMessageVh(view);
                case Resource.Layout.frag_conversation_message_date:
                    return new ConversationMessageDateVh(view);
                default:
                    throw new Exception("unknown item");
            }
        }

        public override int GetItemViewType(int position)
        {
            switch (_items[position])
            {
                case ConversationMessageItem messageItem:
                    return Resource.Layout.frag_conversation_message_item;
                case ConversationMessageDate dateItem:
                    return Resource.Layout.frag_conversation_message_date;
                default:
                    throw new Exception("unknown item");
            }
        }

        public override int ItemCount => _items.Count;

        public void UpdateItems(List<Message> newItems)
        {
            _items = newItems
                .GroupBy(message => message.SentAt?.Date)
                .Aggregate(new List<ConversationMessageRecyclerItem>(),
                    (result, messages) =>
                    {
                        result.Add(new ConversationMessageDate(messages.Key));
                        result.AddRange(messages.Select(message => new ConversationMessageItem(message)));
                        return result;
                    })
                .ToList();
            NotifyDataSetChanged();
        }

        private class ConversationMessageVh : RecyclerView.ViewHolder
        {
            private readonly TextView _message;
            private readonly LinearLayout _container;

            public ConversationMessageVh(View itemView) : base(itemView)
            {
                _message = itemView.FindViewById<TextView>(Resource.Id.conversation_message_tv);
                _container = itemView.FindViewById<LinearLayout>(Resource.Id.conversation_message_fl);
            }

            public void Bind(Message message, int myPhoneId)
            {
                var lp = _message.LayoutParameters as FrameLayout.LayoutParams;

                var isMyMessage = message.From.Id == myPhoneId;
                if (isMyMessage)
                {
                    _message.SetBackgroundResource(Resource.Drawable.bg_message_cloud_right);
                    _container.SetGravity(GravityFlags.Right);
                    lp.Gravity = GravityFlags.Right;
                }
                else
                {
                    _message.SetBackgroundResource(Resource.Drawable.bg_message_cloud_left);
                    _container.SetGravity(GravityFlags.Left);
                    lp.Gravity = GravityFlags.Left;
                }

                _message.LayoutParameters = lp;

                _message.SetText(message.Text, TextView.BufferType.Normal);
            }
        }

        private class ConversationMessageDateVh : RecyclerView.ViewHolder
        {
            private readonly TextView _text;

            public ConversationMessageDateVh(View itemView) : base(itemView)
            {
                _text = itemView.FindViewById<TextView>(Resource.Id.frag_conversation_message_date_tv);
            }

            public void Bind(DateTime? dateTime)
            {
                _text.SetText(dateTime.ToString(), TextView.BufferType.Normal);
            }
        }
    }
}
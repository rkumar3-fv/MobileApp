using System;
using Android;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;


namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    class ConversationMessageDateVh : RecyclerView.ViewHolder
    {
        public readonly TextView DateTv;

        public ConversationMessageDateVh(View itemView) : base(itemView)
        {
            DateTv = itemView.FindViewById<TextView>(Resource.Id.frag_conversation_message_date_tv);
        }
    }

    public class ConversationMessageDate : ConversationMessageRecyclerItem
    {
        private readonly string _dateTimeText;

        public ConversationMessageDate(string dateTime)
        {
            _dateTimeText = dateTime;
        }

        public int getLayoutResId() => Resource.Layout.frag_conversation_message_date;

        public RecyclerView.ViewHolder createViewHolder(View view) => new ConversationMessageDateVh(view);

        public void Bind(RecyclerView.ViewHolder holder)
        {
            var vh = holder as ConversationMessageDateVh;

            vh.DateTv.SetText(_dateTimeText, TextView.BufferType.Normal);
        }
    }
}
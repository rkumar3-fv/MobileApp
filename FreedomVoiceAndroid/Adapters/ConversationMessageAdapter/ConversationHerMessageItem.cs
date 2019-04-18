using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FreedomVoice.Entities;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    internal class ConversationHerMessageVh : RecyclerView.ViewHolder
    {
        public readonly TextView MessageTv;
        public readonly LinearLayout ContainerLl;
        public readonly TextView Date;

        public ConversationHerMessageVh(View itemView) : base(itemView)
        {
            MessageTv = itemView.FindViewById<TextView>(Resource.Id.conversation_message_tv);
            ContainerLl = itemView.FindViewById<LinearLayout>(Resource.Id.conversation_message_fl);
            Date = itemView.FindViewById<TextView>(Resource.Id.item_message_date);
        }

    }

    public class ConversationHerMessageItem : ConversationMessageRecyclerItem
    {
        private readonly Message _message;

        public ConversationHerMessageItem(Message message)
        {
            _message = message;
        }

        public int getLayoutResId() => Resource.Layout.item_her_message;
        public RecyclerView.ViewHolder createViewHolder(View view) => new ConversationHerMessageVh(view);

        public void Bind(RecyclerView.ViewHolder holder)
        {
            var vh = holder as ConversationHerMessageVh;
            vh.MessageTv.SetText(_message.Text, TextView.BufferType.Normal);
            vh.Date.SetText(_message.SentAt.ToString(), TextView.BufferType.Normal);
        }
    }
}
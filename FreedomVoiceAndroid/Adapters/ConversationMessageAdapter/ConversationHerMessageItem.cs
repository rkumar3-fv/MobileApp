using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FreedomVoice.Core.ViewModels;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    internal class ConversationHerMessageVh : RecyclerView.ViewHolder
    {
        public readonly TextView MessageTv;
        public readonly TextView Date;

        public ConversationHerMessageVh(View itemView) : base(itemView)
        {
            MessageTv = itemView.FindViewById<TextView>(Resource.Id.conversation_message_tv);
            Date = itemView.FindViewById<TextView>(Resource.Id.item_message_date);
        }
    }

    public class ConversationHerMessageItem : ConversationMessageRecyclerItem
    {
        private readonly IChatMessage _chatMessage;

        public ConversationHerMessageItem(IChatMessage chatMessage)
        {
            _chatMessage = chatMessage;
        }

        public int getLayoutResId() => Resource.Layout.item_her_message;
        public RecyclerView.ViewHolder createViewHolder(View view) => new ConversationHerMessageVh(view);

        public void Bind(RecyclerView.ViewHolder holder)
        {
            var vh = holder as ConversationHerMessageVh;
            vh.MessageTv.SetText(_chatMessage.Message, TextView.BufferType.Normal);
            vh.Date.SetText(_chatMessage.Time, TextView.BufferType.Normal);
        }
    }
}
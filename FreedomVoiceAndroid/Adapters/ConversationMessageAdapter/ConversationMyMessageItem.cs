using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FreedomVoice.Core.ViewModels;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    class ConversationMyMessageVh : RecyclerView.ViewHolder
    {
        public readonly TextView MessageTv;
        public readonly LinearLayout ContainerLl;
        public readonly TextView Date;

        public ConversationMyMessageVh(View itemView) : base(itemView)
        {
            MessageTv = itemView.FindViewById<TextView>(Resource.Id.conversation_message_tv);
            ContainerLl = itemView.FindViewById<LinearLayout>(Resource.Id.conversation_message_fl);
            Date = itemView.FindViewById<TextView>(Resource.Id.item_message_date);
        }
    }

    public class ConversationMyMessageItem : ConversationMessageRecyclerItem
    {
        private readonly IChatMessage _chatMessage;

        public ConversationMyMessageItem(IChatMessage chatMessage)
        {
            _chatMessage = chatMessage;
        }

        public int getLayoutResId() => Resource.Layout.item_my_message;
        public RecyclerView.ViewHolder createViewHolder(View view) => new ConversationMyMessageVh(view);

        public void Bind(RecyclerView.ViewHolder holder)
        {
            var vh = holder as ConversationMyMessageVh;
            vh.MessageTv.SetText(_chatMessage.Message, TextView.BufferType.Normal);
            vh.Date.SetText(_chatMessage.Time, TextView.BufferType.Normal);
        }
    }
}
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    class ConversationMyMessageVh : RecyclerView.ViewHolder
    {
        public readonly TextView MessageTv;
        public readonly FrameLayout ContainerLl;
        public readonly TextView Date;
        public readonly ImageView Icon;

        public ConversationMyMessageVh(View itemView) : base(itemView)
        {
            MessageTv = itemView.FindViewById<TextView>(Resource.Id.conversation_message_tv);
            ContainerLl = itemView.FindViewById<FrameLayout>(Resource.Id.conversation_message_fl);
            Date = itemView.FindViewById<TextView>(Resource.Id.item_message_date);
            Icon = itemView.FindViewById<ImageView>(Resource.Id.item_my_message_iv);
        }
    }

}
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.Entities.Enums;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    internal class ConversationHerMessageVh : RecyclerView.ViewHolder
    {
        public readonly TextView MessageTv;
        public readonly TextView Date;
        public readonly ImageView Icon;

        public ConversationHerMessageVh(View itemView) : base(itemView)
        {
            MessageTv = itemView.FindViewById<TextView>(Resource.Id.conversation_message_tv);
            Date = itemView.FindViewById<TextView>(Resource.Id.item_message_date);
            Icon = itemView.FindViewById<ImageView>(Resource.Id.item_my_message_iv);
        }
    }
}
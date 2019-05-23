using System;
using Android;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;


namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    internal class ConversationMessageDateVh : RecyclerView.ViewHolder
    {
        public readonly TextView DateTv;

        public ConversationMessageDateVh(View itemView) : base(itemView)
        {
            DateTv = itemView.FindViewById<TextView>(Resource.Id.frag_conversation_message_date_tv);
        }
    }
}
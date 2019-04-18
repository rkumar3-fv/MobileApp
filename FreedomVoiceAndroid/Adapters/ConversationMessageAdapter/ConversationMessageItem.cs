using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FreedomVoice.Entities;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    internal class ConversationMessageVh : RecyclerView.ViewHolder
    {
        public readonly TextView MessageTv;
        public readonly LinearLayout ContainerLl;

        public ConversationMessageVh(View itemView) : base(itemView)
        {
            MessageTv = itemView.FindViewById<TextView>(Resource.Id.conversation_message_tv);
            ContainerLl = itemView.FindViewById<LinearLayout>(Resource.Id.conversation_message_fl);
        }
    }
    
    public class ConversationMessageItem : ConversationMessageRecyclerItem
    {
        private readonly Message _message;
        private readonly int _myPhoneId;

        public ConversationMessageItem(Message message, int myPhoneId)
        {
            _message = message;
            _myPhoneId = myPhoneId;
        }

        public int getLayoutResId() => Resource.Layout.frag_conversation_message_item;
        public RecyclerView.ViewHolder createViewHolder(View view) => new ConversationMessageVh(view);

        public void Bind(RecyclerView.ViewHolder holder)
        {
            var vh = holder as ConversationMessageVh;

            var lp = vh.MessageTv.LayoutParameters as FrameLayout.LayoutParams;

            var isMyMessage = _message.From.Id == _myPhoneId;
            if (isMyMessage)
            {
                vh.MessageTv.SetBackgroundResource(Resource.Drawable.bg_message_cloud_right);
                vh.ContainerLl.SetGravity(GravityFlags.Right);
                lp.Gravity = GravityFlags.Right;
            }
            else
            {
                vh.MessageTv.SetBackgroundResource(Resource.Drawable.bg_message_cloud_left);
                vh.ContainerLl.SetGravity(GravityFlags.Left);
                lp.Gravity = GravityFlags.Left;
            }

            vh.MessageTv.LayoutParameters = lp;

            vh.MessageTv.SetText(_message.Text, TextView.BufferType.Normal);
        }
    }
}
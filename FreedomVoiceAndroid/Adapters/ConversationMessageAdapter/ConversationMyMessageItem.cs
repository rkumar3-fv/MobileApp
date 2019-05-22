using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.Entities.Enums;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    class ConversationMyMessageVh : RecyclerView.ViewHolder
    {
        public readonly TextView MessageTv;
        public readonly LinearLayout ContainerLl;
        public readonly TextView Date;
        public readonly ImageView Icon;

        public ConversationMyMessageVh(View itemView) : base(itemView)
        {
            MessageTv = itemView.FindViewById<TextView>(Resource.Id.conversation_message_tv);
            ContainerLl = itemView.FindViewById<LinearLayout>(Resource.Id.conversation_message_fl);
            Date = itemView.FindViewById<TextView>(Resource.Id.item_message_date);
            Icon = itemView.FindViewById<ImageView>(Resource.Id.item_my_message_iv);
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
            
            switch (_chatMessage.SendingState)
            {
                case SendingState.Error:
                    vh.Icon.SetImageResource(Resource.Drawable.ic_error);
                    vh.Icon.Visibility = ViewStates.Visible;
                    break;
                case SendingState.Sending:
                    vh.Icon.SetImageResource(Resource.Drawable.ic_anim_sending);
                    var sendAnimationDrawable = vh.Icon.Drawable as AnimationDrawable;
                    sendAnimationDrawable.SetCallback(vh.Icon);
                    sendAnimationDrawable.SetVisible(true, true);
                    sendAnimationDrawable.Start();
                    vh.Icon.Visibility = ViewStates.Visible;
                    break;
                default:
                    vh.Icon.Visibility = ViewStates.Gone;
                    break;
            }
        }
    }
}
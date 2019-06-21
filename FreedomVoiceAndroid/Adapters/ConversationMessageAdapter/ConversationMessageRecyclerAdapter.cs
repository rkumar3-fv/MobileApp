using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.Entities.Enums;
using Java.Lang;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    public partial class ConversationMessageRecyclerAdapter : RecyclerView.Adapter
    {
        private List<IChatMessage> _items = new List<IChatMessage>();

        public override int ItemCount => _items.Count;

        public override int GetItemViewType(int position) => (int) _items[position].Type;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var message = _items[position];
            switch (message.Type)
            {
                case ChatMessageType.Date:
                    var vh1 = holder as ConversationMessageDateVh;
                    vh1.DateTv.SetText(message.Message, TextView.BufferType.Normal);
                    break;
                case ChatMessageType.Incoming:
                    var vh2 = holder as ConversationHerMessageVh;
                    vh2.MessageTv.SetText(message.Message, TextView.BufferType.Normal);
                    vh2.Date.SetText(message.Time, TextView.BufferType.Normal);

                    switch (message.SendingState)
                    {
                        case SendingState.Error:
                            vh2.Icon.SetImageResource(Resource.Drawable.ic_error);
                            vh2.Icon.Visibility = ViewStates.Visible;
                            break;
                        case SendingState.Sending:
                            vh2.Icon.SetImageResource(Resource.Drawable.ic_anim_sending);
                            var sendAnimationDrawable = vh2.Icon.Drawable as AnimationDrawable;
                            sendAnimationDrawable.SetCallback(vh2.Icon);
                            sendAnimationDrawable.SetVisible(true, true);
                            sendAnimationDrawable.Start();
                            vh2.Icon.Visibility = ViewStates.Visible;
                            break;
                        default:
                            vh2.Icon.Visibility = ViewStates.Gone;
                            break;
                    }

                    break;
                case ChatMessageType.Outgoing:
                    var vh3 = holder as ConversationMyMessageVh;
                    vh3.MessageTv.SetText(message.Message, TextView.BufferType.Normal);
                    vh3.Date.SetText(message.Time, TextView.BufferType.Normal);

                    switch (message.SendingState)
                    {
                        case SendingState.Error:
                            vh3.Icon.SetImageResource(Resource.Drawable.ic_error);
                            vh3.Icon.Visibility = ViewStates.Visible;
                            break;
                        case SendingState.Sending:
                            vh3.Icon.SetImageResource(Resource.Drawable.ic_anim_sending);
                            var sendAnimationDrawable = vh3.Icon.Drawable as AnimationDrawable;
                            sendAnimationDrawable.SetCallback(vh3.Icon);
                            sendAnimationDrawable.SetVisible(true, true);
                            sendAnimationDrawable.Start();
                            vh3.Icon.Visibility = ViewStates.Visible;
                            break;
                        default:
                            vh3.Icon.Visibility = ViewStates.Gone;
                            break;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var layoutInflater = LayoutInflater.From(parent.Context);
            switch ((ChatMessageType) viewType)
            {
                case ChatMessageType.Date:
                    var view1 = layoutInflater.Inflate(Resource.Layout.frag_conversation_message_date, parent, false);
                    return new ConversationMessageDateVh(view1);
                    break;
                case ChatMessageType.Incoming:
                    var view2 = layoutInflater.Inflate(Resource.Layout.item_her_message, parent, false);
                    return new ConversationHerMessageVh(view2);
                    break;
                case ChatMessageType.Outgoing:
                    var view3 = layoutInflater.Inflate(Resource.Layout.item_my_message, parent, false);
                    return new ConversationMyMessageVh(view3);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateItems(IEnumerable<IChatMessage> newItems)
        {
            _items = newItems.ToList();
            NotifyDataSetChanged();
        }
    }
}
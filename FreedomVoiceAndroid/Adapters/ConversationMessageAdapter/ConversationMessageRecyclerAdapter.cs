using System;
using System.Collections.Generic;
using System.Linq;
using Android.Support.V7.Widget;
using Android.Views;
using FreedomVoice.Core.ViewModels;
using Java.Lang;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    public partial class ConversationMessageRecyclerAdapter : RecyclerView.Adapter
    {
        private List<ConversationMessageRecyclerItem> _items = new List<ConversationMessageRecyclerItem>();

        public override int ItemCount => _items.Count;

        public override int GetItemViewType(int position) => _items[position].getLayoutResId();

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) =>
            _items[position].Bind(holder);


        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(viewType, parent, false);

            foreach (var item in _items)
            {
                if (item.getLayoutResId() == viewType)
                {
                    return item.createViewHolder(view);
                }
            }

            throw new IllegalStateException($"no have items with viewType = ${viewType}");
        }

        public void UpdateItems(IEnumerable<IChatMessage> newItems)
        {
            _items = newItems
                .Aggregate(new List<ConversationMessageRecyclerItem>(),
                    (result, message) =>
                    {
                        switch (message.Type)
                        {
                            case ChatMessageType.Date:
                                result.Add(new ConversationMessageDate(message.Message));
                                break;
                            case ChatMessageType.Incoming:
                                result.Add(new ConversationHerMessageItem(message));
                                break;
                            case ChatMessageType.Outgoing:
                                result.Add(new ConversationMyMessageItem(message));
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        return result;
                    })
                .ToList();
            NotifyDataSetChanged();
        }
    }
}
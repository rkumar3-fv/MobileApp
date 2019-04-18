using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using FreedomVoice.Entities;
using Java.Lang;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    public partial class ConversationMessageRecyclerAdapter : RecyclerView.Adapter
    {
        private readonly int _myPhoneId;
        private List<ConversationMessageRecyclerItem> _items = new List<ConversationMessageRecyclerItem>();

        public ConversationMessageRecyclerAdapter(int myPhoneId)
        {
            _myPhoneId = myPhoneId;
        }

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

        public void UpdateItems(List<Message> newItems)
        {
            _items = newItems
                .GroupBy(message => message.SentAt?.Date)
                .Aggregate(new List<ConversationMessageRecyclerItem>(),
                    (result, messages) =>
                    {
                        result.Add(new ConversationMessageDate(messages.Key));
                        result.AddRange(messages.Select(message => _myPhoneId == message.From.Id
                            ? (ConversationMessageRecyclerItem) new ConversationMyMessageItem(message)
                            : (ConversationMessageRecyclerItem) new ConversationHerMessageItem(message)));
                        
                        return result;
                    })
                .ToList();
            NotifyDataSetChanged();
        }
    }
}
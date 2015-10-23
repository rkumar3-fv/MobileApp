using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Entities;
using FreedomVoice.Core.Utils;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    /// <summary>
    /// Recents list adapter
    /// </summary>
    public class RecentsRecyclerAdapter : RecyclerView.Adapter
    {
        private readonly Context _context;
        private readonly SortedDictionary<long, Recent> _currentContent;

        /// <summary>
        /// Item short click event
        /// </summary>
        public event EventHandler<long> ItemClick;

        /// <summary>
        /// Additional item sector short click
        /// </summary>
        public event EventHandler<long> AdditionalSectorClick;

        private void OnClick(int position)
        {
            var keys = _currentContent.Keys.ToList();
            ItemClick?.Invoke(this, keys[position]);
        }

        private void OnAditionalClick(int position)
        {
            var keys = _currentContent.Keys.ToList();
            AdditionalSectorClick?.Invoke(this, keys[position]);
        }

        public RecentsRecyclerAdapter(SortedDictionary<long, Recent> currentContent, Context context)
        {
            _context = context;
            _currentContent = currentContent;
        }

        /// <summary>
        /// Remove item
        /// </summary>
        /// <param name="index">item index</param>
        public void RemoveItem(int index)
        {
            var keys = _currentContent.Keys.ToList();
            _currentContent.Remove(keys[index]);
            NotifyItemRemoved(index);
        }

        /// <summary>
        /// Insert item notification
        /// </summary>
        /// <param name="key">changed element key</param>
        public void NotifyInsertItem(long key)
        {
            var keys = _currentContent.Keys.ToList();
            NotifyItemInserted(keys.IndexOf(key));
        }

        /// <summary>
        /// Adding new to resent
        /// </summary>
        public void NotifyAddItem()
        {
            NotifyItemInserted(0);
        }

        /// <summary>
        /// Get recent by position
        /// </summary>
        /// <param name="position">position number</param>
        /// <returns>recent item</returns>
        public Recent GetContentItem(int position)
        {
            var keys = _currentContent.Keys.ToList();
            return (_currentContent.Count < position) ? null : _currentContent[keys[position]];
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as ViewHolder;
            var keys = _currentContent.Keys.ToList();
            if (viewHolder == null) return;
            viewHolder.DestinationNumberText.Text = DataFormatUtils.ToPhoneNumber(_currentContent[keys[position]].PhoneNumber);
            viewHolder.CallDateText.Text =
                DataFormatUtils.ToShortFormattedDate(_context.GetString(Resource.String.Timestamp_yesterday),
                    _currentContent[keys[position]].CallDate);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_recent, parent, false);
            return new ViewHolder(itemView, OnClick, OnAditionalClick);
        }

        /// <summary>
        /// Account list length
        /// </summary>
        public override int ItemCount => _currentContent?.Count ?? 0;

        /// <summary>
        /// Account selection viewholder
        /// </summary>
        private class ViewHolder : RecyclerView.ViewHolder
        {

            /// <summary>
            /// Destination number field
            /// </summary>
            public TextView DestinationNumberText { get; }

            /// <summary>
            /// Call date or time
            /// </summary>
            public TextView CallDateText { get; }

            /// <summary>
            /// Main clickable area
            /// </summary>
            private RelativeLayout _mainSectorLayout;

            /// <summary>
            /// Additional clickable area
            /// </summary>
            private LinearLayout _additionalSectorLayout;

            public ViewHolder(View itemView, Action<int> mainListener, Action<int> additionalListener) : base(itemView)
            {
                DestinationNumberText = itemView.FindViewById<TextView>(Resource.Id.itemRecent_phone);
                CallDateText = itemView.FindViewById<TextView>(Resource.Id.itemRecent_date);
                _mainSectorLayout = itemView.FindViewById<RelativeLayout>(Resource.Id.itemRecent_mainArea);
                _additionalSectorLayout = itemView.FindViewById<LinearLayout>(Resource.Id.itemRecent_additionalArea);
                _mainSectorLayout.Click += (sender, e) => mainListener(AdapterPosition);
                _additionalSectorLayout.Click += (sender, e) => additionalListener(AdapterPosition);
            }
        }
    }
}
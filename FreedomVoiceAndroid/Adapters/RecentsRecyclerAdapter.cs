using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Database;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Entities;
using com.FreedomVoice.MobileApp.Android.Utils;
using FreedomVoice.Core.Utils;
using Java.Interop;
using Uri = Android.Net.Uri;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    /// <summary>
    /// Recents list adapter
    /// </summary>
    public class RecentsRecyclerAdapter : RecyclerView.Adapter
    {
        private readonly Context _context;
        private readonly SortedDictionary<long, Recent> _currentContent;
        private readonly ContactsHelper _helper;

        /// <summary>
        /// Item short click event
        /// </summary>
        public event EventHandler<int> ItemClick;

        /// <summary>
        /// Additional item sector short click
        /// </summary>
        public event EventHandler<string> AdditionalSectorClick;

        private void OnClick(int position)
        {
            if ((_currentContent!=null)&&(position<_currentContent.Count))
                ItemClick?.Invoke(this, position);
        }

        private void OnAditionalClick(int position)
        {
            if ((_currentContent != null) && (position < _currentContent.Count))
            {
                var keys = _currentContent.Keys.ToList();
                var normalizedPhone = DataFormatUtils.NormalizePhone(_currentContent[keys[position]].PhoneNumber);
                var uri = Uri.WithAppendedPath(ContactsContract.PhoneLookup.ContentFilterUri,
                    Uri.Encode(normalizedPhone));
                var selection = string.Format("(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1'))",
                    ContactsContract.Contacts.InterfaceConsts.DisplayName,
                    ContactsContract.Contacts.InterfaceConsts.InVisibleGroup);
                string[] projection = {ContactsContract.Contacts.InterfaceConsts.Id};
                var loader = new CursorLoader(_context, uri, projection, selection, null, null);
                var cursor = loader.LoadInBackground().JavaCast<ICursor>();
                if (cursor == null) return;
                if (cursor.MoveToFirst())
                    AdditionalSectorClick?.Invoke(this, cursor.GetString(0));
                cursor.Close();
            }
        }

        public RecentsRecyclerAdapter(SortedDictionary<long, Recent> currentContent, Context context)
        {
            _context = context;
            _currentContent = currentContent;
            _helper = ContactsHelper.Instance(_context);
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

        public void NotifyUpdateItem(int key)
        {
            if (key != 0)
                NotifyItemMoved(key, 0);
            NotifyItemChanged(0);
        }

        /// <summary>
        /// Adding new to resent
        /// </summary>
        public void NotifyAddItem()
        {
            NotifyItemInserted(0);
        }

        public void NotifyClear()
        {
            if ((_currentContent == null)||(_currentContent.Count == 0)) return;
            if (_currentContent.Count == 1)
                NotifyItemRemoved(0);
            for (var i = _currentContent.Count; i>=0; i--)
                NotifyItemRemoved(i);
            _currentContent.Clear();
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
            string text;
            viewHolder.AdditionalLayout.Visibility = !_helper.GetName(_currentContent[keys[position]].PhoneNumber, out text) ? ViewStates.Invisible : ViewStates.Visible;
            string countText;
            if (_currentContent[keys[position]].Count > 99)
                countText = " (99+)";
            else if (_currentContent[keys[position]].Count < 2)
                countText = "";
            else
                countText = $" ({_currentContent[keys[position]].Count})";
            viewHolder.DestinationNumberText.Text = $"{text}{countText}";
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

            public LinearLayout AdditionalLayout { get; }

            public ViewHolder(View itemView, Action<int> mainListener, Action<int> additionalListener) : base(itemView)
            {
                DestinationNumberText = itemView.FindViewById<TextView>(Resource.Id.itemRecent_phone);
                CallDateText = itemView.FindViewById<TextView>(Resource.Id.itemRecent_date);
                var mainSectorLayout = itemView.FindViewById<RelativeLayout>(Resource.Id.itemRecent_mainArea);
                AdditionalLayout = itemView.FindViewById<LinearLayout>(Resource.Id.itemRecent_additionalArea);
                mainSectorLayout.Click += (sender, e) => 
                {
                    if (sender != null)
                        mainListener(AdapterPosition);
                };
                AdditionalLayout.Click += (sender, e) =>
                {
                    if (sender != null)
                        additionalListener(AdapterPosition);
                };
            }
        }
    }
}
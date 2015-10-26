using System;
using System.Collections.Generic;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FreedomVoice.Core.Utils;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    /// <summary>
    /// SelectAccount recycler view adapter
    /// </summary>
    public class PhonesRecyclerAdapter : RecyclerView.Adapter
    {
        private List<string> _phonesList;

        /// <summary>
        /// Item short click event
        /// </summary>
        public event EventHandler<string> ItemClick;

        public PhonesRecyclerAdapter(List<string> phonesList)
        {
            _phonesList = phonesList;
        }

        /// <summary>
        /// Phones list
        /// </summary>
        public List<string> PhonesList
        {
            get { return _phonesList; }
            set
            {
                _phonesList = value;
                NotifyDataSetChanged();
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as ViewHolder;
            if (viewHolder != null)
                viewHolder.Phone.Text = DataFormatUtils.ToPhoneNumber(_phonesList[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_phone, parent, false);
            return new ViewHolder(itemView, OnClick);
        }

        private void OnClick(int position)
        {
            ItemClick?.Invoke(this, _phonesList[position]);
        }

        /// <summary>
        /// Phones list length
        /// </summary>
        public override int ItemCount => _phonesList?.Count ?? 0;

        /// <summary>
        /// Phone selection viewholder
        /// </summary>
        private class ViewHolder : RecyclerView.ViewHolder
        {

            /// <summary>
            /// Phone text field
            /// </summary>
            public TextView Phone { get; }

            public ViewHolder(View itemView, Action<int> listener) : base(itemView)
            {
                Phone = itemView.FindViewById<TextView>(Resource.Id.itemPhone_numberText);
                itemView.Click += (sender, e) => listener(AdapterPosition);
            }
        }
    }
}
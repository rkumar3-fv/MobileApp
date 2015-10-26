using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Utils;
using Object = Java.Lang.Object;
using FreedomVoice.Core.Utils;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    /// <summary>
    /// Custem adapter for caller ID spinner
    /// </summary>
    class CallerIdSpinnerAdapter: BaseAdapter, ISpinnerAdapter
    {
        private readonly Context _context;
        private List<string> _numbersList;

        public CallerIdSpinnerAdapter(Context context, List<string> list)
        {
            _context = context;
            _numbersList = list;
        }

        /// <summary>
        /// Caller IDs list
        /// </summary>
        public List<string> NumbersList
        {
            get { return _numbersList; }
            set 
            {
                _numbersList = value;
                NotifyDataSetChanged();
            }
        }

        public CallerIdSpinnerAdapter(Context context) : this (context, new List<string>())
        { }

        /// <summary>
        /// Get list item
        /// </summary>
        /// <param name="position">position</param>
        /// <returns>Account object</returns>
        public override Object GetItem(int position)
        {
            return _numbersList[position];
        }

        /// <summary>
        /// Get selected account
        /// </summary>
        /// <param name="position">position</param>
        /// <returns>Account</returns>
        public string GetPresentationNumber(int position)
        {
            return _numbersList == null ? "" : _numbersList[position];
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder holder = null;
            var view = convertView;
            if(view != null)
                holder = view.Tag as ViewHolder;

            if (holder == null)
            {
                holder = new ViewHolder();
                view = LayoutInflater.From(_context).Inflate(Resource.Layout.item_spinner, null);
                holder.SpinnerValue = view.FindViewById<TextView>(Resource.Id.itemSpinner_idText);
                view.Tag = holder;
            }
            if (_numbersList != null)
                holder.SpinnerValue.Text = DataFormatUtils.ToPhoneNumber(_numbersList[position]);
            return view;
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            ViewHolderDrop holder = null;
            var view = convertView;
            if (view != null)
                holder = view.Tag as ViewHolderDrop;

            if (holder == null)
            {
                holder = new ViewHolderDrop();
                view = LayoutInflater.From(_context).Inflate(Resource.Layout.item_spinner_drop, null);
                holder.SpinnerValue = view.FindViewById<TextView>(Resource.Id.itemSpinnerDrop_idText);
                view.Tag = holder;
            }
            if (_numbersList != null)
                holder.SpinnerValue.Text = DataFormatUtils.ToPhoneNumber(_numbersList[position]);
            return view;
        }

        /// <summary>
        /// Get accounts count
        /// </summary>
        public override int Count => _numbersList?.Count ?? 0;

        private class ViewHolder: Object
        {
            public TextView SpinnerValue { get; set; }
        }

        private class ViewHolderDrop : Object
        {
            public TextView SpinnerValue { get; set; }
        }
    }
}
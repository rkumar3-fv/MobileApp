using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Entities;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    /// <summary>
    /// SelectAccount recycler view adapter
    /// </summary>
    public class AccountsRecyclerAdapter : RecyclerView.Adapter
    {
        private List<Account> _accountsList;
        private IPhoneFormatter _formatter;

        /// <summary>
        /// Item short click event
        /// </summary>
        public event EventHandler<int> ItemClick;

        public AccountsRecyclerAdapter()
        {
            _accountsList = new List<Account>();
        }

        public AccountsRecyclerAdapter(List<Account> accountsList)
        {
            _formatter = ServiceContainer.Resolve<IPhoneFormatter>();
            _accountsList = accountsList;
        }

        /// <summary>
        /// Accounts list
        /// </summary>
        public List<Account> AccountsList
        {
            get { return _accountsList; }
            set
            {
                _accountsList = value;
                NotifyDataSetChanged();
            }
        }

        /// <summary>
        /// Get account name by position
        /// </summary>
        /// <param name="position">position number</param>
        /// <returns>account name</returns>
        public string AccountName(int position)
        {
            return (_accountsList.Count < position) ? null : _accountsList[position].AccountName;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is ViewHolder viewHolder)
            {
                viewHolder.AccountText.Text = _formatter.Format(_accountsList[position].AccountName);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_account, parent, false);
            return new ViewHolder(itemView, OnClick);
        }

        private void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

        /// <summary>
        /// Account list length
        /// </summary>
        public override int ItemCount => _accountsList?.Count ?? 0;

        /// <summary>
        /// Account selection viewholder
        /// </summary>
        private class ViewHolder : RecyclerView.ViewHolder
        {
            
            /// <summary>
            /// Account text field
            /// </summary>
            public TextView AccountText { get; }

            public ViewHolder(View itemView, Action<int> listener ) : base(itemView)
            {
                AccountText = itemView.FindViewById<TextView>(Resource.Id.itemAccount_numberText);
                itemView.Click += (sender, e) => listener(AdapterPosition);
            }
        }
    }
}
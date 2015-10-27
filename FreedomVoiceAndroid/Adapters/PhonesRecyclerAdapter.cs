using System;
using System.Collections.Generic;
using Android.Content;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Entities;
using FreedomVoice.Core.Utils;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    /// <summary>
    /// SelectAccount recycler view adapter
    /// </summary>
    public class PhonesRecyclerAdapter : RecyclerView.Adapter
    {
        private List<Phone> _phonesList;
        private readonly Context _context;

        /// <summary>
        /// Item short click event
        /// </summary>
        public event EventHandler<Phone> ItemClick;

        public PhonesRecyclerAdapter(Context context, List<Phone> phonesList)
        {
            _context = context;
            _phonesList = phonesList;
        }

        /// <summary>
        /// Phones list
        /// </summary>
        public List<Phone> PhonesList
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
            if (viewHolder == null) return;
            viewHolder.Phone.Text = DataFormatUtils.ToPhoneNumber(_phonesList[position].PhoneNumber);
            PhoneDataKind dataKind;
            switch (_phonesList[position].TypeCode)
            {
                case 19:            //TYPE_ASSISTANT
                    dataKind = PhoneDataKind.Assistant;
                    break;
                case 8:             //TYPE_CALLBACK
                    dataKind = PhoneDataKind.Callback;
                    break;
                case 9:             //TYPE_CAR
                    dataKind = PhoneDataKind.Car;
                    break;
                case 10:            //TYPE_COMPANY_MAIN
                    dataKind = PhoneDataKind.CompanyMain;
                    break;
                case 5:             //TYPE_FAX_HOME
                    dataKind = PhoneDataKind.FaxHome;
                    break;
                case 4:             //TYPE_FAX_WORK
                    dataKind = PhoneDataKind.FaxWork;
                    break;
                case 1:             //TYPE_HOME
                    dataKind = PhoneDataKind.Home;
                    break;
                case 11:            //TYPE_ISDN
                    dataKind = PhoneDataKind.Isdn;
                    break;
                case 12:            //TYPE_MAIN
                    dataKind = PhoneDataKind.Main;
                    break;
                case 20:            //TYPE_MMS
                    dataKind = PhoneDataKind.Mms;
                    break;
                case 2:             //TYPE_MOBILE
                    dataKind = PhoneDataKind.Mobile;
                    break;
                case 7:             //TYPE_OTHER
                    dataKind = PhoneDataKind.Other;
                    break;
                case 13:            //TYPE_OTHER_FAX
                    dataKind = PhoneDataKind.OtherFax;
                    break;
                case 6:             //TYPE_PAGER
                    dataKind = PhoneDataKind.Pager;
                    break;
                case 14:            //TYPE_RADIO
                    dataKind = PhoneDataKind.Radio;
                    break;
                case 15:            //TYPE_TELEX
                    dataKind = PhoneDataKind.Telex;
                    break;
                case 16:            //TYPE_TTY_TDD
                    dataKind = PhoneDataKind.TtyTdd;
                    break;
                case 3:             //TYPE_WORK
                    dataKind = PhoneDataKind.Work;
                    break;
                case 17:            //TYPE_WORK_MOBILE
                    dataKind = PhoneDataKind.WorkMobile;
                    break;
                case 18:            //TYPE_WORK_PAGER
                    dataKind = PhoneDataKind.WorkPager;
                    break;
                default:
                    dataKind = PhoneDataKind.Custom;
                    break;
            }
            viewHolder.Type.Text = _context.GetString(ContactsContract.CommonDataKinds.Phone.GetTypeLabelResource(dataKind));
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

            public TextView Type { get; }

            public ViewHolder(View itemView, Action<int> listener) : base(itemView)
            {
                Phone = itemView.FindViewById<TextView>(Resource.Id.itemPhone_numberText);
                Type = itemView.FindViewById<TextView>(Resource.Id.itemPhone_typeText);
                itemView.Click += (sender, e) => listener(AdapterPosition);
            }
        }
    }
}
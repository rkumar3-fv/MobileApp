using System;
using System.Collections.Generic;
using Android.Content;
using Android.Database;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Entities;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    /// <summary>
    /// Contacts recyclerView adapter
    /// </summary>
    public class ContactsRecyclerAdapter : RecyclerView.Adapter
    {
        private readonly Context _context;
        private readonly int _id;

        /// <summary>
        /// Item short click event
        /// </summary>
        public event EventHandler<List<Phone>> ItemClick;

        private void OnClick(int position)
        {
            if (Cursor == null) return;
            if (!Cursor.MoveToPosition(position)) return;
            var id = Cursor.GetString(Cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id));
            var hasPhone = Cursor.GetString(Cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber));
            if (hasPhone.Equals("0"))
                ItemClick?.Invoke(this, new List<Phone>());
            else
            {
                string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.CommonDataKinds.Phone.Number, "data2" };
                var loader = new CursorLoader(_context, ContactsContract.CommonDataKinds.Phone.ContentUri, projection,
                    $"contact_id={id}", null, null);
                var cursor = (ICursor)loader.LoadInBackground();
                if (cursor != null)
                {
                    var phones = new List<Phone>();
                    while (cursor.MoveToNext())
                    {
                        var phone = PhoneNumberUtils.NormalizeNumber(cursor.GetString(cursor.GetColumnIndex(projection[1])));
                        var type = Convert.ToInt32(cursor.GetString(cursor.GetColumnIndex(projection[2])));
                        phones.Add(new Phone(phone, type));
                    }
                    cursor.Close();
                    ItemClick?.Invoke(this, phones);
                }
                else
                    ItemClick?.Invoke(this, new List<Phone>());
            }
        }

        public ContactsRecyclerAdapter(Context context, ICursor cursor)
        {
            _context = context;
            Cursor = cursor;
            _id = Cursor?.GetColumnIndex("_id") ?? -1;
            DataSetObserver observer = new NotifyingDataSetObserver(this);
            Cursor?.RegisterDataSetObserver(observer);
        }

        public ICursor Cursor { get; }

        public override long GetItemId(int position)
        {
            if (Cursor != null && Cursor.MoveToPosition(position))
                return Cursor.GetLong(_id);
            return 0;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as ViewHolder;
            if ((viewHolder == null)||(Cursor==null)) return;
            if (!Cursor.MoveToPosition(position))
                return;
            var name = Cursor.GetString(Cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
            if (!Cursor.MoveToPrevious())
            {
                if (name.Length > 0)
                    viewHolder.ContactLetter.Text = name.Substring(0, 1).ToUpper();
                else
                    viewHolder.ContactLetter.Text = "";
            }
            else
            {
                var prevName = Cursor.GetString(Cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
                if ((name.Length > 0)&&(prevName.Length>0)&&((prevName.Substring(0,1).ToLowerInvariant()).Equals(name.Substring(0,1).ToLowerInvariant())))
                    viewHolder.ContactLetter.Text = "";
                else
                    viewHolder.ContactLetter.Text = name.Substring(0, 1).ToUpper();
                
            }

            viewHolder.ContactText.Text = name;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_contact, parent, false);
            return new ViewHolder(itemView, OnClick);
        }

        public override int ItemCount => Cursor?.Count ?? 0;

        /// <summary>
        /// Contacts selection viewholder
        /// </summary>
        private class ViewHolder : RecyclerView.ViewHolder
        {
            /// <summary>
            /// First letter label
            /// </summary>
            public TextView ContactLetter { get; }

            /// <summary>
            /// Contact text field
            /// </summary>
            public TextView ContactText { get; }

            public ViewHolder(View itemView, Action<int> listener) : base(itemView)
            {
                ContactLetter = itemView.FindViewById<TextView>(Resource.Id.itemContact_letter);
                ContactText = itemView.FindViewById<TextView>(Resource.Id.itemContact_text);
                itemView.Click += (sender, e) => listener(AdapterPosition);
            }
        }

        /// <summary>
        /// Cursor notify observer
        /// </summary>
        private class NotifyingDataSetObserver : DataSetObserver
        {
            private readonly ContactsRecyclerAdapter _adapter;

            public NotifyingDataSetObserver(ContactsRecyclerAdapter adapter)
            {
                _adapter = adapter;
            }

            public override void OnChanged()
            {
                base.OnChanged();
                _adapter.NotifyDataSetChanged();
            }

            public override void OnInvalidated()
            {
                base.OnInvalidated();
                _adapter.NotifyDataSetChanged();
            }
        }
    }
}
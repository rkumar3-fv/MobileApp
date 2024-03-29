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
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;
using Java.Interop;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    /// <summary>
    /// Contacts recyclerView adapter
    /// </summary>
    public class ContactsRecyclerAdapter : RecyclerView.Adapter
    {
        private readonly DataSetObserver _observer;
        private ICursor _oldCursor;
        private readonly Context _context;
        private int _id;
        private ICursor Cursor { get; set; }

        /// <summary>
        /// Item short click event
        /// </summary>
        public event EventHandler<Contact> ItemClick;

        private void OnClick(int position)
        {
            if (Cursor == null) return;
            if (!Cursor.MoveToPosition(position)) return;
            var id = Cursor.GetString(0);
            var name = Cursor.GetString(Cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
            var hasPhone = Cursor.GetString(Cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber));
            if (hasPhone.Equals("0"))
                ItemClick?.Invoke(this, new Contact(name));
            else
            {
                string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.CommonDataKinds.Phone.Number, "data2" };
                var loader = new CursorLoader(_context, ContactsContract.CommonDataKinds.Phone.ContentUri, projection,
                    $"contact_id={id}", null, null);
                ICursor cursor;
                try
                {
                    cursor = loader.LoadInBackground().JavaCast<ICursor>();
                }
                catch (Java.Lang.RuntimeException)
                {
                    cursor = null;
                }
                if (cursor != null)
                {
                    var phonesDict = new Dictionary<string, Phone>();
                    while (cursor.MoveToNext())
                    {
                        var phone = ServiceContainer.Resolve<IPhoneFormatter>().Format(cursor.GetString(cursor.GetColumnIndex(projection[1])));
                        var type = Convert.ToInt32(cursor.GetString(cursor.GetColumnIndex(projection[2])));
                        if (!phonesDict.ContainsKey(phone))
                            phonesDict.Add(phone, new Phone(phone, type));
                    }
                    cursor.Close();
                    ItemClick?.Invoke(this, new Contact(name, phonesDict.Values.ToList()));
                }
                else
                    ItemClick?.Invoke(this, new Contact(name));
            }
        }

        public ContactsRecyclerAdapter(Context context, ICursor cursor)
        {
            _context = context;
            Cursor = cursor;
            _id = Cursor?.GetColumnIndex("_id") ?? -1;
            _observer = new NotifyingDataSetObserver(this);
            Cursor?.RegisterDataSetObserver(_observer);
            _oldCursor = null;
        }

        public ContactsRecyclerAdapter(Context context)
        {
            _context = context;
            Cursor = null;
            _oldCursor = null;
        }

        public override long GetItemId(int position)
        {
            if (Cursor != null && Cursor.MoveToPosition(position))
                return Cursor.GetLong(_id);
            return 0;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as ViewHolder;
            if (viewHolder == null || Cursor == null || Cursor.IsClosed) return;
            if (!Cursor.MoveToPosition(position))
                return;
            var name = Cursor.GetString(Cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
            if (!Cursor.MoveToPrevious())
            {
                viewHolder.ContactLetter.Text = name.Length > 0 ? name.Substring(0, 1).ToUpper() : "";
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

        /// <summary>
        /// Changing one cursor to other
        /// </summary>
        /// <param name="cursor">new cursor</param>
        public void ChangeCursor(ICursor cursor)
        {
            var old = SwapCursor(cursor);
            old?.Close();
        }

        /// <summary>
        /// Add cursor for search
        /// </summary>
        /// <param name="cursor">cursor for search</param>
        public void AddSearchCursor(ICursor cursor)
        {
            if (_oldCursor == null)
                _oldCursor = SwapCursor(cursor);
            else
                ChangeCursor(cursor);
        }

        /// <summary>
        /// Restore old cursor
        /// </summary>
        public void RestoreCursor()
        {
            if (_oldCursor == null) return;
            ChangeCursor(_oldCursor);
            _oldCursor = null;
        }

        /// <summary>
        /// Swap old and new cursor
        /// </summary>
        /// <param name="newCursor">new cursor</param>
        /// <returns>old cursor</returns>
        public ICursor SwapCursor(ICursor newCursor)
        {
            if (newCursor == Cursor)
                return null;
            var oldCursor = Cursor;
            if (oldCursor != null && _observer != null)
                oldCursor.UnregisterDataSetObserver(_observer);

            Cursor = newCursor;
            if (Cursor != null)
            {
                if (_observer != null)
                {
                    Cursor.RegisterDataSetObserver(_observer);
                }
                _id = newCursor?.GetColumnIndex("_id") ?? -1;
                NotifyDataSetChanged();
            }
            else
            {
                _id = -1;
                NotifyDataSetChanged();
            }
            return oldCursor;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_contact, parent, false);
            return new ViewHolder(itemView, OnClick);
        }

        public override int ItemCount
        {
            get
            {
                if (Cursor == null || Cursor.IsClosed) return 0;
                return Cursor?.Count ?? 0;
            }
        }


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
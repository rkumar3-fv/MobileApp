using System.Collections.Generic;
using System.Text.RegularExpressions;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
#if DEBUG
using Android.Util;
#endif
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.CustomControls;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Entities;
using FreedomVoice.Core.Utils;
using Uri = Android.Net.Uri;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    /// <summary>
    /// Contacts tab
    /// </summary>
    public class ContactsFragment : CallerFragment
    {
        private RecyclerView _contactsView;
        private ContactsRecyclerAdapter _adapter;
        private TextView _noResTextView;

        protected override View InitView()
        {
            var view = Inflater.Inflate(Resource.Layout.frag_contacts, null, false);
            IdSpinner = view.FindViewById<Spinner>(Resource.Id.contatnsFragment_idSpinner);
            SingleId = view.FindViewById<TextView>(Resource.Id.contactsFragment_singleId);
            _noResTextView = view.FindViewById<TextView>(Resource.Id.contactsFragment_noResultText);
            _contactsView = view.FindViewById<RecyclerView>(Resource.Id.contactsFragment_recyclerView);
            _contactsView.SetLayoutManager(new LinearLayoutManager(Activity));
            _contactsView.AddItemDecoration(new DividerItemDecorator(Activity, Resource.Drawable.divider));  
            return view;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            _adapter = new ContactsRecyclerAdapter(ContentActivity);
            _adapter.ItemClick += AdapterOnItemClick;
            _contactsView.SetAdapter(_adapter);
        }

        public override void OnStart()
        {
            base.OnStart();
            ReloadContacts();
        }

        public override void OnResume()
        {
            base.OnResume();
            ContentActivity.SearchListener.OnChange += SearchListenerOnChange;
            ContentActivity.SearchListener.OnApply += SearchListenerOnApply;
            ContentActivity.SearchListener.OnCollapse += SearchListenerOnCancel;
        }      

        public override void OnPause()
        {
            base.OnPause();
            ContentActivity.SearchListener.OnChange -= SearchListenerOnChange;
            ContentActivity.SearchListener.OnApply -= SearchListenerOnApply;
            ContentActivity.SearchListener.OnCollapse -= SearchListenerOnCancel;
        }

        private void AdapterOnItemClick(object sender, Contact contact)
        {

            foreach (var phone in contact.PhonesList)
            {
#if DEBUG
                Log.Debug(App.AppPackage, $"{phone.PhoneNumber} - {phone.TypeCode}");
#else
                App.GetApplication(Context).ApplicationHelper.Reports?.Log($"{phone.PhoneNumber} - {phone.TypeCode}");
#endif
            }

            ContentActivity.HideKeyboard();
            switch (contact.PhonesList.Count)
            {
                case 0:
                    var noPhonesDialog = new NoContactsDialogFragment(contact);
                    noPhonesDialog.Show(ContentActivity.SupportFragmentManager, GetString(Resource.String.DlgNumbers_content));
                    break;
                case 1:
                    ContentActivity.Call(contact.PhonesList[0].PhoneNumber);
                    break;
                default:
                    var multiPhonesDialog = new MultiContactsDialogFragment(contact, ContentActivity);
                    multiPhonesDialog.PhoneClick += MultiPhonesDialogOnPhoneClick;
                    multiPhonesDialog.Show(ContentActivity.SupportFragmentManager, GetString(Resource.String.DlgNumbers_title));
                    break;
            }
        }

        private void MultiPhonesDialogOnPhoneClick(object sender, Phone phone)
        {
            ContentActivity.Call(phone.PhoneNumber);
            _contactsView.RequestFocus();
        }

        private void SearchListenerOnCancel(object sender, bool b)
        {
            _adapter.RestoreCursor();
            if (_adapter.ItemCount > 0)
            {
                if (_noResTextView.Visibility == ViewStates.Visible)
                    _noResTextView.Visibility = ViewStates.Invisible;
                if (_contactsView.Visibility == ViewStates.Invisible)
                    _contactsView.Visibility = ViewStates.Visible;
            }
            else
            {
                if (_noResTextView.Visibility == ViewStates.Invisible)
                    _noResTextView.Visibility = ViewStates.Visible;
                if (_contactsView.Visibility == ViewStates.Visible)
                    _contactsView.Visibility = ViewStates.Invisible;
                _noResTextView.Text = GetString(Resource.String.FragmentContacts_empty);
            }
        }

        private void SearchListenerOnApply(object sender, string s)
        {
            var resCursor = Search(s);
            if (resCursor.Count > 0)
            {
                if (_noResTextView.Visibility == ViewStates.Visible)
                    _noResTextView.Visibility = ViewStates.Invisible;
                if (_contactsView.Visibility == ViewStates.Invisible)
                    _contactsView.Visibility = ViewStates.Visible;
            }
            else
            {
                if (_noResTextView.Visibility == ViewStates.Invisible)
                    _noResTextView.Visibility = ViewStates.Visible;
                if (_contactsView.Visibility == ViewStates.Visible)
                    _contactsView.Visibility = ViewStates.Invisible;
                _noResTextView.Text = GetString(Resource.String.FragmentContacts_no);
            }
            _adapter.AddSearchCursor(resCursor);
        }

        private void SearchListenerOnChange(object sender, string s)
        {
            var resCursor = Search(s);
            if (resCursor.Count > 0)
            {
                if (_noResTextView.Visibility == ViewStates.Visible)
                    _noResTextView.Visibility = ViewStates.Invisible;
                if (_contactsView.Visibility == ViewStates.Invisible)
                    _contactsView.Visibility = ViewStates.Visible;
            }
            else
            {
                if (_noResTextView.Visibility == ViewStates.Invisible)
                    _noResTextView.Visibility = ViewStates.Visible;
                if (_contactsView.Visibility == ViewStates.Visible)
                    _contactsView.Visibility = ViewStates.Invisible;
                _noResTextView.Text = GetString(Resource.String.FragmentContacts_no);
            }
            _adapter.AddSearchCursor(resCursor);
        }

        private ICursor Search(string enteredQuery)
        {
            var query = enteredQuery.Trim();
            var sortOrder = $"{ContactsContract.Contacts.InterfaceConsts.DisplayName} COLLATE LOCALIZED ASC";
            ICursor phonesCursor = null;
            var uri = ContactsContract.Contacts.ContentUri;
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber, ContactsContract.Contacts.InterfaceConsts.PhotoUri };
            var selection = string.Format("(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1') AND ({0} like '%{2}%'))",
                ContactsContract.Contacts.InterfaceConsts.DisplayName, ContactsContract.Contacts.InterfaceConsts.InVisibleGroup, query);
            var loader = new CursorLoader(ContentActivity, uri, projection, selection, null, sortOrder);
            ICursor namesCursor;
            try
            {
                namesCursor = loader.LoadInBackground().JavaCast<ICursor>();
            }
            catch (Java.Lang.RuntimeException)
            {
                namesCursor = null;
            }

            if (Regex.IsMatch(query, @"^[0-9+()\-\s]+$"))
            {
                var iDs = new List<string>();
                if ((namesCursor != null) && (namesCursor.Count > 0))
                {
                    while (namesCursor.MoveToNext())
                    {
                        var id = namesCursor.GetString(namesCursor.GetColumnIndex(projection[0]));
                        if (!string.IsNullOrEmpty(id))
                            iDs.Add(id);
                    }
                }

                var uriPhones = Uri.Parse($"content://com.android.contacts/data/phones/filter/*{DataFormatUtils.NormalizePhone(query)}*");
                string[] projectionPhones = { "contact_id", ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber, ContactsContract.Contacts.InterfaceConsts.PhotoUri };
                string selectionPhones;
                if (iDs.Count == 0)
                    selectionPhones = string.Format("(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1'))",
                    ContactsContract.Contacts.InterfaceConsts.DisplayName, ContactsContract.Contacts.InterfaceConsts.InVisibleGroup);
                else
                    selectionPhones = string.Format("(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1') AND ({2} NOT IN ('{3}')))",
                ContactsContract.Contacts.InterfaceConsts.DisplayName, ContactsContract.Contacts.InterfaceConsts.InVisibleGroup, "contact_id", string.Join("', '", iDs.ToArray()));
                var loaderPhones = new CursorLoader(ContentActivity, uriPhones, projectionPhones, selectionPhones, null, sortOrder);
                try
                {
                    phonesCursor = loaderPhones.LoadInBackground().JavaCast<ICursor>();
                }
                catch (Java.Lang.RuntimeException)
                {
                    phonesCursor = null;
                }
            }

            if (phonesCursor == null)
                return namesCursor;
            if ((namesCursor == null)||(namesCursor.Count == 0))
                return phonesCursor;
            return new MergeCursor(new[] {phonesCursor, namesCursor});
        }


        public void ReloadContacts()
        {
            var uri = ContactsContract.Contacts.ContentUri;
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber, ContactsContract.Contacts.InterfaceConsts.PhotoUri };
            var selection = string.Format("(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1'))",
                ContactsContract.Contacts.InterfaceConsts.DisplayName, ContactsContract.Contacts.InterfaceConsts.InVisibleGroup);
            var sortOrder = $"{ContactsContract.Contacts.InterfaceConsts.DisplayName} COLLATE LOCALIZED ASC";
            var loader = new CursorLoader(ContentActivity, uri, projection, selection, null, sortOrder);
            ICursor cursor;
            try
            {
                cursor = loader.LoadInBackground().JavaCast<ICursor>();
            }
            catch (Java.Lang.RuntimeException)
            {
                cursor = null;
            }
            _adapter?.SwapCursor(cursor);
            if ((_noResTextView != null) && (_contactsView != null))
            {
                if ((cursor != null) && (cursor.Count > 0))
                {
                    if (_noResTextView.Visibility == ViewStates.Visible)
                        _noResTextView.Visibility = ViewStates.Invisible;
                    if (_contactsView.Visibility == ViewStates.Invisible)
                        _contactsView.Visibility = ViewStates.Visible;
                }
                else
                {
                    if (_noResTextView.Visibility == ViewStates.Invisible)
                        _noResTextView.Visibility = ViewStates.Visible;
                    if (_contactsView.Visibility == ViewStates.Visible)
                        _contactsView.Visibility = ViewStates.Invisible;
                    _noResTextView.Text = GetString(Resource.String.FragmentContacts_empty);
                }
            }
        }
    }
}
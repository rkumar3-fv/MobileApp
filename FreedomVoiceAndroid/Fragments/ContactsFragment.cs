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
            var uri = ContactsContract.Contacts.ContentUri;
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber, ContactsContract.Contacts.InterfaceConsts.PhotoUri };
            var selection = string.Format("(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1'))",
                ContactsContract.Contacts.InterfaceConsts.DisplayName, ContactsContract.Contacts.InterfaceConsts.InVisibleGroup);
            var sortOrder = $"{ContactsContract.Contacts.InterfaceConsts.DisplayName} COLLATE LOCALIZED ASC";
            var loader = new CursorLoader(ContentActivity, uri, projection, selection, null, sortOrder);
            var cursor = loader.LoadInBackground().JavaCast<ICursor>();
            _adapter.SwapCursor(cursor);
            if (cursor.Count > 0)
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
#if DEBUG
            foreach (var phone in contact.PhonesList)
            {
                Log.Debug(App.AppPackage, $"{phone.PhoneNumber} - {phone.TypeCode}");
            }
#endif
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

        private ICursor Search(string query)
        {
            string selection;
            if (Regex.IsMatch(query, @"^\d+$"))
            {
                //TODO: inner selection
                selection = string.Format("(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1') AND ({0} like '%{2}%'))",
                ContactsContract.Contacts.InterfaceConsts.DisplayName, ContactsContract.Contacts.InterfaceConsts.InVisibleGroup, query);
            }
            else
            {
                selection = string.Format("(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1') AND ({0} like '%{2}%'))",
                ContactsContract.Contacts.InterfaceConsts.DisplayName, ContactsContract.Contacts.InterfaceConsts.InVisibleGroup, query);
            }
            
            var uri = ContactsContract.Contacts.ContentUri;
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber, ContactsContract.Contacts.InterfaceConsts.PhotoUri };
            var sortOrder = $"{ContactsContract.Contacts.InterfaceConsts.DisplayName} COLLATE LOCALIZED ASC";
            var loader = new CursorLoader(ContentActivity, uri, projection, selection, null, sortOrder);
            var namesCursor = loader.LoadInBackground().JavaCast<ICursor>();
            return namesCursor;
        }

    }
}
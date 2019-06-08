using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V7.Widget;
#if DEBUG
using Android.Util;
#endif
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.CustomControls;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Entities;
using com.FreedomVoice.MobileApp.Android.Utils;
using FreedomVoice.Core.Utils;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using CursorLoader = Android.Support.V4.Content.CursorLoader;
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
        private ContactsHelper _helper;

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
            
            _helper = ContactsHelper.Instance(ContentActivity);

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
            ContentActivity.HideKeyboard();
            
            
            foreach (var phone in contact.PhonesList)
            {
#if DEBUG
                Log.Debug(App.AppPackage, $"{phone.PhoneNumber} - {phone.TypeCode}");

#else
                App.GetApplication(Context).ApplicationHelper.Reports?.Log($"{phone.PhoneNumber} - {phone.TypeCode}");
#endif
            }

            var selectView = LayoutInflater.From(Context).Inflate(Resource.Layout.dlg_contact_select_action, null, false);
            var phoneView = selectView.FindViewById<ViewGroup>(Resource.Id.dlg_contact_select_phone_block);
            var smsView = selectView.FindViewById<ViewGroup>(Resource.Id.dlg_contact_select_sms_block);

            AlertDialog alertDialog = new AlertDialog.Builder(Context)
                .SetTitle("Select:")
                .SetView(selectView)
                .SetNegativeButton("Cancel", (o, args) =>
                {
                    (o as Dialog)?.Dismiss();
                })
                .Create();


            phoneView.Click += (o, args) =>
            {
                alertDialog.Dismiss();
                switch (contact.PhonesList.Count)
                {
                    case 0:
                        var noPhonesDialog = new NoContactsDialogFragment(contact);
                        var transaction = ContentActivity.SupportFragmentManager.BeginTransaction();
                        transaction.Add(noPhonesDialog, GetString(Resource.String.DlgNumbers_content));
                        transaction.CommitAllowingStateLoss();
                        break;
                    case 1:
                        ContentActivity.Call(contact.PhonesList[0].PhoneNumber);
                        break;
                    default:
                        var multiPhonesDialog = new MultiContactsDialogFragment(contact, ContentActivity);
                        multiPhonesDialog.PhoneClick += MultiPhonesDialogOnPhoneClick;
                        var transactionMultiPhones = ContentActivity.SupportFragmentManager.BeginTransaction();
                        transactionMultiPhones.Add(multiPhonesDialog,
                            GetString(Resource.String.DlgNumbers_title));
                        transactionMultiPhones.CommitAllowingStateLoss();
                        break;
                }
            };

            smsView.Click += (o, args) =>
            {
                alertDialog.Dismiss();
                switch (contact.PhonesList.Count)
                {
                    case 0:
                        var noPhonesDialog = new NoContactsDialogFragment(contact);
                        var transaction = ContentActivity.SupportFragmentManager.BeginTransaction();
                        transaction.Add(noPhonesDialog, GetString(Resource.String.DlgNumbers_content));
                        transaction.CommitAllowingStateLoss();
                        break;
                    case 1:
                        ChatActivity.StartNewChat(Activity, contact.PhonesList[0].PhoneNumber);
                        break;
                    default:
                        var multiPhonesDialog = new MultiContactsDialogFragment(contact, ContentActivity);
                        multiPhonesDialog.PhoneClick += (sender1, phone) =>
                        {
                            ChatActivity.StartNewChat(Activity, phone.PhoneNumber);
                        };
                        var transactionMultiPhones = ContentActivity.SupportFragmentManager.BeginTransaction();
                        transactionMultiPhones.Add(multiPhonesDialog,
                            GetString(Resource.String.DlgNumbers_title));
                        transactionMultiPhones.CommitAllowingStateLoss();
                        break;
                }
            };
          
            alertDialog.Show();
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
            return _helper.Search(enteredQuery);
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
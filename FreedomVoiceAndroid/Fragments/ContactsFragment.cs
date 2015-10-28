using System.Collections.Generic;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
#if DEBUG
using Android.Util;
#endif
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
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

        protected override View InitView()
        {
            var view = Inflater.Inflate(Resource.Layout.frag_contacts, null, false);
            IdSpinner = view.FindViewById<Spinner>(Resource.Id.contatnsFragment_idSpinner);
            SingleId = view.FindViewById<TextView>(Resource.Id.contactsFragment_singleId);
            _contactsView = view.FindViewById<RecyclerView>(Resource.Id.contactsFragment_recyclerView);
            _contactsView.SetLayoutManager(new LinearLayoutManager(Activity));
            _contactsView.AddItemDecoration(new DividerItemDecorator(Activity, Resource.Drawable.divider));  
            return view;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            var uri = ContactsContract.Contacts.ContentUri;
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber, ContactsContract.Contacts.InterfaceConsts.PhotoUri };
            var selection = string.Format("(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1'))", 
                ContactsContract.Contacts.InterfaceConsts.DisplayName, ContactsContract.Contacts.InterfaceConsts.InVisibleGroup);
            var sortOrder = $"{ContactsContract.Contacts.InterfaceConsts.DisplayName} COLLATE LOCALIZED ASC";
            var loader = new CursorLoader(ContentActivity, uri, projection, selection, null, sortOrder);
            var cursor = (ICursor)loader.LoadInBackground();
            _adapter = new ContactsRecyclerAdapter(ContentActivity, cursor);
            _adapter.ItemClick += AdapterOnItemClick;
            _contactsView.SetAdapter(_adapter);
        }

        private void AdapterOnItemClick(object sender, List<Phone> list)
        {
#if DEBUG
            foreach (var phone in list)
            {
                Log.Debug(App.AppPackage, $"{phone.PhoneNumber} - {phone.TypeCode}");
            }
#endif
            switch (list.Count)
            {
                case 0:
                    var noPhonesDialog = new NoContactsDialogFragment();
                    noPhonesDialog.Show(ContentActivity.SupportFragmentManager, GetString(Resource.String.DlgNumbers_content));
                    break;
                case 1:
                    ContentActivity.Call(list[0].PhoneNumber);
                    break;
                default:
                    var multiPhonesDialog = new MultiContactsDialogFragment(list, ContentActivity);
                    multiPhonesDialog.PhoneClick += MultiPhonesDialogOnPhoneClick;
                    multiPhonesDialog.Show(ContentActivity.SupportFragmentManager, GetString(Resource.String.DlgNumbers_title));
                    break;
            }
        }

        private void MultiPhonesDialogOnPhoneClick(object sender, Phone phone)
        {
            ContentActivity.Call(phone.PhoneNumber);
        }
    }
}
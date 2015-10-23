using System;
using System.Collections.Generic;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.Helpers;
using FreedomVoice.Core.Utils;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    /// <summary>
    /// Contacts tab
    /// </summary>
    public class ContactsFragment : BasePagerFragment
    {
        private Spinner _idSpinner;
        private RecyclerView _contactsView;
        private ContactsRecyclerAdapter _adapter;

        protected override View InitView()
        {
            var view = Inflater.Inflate(Resource.Layout.frag_contacts, null, false);
            _idSpinner = view.FindViewById<Spinner>(Resource.Id.contatnsFragment_idSpinner);
            _idSpinner.ItemSelected += (sender, args) =>
            {
                Helper.SelectedAccount.SelectedPresentationNumber = args.Position;
                Log.Debug(App.AppPackage, $"PRESENTATION NUMBER SET to {DataFormatUtils.ToPhoneNumber(Helper.SelectedAccount.PresentationNumber)}");
            };

            _contactsView = view.FindViewById<RecyclerView>(Resource.Id.contactsFragment_recyclerView);
            _contactsView.SetLayoutManager(new LinearLayoutManager(Activity));
            _contactsView.AddItemDecoration(new DividerItemDecorator(Activity, Resource.Drawable.divider));  
            return view;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            var adapter = new CallerIdSpinnerAdapter(Activity, Helper.SelectedAccount.PresentationNumbers);
            _idSpinner.Adapter = adapter;

            var uri = ContactsContract.Contacts.ContentUri;
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.Contacts.InterfaceConsts.DisplayName, ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber };
            var loader = new CursorLoader(ContentActivity, uri, projection, null, null, null);
            var cursor = (ICursor)loader.LoadInBackground();
            _adapter = new ContactsRecyclerAdapter(ContentActivity, cursor);
            _adapter.ItemClick += AdapterOnItemClick;
            _contactsView.SetAdapter(_adapter);
        }

        public override void OnResume()
        {
            base.OnResume();
            _idSpinner.SetSelection(Helper.SelectedAccount.SelectedPresentationNumber);
        }

        private void AdapterOnItemClick(object sender, List<string> list)
        {
            foreach (var item in list)
            {
                Log.Debug(App.AppPackage, $"Phone: {item}");
            }
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            
        }
    }
}
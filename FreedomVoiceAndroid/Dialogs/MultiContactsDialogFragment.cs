using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.Entities;

namespace com.FreedomVoice.MobileApp.Android.Dialogs
{
    public class MultiContactsDialogFragment : BaseDialogFragment
    {
        /// <summary>
        /// Phone click
        /// </summary>
        public event EventHandler<Phone> PhoneClick;

        private readonly Context _context;
        private readonly List<Phone> _content; 
        private RecyclerView _phonesView;

        public MultiContactsDialogFragment(List<Phone> content, Context context)
        {
            _context = context;
            _content = content;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dlg_contacts_many, container, false);
            CancelButton = view.FindViewById<Button>(Resource.Id.ManyContactsDlg_cancel);
            _phonesView = view.FindViewById<RecyclerView>(Resource.Id.ManyContactsDlg_list);
            _phonesView.SetLayoutManager(new LinearLayoutManager(_context));
            _phonesView.AddItemDecoration(new DividerItemDecorator(Activity, Resource.Drawable.divider));
            return view;
        }

        public override void OnStart()
        {
            base.OnStart();
            var phonesAdapter = new PhonesRecyclerAdapter(_context, _content);
            phonesAdapter.ItemClick += PhonesAdapterOnItemClick;
            _phonesView.SetAdapter(phonesAdapter);
        }

        /// <summary>
        /// Phone click event
        /// </summary>
        private void PhonesAdapterOnItemClick(object sender, Phone phone)
        {
            PhoneClick?.Invoke(this, phone);
            Dismiss();
        }
    }
}
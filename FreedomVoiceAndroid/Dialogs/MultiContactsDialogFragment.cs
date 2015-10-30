using System;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.CustomControls;
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
        private readonly Contact _contact; 
        private RecyclerView _phonesView;
        private TextView _titleView;

        public MultiContactsDialogFragment(Contact contact, Context context)
        {
            _context = context;
            _contact = contact;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dlg_contacts_many, container, false);
            CancelButton = view.FindViewById<Button>(Resource.Id.ManyContactsDlg_cancel);
            _titleView = view.FindViewById<TextView>(Resource.Id.ManyContactsDlg_title);
            _titleView.Text = $"{GetString(Resource.String.DlgNumbers_title)} {_contact.Name}";
            _phonesView = view.FindViewById<RecyclerView>(Resource.Id.ManyContactsDlg_list);
            _phonesView.SetLayoutManager(new LinearLayoutManager(_context));
            _phonesView.AddItemDecoration(new DividerItemDecorator(Activity, Resource.Drawable.divider));
            return view;
        }

        public override void OnStart()
        {
            base.OnStart();
            var phonesAdapter = new PhonesRecyclerAdapter(_context, _contact.PhonesList);
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
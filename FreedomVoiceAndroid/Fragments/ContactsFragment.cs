using Android.OS;
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

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.frag_contacts, container, false);
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
        }

        public override void OnResume()
        {
            base.OnResume();
            _idSpinner.SetSelection(Helper.SelectedAccount.SelectedPresentationNumber);
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            
        }
    }
}
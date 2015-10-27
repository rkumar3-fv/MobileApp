using Android.OS;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Util;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.Helpers;
using FreedomVoice.Core.Utils;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    /// <summary>
    /// Recents tab
    /// </summary>
    public class RecentsFragment : BasePagerFragment
    {
        private Spinner _idSpinner;
        private RecyclerView _recentsView;
        private ItemTouchHelper _swipeTouchHelper;
        private RecentsRecyclerAdapter _adapter;

        protected override View InitView()
        {
            var view = Inflater.Inflate(Resource.Layout.frag_recents, null, false);
            _idSpinner = view.FindViewById<Spinner>(Resource.Id.recentsFragment_idSpinner);
            _idSpinner.ItemSelected += (sender, args) => Helper.SetPresentationNumber(args.Position);

            _recentsView = view.FindViewById<RecyclerView>(Resource.Id.recentsFragment_recyclerView);
            _recentsView.SetLayoutManager(new LinearLayoutManager(Activity));
            _recentsView.AddItemDecoration(new DividerItemDecorator(Activity, Resource.Drawable.divider));

            var swipeListener = new SwipeCallback(0, ItemTouchHelper.Left | ItemTouchHelper.Right, ContentActivity, Resource.Color.colorRemoveList, Resource.Drawable.ic_action_delete);
            swipeListener.SwipeEvent += OnSwipeEvent;
            _swipeTouchHelper = new ItemTouchHelper(swipeListener);
            _swipeTouchHelper.AttachToRecyclerView(_recentsView);
            return view;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            var adapter = new CallerIdSpinnerAdapter(Activity, Helper.SelectedAccount.PresentationNumbers);
            _idSpinner.Adapter = adapter;

            _adapter = new RecentsRecyclerAdapter(Helper.RecentsDictionary, ContentActivity);
            _adapter.ItemClick += AdapterOnItemClick;
            _adapter.AdditionalSectorClick += AdapterOnAdditionalSectorClick;
            _recentsView.SetAdapter(_adapter);
        }

        public override void OnResume()
        {
            base.OnResume();
            if (_idSpinner.SelectedItemPosition != Helper.SelectedAccount.SelectedPresentationNumber)
                _idSpinner.SetSelection(Helper.SelectedAccount.SelectedPresentationNumber);
            _adapter.NotifyDataSetChanged();
        }

        /// <summary>
        /// Opening additional info
        /// </summary>
        private void AdapterOnAdditionalSectorClick(object sender, long l)
        {
            Log.Debug(App.AppPackage, $"ADDITIONAL INFO ABOUT {DataFormatUtils.ToPhoneNumber(Helper.RecentsDictionary[l].PhoneNumber)}");
            //TODO: when info content will be created
        }

        /// <summary>
        /// Main item click - call again
        /// </summary>
        private void AdapterOnItemClick(object sender, long l)
        {
            ContentActivity.Call(Helper.RecentsDictionary[l].PhoneNumber);
        }

        /// <summary>
        /// Recent swipe
        /// </summary>
        private void OnSwipeEvent(object sender, SwipeCallbackEventArgs args)
        {
            Log.Debug(App.AppPackage, $"SWIPED recent {args.ElementIndex}");
            _adapter.RemoveItem(args.ElementIndex);
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.CallReservationOk:
                    case ActionsHelperEventArgs.CallReservationFail:
                        _adapter.NotifyAddItem();
                        break;
                    case ActionsHelperEventArgs.ChangePresentation:
                        if (_idSpinner.SelectedItemPosition != Helper.SelectedAccount.SelectedPresentationNumber)
                            _idSpinner.SetSelection(Helper.SelectedAccount.SelectedPresentationNumber);
                        break;
                    case ActionsHelperEventArgs.ClearRecents:
                        _adapter.NotifyClear();
                        break;
                }
            }
        }
    }
}
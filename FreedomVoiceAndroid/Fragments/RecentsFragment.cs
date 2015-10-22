using System;
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

        protected override View InitView()
        {
            var view = Inflater.Inflate(Resource.Layout.frag_recents, null, false);
            _idSpinner = view.FindViewById<Spinner>(Resource.Id.recentsFragment_idSpinner);
            _idSpinner.ItemSelected += (sender, args) =>
            {
                Helper.SelectedAccount.SelectedPresentationNumber = args.Position;
                Log.Debug(App.AppPackage, $"PRESENTATION NUMBER SET to {DataFormatUtils.ToPhoneNumber(Helper.SelectedAccount.PresentationNumber)}");
            };

            _recentsView = view.FindViewById<RecyclerView>(Resource.Id.recentsFragment_recyclerView);
            _recentsView.SetLayoutManager(new LinearLayoutManager(Activity));
            _recentsView.AddItemDecoration(new DividerItemDecorator(Activity, Resource.Drawable.divider));

            var swipeListener = new SwipeCallback(0, ItemTouchHelper.Left | ItemTouchHelper.Right, ContentActivity, Resource.Color.colorRemoveList, Resource.Drawable.ic_action_delete);
            swipeListener.SwipeEvent += OnSwipeEvent;
            _swipeTouchHelper = new ItemTouchHelper(swipeListener);
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

        /// <summary>
        /// Recent swipe
        /// </summary>
        private void OnSwipeEvent(object sender, SwipeCallbackEventArgs args)
        {
            Log.Debug(App.AppPackage, $"SWIPED recent {args.ElementIndex}");
            
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            
        }
    }
}
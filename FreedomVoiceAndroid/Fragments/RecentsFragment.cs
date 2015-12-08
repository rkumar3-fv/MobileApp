using System.Linq;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
#if DEBUG
using Android.Util;
#endif
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.CustomControls;
using com.FreedomVoice.MobileApp.Android.CustomControls.Callbacks;
using com.FreedomVoice.MobileApp.Android.CustomControls.CustomEventArgs;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    /// <summary>
    /// Recents tab
    /// </summary>
    public class RecentsFragment : CallerFragment
    {
        private RecyclerView _recentsView;
        private ItemTouchHelper _swipeTouchHelper;
        private RecentsRecyclerAdapter _adapter;
        private TextView _noRecentsTextView;
        private int _lastClicked;

        protected override View InitView()
        {
            _lastClicked = -1;
            var view = Inflater.Inflate(Resource.Layout.frag_recents, null, false);
            IdSpinner = view.FindViewById<Spinner>(Resource.Id.recentsFragment_idSpinner);
            SingleId = view.FindViewById<TextView>(Resource.Id.recentsFragment_singleId);
            _recentsView = view.FindViewById<RecyclerView>(Resource.Id.recentsFragment_recyclerView);
            _recentsView.SetLayoutManager(new LinearLayoutManager(Activity));
            _recentsView.AddItemDecoration(new DividerItemDecorator(Activity, Resource.Drawable.divider));
            _noRecentsTextView = view.FindViewById<TextView>(Resource.Id.recentsFragment_noResultText);
            var swipeListener = new SwipeCallback(0, ItemTouchHelper.Left | ItemTouchHelper.Right, ContentActivity, Resource.Color.colorRemoveList, Resource.Drawable.ic_action_delete);
            swipeListener.SwipeEvent += OnSwipeEvent;
            _swipeTouchHelper = new ItemTouchHelper(swipeListener);
            _swipeTouchHelper.AttachToRecyclerView(_recentsView);
            return view;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            _adapter = new RecentsRecyclerAdapter(Helper.RecentsDictionary, ContentActivity);
            _adapter.ItemClick += AdapterOnItemClick;
            _adapter.AdditionalSectorClick += AdapterOnAdditionalSectorClick;
            _recentsView.SetAdapter(_adapter);
        }

        public override void OnResume()
        {
            base.OnResume();
            _adapter.NotifyDataSetChanged();
            CheckVisibility();
        }

        /// <summary>
        /// Opening additional info
        /// </summary>
        private void AdapterOnAdditionalSectorClick(object sender, string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            var intent = new Intent(Intent.ActionView);
            var uri = Uri.WithAppendedPath(ContactsContract.Contacts.ContentUri, id);
            intent.SetData(uri);
            ContentActivity.StartActivity(intent);
        }

        /// <summary>
        /// Main item click - call again
        /// </summary>
        private void AdapterOnItemClick(object sender, int l)
        {
            var keys = Helper.RecentsDictionary.Keys.ToList();
            if ((l < keys.Count) && (l != -1))
            {
                ContentActivity.Call(Helper.RecentsDictionary[keys[l]].SingleRecent.PhoneNumber);
                _lastClicked = l;
            }
        }

        /// <summary>
        /// Recent swipe
        /// </summary>
        private void OnSwipeEvent(object sender, SwipeCallbackEventArgs args)
        {
#if DEBUG
            Log.Debug(App.AppPackage, $"SWIPED recent {args.ElementIndex}");
#endif
            Helper.RemoveRecent(_adapter.GetContentItem(args.ElementIndex));
            _adapter.RemoveItem(args.ElementIndex);
            CheckVisibility();
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            base.OnHelperEvent(args);
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.CallReservationOk:
                    case ActionsHelperEventArgs.CallReservationFail:
                    case ActionsHelperEventArgs.CallReservationWrong:
                        if (_adapter.ItemCount < Helper.RecentsDictionary.Count)
                            _adapter.NotifyAddItem();
                        else if (_lastClicked != -1)
                        {
                            _adapter.NotifyUpdateItem(_lastClicked);
                            _lastClicked = -1;
                        }
                        else
                            _adapter.NotifyDataSetChanged();
                        break;
                    case ActionsHelperEventArgs.ClearRecents:
                        _adapter.NotifyClear();
                        break;
                }
                CheckVisibility();
            }
        }

        private void CheckVisibility()
        {
            if (_adapter.ItemCount == 0)
            {
                if (_recentsView.Visibility == ViewStates.Visible)
                    _recentsView.Visibility = ViewStates.Invisible;
                if (_noRecentsTextView.Visibility == ViewStates.Invisible)
                    _noRecentsTextView.Visibility = ViewStates.Visible;
            }
            else
            {
                if (_recentsView.Visibility == ViewStates.Invisible)
                    _recentsView.Visibility = ViewStates.Visible;
                if (_noRecentsTextView.Visibility == ViewStates.Visible)
                    _noRecentsTextView.Visibility = ViewStates.Invisible;
            }
        }
    }
}
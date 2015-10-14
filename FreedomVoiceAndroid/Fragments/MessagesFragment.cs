using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    /// <summary>
    /// Messages tab
    /// </summary>
    public class MessagesFragment : BasePagerFragment, SwipeRefreshLayout.IOnRefreshListener
    {
        private MessagesRecyclerAdapter _adapter;
        private RecyclerView _recyclerView;
        private SwipeRefreshLayout _swipeRefresh;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.frag_messages, container, false);
            _swipeRefresh = view.FindViewById<SwipeRefreshLayout>(Resource.Id.messagesFragment_swipe);
            _swipeRefresh.SetOnRefreshListener(this);
            _swipeRefresh.SetColorSchemeResources(Resource.Color.colorPullRefreshFirst, Resource.Color.colorPullRefreshSecond, Resource.Color.colorPullRefreshThird);
            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.messagesFragment_recyclerView);
            _recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            _recyclerView.AddItemDecoration(new DividerItemDecorator(Activity, Resource.Drawable.divider));
            _adapter = new MessagesRecyclerAdapter(Context);
            _recyclerView.SetAdapter(_adapter);
            _adapter.ItemClick += MessageViewOnClick;
            return _recyclerView;
        }

        /// <summary>
        /// Message item click
        /// </summary>
        private void MessageViewOnClick(object sender, int position)
        {
            if (position > _adapter.CurrentContent.Count) return;
            Log.Debug(App.AppPackage, $"FRAGMENT {GetType().Name}: select item #{position}");
            _adapter.CurrentContent = Helper.GetNext(position);
            if (Helper.SelectedMessage != -1)
                ;//TODO: OPEN MSG DETAILS
            else if (Helper.SelectedFolder != -1)
                Helper.ForceLoadMessages();
            else
                Helper.ForceLoadFolders();
            ContentActivity.SetToolbarContent();
        }

        public override void OnResume()
        {
            base.OnResume();
            _adapter.CurrentContent = Helper.GetCurrent();
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.MsgUpdated:
                        _adapter.CurrentContent = Helper.GetCurrent();
                        _swipeRefresh.Refreshing = false;
                        break;
                }
            }
        }

        public void OnRefresh()
        {
            Helper.ForceLoadExtensions();
        }
    }
}
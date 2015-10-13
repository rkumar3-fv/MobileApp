using Android.OS;
using Android.Runtime;
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
    public class MessagesFragment : BasePagerFragment
    {
        private MessagesRecyclerAdapter _adapter;
        private RecyclerView _recyclerView;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.frag_messages, container, false);
            _recyclerView = view.JavaCast<RecyclerView>();
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
            ContentActivity.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            ContentActivity.SupportActionBar.SetHomeButtonEnabled(true);
            if (Helper.SelectedFolder != -1)
                ContentActivity.SupportActionBar.Title = Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].FolderName;
            else if (Helper.SelectedExtension != -1)
                ContentActivity.SupportActionBar.Title = $"{Helper.ExtensionsList[Helper.SelectedExtension].Id} - {Helper.ExtensionsList[Helper.SelectedExtension].ExtensionName}";
            else
                ContentActivity.SupportActionBar.Title = GetString(Resource.String.FragmentMessages_title);
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
                        break;
                }
            }
        }
    }
}
using System.Linq;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.Entities;
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
            return _recyclerView;
        }
        
        public override void OnResume()
        {
            base.OnResume();
            _adapter.CurrentContent = Helper.ExtensionsList.Cast<MessageItem>().ToList();
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.MsgExtensionsUpdated:
                        _adapter.CurrentContent = Helper.ExtensionsList.Cast<MessageItem>().ToList();
                        break;
                }
            }
        }
    }
}
using System;
using Android.Support.V7.Widget;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public class XamarinRecyclerViewOnScrollListener : RecyclerView.OnScrollListener
    {
        public delegate void LoadMoreEventHandler(object sender, EventArgs e);

        public event LoadMoreEventHandler ScrollEvent;

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);
            ScrollEvent(this, null);
        }
    }
}
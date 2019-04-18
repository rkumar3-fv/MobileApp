using Android.Support.V7.Widget;
using Android.Views;
using Org.Apache.Http.Client.Methods;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    // interface marker
    public interface ConversationMessageRecyclerItem
    {
        int getLayoutResId();
        RecyclerView.ViewHolder createViewHolder(View view);
        void Bind(RecyclerView.ViewHolder holder);
    }
}
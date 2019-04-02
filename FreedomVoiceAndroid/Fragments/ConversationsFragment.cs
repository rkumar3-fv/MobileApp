using System;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    public class ConversationsFragment : BasePagerFragment
    {
        public ConversationsFragment()
        {
        }

        protected override View InitView()
        {
            var view = Inflater.Inflate(Resource.Layout.frag_conversations, null, false);
            return view;
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            //throw new NotImplementedException();
        }
    }
}

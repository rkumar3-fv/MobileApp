using System;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    public class ConversationMessageDate : ConversationMessageRecyclerItem
    {
        public readonly DateTime? DateTime;

        public ConversationMessageDate(DateTime? dateTime)
        {
            DateTime = dateTime?.Date;
        }
    }
}
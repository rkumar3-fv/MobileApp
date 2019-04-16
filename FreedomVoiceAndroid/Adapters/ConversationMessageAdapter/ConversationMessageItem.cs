using FreedomVoice.Entities;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    public class ConversationMessageItem : ConversationMessageRecyclerItem
    {
        public readonly Message Message;

        public ConversationMessageItem(Message message)
        {
            Message = message;
        }
    }
}
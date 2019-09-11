namespace FreedomVoice.Entities.Request
{
    public class SearchConversationRequest : FrameRequest
    {
        public string Text { get; set; }
        public string[] PhoneNumbers { get; set; }
        public string SystemPhone { get; set; }
    }
}

namespace FreedomVoice.Entities.Request.Texting
{
    public class ConversationsRequest : FrameRequest
    {
        public string PhoneNumber { get; set; }
        public string ApplicationNumber { get; set; }
    }
}

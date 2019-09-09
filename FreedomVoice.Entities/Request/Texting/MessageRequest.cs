namespace FreedomVoice.Entities.Request.Texting
{
    public class MessageRequest
    {
        public string ApplicationNumber { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Text { get; set; }
    }
}

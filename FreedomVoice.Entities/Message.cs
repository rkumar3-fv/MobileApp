using System;

namespace FreedomVoice.Entities
{
    public class Message
    {
        public string Text { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}

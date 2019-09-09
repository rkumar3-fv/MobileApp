using FreedomVoice.Entities.Enums;
using System;

namespace FreedomVoice.Entities
{
    public class Message
    {
        public long Id { get; set; }
        public Phone From { get; set; }
        public Phone To { get; set; }
        public string Text { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public SendingState State { get; set; }
    }
}

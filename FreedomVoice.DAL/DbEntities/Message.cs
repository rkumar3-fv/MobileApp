using System;
using FreedomVoice.DAL.DbEntities.Enums;

namespace FreedomVoice.DAL.DbEntities
{
    public class Message : BaseEntity
    {
        public Phone From { get; set; }
        public Phone To { get; set; }
        public string Text { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        
        public DateTime? LastUpdateDate { get; set; }
        public Conversation Conversation { get; set; }
        public SendingState State { get; set; }
    }
}

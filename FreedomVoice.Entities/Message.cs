﻿using System;

namespace FreedomVoice.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public Phone From { get; set; }
        public Phone To { get; set; }
        public string Text { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace FreedomVoice.Entities.Response
{
    public class Conversation
    {
        public long Id { get; set; }
        public Phone SystemPhone { get; set; }
        public Phone ToPhone { get; set; }
        public DateTime LastSyncDate { get; set; }
        public ICollection<Message> Messages { get; set; }
        public bool IsRemoved { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace FreedomVoice.Entities.Response
{
    public class Conversation
    {
        public int Id { get; set; }
        public Phone CurrentPhone { get; set; }
        public Phone CollocutorPhone { get; set; }
        public DateTime LastSyncDate { get; set; }
        public IEnumerable<Message> Messages { get; set; }
    }
}

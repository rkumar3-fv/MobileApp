using System;
using System.Collections.Generic;

namespace FreedomVoice.DAL.DbEntities
{
    public class Conversation : BaseEntity
    {
        public Phone CurrentPhone { get; set; }
        public Phone CollocutorPhone { get; set; }
        public DateTime LastSyncDate { get; set; }
        public IEnumerable<Message> Messages { get; set; }
    }
}

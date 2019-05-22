using System;
using System.Collections.Generic;

namespace FreedomVoice.DAL.DbEntities
{
    public class Conversation : BaseEntity
    {
        public Phone SystemPhone { get; set; }
        public Phone ToPhone { get; set; }
        public DateTime LastSyncDate { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}

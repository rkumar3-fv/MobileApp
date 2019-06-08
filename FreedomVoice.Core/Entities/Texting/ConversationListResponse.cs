using FreedomVoice.DAL.DbEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreedomVoice.Core.Entities.Texting
{
    public class ConversationListResponse : BaseListResponse
    {
        public IEnumerable<Conversation> Conversations { get; set; }
    }
}

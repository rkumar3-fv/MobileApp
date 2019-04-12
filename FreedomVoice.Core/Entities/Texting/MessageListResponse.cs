using System.Collections.Generic;

namespace FreedomVoice.Core.Entities.Texting
{
    public class MessageListResponse : BaseListResponse
    {
        public IEnumerable<DAL.DbEntities.Message> Messages { get; set; }
    }
}

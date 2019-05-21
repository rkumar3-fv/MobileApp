using FreedomVoice.DAL.DbEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreedomVoice.Core.Entities.Texting
{
    public class ConversationResponse : BaseResponse
    {
        public Conversation Conversation { get; set; }
    }
}

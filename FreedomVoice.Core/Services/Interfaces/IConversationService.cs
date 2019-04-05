using FreedomVoice.Core.Entities.Texting;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreedomVoice.Core.Services.Interfaces
{
    public interface IConversationService
    {
        ConversationListResponse GetList(int count);
    }
}

using FreedomVoice.Core.Entities.Texting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FreedomVoice.Core.Services.Interfaces
{
    public interface IConversationService
    {
        Task<ConversationListResponse> GetList(string phone, int count, int page);
    }
}

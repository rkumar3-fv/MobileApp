using FreedomVoice.Core.Entities.Texting;
using FreedomVoice.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FreedomVoice.Core.Services
{
    public class ConversationService : IConversationService
    {
        public async Task<ConversationListResponse> GetList(int count, int page)
        {
            throw new NotImplementedException();
        }
    }
}

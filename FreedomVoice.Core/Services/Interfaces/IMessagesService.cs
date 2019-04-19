using FreedomVoice.Core.Entities.Texting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FreedomVoice.Core.Services.Interfaces
{
    public interface IMessagesService
    {
        /// <summary>
        /// Get list of conversations filtered by provided parameters
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="current"></param>
        /// <param name="count"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<MessageListResponse> GetList(long conversationId, DateTime current, int count = 10, int page = 1);
    }
}

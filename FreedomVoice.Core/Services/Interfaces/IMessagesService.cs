using FreedomVoice.Core.Entities.Texting;
using FreedomVoice.Entities.Response;
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

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        Task<SendingResponse<DAL.DbEntities.Conversation>> SendMessage(long conversationId, string text);

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="currentNumber"></param>
        /// <param name="to"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        Task<SendingResponse<DAL.DbEntities.Conversation>> SendMessage(string currentNumber, string to, string text);
    }
}

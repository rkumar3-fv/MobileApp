using FreedomVoice.Core.Entities.Base;
using FreedomVoice.Entities.Request;
using FreedomVoice.Entities.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FreedomVoice.Core.Services.Interfaces
{
    public interface INetworkService
    {
        /// <summary>
        /// Get conversations from API by provided parameters
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="startDate"></param>
        /// <param name="lastUpdateDate"></param>
        /// <param name="start"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<BaseResult<List<Conversation>>> GetConversations(string phone, DateTime startDate, DateTime lastUpdateDate, int start, int limit);

        /// <summary>
        /// Get conversation by current phone and collocutor phone
        /// </summary>
        /// <param name="currentPhone"></param>
        /// <param name="collocutorPhone"></param>
        /// <returns></returns>
        Task<BaseResult<Conversation>> GetConversation(string currentPhone, string collocutorPhone);

        /// <summary>
        /// Get messages from API by provided parameters
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="startDate"></param>
        /// <param name="lastUpdateDate"></param>
        /// <param name="start"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<BaseResult<List<FreedomVoice.Entities.Message>>> GetMessages(long conversationId, DateTime startDate, DateTime lastUpdateDate, int start, int limit);

        /// <summary>
        /// Sending message to API by provided parameter
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Response from API about sending results</returns>
        Task<BaseResult<SendingResponse<Conversation>>> SendMessage(MessageRequest request);

        /// <summary>
        /// Sending push-token to API by provided parameter
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Response from API about sending results</returns>
        Task<BaseResult<bool>> SendPushToken(PushRequest request);
    }
}

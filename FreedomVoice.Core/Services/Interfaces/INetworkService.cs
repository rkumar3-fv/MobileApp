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
        /// <param name="systemPhoneNumber"></param>
        /// <param name="presentationPhoneNumber"></param>
        /// <param name="startDate"></param>
        /// <param name="lastUpdateDate"></param>
        /// <param name="start"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<BaseResult<List<Conversation>>> GetConversations(
            string systemPhoneNumber, 
            string presentationPhoneNumber, 
            DateTime startDate, 
            DateTime lastUpdateDate, 
            int start, 
            int limit);

        /// <summary>
        /// Search conversations from API by provided parameters
        /// </summary>
        /// <param name="systemPhoneNumber"></param>
        /// <param name="presentationPhoneNumber"></param>
        /// <param name="searchConversationRequest"></param>
        /// <returns></returns>
        Task<BaseResult<List<Conversation>>> SearchConversations(
            string systemPhoneNumber, 
            string presentationPhoneNumber, 
            SearchConversationRequest 
            searchConversationRequest);

        /// <summary>
        /// Get conversation by current phone and to phone
        /// </summary>
        /// <param name="systemPhoneNumber"></param>
        /// <param name="presentationPhoneNumber"></param>
        /// <param name="toPhone"></param>
        /// <returns></returns>
        Task<BaseResult<Conversation>> GetConversation(
            string systemPhoneNumber, 
            string presentationPhoneNumber, 
            string toPhone);

        /// <summary>
        /// Get messages from API by provided parameters
        /// </summary>
        /// <param name="systemPhoneNumber"></param>
        /// <param name="presentationPhoneNumber"></param>
        /// <param name="conversationId"></param>
        /// <param name="startDate"></param>
        /// <param name="lastUpdateDate"></param>
        /// <param name="start"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<BaseResult<List<FreedomVoice.Entities.Message>>> GetMessages(
            string systemPhoneNumber, 
            string presentationPhoneNumber, 
            long conversationId, 
            DateTime startDate, 
            DateTime lastUpdateDate, 
            int start, 
            int limit);

        /// <summary>
        /// Sending message to API by provided parameter
        /// </summary>
        /// <param name="systemPhoneNumber"></param>
        /// <param name="presentationPhoneNumber"></param>
        /// <param name="request"></param>
        /// <returns>Response from API about sending results</returns>
        Task<BaseResult<SendingResponse<Conversation>>> SendMessage(
            string systemPhoneNumber, 
            string presentationPhoneNumber, 
            MessageRequest request);

        
        /// <summary>
        /// Sending push-token to API by provided parameter 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="isRegistration"></param>
        /// <returns></returns>
        Task<BaseResult<string>> SendPushToken(string systemPhoneNumber, PushRequest request, bool isRegistration);
    }
}

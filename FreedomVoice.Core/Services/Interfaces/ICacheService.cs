using FreedomVoice.DAL.DbEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreedomVoice.Core.Services.Interfaces
{
    public interface ICacheService
    {
        /// <summary>
        /// Updating conversations cache by recieving entities
        /// </summary>
        /// <param name="conversations"></param>
        void UpdateConversationsCache(IEnumerable<FreedomVoice.Entities.Response.Conversation> conversations);

        /// <summary>
        /// Updating messages cache by provided conversation id and list of messages
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="messages"></param>
        void UpdateMessagesCache(long conversationId, IEnumerable<FreedomVoice.Entities.Message> messages);

        /// <summary>
        /// Updating messages cache by provided conversation and list of messages
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="messages"></param>
        void UpdateMessagesCache(Conversation conversation, IEnumerable<FreedomVoice.Entities.Message> messages);

        /// <summary>
        /// Get list of cached conversations by <paramref name="phone"/> from <paramref name="start"/> position 
        /// with limitation count of entities by <paramref name="limit"/>
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="limit"></param>
        /// <param name="start"></param>
        /// <returns>List of cached conversations</returns>
        IEnumerable<Conversation> GetConversations(string phone, int limit, int start);

        /// <summary>
        /// Get conversation by Id
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns>Conversation from cache</returns>
        Conversation GetConversation(long conversationId);

        /// <summary>
        /// Get list of cached messages for <paramref name="conversation"/> object from <paramref name="start"/> position 
        /// with limitation count of entities by <paramref name="limit"/>
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="limit"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        IEnumerable<Message> GetMessagesByConversation(Conversation conversation, int limit, int start);

        /// <summary>
        /// Get list of cached messages for <paramref name="conversationId"/> id from <paramref name="start"/> position 
        /// with limitation count of entities by <paramref name="limit"/>
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="limit"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        IEnumerable<Message> GetMessagesByConversation(long conversationId, int limit, int start);

        /// <summary>
        /// Get last modified conversation date by <paramref name="startTime"/>
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        DateTime GetLastConversationUpdateDate(DateTime startTime);
        
        /// <summary>
        /// One message by Id and conversation Id
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        Message GetMessageBy(long conversationId, long messageId);
    }
}

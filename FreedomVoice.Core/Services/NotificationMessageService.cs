using System;
using System.Collections.Generic;
using System.Linq;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.DAL.DbEntities;

namespace FreedomVoice.Core.Services
{

    public class MessageEventArg : EventArgs
    {
        public Message Message { get; set; }
    } 
    
    public class ConversationEventArg : EventArgs
    {
        public Conversation Conversation { get; set; }
    } 
    
    public class NotificationMessageService
    {
        public event EventHandler<ConversationEventArg> NewMessageEventHandler;
        public event EventHandler<MessageEventArg> MessageUpdatedHandler;
        
        private static NotificationMessageService _instance;
        private readonly ICacheService _cacheService;

        private NotificationMessageService()
        {
            _cacheService = ServiceContainer.Resolve<ICacheService>();
        }
        
        public static NotificationMessageService Instance()
        {
            if (_instance != null) return _instance;
            _instance = new NotificationMessageService();
            return _instance;
        }
        

        public enum NotificationType
        {   
            /// <summary>
            /// Update message (status, text, etc)
            /// </summary>
            Update,
            /// <summary>
            /// New incoming message
            /// </summary>
            Incoming
        }

        public void ReceivedNotification(NotificationType type, FreedomVoice.Entities.Response.Conversation model)
        {
            var savedMessage = _saveMessage(model);
            switch (type)
            {
                case NotificationType.Incoming:
                    var savedConversation = _saveConversation(model);
                    NewMessageEventHandler?.Invoke(this,
                        new ConversationEventArg {Conversation = savedConversation});
                    break;
                case NotificationType.Update:
                    break;
            }
            MessageUpdatedHandler?.Invoke(this, new MessageEventArg {Message = savedMessage});
        }

        private Conversation _saveConversation(FreedomVoice.Entities.Response.Conversation conversation)
        {
            var list = new[] {conversation};
            _cacheService.UpdateConversationsCache(list);
            return _cacheService.GetConversation(conversation.Id);
        }
        
        private Message _saveMessage(FreedomVoice.Entities.Response.Conversation conversation)
        {
            _cacheService.UpdateMessagesCache(conversation.Id, conversation.Messages);
            return _cacheService.GetMessageBy(conversation.Id, conversation.Messages.First().Id);
        }
    }
}
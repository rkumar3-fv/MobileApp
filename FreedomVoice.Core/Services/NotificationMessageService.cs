using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.DAL.DbEntities;
using FreedomVoice.Entities.Enums;

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

        public async Task ReceivedNotification(PushType type, FreedomVoice.Entities.Response.Conversation model)
        {
            var savedConversation = await _saveConversation(model);
            switch (type)
            {
                case PushType.NewMessage:
                    NewMessageEventHandler?.Invoke(this, new ConversationEventArg { Conversation = savedConversation });
                    break;
                case PushType.StatusChanged:
                    var savedMessage = savedConversation.Messages.LastOrDefault();
                    savedMessage.Conversation = savedConversation;
                    MessageUpdatedHandler?.Invoke(this, new MessageEventArg {Message = savedMessage});
                    break;
            }
        }

        private async Task<Conversation> _saveConversation(FreedomVoice.Entities.Response.Conversation conversation)
        {
            var list = new[] {conversation};
            await _cacheService.UpdateConversationsCache(list);
            var saveConversation = await _cacheService.GetConversation(conversation.Id);
            return saveConversation;
        }
        
        private async Task<Message> _saveMessage(FreedomVoice.Entities.Response.Conversation conversation)
        {
            await _cacheService.UpdateMessagesCache(conversation.Id, conversation.Messages);
            return await _cacheService.GetMessageBy(conversation.Id, conversation.Messages.First().Id);
        }
    }
}
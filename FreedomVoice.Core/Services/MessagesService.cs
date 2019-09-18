using AutoMapper;
using FreedomVoice.Core.Entities.Texting;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.DAL;
using FreedomVoice.DAL.DbEntities;
using FreedomVoice.Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreedomVoice.Core.Services
{
    public class MessageService : IMessagesService
    {
        private readonly ICacheService _cacheService;
        private readonly INetworkService _networkService;
        private readonly IMapper _mapper;
        private readonly IConversationService _conversationService;

        public MessageService(ICacheService cacheService, INetworkService networkService, IMapper mapper, IConversationService conversationService)
        {
            _cacheService = cacheService;
            _networkService = networkService;
            _mapper = mapper;
            _conversationService = conversationService;
        }

        public async Task<MessageListResponse> GetList(
            string systemPhoneNumber, 
            string presentationPhoneNumber, 
            long conversationId, 
            DateTime current, 
            int count = 10, 
            int page = 1)
        {
            if (page <= 0) throw new ArgumentException(nameof(page));
            if (count <= 0) throw new ArgumentException(nameof(count));

            var result = new MessageListResponse();
            var lastSyncDate = await _cacheService.GetLastConversationUpdateDate(current);
            var start = count * (page - 1);
            var netMessages = await _networkService.GetMessages(systemPhoneNumber, presentationPhoneNumber, conversationId, current, lastSyncDate, start, count);
            result.ResponseCode = netMessages.Code;
            result.Message = netMessages.ErrorText;
            result.Messages = netMessages.Result.Select(x => _mapper.Map<Message>(x));
            result.IsEnd = netMessages.Result == null || netMessages.Result.Count < count;
            return result;
        }

        public async Task<SendingResponse<DAL.DbEntities.Conversation>> SendMessage(string systemPhoneNumber, long conversationId, string text)
        {
            var conversation = await _cacheService.GetConversation(conversationId);
            if (conversation == null)
                throw new ArgumentException("Conversation not found");
            var res = await SendMessage(systemPhoneNumber, conversation.SystemPhone.PhoneNumber, conversation.ToPhone.PhoneNumber, text);
            return res;
        }

        public async Task<SendingResponse<DAL.DbEntities.Conversation>> SendMessage(
            string systemPhoneNumber, 
            string presentationPhoneNumber, 
            string toPhone, 
            string text)
        {
            var sendingResult = await _networkService.SendMessage(systemPhoneNumber, presentationPhoneNumber, 
                new FreedomVoice.Entities.Request.MessageRequest { From = systemPhoneNumber, To = toPhone, Text = text });
            if (sendingResult.Code != Entities.Enums.ErrorCodes.Ok || sendingResult.Result == null)
                return new SendingResponse<DAL.DbEntities.Conversation> { ErrorMessage = sendingResult.ErrorText, State = FreedomVoice.Entities.Enums.SendingState.Error };
            return new SendingResponse<DAL.DbEntities.Conversation>
            {
                ErrorMessage = sendingResult.Result.ErrorMessage,
                State = sendingResult.Result.State,
                Entity = _mapper.Map<DAL.DbEntities.Conversation>(sendingResult.Result.Entity)
            };
        }

        public async Task UpdateMessageReadStatus(string systemPhone, long conversationId)
        {
            var conversation = await _cacheService.GetConversation(conversationId);
            if (conversation == null)
                throw new ArgumentException("Conversation not found");
            await _networkService.UpdateReadMessageStatus(systemPhone, conversation.SystemPhone.PhoneNumber, conversationId);
        }
    }
}

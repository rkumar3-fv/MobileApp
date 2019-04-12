using AutoMapper;
using FreedomVoice.Core.Entities.Texting;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.DAL;
using FreedomVoice.DAL.DbEntities;
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

        public MessageService(ICacheService cacheService, INetworkService networkService, IMapper mapper)
        {
            _cacheService = cacheService;
            _networkService = networkService;
            _mapper = mapper;
        }

        public async Task<MessageListResponse> GetList(int conversationId, DateTime current, int count = 10, int page = 1)
        {
            if (page <= 0) throw new ArgumentException(nameof(page));
            if (count <= 0) throw new ArgumentException(nameof(count));

            var result = new MessageListResponse();
            var lastSyncDate = _cacheService.GetLastConversationUpdateDate(current);
            var netMessages = await _networkService.GetMessages(conversationId, current, lastSyncDate, count, page);
            result.ResponseCode = netMessages.Code;
            result.Message = netMessages.ErrorText;
            result.Messages = netMessages.Result.Select(x => _mapper.Map<Message>(x));
            result.IsEnd = netMessages.Result != null ? netMessages.Result.Count < count : true;
            return result;
        }
    }
}

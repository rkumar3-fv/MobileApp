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
    public class ConversationService : IConversationService
    {
        private readonly ICacheService _cacheService;
        private readonly INetworkService _networkService;
        private readonly IMapper _mapper;

        public ConversationService(ICacheService cacheService, INetworkService networkService, IMapper mapper)
        {
            _cacheService = cacheService;
            _networkService = networkService;
            _mapper = mapper;
        }

        public async Task<ConversationListResponse> GetList(string phone, DateTime current, int count = 10, int page = 1)
        {
            if (page <= 0) throw new ArgumentException(nameof(page));
            if (count <= 0) throw new ArgumentException(nameof(count));

            var result = new ConversationListResponse();
            var lastSyncDate = _cacheService.GetLastConversationUpdateDate(current);
            var start = count * (page - 1);
            var netConversations = await _networkService.GetConversations(phone, current, lastSyncDate, start, count);
            result.ResponseCode = netConversations.Code;
            result.Message = netConversations.ErrorText;
            result.Conversations = netConversations.Result.Select(x => _mapper.Map<Conversation>(x));
            result.IsEnd = netConversations.Result == null || netConversations.Result.Count < count;
            return result;
        }

        public async Task<ConversationResponse> Get(string currentPhone, string toPhone)
        {
            var result = new ConversationResponse();
            var netConversation = await _networkService.GetConversation(currentPhone, toPhone);
            result.ResponseCode = netConversation.Code;
            result.Message = netConversation.ErrorText;
            result.Conversation = _mapper.Map<Conversation>(netConversation.Result);
            return result;
        }
    }
}

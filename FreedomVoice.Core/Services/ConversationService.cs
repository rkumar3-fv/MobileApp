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
using FreedomVoice.Entities.Request;

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

        public async Task<ConversationListResponse> GetList(string phone, DateTime current, int count = 10, int page = 1, string systemPhone = null, string query = null,
            string[] foundInNumbers = null)
        {
            if (page <= 0) throw new ArgumentException(nameof(page));
            if (count <= 0) throw new ArgumentException(nameof(count));
            var result = new ConversationListResponse();
            var lastSyncDate = _cacheService.GetLastConversationUpdateDate(current);
            var start = count * (page - 1);
            ConversationRequest search = null;
            if (!string.IsNullOrEmpty(query) && !string.IsNullOrEmpty(systemPhone))
            {
                search = new ConversationRequest
                {
                    Telnumbers = foundInNumbers,
                    SystemNumber = systemPhone,
                    Text = query
                };
            }
            var netConversations = await _networkService.GetConversations(phone, current, lastSyncDate, start, count, search);
            result.ResponseCode = netConversations.Code;
            result.Message = netConversations.ErrorText;
            result.Conversations = netConversations.Result.Select(x => _mapper.Map<Conversation>(x));
            result.IsEnd = netConversations.Result == null || netConversations.Result.Count < count;
            return result;
        }

        public async Task<ConversationResponse> Get(string currentPhone, string collocutorPhone)
        {
            var result = new ConversationResponse();
            var netConversation = await _networkService.GetConversation(currentPhone, collocutorPhone);
            result.ResponseCode = netConversation.Code;
            result.Message = netConversation.ErrorText;
            result.Conversation = _mapper.Map<Conversation>(netConversation.Result);
            return result;
        }
    }
}

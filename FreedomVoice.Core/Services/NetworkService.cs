using AutoMapper;
using FreedomVoice.Core.Entities.Base;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.DAL;
using FreedomVoice.Entities;
using FreedomVoice.Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreedomVoice.Core.Services
{
    public class NetworkService : INetworkService
    {
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public NetworkService(ICacheService cacheService, IMapper mapper)
        {
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<BaseResult<List<Conversation>>> GetConversations(string phone, DateTime startDate, DateTime lastUpdateDate, int start, int limit)
        {
            try
            {
                BaseResult<List<Conversation>> result = await ApiHelper.GetConversations(phone, startDate, lastUpdateDate, start, limit);
                if (result.Result == null)
                    result.Result = _cacheService.GetConversations(phone, limit, start).Select(x => _mapper.Map<Conversation>(x)).ToList();
                if (result.Code == Entities.Enums.ErrorCodes.Ok)
                    _cacheService.UpdateConversationsCache(result.Result);

                result.Result = result.Result.Where(x => !x.IsRemoved).ToList();
                return result;
            }
            catch (Exception)
            {
                return new BaseResult<List<Conversation>>()
                {
                    Code = Entities.Enums.ErrorCodes.ConnectionLost,
                    Result = _cacheService.GetConversations(phone, limit, start).Select(x => _mapper.Map<Conversation>(x)).ToList()
                };
            }
        }

        public async Task<BaseResult<List<Message>>> GetMessages(int conversationId, DateTime startDate, DateTime lastUpdateDate, int start, int limit)
        {
            try
            {
                BaseResult<List<Message>> result = await ApiHelper.GetMessages(conversationId, startDate, lastUpdateDate, start, limit);
                if (result.Result == null)
                    result.Result = _cacheService.GetMessagesByConversation(conversationId, limit, start).Select(x => _mapper.Map<Message>(x)).ToList();
                if (result.Code == Entities.Enums.ErrorCodes.Ok)
                    _cacheService.UpdateMessagesCache(conversationId, result.Result);

                result.Result = result.Result.ToList();
                return result;
            }
            catch (Exception)
            {
                return new BaseResult<List<Message>>()
                {
                    Code = Entities.Enums.ErrorCodes.ConnectionLost,
                    Result = _cacheService.GetMessagesByConversation(conversationId, limit, start).Select(x => _mapper.Map<Message>(x)).ToList()
                };
            }
        }
    }
}

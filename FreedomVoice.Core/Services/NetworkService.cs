using AutoMapper;
using FreedomVoice.Core.Entities.Base;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Entities;
using FreedomVoice.Entities.Request;
using FreedomVoice.Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
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
                BaseResult<List<Conversation>> result = await ApiHelper.GetConversations(new ConversationsRequest
                {
                    PhoneNumber = phone,
                    From = startDate.Ticks,
                    To = lastUpdateDate.Ticks,
                    Start = start,
                    Limit = limit
                });

                if (result.Result == null)
                {
                    var messages = await _cacheService.GetConversations(phone, limit, start);
                    result.Result = messages.Select(x => _mapper.Map<Conversation>(x)).ToList();
                }
                if (result.Code == Entities.Enums.ErrorCodes.Ok) 
                    _cacheService.UpdateConversationsCache(result.Result);

                result.Result = result.Result.Where(x => !x.IsRemoved).ToList();
                return result;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"GetConversations (phone: {phone}, startDate: {startDate}, lastUpdateDate: {lastUpdateDate}, start: {start}, limit: {lastUpdateDate})" +
                                  $" has been finished failed with error:\t\n{exception}");
                var messages = await _cacheService.GetConversations(phone, limit, start);
                return new BaseResult<List<Conversation>>
                {
                    Code = Entities.Enums.ErrorCodes.ConnectionLost,
                    Result = messages.Select(x => _mapper.Map<Conversation>(x)).ToList()
                };
            }
        }

        public async Task<BaseResult<List<Conversation>>> SearchConversations(SearchConversationRequest searchConversationRequest)
        {
            try
            {
                BaseResult<List<Conversation>> result = await ApiHelper.SearchConversations(searchConversationRequest);
                result.Result = result.Result.Where(x => !x.IsRemoved).ToList();
                return result;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"SearchConversations has been finished failed with error:\t\n {exception}");
                return new BaseResult<List<Conversation>>
                {
                    Code = Entities.Enums.ErrorCodes.ConnectionLost,
                    Result = null
                };
            }
        }

        public async Task<BaseResult<Conversation>> GetConversation(string systemPhone, string toPhone)
        {
            try
            {
                BaseResult<Conversation> result = await ApiHelper.GetConversation(new ConversationRequest
                {
                    ToPhone = toPhone,
                    SystemPhone = systemPhone
                });
                
                if (result.Code == Entities.Enums.ErrorCodes.Ok && result.Result != null)
                    _cacheService.UpdateConversationsCache(new[] { result.Result });

                return result;
            }
            catch (Exception)
            {
                return new BaseResult<Conversation>
                {
                    Code = Entities.Enums.ErrorCodes.ConnectionLost,
                    Result = null
                };
            }
        }

        public async Task<BaseResult<List<Message>>> GetMessages(long conversationId, DateTime startDate, DateTime lastUpdateDate, int start, int limit)
        {
            try
            {
                BaseResult<List<Message>> result = await ApiHelper.GetMessages(
                    new MessagesRequest()
                    {
                        ConversationId = conversationId,
                        From = startDate.Ticks,
                        To = lastUpdateDate.Ticks,
                        Start = start,
                        Limit = limit
                    });

                if (result.Result == null)
                {
                    var messages = await _cacheService.GetMessagesByConversation(conversationId, limit, start);
                    result.Result = messages.Select(x => _mapper.Map<Message>(x)).ToList();
                }
                if (result.Code == Entities.Enums.ErrorCodes.Ok)
                    _cacheService.UpdateMessagesCache(conversationId, result.Result);

                result.Result = result.Result.ToList();
                return result;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"GetMessages (conversationId: {conversationId}, startDate: {startDate}, lastUpdateDate: {lastUpdateDate}, start: {start}, limit: {lastUpdateDate})" +
                                  $" has been finished failed with error:\t\n{exception}");
                var messages = await _cacheService.GetMessagesByConversation(conversationId, limit, start);
                return new BaseResult<List<Message>>
                {
                    Code = Entities.Enums.ErrorCodes.ConnectionLost,
                    Result = messages.Select(x => _mapper.Map<Message>(x)).ToList()
                };
            }
        }

        public async Task<BaseResult<SendingResponse<Conversation>>> SendMessage(MessageRequest request)
        {
            var result = await ApiHelper.SendMessage(request);
            if(result.Result != null && result.Result.Entity != null)
                _cacheService.UpdateConversationsCache(new[] { result.Result.Entity });

            return result;
        }

        public async Task<BaseResult<string>> SendPushToken(PushRequest request, bool isRegistration)
        {
            var result = await ApiHelper.SendPushToken(request, isRegistration: isRegistration);
            //if (result.Result != null && result.Result.Entity != null)
                //_cacheService.UpdateConversationsCache(new[] { result.Result.Entity });

            return result;
        }
    }
}

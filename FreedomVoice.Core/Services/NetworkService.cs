using AutoMapper;
using FreedomVoice.Core.Entities.Base;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Entities;
using FreedomVoice.Entities.Request;
using FreedomVoice.Entities.Request.Weblink;
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
        
        public async Task<BaseResult<List<Conversation>>> GetConversations(
            string systemPhoneNumber, 
            string presentationPhoneNumber, 
            DateTime startDate, 
            DateTime lastUpdateDate, 
            int start, 
            int limit)
        {
            try
            {
                BaseResult<List<Conversation>> result = await ApiHelper.GetConversations(
                    systemPhoneNumber, 
                    presentationPhoneNumber, 
                    new FrameRequest
                    {
                        From = startDate.Ticks,
                        To = lastUpdateDate.Ticks,
                        Start = start,
                        Limit = limit
                    });

                if (result.Result == null)
                {
                    var messages = await _cacheService.GetConversations(systemPhoneNumber, limit, start);
                    result.Result = messages.Select(x => _mapper.Map<Conversation>(x)).ToList();
                }
                if (result.Code == Entities.Enums.ErrorCodes.Ok) 
                    await _cacheService.UpdateConversationsCache(result.Result);
                result.Result = result.Result.Where(x => !x.IsRemoved).ToList();
                return result;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"GetConversations (systemPhone: {systemPhoneNumber}, startDate: {startDate}, lastUpdateDate: {lastUpdateDate}, start: {start}, limit: {lastUpdateDate})" +
                                  $" has been finished failed with error:\t\n{exception}");
                var messages = await _cacheService.GetConversations(systemPhoneNumber, limit, start);
                return new BaseResult<List<Conversation>>
                {
                    Code = Entities.Enums.ErrorCodes.ConnectionLost,
                    Result = messages.Select(x => _mapper.Map<Conversation>(x)).ToList()
                };
            }
        }

        public async Task<BaseResult<List<Conversation>>> SearchConversations(
            string systemPhoneNumber,
            string presentationPhoneNumber, 
            SearchConversationRequest searchConversationRequest)
        {
            try
            {
                BaseResult<List<Conversation>> result = await ApiHelper.SearchConversations(
                    systemPhoneNumber, 
                    presentationPhoneNumber, 
                    searchConversationRequest);
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

        public async Task<BaseResult<Conversation>> GetConversation(
            string systemPhoneNumber,
            string presentationPhoneNumber, 
            string toPhone)
        {
            try
            {
                BaseResult<Conversation> result = await ApiHelper.GetConversation(
                    systemPhoneNumber, 
                    presentationPhoneNumber, 
                    new ConversationRequest
                    {
                        ToPhone = toPhone
                    });
                
                if (result.Code == Entities.Enums.ErrorCodes.Ok && result.Result != null)
                    await _cacheService.UpdateConversationsCache(new[] { result.Result });

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

        public async Task<BaseResult<List<Message>>> GetMessages(
            string systemPhoneNumber,
            string presentationPhoneNumber, 
            long conversationId, 
            DateTime startDate, 
            DateTime lastUpdateDate, 
            int start, 
            int limit)
        {
            try
            {
                BaseResult<List<Message>> result = await ApiHelper.GetMessages(
                    systemPhoneNumber,
                    presentationPhoneNumber,
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
                    await _cacheService.UpdateMessagesCache(conversationId, result.Result);

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

        public async Task<BaseResult<SendingResponse<Conversation>>> SendMessage(
            string systemPhoneNumber,
            string presentationPhoneNumber, 
            MessageRequest request)
        {
            var result = await ApiHelper.SendMessage(systemPhoneNumber, presentationPhoneNumber, request);
            if(result.Result != null && result.Result.Entity != null)
                await _cacheService.UpdateConversationsCache(new[] { result.Result.Entity });

            return result;
        }

        public async Task<BaseResult<string>> SendPushToken(string systemPhoneNumber, PushRequest request, bool isRegistration)
        {
            return await ApiHelper.SendPushToken(systemPhoneNumber, request, isRegistration: isRegistration);
        }
    }
}

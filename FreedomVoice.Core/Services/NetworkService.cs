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
                var result = await ApiHelper.GetConversations(phone, startDate, lastUpdateDate, start, limit);
                //var result = await GenerateDummyConversationsResult(phone);
                if (result.Result == null)
                    result.Result = _cacheService.GetConversations(phone, limit, start).Select(x => _mapper.Map<Conversation>(x)).ToList(); //new List<Conversation>();
                if (result.Code == Entities.Enums.ErrorCodes.Ok)
                    _cacheService.UpdateConversationsCache(result.Result);

                result.Result = result.Result.Where(x => !x.IsRemoved).ToList();
                return result;
            }
            catch (Exception ex)
            {
                return new BaseResult<List<Conversation>>()
                {
                    Code = Entities.Enums.ErrorCodes.ConnectionLost,
                    Result = _cacheService.GetConversations(phone, limit, start).Select(x => _mapper.Map<Conversation>(x)).ToList()
                };
            }
        }

        private async Task<BaseResult<List<Conversation>>> GenerateDummyConversationsResult(string phone)
        {
            await Task.Delay(2000);
            var currentphone = new Phone
            {
                Id = 1,
                PhoneNumber = phone
            };
            var tophone1 = new Phone
            {
                Id = 2,
                PhoneNumber = "8885551212"
            };
            var tophone2 = new Phone
            {
                Id = 3,
                PhoneNumber = "5555228243"
            };
            var tophone3 = new Phone
            {
                Id = 4,
                PhoneNumber = "5555228241"
            };
            return new BaseResult<List<Conversation>>
            {
                Code = Entities.Enums.ErrorCodes.Ok,
                Result = new List<Conversation>
                {
                    new Conversation {
                        CurrentPhone = currentphone,
                        CollocutorPhone = tophone1,
                        Id = 1,
                        Messages = new List<Message>() {
                            new Message()
                            {
                                Id = 1,
                                Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                                From = tophone1,
                                To = currentphone,
                                ReceivedAt = DateTime.Now.AddSeconds(-1),
                                ReadAt = null,
                                SentAt = DateTime.Now.AddSeconds(-1)

                            },
                            new Message()
                            {
                                Id = 2,
                                Text = "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                                From = tophone1,
                                To = currentphone,
                                SentAt = DateTime.Now.AddMinutes(-10)
                            },
                            new Message()
                            {
                                Id = 3,
                                Text = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.",
                                From = currentphone,
                                To = tophone1,
                                SentAt = DateTime.Now.AddDays(-3)
                            }
                        }
                    },
                    new Conversation {
                        CurrentPhone = currentphone,
                        CollocutorPhone = tophone2,
                        Id = 2,
                        Messages = new List<Message>() {
                            new Message()
                            {
                                Id = 4,
                                Text = "I have a text field where the user types in a telephone number. When the user is done typing the number and the text field loses its focus I need to take whatever the user typed and format it to a standard format. i.e. ###-###-####",
                                To = tophone2,
                                From = currentphone,
                                SentAt = DateTime.Now.AddDays(-3)
                            }
                        }
                    },
                    new Conversation {
                        CurrentPhone = currentphone,
                        CollocutorPhone = tophone3,
                        Id = 3,
                        Messages = new List<Message>() {
                            new Message()
                            {
                                Id = 5,
                                Text = "This is how to format a string of numbers into a proper phone number format. For example say a user types in the following for a phone number: 1234567890. The following code will take that value and turn it into this: 123-456-7890",
                                To = tophone3,
                                From = currentphone,
                                SentAt = DateTime.Now.AddDays(-10)
                            }
                        }
                    }
                }
            };
        }
    }
}

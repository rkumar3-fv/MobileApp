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

        public NetworkService(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<BaseResult<List<Conversation>>> GetConversations(string phone, DateTime startDate, DateTime lastUpdateDate, int start, int limit)
        {
            //var result = await ApiHelper.GetConversations(phone, startDate, lastUpdateDate, start, limit);
            var result = await GenerateDummyConversationsResult(phone);

            if (result.Code == Entities.Enums.ErrorCodes.Ok)
                _cacheService.UpdateConversationsCache(result.Result);

            return result;
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
                                Text = "test",
                                From = tophone1,
                                To = currentphone,
                                ReceivedAt = new DateTime(),
                                ReadAt = new DateTime(),
                                SentAt = new DateTime()
                            },
                            new Message()
                            {
                                Id = 2,
                                Text = "2test",
                                From = tophone1,
                                To = currentphone,
                                ReceivedAt = new DateTime(),
                                ReadAt = new DateTime(),
                                SentAt = new DateTime()
                            },
                            new Message()
                            {
                                Id = 3,
                                Text = "3test",
                                From = currentphone,
                                To = tophone1,
                                ReceivedAt = new DateTime(),
                                ReadAt = new DateTime(),
                                SentAt = new DateTime()
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
                                Text = "test2",
                                To = tophone2,
                                From = currentphone,
                                SentAt = new DateTime()
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
                                Text = "test2",
                                To = tophone3,
                                From = currentphone,
                                SentAt = new DateTime()
                            }
                        }
                    }
                }
            };
        }
    }
}

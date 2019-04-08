using FreedomVoice.Core.Entities.Texting;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.DAL.DbEntities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FreedomVoice.Core.Services
{
    public class ConversationService : IConversationService
    {
        public async Task<ConversationListResponse> GetList(string phone, int count, int page)
        {
            await Task.Delay(2000);
            return new ConversationListResponse
            {
                IsEnd = true,
                Conversations = new List<Conversation>()
                {
                    new Conversation {
                        CurrentPhone = new Phone {
                            PhoneNumber = phone
                        },
                        CollocutorPhone = new Phone
                        {
                            PhoneNumber = "8885551212"
                        },
                        Id = 1,
                        Messages = new List<Message>() {
                            new Message()
                            {
                                Text = "test",
                                From = new Phone
                                {
                                    PhoneNumber = "8885551212"
                                },
                                To = new Phone {
                                    PhoneNumber = phone
                                },
                                ReceivedAt = new DateTime(),
                                ReadAt = new DateTime(),
                                SentAt = new DateTime()
                            }
                        }
                    },
                    new Conversation {
                        CurrentPhone = new Phone {
                            PhoneNumber = phone
                        },
                        CollocutorPhone = new Phone
                        {
                            PhoneNumber = "5555228243"
                        },
                        Id = 1,
                        Messages = new List<Message>() {
                            new Message()
                            {
                                Text = "test2",
                                To = new Phone
                                {
                                    PhoneNumber = "5555228243"
                                },
                                From = new Phone {
                                    PhoneNumber = phone
                                },
                                SentAt = new DateTime()
                            }
                        }
                    },
                    new Conversation {
                        CurrentPhone = new Phone {
                            PhoneNumber = phone
                        },
                        CollocutorPhone = new Phone
                        {
                            PhoneNumber = "5555228241"
                        },
                        Id = 1,
                        Messages = new List<Message>() {
                            new Message()
                            {
                                Text = "test2",
                                To = new Phone
                                {
                                    PhoneNumber = "5555228243"
                                },
                                From = new Phone {
                                    PhoneNumber = phone
                                },
                                SentAt = new DateTime()
                            }
                        }
                    }

                }

            };

        }
    }
}

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
                Conversations = new List<Conversation>
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
                        Messages = new List<Message>
                        {
                            new Message
                            {
                                Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
                                From = new Phone
                                {
                                    PhoneNumber = "8885551212"
                                },
                                To = new Phone {
                                    PhoneNumber = phone
                                },
                                ReceivedAt = DateTime.Now.AddSeconds(-1),
                                ReadAt = null,
                                SentAt = DateTime.Now.AddSeconds(-1)
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
                        Messages = new List<Message>
                        {
                            new Message
                            {
                                Text = "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                                To = new Phone
                                {
                                    PhoneNumber = "5555228243"
                                },
                                From = new Phone {
                                    PhoneNumber = phone
                                },
                                SentAt = DateTime.Now.AddMinutes(-10)
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
                        Messages = new List<Message>
                        {
                            new Message
                            {
                                Text = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.",
                                To = new Phone
                                {
                                    PhoneNumber = "5555228243"
                                },
                                From = new Phone {
                                    PhoneNumber = phone
                                },
                                SentAt = DateTime.Now.AddDays(-3)
                            }
                        }
                    }

                }

            };

        }
    }
}

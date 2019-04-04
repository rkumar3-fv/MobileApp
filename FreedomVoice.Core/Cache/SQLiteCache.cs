using FreedomVoice.DAL;
using FreedomVoice.DAL.DbEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreedomVoice.Core.Cache
{
    public class SQLiteCache
    {
        private FreedomVoiceContext _freedomVoiceContext;

        public SQLiteCache(string dbPath)
        {
            _freedomVoiceContext = new FreedomVoiceContext(dbPath);
            _freedomVoiceContext.Database.EnsureCreated();
            if (_freedomVoiceContext.Conversations.Count() == 0)
            {
                var phone1 = new Phone { Id = 1, PhoneNumber = "1111111111" };
                var phone2 = new Phone { Id = 2, PhoneNumber = "2222222222" };
                var phone3 = new Phone { Id = 3, PhoneNumber = "3333333333" };
                var phone4 = new Phone { Id = 4, PhoneNumber = "4444444444" };

                _freedomVoiceContext.Conversations.Add(new Conversation
                {
                    Id = 1,
                    CurrentPhone = phone1,
                    CollocutorPhone = phone2,
                    Messages = new List<Message>
                    {
                        new Message
                        {
                            Id = 1,
                            Text = "Conversation 1 Test 1",
                            ReadAt = DateTime.Now,
                            ReceivedAt = DateTime.Now,
                            SentAt = DateTime.Now,
                            From = phone1,
                            To = phone2
                        },
                        new Message
                        {
                            Id = 2,
                            Text = "Conversation 1 Test 2",
                            ReadAt = DateTime.Now,
                            ReceivedAt = DateTime.Now,
                            SentAt = DateTime.Now,
                            From = phone2,
                            To = phone1
                        }
                    }
                });
                _freedomVoiceContext.Conversations.Add(new Conversation
                {
                    Id = 2,
                    CurrentPhone = phone3,
                    CollocutorPhone = phone4,
                    Messages = new List<Message>
                    {
                        new Message
                        {
                            Id = 3,
                            Text = "Conversation 2 Test 3",
                            ReadAt = DateTime.Now,
                            ReceivedAt = DateTime.Now,
                            SentAt = DateTime.Now,
                            From = phone3,
                            To = phone4
                        },
                        new Message
                        {
                            Id = 4,
                            Text = "Conversation 2 Test 4",
                            ReadAt = DateTime.Now,
                            ReceivedAt = DateTime.Now,
                            SentAt = DateTime.Now,
                            From = phone4,
                            To = phone3
                        }
                    }
                });
                _freedomVoiceContext.SaveChanges();
            }
        }

        public Conversation GetConversationById(int id)
        {
            return _freedomVoiceContext.Conversations.Include(x => x.CurrentPhone).Include(x => x.CollocutorPhone).Include(x => x.Messages).FirstOrDefault(x => x.Id == id);
        }
    }
}

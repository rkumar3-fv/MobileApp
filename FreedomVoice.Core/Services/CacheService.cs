using AutoMapper;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.DAL;
using FreedomVoice.DAL.DbEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreedomVoice.DAL.DbEntities.Enums;

namespace FreedomVoice.Core.Services
{
    public class CacheService : ICacheService
    {
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IRepository<Message> _messagesRepository;
        private readonly IRepository<Phone> _phoneRepository;
        private readonly IMapper _mapper;
        readonly object _objLock = new object();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="conversationRepository"></param>
        /// <param name="messagesRepository"></param>
        /// <param name="phoneRepository"></param>
        /// <param name="mapper"></param>
        public CacheService(IRepository<Conversation> conversationRepository, IRepository<Message> messagesRepository, IRepository<Phone> phoneRepository, IMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _messagesRepository = messagesRepository;
            _phoneRepository = phoneRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Updating messages cache without saving any entities to cache
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="messages"></param>
        /// <param name="alreadyCreatedPhones"></param>
        private void UpdateMessagesCacheWithoutSaving(Conversation conversation, IEnumerable<FreedomVoice.Entities.Message> messages, List<Phone> alreadyCreatedPhones)
        {
            if (conversation == null)
            {
                Console.WriteLine($"CacheService has been finished failed without saving because conversation does not exists for messages: ${messages}");
                return;
            }
            if (conversation.Messages == null)
                conversation.Messages = new List<Message>();
            // Removing - we cannot to remove messages

            var messagesForUpdate = _messagesRepository.Table.Where(messageFromRepository => messages.Any(message => message.Id == messageFromRepository.Id));
            foreach (var messageFromApi in messages)
            {
                var message = messagesForUpdate.FirstOrDefault(messageForUpdate => messageForUpdate.Id == messageFromApi.Id);
                // Adding
                if (message == null)
                {
                    var newMessage = _mapper.Map<Message>(messageFromApi);
                    newMessage.Conversation = conversation;
                    conversation.Messages.Add(newMessage);
                }
                // Updating
                else
                {
                    message.Conversation = conversation;
                    message.ReadAt = messageFromApi.ReadAt;
                    message.State = (SendingState) messageFromApi.State;
                }
            }

            UpdatePhones(conversation, alreadyCreatedPhones);
        }

        private Conversation UpdatePhones(Conversation conversation, List<Phone> alreadyCreatedPhones)
        {
            if(conversation.SystemPhone != null)
            {
                conversation.SystemPhone = _phoneRepository.Table.FirstOrDefault(phone => phone.Id == conversation.SystemPhone.Id) ??
                    alreadyCreatedPhones.FirstOrDefault(phone => phone.Id == conversation.SystemPhone.Id) ?? conversation.SystemPhone;

                if (alreadyCreatedPhones.All(phone => phone.Id != conversation.SystemPhone.Id))
                    alreadyCreatedPhones.Add(conversation.SystemPhone);
            }
            if (conversation.ToPhone != null)
            {
                conversation.ToPhone = _phoneRepository.Table.FirstOrDefault(phone => phone.Id == conversation.ToPhone.Id) ??
                    alreadyCreatedPhones.FirstOrDefault(phone => phone.Id == conversation.ToPhone.Id) ?? conversation.ToPhone;
                if (alreadyCreatedPhones.All(phone => phone.Id != conversation.ToPhone.Id))
                    alreadyCreatedPhones.Add(conversation.ToPhone);
            }

            foreach (var message in conversation.Messages)
            {
                if (message.From != null)
                {
                    message.From = _phoneRepository.Table.FirstOrDefault(phone => phone.Id == message.From.Id) ??
                    alreadyCreatedPhones.FirstOrDefault(phone => phone.Id == message.From.Id) ?? message.From;
                    if (alreadyCreatedPhones.All(phone => phone.Id != message.From.Id))
                        alreadyCreatedPhones.Add(message.From);
                }

                if (message.To == null) continue;
                {
                    message.To = _phoneRepository.Table.FirstOrDefault(phone => phone.Id == message.To.Id) ??
                                 alreadyCreatedPhones.FirstOrDefault(phone => phone.Id == message.To.Id) ?? message.To;
                    if (alreadyCreatedPhones.All(phone => phone.Id != message.To.Id))
                        alreadyCreatedPhones.Add(message.To);
                }
            }

            return conversation;
        }

        /// <summary>
        /// Updating conversations cache by recieving entities
        /// </summary>
        /// <param name="conversations"></param>
        public void UpdateConversationsCache(IEnumerable<FreedomVoice.Entities.Response.Conversation> conversations)
        {
            lock (_objLock)
            {
                var usedPhones = new List<Phone>();
                foreach (var conversation in conversations)
                {
                    var cachedConversation = _conversationRepository.Table.Include(row => row.Messages)
                        .Include(row => row.SystemPhone)
                        .Include(row => row.ToPhone)
                        .FirstOrDefault(row => conversation.Id == row.Id);
                    // Adding
                    if (cachedConversation == null && !conversation.IsRemoved)
                        _conversationRepository.InsertWithoutSaving(UpdatePhones(_mapper.Map<Conversation>(conversation), usedPhones));
                    // Removing
                    else if (cachedConversation != null && conversation.IsRemoved)
                        _conversationRepository.RemoveWithoutSave(cachedConversation);
                    // Updating
                    else if (cachedConversation != null)
                    {
                        cachedConversation.ToPhone = _mapper.Map<Phone>(conversation.ToPhone);
                        cachedConversation.SystemPhone = _mapper.Map<Phone>(conversation.SystemPhone);
                        UpdateMessagesCacheWithoutSaving(cachedConversation, conversation.Messages, usedPhones);
                    }
                }
                _conversationRepository.SaveChanges();
            }
        }

        /// <summary>
        /// Updating messages cache by provided conversation id and list of messages
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="messages"></param>
        public void UpdateMessagesCache(long conversationId, IEnumerable<FreedomVoice.Entities.Message> messages)
        {
            lock (_objLock)
            {
                var cachedConversation = _conversationRepository.Table.Include(conversation => conversation.Messages).FirstOrDefault(conversation => conversationId == conversation.Id);
                UpdateMessagesCacheWithoutSaving(cachedConversation, messages, new List<Phone>());
                _conversationRepository.SaveChanges();
            }
        }

        /// <summary>
        /// Updating messages cache by provided conversation and list of messages
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="messages"></param>
        public void UpdateMessagesCache(Conversation conversation, IEnumerable<FreedomVoice.Entities.Message> messages)
        {
            lock (_objLock)
            {
                UpdateMessagesCacheWithoutSaving(conversation, messages, new List<Phone>());
                _conversationRepository.SaveChanges();
            }
        }

        /// <summary>
        /// Get list of cached conversations by <paramref name="phone"/> from <paramref name="start"/> position 
        /// with limitation count of entities by <paramref name="limit"/>
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="limit"></param>
        /// <param name="start"></param>
        /// <returns>List of cached conversations</returns>
        public IEnumerable<Conversation> GetConversations(string phone, int limit, int start)
        {
            lock (_objLock)
            {
                var conversations = _conversationRepository
                    .TableNoTracking
                    .Include(conversation => conversation.SystemPhone)
                    .Include(conversation => conversation.ToPhone)
                    .Include(conversation => conversation.Messages)
                    .Where(conversation => conversation.SystemPhone != null && conversation.SystemPhone.PhoneNumber == phone && conversation.Messages.Count > 0)
                    .OrderByDescending(conversation => conversation.Messages.First().OrderDate)
                    .Skip(start)
                    .Take(limit);

                foreach (var conversation in conversations)
                    conversation.Messages = new[]
                    {
                        _messagesRepository
                            .TableNoTracking
                            .Include(message => message.From)
                            .Include(message => message.To)
                            .OrderByDescending(message => message.CreatedAt)
                            .FirstOrDefault(message => message.Conversation.Id == conversation.Id)
                    };
                return conversations;
            }
        }

        /// <summary>
        /// Get conversation by Id
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns>Conversation from cache</returns>
        public Conversation GetConversation(long conversationId)
        {
            lock (_objLock)
            {
                var conversation = _conversationRepository.TableNoTracking
                    .Include(x => x.SystemPhone)
                    .Include(x => x.Messages)
                    .Include(x => x.ToPhone)
                    .LastOrDefault(x => x.Id == conversationId);

                conversation.Messages = new[]
                    {
                        _messagesRepository
                            .TableNoTracking
                            .Include(message => message.From)
                            .Include(message => message.To)
                            .OrderByDescending(message => message.CreatedAt)
                            .FirstOrDefault(message => message.Conversation.Id == conversation.Id)
                    };
                return conversation;
            }
        }

        /// <summary>
        /// Get list of cached messages for <paramref name="conversation"/> object from <paramref name="start"/> position 
        /// with limitation count of entities by <paramref name="limit"/>
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="limit"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public IEnumerable<Message> GetMessagesByConversation(Conversation conversation, int limit, int start)
        {
            return GetMessagesByConversation(conversation.Id, limit, start);
        }

        /// <summary>
        /// Get list of cached messages for <paramref name="conversationId"/> id from <paramref name="start"/> position 
        /// with limitation count of entities by <paramref name="limit"/>
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="limit"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public IEnumerable<Message> GetMessagesByConversation(long conversationId, int limit, int start)
        {
            lock (_objLock)
            {
                var conversationWithMessages = _messagesRepository.TableNoTracking
                    .Include(conversation => conversation.Conversation)
                    .Include(conversation => conversation.From)
                    .Include(conversation => conversation.To)
                    .Where(conversation => conversation.Conversation.Id == conversationId);
                return conversationWithMessages.Skip(start).Take(limit);
            }
        }
        
        /// <summary>
        /// Get last modified conversation date by <paramref name="startTime"/>
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public DateTime GetLastConversationUpdateDate(DateTime startTime)
        {
            lock (_objLock)
            {
                if (!_conversationRepository.TableNoTracking.Any(conversation => conversation.LastSyncDate < startTime))
                    return new DateTime();
                return _conversationRepository.TableNoTracking.Where(conversation => conversation.LastSyncDate < startTime).Max(conversation => conversation.LastSyncDate);
            }
        }

        public Message GetMessageBy(long conversationId, long messageId)
        {
            lock (_objLock)
            {
                var conversationWithMessages = _messagesRepository.TableNoTracking
                    .Include(conversation => conversation.Conversation)
                    .Include(conversation => conversation.From)
                    .Include(conversation => conversation.To)
                    .Where(conversation => conversation.Conversation.Id == conversationId && conversation.Id == messageId);

                return conversationWithMessages.FirstOrDefault();
            }
        }
    }
}

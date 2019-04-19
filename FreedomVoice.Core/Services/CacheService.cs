using AutoMapper;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.DAL;
using FreedomVoice.DAL.DbEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreedomVoice.Core.Services
{
    public class CacheService : ICacheService
    {
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IRepository<Message> _messagesRepository;
        private readonly IRepository<Phone> _phoneRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="conversationRepository"></param>
        /// <param name="messagesRepository"></param>
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
        private void UpdateMessagesCacheWithoutSaving(Conversation conversation, IEnumerable<FreedomVoice.Entities.Message> messages, List<Phone> alreadyCreatedPhones)
        {
            if (conversation == null)
                throw new ArgumentException("Conversation not found");
            if (conversation.Messages == null)
                conversation.Messages = new List<Message>();
            // Removing - we cannot to remove messages

            var messagesForUpdate = _messagesRepository.Table.Where(x => messages.Any(xx => xx.Id == x.Id));
            foreach (var messageFromApi in messages)
            {
                var message = messagesForUpdate.FirstOrDefault(x => x.Id == messageFromApi.Id);
                // Adding
                if (message == null)
                    conversation.Messages.Add(_mapper.Map<Message>(messageFromApi));
                // Updating
                else
                    message.ReadAt = messageFromApi.ReadAt;
            }

            UpdatePhones(conversation, alreadyCreatedPhones);
        }

        private Conversation UpdatePhones(Conversation conversation, List<Phone> alreadyCreatedPhones)
        {
            if(conversation.CurrentPhone != null)
            {
                conversation.CurrentPhone = _phoneRepository.Table.FirstOrDefault(x => x.Id == conversation.CurrentPhone.Id) ??
                    alreadyCreatedPhones.FirstOrDefault(x => x.Id == conversation.CurrentPhone.Id) ?? conversation.CurrentPhone;

                if (alreadyCreatedPhones.All(x => x.Id != conversation.CurrentPhone.Id))
                    alreadyCreatedPhones.Add(conversation.CurrentPhone);
            }
            if (conversation.CollocutorPhone != null)
            {
                conversation.CollocutorPhone = _phoneRepository.Table.FirstOrDefault(x => x.Id == conversation.CollocutorPhone.Id) ??
                    alreadyCreatedPhones.FirstOrDefault(x => x.Id == conversation.CollocutorPhone.Id) ?? conversation.CollocutorPhone;
                if (alreadyCreatedPhones.All(x => x.Id != conversation.CollocutorPhone.Id))
                    alreadyCreatedPhones.Add(conversation.CollocutorPhone);
            }

            foreach (var message in conversation.Messages)
            {
                if (message.From != null)
                {
                    message.From = _phoneRepository.Table.FirstOrDefault(x => x.Id == message.From.Id) ??
                    alreadyCreatedPhones.FirstOrDefault(x => x.Id == message.From.Id) ?? message.From;
                    if (alreadyCreatedPhones.All(x => x.Id != message.From.Id))
                        alreadyCreatedPhones.Add(message.From);
                }
                if (message.To != null)
                {
                    message.To = _phoneRepository.Table.FirstOrDefault(x => x.Id == message.To.Id) ??
                    alreadyCreatedPhones.FirstOrDefault(x => x.Id == message.To.Id) ?? message.To;
                    if (alreadyCreatedPhones.All(x => x.Id != message.To.Id))
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
            List<Phone> usedPhones = new List<Phone>();
            foreach (var conversation in conversations)
            {
                var cachedConversation = _conversationRepository.Table.Include(x => x.Messages).FirstOrDefault(x => conversation.Id == x.Id);
                // Adding
                if (cachedConversation == null && !conversation.IsRemoved)
                    _conversationRepository.InsertWithoutSaving(UpdatePhones(_mapper.Map<Conversation>(conversation), usedPhones));
                // Removing
                else if (cachedConversation != null && conversation.IsRemoved)
                    _conversationRepository.RemoveWithoutSave(cachedConversation);
                // Updating
                else if (cachedConversation != null)
                    UpdateMessagesCacheWithoutSaving(cachedConversation, conversation.Messages, usedPhones);
            }

            _conversationRepository.SaveChanges();
        }

        /// <summary>
        /// Updating messages cache by provided conversation id and list of messages
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="messages"></param>
        public void UpdateMessagesCache(long conversationId, IEnumerable<FreedomVoice.Entities.Message> messages)
        {
            var cachedConversation = _conversationRepository.Table.Include(x => x.Messages).FirstOrDefault(x => conversationId == x.Id);
            UpdateMessagesCache(cachedConversation, messages);
        }

        /// <summary>
        /// Updating messages cache by provided conversation and list of messages
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="messages"></param>
        public void UpdateMessagesCache(Conversation conversation, IEnumerable<FreedomVoice.Entities.Message> messages)
        {
            UpdateMessagesCacheWithoutSaving(conversation, messages, new List<Phone>());
            _conversationRepository.SaveChanges();
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
            return _conversationRepository.TableNoTracking.Include(x => x.CurrentPhone).Where(x => x.CurrentPhone != null && x.CurrentPhone.PhoneNumber == phone).Skip(start).Take(limit);
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
            var conversationWithMessages = _conversationRepository.TableNoTracking.Include(x => x.Messages).FirstOrDefault(x => x.Id == conversationId);
            if (conversationWithMessages == null) return new List<Message>();
            return conversationWithMessages.Messages.Skip(start).Take(limit);
        }
        
        /// <summary>
        /// Get last modified conversation date by <paramref name="startTime"/>
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public DateTime GetLastConversationUpdateDate(DateTime startTime)
        {
            if (!_conversationRepository.TableNoTracking.Any(x => x.LastSyncDate < startTime))
                return new DateTime();
            return _conversationRepository.TableNoTracking.Where(x => x.LastSyncDate < startTime).Max(x => x.LastSyncDate);
        }
    }
}

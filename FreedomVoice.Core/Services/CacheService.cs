﻿using AutoMapper;
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

            var messagesForUpdate = _messagesRepository.Table.Where(messageFromRepository => messages.Any(message => message.Id == messageFromRepository.Id));
            foreach (var messageFromApi in messages)
            {
                var message = messagesForUpdate.FirstOrDefault(messageForUpdate => messageForUpdate.Id == messageFromApi.Id);
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
                conversation.CurrentPhone = _phoneRepository.Table.FirstOrDefault(phone => phone.Id == conversation.CurrentPhone.Id) ??
                    alreadyCreatedPhones.FirstOrDefault(phone => phone.Id == conversation.CurrentPhone.Id) ?? conversation.CurrentPhone;

                if (alreadyCreatedPhones.All(phone => phone.Id != conversation.CurrentPhone.Id))
                    alreadyCreatedPhones.Add(conversation.CurrentPhone);
            }
            if (conversation.CollocutorPhone != null)
            {
                conversation.CollocutorPhone = _phoneRepository.Table.FirstOrDefault(phone => phone.Id == conversation.CollocutorPhone.Id) ??
                    alreadyCreatedPhones.FirstOrDefault(phone => phone.Id == conversation.CollocutorPhone.Id) ?? conversation.CollocutorPhone;
                if (alreadyCreatedPhones.All(phone => phone.Id != conversation.CollocutorPhone.Id))
                    alreadyCreatedPhones.Add(conversation.CollocutorPhone);
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
                if (message.To != null)
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
            List<Phone> usedPhones = new List<Phone>();
            foreach (var conversation in conversations)
            {
                var cachedConversation = _conversationRepository.Table.Include(row => row.Messages).FirstOrDefault(row => conversation.Id == row.Id);
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
            var cachedConversation = _conversationRepository.Table.Include(conversation => conversation.Messages).FirstOrDefault(conversation => conversationId == conversation.Id);
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
            var conversations = _conversationRepository
                .TableNoTracking
                .Include(conversation => conversation.CurrentPhone)
                .Include(conversation => conversation.CollocutorPhone)
                .Where(conversation => conversation.CurrentPhone != null && conversation.CurrentPhone.PhoneNumber == phone)
                .OrderByDescending(conversation => conversation.LastSyncDate)
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

        /// <summary>
        /// Get conversation by Id
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns>Conversation from cache</returns>
        public Conversation GetConversation(long conversationId)
        {
            return _conversationRepository.TableNoTracking.Include(x => x.CurrentPhone).Include(x => x.CollocutorPhone).FirstOrDefault(x => x.Id == conversationId);
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
            var conversationWithMessages = _messagesRepository.TableNoTracking
                .Include(conversation => conversation.Conversation)
                .Include(conversation => conversation.From)
                .Include(conversation => conversation.To)
                .Where(conversation => conversation.Conversation.Id == conversationId);
            if (conversationWithMessages == null) return new List<Message>();
            return conversationWithMessages.Skip(start).Take(limit);
        }
        
        /// <summary>
        /// Get last modified conversation date by <paramref name="startTime"/>
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public DateTime GetLastConversationUpdateDate(DateTime startTime)
        {
            if (!_conversationRepository.TableNoTracking.Any(conversation => conversation.LastSyncDate < startTime))
                return new DateTime();
            return _conversationRepository.TableNoTracking.Where(conversation => conversation.LastSyncDate < startTime).Max(conversation => conversation.LastSyncDate);
        }

        public Message GetMessageBy(long conversationId, long messageId)
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

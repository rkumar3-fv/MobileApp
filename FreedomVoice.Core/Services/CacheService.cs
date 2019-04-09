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
        private readonly IMapper _mapper;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="conversationRepository"></param>
        /// <param name="messagesRepository"></param>
        /// <param name="mapper"></param>
        public CacheService(IRepository<Conversation> conversationRepository, IRepository<Message> messagesRepository, IMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _messagesRepository = messagesRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Updating messages cache without saving any entities to cache
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="messages"></param>
        private void UpdateMessagesCacheWithoutSaving(Conversation conversation, IEnumerable<FreedomVoice.Entities.Message> messages)
        {
            if (conversation == null)
                throw new ArgumentException("Conversation not found");
            if (conversation.Messages == null)
                conversation.Messages = new List<Message>();
            // Removing - we cannot to remove messages
            // Updating - we cannot to update message
            // Adding
            foreach (var messageForAdding in messages.Where(x => _messagesRepository.TableNoTracking.All(xx => xx.Id != x.Id)))
                conversation.Messages.Add(_mapper.Map<Message>(messageForAdding));
        }

        /// <summary>
        /// Updating conversations cache by recieving entities
        /// </summary>
        /// <param name="conversations"></param>
        public void UpdateConversationsCache(IEnumerable<FreedomVoice.Entities.Response.Conversation> conversations)
        {
            foreach (var conversation in conversations)
            {
                var cachedConversation = _conversationRepository.Table.Include(x => x.Messages).FirstOrDefault(x => conversation.Id == x.Id);
                // Adding
                if (cachedConversation == null && !conversation.IsRemoved)
                    _conversationRepository.InsertWithoutSaving(_mapper.Map<Conversation>(conversation));
                // Removing
                else if (cachedConversation != null && conversation.IsRemoved)
                    _conversationRepository.RemoveWithoutSave(cachedConversation);
                // Updating
                else if (cachedConversation != null)
                    UpdateMessagesCacheWithoutSaving(cachedConversation, conversation.Messages);
            }

            _conversationRepository.SaveChanges();
        }

        /// <summary>
        /// Updating messages cache by provided conversation id and list of messages
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="messages"></param>
        public void UpdateMessagesCache(int conversationId, IEnumerable<FreedomVoice.Entities.Message> messages)
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
            UpdateMessagesCacheWithoutSaving(conversation, messages);
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
        public IEnumerable<Message> GetMessagesByConversation(int conversationId, int limit, int start)
        {
            var conversationWithMessages = _conversationRepository.TableNoTracking.Include(x => x.Messages).FirstOrDefault(x => x.Id == conversationId);
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

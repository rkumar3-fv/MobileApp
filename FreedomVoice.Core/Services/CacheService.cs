using AutoMapper;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.DAL;
using FreedomVoice.DAL.DbEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FreedomVoice.Core.Utils.Extensions;
using FreedomVoice.DAL.DbEntities.Enums;

namespace FreedomVoice.Core.Services
{
    public class CacheService : ICacheService
    {
        private readonly IRepository<Conversation> _conversationRepository;
        private readonly IRepository<Message> _messagesRepository;
        private readonly IRepository<Phone> _phoneRepository;
        private readonly IMapper _mapper;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1); 

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
        private async Task UpdateMessagesCacheWithoutSaving(Conversation conversation, IEnumerable<FreedomVoice.Entities.Message> messages)
        {
            if (conversation == null)
            {
                Console.WriteLine($"CacheService has been finished failed without saving because conversation does not exists for messages: ${messages}");
                return;
            }
            if (conversation.Messages == null)
                conversation.Messages = new List<Message>();
            // Removing - we cannot to remove messages

            var messagesForUpdate = await _messagesRepository.Table
                .Where(messageFromRepository => messages.Any(message => message.Id == messageFromRepository.Id))
                .ToDictionaryAsync(message => message.Id);
            foreach (var messageFromApi in messages)
            {
                var message = messagesForUpdate.ContainsKey(messageFromApi.Id) ? messagesForUpdate[messageFromApi.Id] : null;
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
        }

        private Conversation UpdateCachedPhones(Conversation conversation, IDictionary<long, Phone> alreadyCreatedPhones)
        {
            if(conversation.SystemPhone != null && alreadyCreatedPhones.ContainsKey(conversation.SystemPhone.Id))
            {
                conversation.SystemPhone = alreadyCreatedPhones[conversation.SystemPhone.Id];
            }
            if(conversation.ToPhone != null && alreadyCreatedPhones.ContainsKey(conversation.ToPhone.Id))
            {
                conversation.ToPhone = alreadyCreatedPhones[conversation.ToPhone.Id];
            }

            foreach (var message in conversation.Messages)
            {
                if (message.From != null && alreadyCreatedPhones.ContainsKey(message.From.Id))
                {
                    message.From = alreadyCreatedPhones[message.From.Id];
                }
                if (message.To != null && alreadyCreatedPhones.ContainsKey(message.To.Id))
                {
                    message.To = alreadyCreatedPhones[message.To.Id];
                }
            }

            return conversation;
        }

        private async Task<List<Phone>> GetCachedPhones(ICollection<long> ids)
        {
            return await _phoneRepository.Table.Where(row => ids.Contains(row.Id)).ToListAsync();
        }

        private async Task<List<Conversation>> GetCachedConversations(ICollection<long> ids)
        {
            var query = _conversationRepository.Table.Include(row => row.Messages)
                .Include(row => row.SystemPhone)
                .Include(row => row.ToPhone)
                .Where(row => ids.Contains(row.Id));
            return await query.ToListAsync();
        }


        /// <summary>
        /// Updating conversations cache by recieving entities
        /// </summary>
        /// <param name="conversations"></param>
        public async Task UpdateConversationsCache(IEnumerable<FreedomVoice.Entities.Response.Conversation> conversations)
        {
            await _lock.WaitAsync();
            try
            {
                var ids = new List<long>();
                var phoneIds = new List<long>();
                foreach (var conversation in conversations)
                {
                    phoneIds.AddRange(from phone in conversation.AllPhones() select phone.Id);
                    ids.Add(conversation.Id);
                }
                var cachedConversations = (await GetCachedConversations(ids)).ToDictionary(conversation => conversation.Id);
                var cachedPhones = (await GetCachedPhones(phoneIds)).ToDictionary(phone => phone.Id);
                foreach (var conversation in conversations)
                {
                    var cachedConversation = cachedConversations.ContainsKey(conversation.Id) ? cachedConversations[conversation.Id] : null;
                    
                    // Adding
                    if (cachedConversation == null && !conversation.IsRemoved)
                    {
                        _conversationRepository.InsertWithoutSaving(UpdateCachedPhones(_mapper.Map<Conversation>(conversation), cachedPhones));
                    }
                    // Removing
                    else if (cachedConversation != null && conversation.IsRemoved)
                        _conversationRepository.RemoveWithoutSave(cachedConversation);
                    // Updating
                    else if (cachedConversation != null)
                    {
                        cachedConversation = UpdateCachedPhones(cachedConversation, cachedPhones);
                        await UpdateMessagesCacheWithoutSaving(cachedConversation, conversation.Messages);
                    }
                }
                _conversationRepository.SaveChanges();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Updating messages cache by provided conversation id and list of messages
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="messages"></param>
        public async Task UpdateMessagesCache(long conversationId, IEnumerable<FreedomVoice.Entities.Message> messages)
        {
            await _lock.WaitAsync();
            try
            {
                var cachedConversation = _conversationRepository.Table.Include(conversation => conversation.Messages).FirstOrDefault(conversation => conversationId == conversation.Id);
                await UpdateMessagesCacheWithoutSaving(cachedConversation, messages);
                _conversationRepository.SaveChanges();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Updating messages cache by provided conversation and list of messages
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="messages"></param>
        public async Task UpdateMessagesCache(Conversation conversation, IEnumerable<FreedomVoice.Entities.Message> messages)
        {
            await _lock.WaitAsync();
            try
            {
                await UpdateMessagesCacheWithoutSaving(conversation, messages);
                _conversationRepository.SaveChanges();
            }
            finally
            {
                _lock.Release();
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
        public async Task<IEnumerable<Conversation>> GetConversations(string phone, int limit, int start)
        {
            await _lock.WaitAsync();
            try
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
                        await  _messagesRepository
                            .TableNoTracking
                            .Include(message => message.From)
                            .Include(message => message.To)
                            .OrderByDescending(message => message.CreatedAt)
                            .FirstOrDefaultAsync(message => message.Conversation.Id == conversation.Id)
                    };
                return conversations;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Get conversation by Id
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns>Conversation from cache</returns>
        public async Task<Conversation> GetConversation(long conversationId)
        {
            await _lock.WaitAsync();
            try
            {
                var conversation = await  _conversationRepository.TableNoTracking
                    .Include(x => x.SystemPhone)
                    .Include(x => x.Messages)
                    .Include(x => x.ToPhone)
                    .LastOrDefaultAsync(x => x.Id == conversationId);

                conversation.Messages = new[]
                    {
                        await _messagesRepository
                            .TableNoTracking
                            .Include(message => message.From)
                            .Include(message => message.To)
                            .OrderByDescending(message => message.CreatedAt)
                            .FirstOrDefaultAsync(message => message.Conversation.Id == conversation.Id)
                    };
                return conversation;
            }
            finally
            {
                _lock.Release();
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
        public async Task<IEnumerable<Message>> GetMessagesByConversation(Conversation conversation, int limit, int start)
        {
            return await GetMessagesByConversation(conversation.Id, limit, start);
        }

        /// <summary>
        /// Get list of cached messages for <paramref name="conversationId"/> id from <paramref name="start"/> position 
        /// with limitation count of entities by <paramref name="limit"/>
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="limit"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Message>> GetMessagesByConversation(long conversationId, int limit, int start)
        {
            await _lock.WaitAsync();
            try
            {
                var conversationWithMessages = _messagesRepository.TableNoTracking
                    .Include(conversation => conversation.Conversation)
                    .Include(conversation => conversation.From)
                    .Include(conversation => conversation.To)
                    .Where(conversation => conversation.Conversation.Id == conversationId);
                return conversationWithMessages.Skip(start).Take(limit);
            }
            finally
            {
                _lock.Release();
            }
        }
        
        /// <summary>
        /// Get last modified conversation date by <paramref name="startTime"/>
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public async Task<DateTime> GetLastConversationUpdateDate(DateTime startTime)
        {
            await _lock.WaitAsync();
            try
            {
                if (!_conversationRepository.TableNoTracking.Any(conversation => conversation.LastSyncDate < startTime))
                    return new DateTime();
                return _conversationRepository.TableNoTracking.Where(conversation => conversation.LastSyncDate < startTime).Max(conversation => conversation.LastSyncDate);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Message> GetMessageBy(long conversationId, long messageId)
        {
            await _lock.WaitAsync();
            try
            {
                var conversationWithMessages = _messagesRepository.TableNoTracking
                    .Include(conversation => conversation.Conversation)
                    .Include(conversation => conversation.From)
                    .Include(conversation => conversation.To)
                    .Where(conversation => conversation.Conversation.Id == conversationId && conversation.Id == messageId);

                return await conversationWithMessages.FirstOrDefaultAsync();
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}

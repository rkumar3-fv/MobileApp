using AutoMapper;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.DAL;
using FreedomVoice.DAL.DbEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private async Task UpdateMessagesCacheWithoutSaving(Conversation conversation, IEnumerable<FreedomVoice.Entities.Message> messages, List<Phone> alreadyCreatedPhones)
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

            await UpdatePhones(conversation, alreadyCreatedPhones);
        }

        private async Task<Conversation> UpdatePhones(Conversation conversation, List<Phone> alreadyCreatedPhones)
        {
            if(conversation.SystemPhone != null)
            {
                conversation.SystemPhone = await _phoneRepository.Table.FirstOrDefaultAsync(phone => phone.Id == conversation.SystemPhone.Id) ??
                    alreadyCreatedPhones.FirstOrDefault(phone => phone.Id == conversation.SystemPhone.Id) ?? conversation.SystemPhone;

                if (alreadyCreatedPhones.All(phone => phone.Id != conversation.SystemPhone.Id))
                    alreadyCreatedPhones.Add(conversation.SystemPhone);
            }
            if (conversation.ToPhone != null)
            {
                conversation.ToPhone = await _phoneRepository.Table.FirstOrDefaultAsync(phone => phone.Id == conversation.ToPhone.Id) ??
                    alreadyCreatedPhones.FirstOrDefault(phone => phone.Id == conversation.ToPhone.Id) ?? conversation.ToPhone;
                if (alreadyCreatedPhones.All(phone => phone.Id != conversation.ToPhone.Id))
                    alreadyCreatedPhones.Add(conversation.ToPhone);
            }

            foreach (var message in conversation.Messages)
            {
                if (message.From != null)
                {
                    message.From = await _phoneRepository.Table.FirstOrDefaultAsync(phone => phone.Id == message.From.Id) ??
                    alreadyCreatedPhones.FirstOrDefault(phone => phone.Id == message.From.Id) ?? message.From;
                    if (alreadyCreatedPhones.All(phone => phone.Id != message.From.Id))
                        alreadyCreatedPhones.Add(message.From);
                }

                if (message.To == null) continue;
                {
                    message.To = await _phoneRepository.Table.FirstOrDefaultAsync(phone => phone.Id == message.To.Id) ??
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
        public async Task UpdateConversationsCache(IEnumerable<FreedomVoice.Entities.Response.Conversation> conversations)
        {
            await _lock.WaitAsync();
            try
            {
                var usedPhones = new List<Phone>();
                foreach (var conversation in conversations)
                {
                    var cachedConversation = await _conversationRepository.Table.Include(row => row.Messages)
                        .Include(row => row.SystemPhone)
                        .Include(row => row.ToPhone)
                        .FirstOrDefaultAsync(row => conversation.Id == row.Id);
                    // Adding
                    if (cachedConversation == null && !conversation.IsRemoved)
                        _conversationRepository.InsertWithoutSaving(
                            await UpdatePhones(_mapper.Map<Conversation>(conversation), usedPhones));
                    // Removing
                    else if (cachedConversation != null && conversation.IsRemoved)
                        _conversationRepository.RemoveWithoutSave(cachedConversation);
                    // Updating
                    else if (cachedConversation != null)
                    {
                        cachedConversation.ToPhone = _mapper.Map<Phone>(conversation.ToPhone);
                        cachedConversation.SystemPhone = _mapper.Map<Phone>(conversation.SystemPhone);
                        await UpdateMessagesCacheWithoutSaving(cachedConversation, conversation.Messages, usedPhones);
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
                await UpdateMessagesCacheWithoutSaving(cachedConversation, messages, new List<Phone>());
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
                await UpdateMessagesCacheWithoutSaving(conversation, messages, new List<Phone>());
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

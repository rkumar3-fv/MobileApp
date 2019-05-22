using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using FreedomVoice.Core.Services;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.DAL.DbEntities;
using FreedomVoice.Entities.Enums;
using SendingState = FreedomVoice.Entities.Enums.SendingState;

namespace FreedomVoice.Core.Presenters
{

    public class ConversationCollectionEventArgs : EventArgs
    {
        public IEnumerable<IChatMessage> Conversations;

        public ConversationCollectionEventArgs(IEnumerable<IChatMessage> conversations)
        {
            Conversations = conversations;
        }
    }

    public class ConversationPresenter : IDisposable
    {
        
        #region Private services 
        
        private readonly IConversationService _conversationService = ServiceContainer.Resolve<IConversationService>();
        private readonly IContactNameProvider _contactNameProvider = ServiceContainer.Resolve<IContactNameProvider>();
        private readonly IMessagesService _messagesService = ServiceContainer.Resolve<IMessagesService>();
       
        #endregion

        #region Private variables

        private DateTime _currentDate;
        private int _currentPage;
        private bool _isLoading;
        private const int DefaultCount = 50;
        private const string DateFormat = "MM/dd/yyyy";
        private Dictionary<string, List<IChatMessage>> _rawData;

        private long? _conversationId;
        private string _phoneNumber;
        
        #endregion

        #region Public events

        public event EventHandler ContactsUpdated;
        public event EventHandler ItemsChanged;

        #endregion

        #region Public variables

        public List<IChatMessage> Items;
        public bool HasMore { get; private set; }
        
        public long? ConversationId
        {
            get => _conversationId;
            set
            {
                if (value == _conversationId)
                    return;
                _conversationId = value;
            }
        }
        
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (_phoneNumber != null && value == _phoneNumber)
                    return;
                _phoneNumber = value;
            }
        }

        #endregion


        public ConversationPresenter()
        {
            _contactNameProvider.RequestContacts();
            _contactNameProvider.ContactsUpdated += ContactNameProviderOnContactsUpdated;
            NotificationMessageService.Instance().NewMessageEventHandler += OnNewMessageEventHandler;
            NotificationMessageService.Instance().MessageUpdatedHandler += OnMessageUpdatedHandler;
            ResetState();
        }

        private void OnMessageUpdatedHandler(object sender, MessageEventArg e)
        {
            if (!e.Message.CreatedAt.HasValue || !_rawData.ContainsKey(e.Message.CreatedAt.Value.ToString(DateFormat))) return;
            var chatMessages = _rawData[e.Message.CreatedAt.Value.ToString(DateFormat)];
            if (chatMessages == null) return;
            
            var visibleItemIndex = chatMessages.FindIndex(chatMessage => chatMessage.MessageId == e.Message.Id);
            if (visibleItemIndex == -1) return;

            chatMessages[visibleItemIndex] = CreateChatMessage(e.Message);
            _updateItems();
            ItemsChanged?.Invoke(this, new ConversationCollectionEventArgs(Items));
        }

        private void OnNewMessageEventHandler(object sender, ConversationEventArg e)
        {
            var message = e.Conversation.Messages.LastOrDefault();
            if (message == null)
            {
                return;
            }

            var rawTo = Regex.Replace(message.To.PhoneNumber, @"\D", "");
            var rawFrom = Regex.Replace(message.From.PhoneNumber, @"\D", "");
            var current = Regex.Replace(PhoneNumber, @"\D", "");
            
            if (rawFrom.Equals(current) || !rawTo.Equals(current) )
            {
                return;
            }
            var model = new IncomingMessageViewModel(e.Conversation.Messages.Last());
            _addMessage(model);
        }

        public async void ReloadAsync()
        {
            ResetState();
            await _PerformLoading();
        }

        public async void LoadMoreAsync()
        {
            if (!HasMore && !_isLoading) return;
            _currentPage++;
            await _PerformLoading();
        }

        public async Task SendMessageAsync(string text)
        {
            if (!_conversationId.HasValue)
            {
                return;
            }
            
            var res = await _messagesService.SendMessage(_conversationId.Value, text);
            switch (res.State)
            {
                case SendingState.Error:
                    MessagedSentError(res.Entity);
                    break;
                
                case SendingState.Sending:
                    MessagedSentSending(res.Entity);
                    break;
                
                case SendingState.Success:
                    MessagedSentSuccess(res.Entity);
                    break;
            }
        }
        
        public async Task<long?> SendMessage(string currentPhone, string toPhone, string text)
        {
            var clearedCurrentPhone = GetClearPhoneNumber(currentPhone);
            var clearedToPhone = GetClearPhoneNumber(toPhone);
            
            if (string.IsNullOrWhiteSpace(clearedCurrentPhone) || string.IsNullOrWhiteSpace(clearedToPhone))
                return null;
            
            var res = await _messagesService.SendMessage(clearedCurrentPhone, clearedToPhone, text);
            
            switch (res.State)
            {
                case SendingState.Error:
                    MessagedSentError(res.Entity);
                    break;
                
                case SendingState.Sending:
                    MessagedSentSending(res.Entity);
                    break;
                
                case SendingState.Success:
                    MessagedSentSuccess(res.Entity);
                    break;
            }

            _conversationId = res.Entity?.Id;
            return _conversationId;
        }

        public void Clear()
        {
            ResetState();
            _updateItems();
            _isLoading = false;
            ItemsChanged?.Invoke(this, new ConversationCollectionEventArgs(Items));
        }
        
        public string GetFormattedPhoneNumber(string phoneNumber)
        {
            return _contactNameProvider.GetFormattedPhoneNumber(phoneNumber);
        }

        public string GetClearPhoneNumber(string formattedPhoneNumber)
        {
            return _contactNameProvider.GetClearPhoneNumber(formattedPhoneNumber);
        }

        public string GetNameOrNull(string phone)
        {
            var clearPhone = GetClearPhoneNumber(phone);
            return _contactNameProvider.GetNameOrNull(clearPhone);
        }

        public async Task<long?> GetConversationId(string currentPhone, string toPhone)
        {
            var clearedCurrentPhone = GetClearPhoneNumber(currentPhone);
            var clearedToPhone = GetClearPhoneNumber(toPhone);

            if (string.IsNullOrWhiteSpace(clearedCurrentPhone) || string.IsNullOrWhiteSpace(clearedToPhone))
                return null;
            
            var conversation = await _conversationService.Get(clearedCurrentPhone, clearedToPhone);
            return conversation?.Conversation?.Id;
        }

        #region Send message handlers

        private void MessagedSentSuccess(Conversation conversation)
        {
            var entity = ServiceContainer.Resolve<IMapper>().Map<FreedomVoice.Entities.Response.Conversation>(conversation);
            NotificationMessageService.Instance().ReceivedNotification(PushType.NewMessage, entity);
            var lastMessage = conversation?.Messages?.Last();
            if (lastMessage == null)
            {
                return;
            }
            _addMessage(new OutgoingMessageViewModel(lastMessage));
        }

        private void MessagedSentError(Conversation conversation)
        {
            var lastMessage = conversation?.Messages?.Last();
            if (lastMessage == null)
            {
                return;
            }
            _addMessage(new OutgoingMessageViewModel(lastMessage));
        }

        private void MessagedSentSending(Conversation conversation)
        {
            MessagedSentSuccess(conversation);
        }

        #endregion

        private void _addMessage(IChatMessage message)
        {
            var dateStr = message.Date.ToString(DateFormat);
            var pack = _rawData.ContainsKey(dateStr) ? _rawData[dateStr] : new List<IChatMessage>();
            pack.Add(message);
            _rawData[dateStr] = pack;
            _updateItems();
            ItemsChanged?.Invoke(this, new ConversationCollectionEventArgs(Items)); 
        } 
        private void ResetState()
        {
            _currentDate = DateTime.Now;
            _currentPage = 1;
            Items = new List<IChatMessage>();
            _rawData = new Dictionary<string, List<IChatMessage>>();
            HasMore = false;
        }

        private async Task _PerformLoading()
        {
            if (!_conversationId.HasValue)
            {
                _updateItems();
                ItemsChanged?.Invoke(this, new ConversationCollectionEventArgs(Items));
                _isLoading = false;
                return;
            }

            _isLoading = true;
            var res = await _messagesService.GetList(_conversationId.Value, _currentDate, DefaultCount, _currentPage);
            HasMore = !res.IsEnd;

            foreach (var row in res.Messages)
            {
                if (row.From == null) continue;
                var dateStr = row.CreatedAt?.ToString(DateFormat);
                if (dateStr == null) continue;
                var pack = _rawData.ContainsKey(dateStr) ? _rawData[dateStr] : new List<IChatMessage>();
                pack.Add(CreateChatMessage(row));
                _rawData[dateStr] = pack;
            }

            _updateItems();
            ItemsChanged?.Invoke(this, new ConversationCollectionEventArgs(Items));
            _isLoading = false;
        }

        private IChatMessage CreateChatMessage(Message row)
        {
            if (row.From.PhoneNumber.Equals(PhoneNumber))
            {
                return new OutgoingMessageViewModel(row);
            }
            else
            {
                return new IncomingMessageViewModel(row);
            }
        }

        private void _updateItems()
        {
            Items = new List<IChatMessage>();
            foreach (var group in _rawData.OrderByDescending(item => item.Key))
            {
                var date = DateTime.MinValue;
                DateTime.TryParseExact(group.Key, DateFormat, null, System.Globalization.DateTimeStyles.None, out date);
                Items.AddRange(group.Value);
                Items.Add(new DateMessageViewModel(date));
            }
        }
        
        private void ContactNameProviderOnContactsUpdated(object sender, EventArgs e)
        {
            ContactsUpdated?.Invoke(sender, e);
        }

        public void Dispose()
        {
            _contactNameProvider.ContactsUpdated -= ContactNameProviderOnContactsUpdated;
            NotificationMessageService.Instance().NewMessageEventHandler -= OnNewMessageEventHandler;
            NotificationMessageService.Instance().MessageUpdatedHandler -= OnMessageUpdatedHandler;
        }
    }
}

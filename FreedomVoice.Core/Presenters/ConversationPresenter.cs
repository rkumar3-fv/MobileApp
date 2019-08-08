using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using FreedomVoice.Core.Services;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;
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

    public class MessageSentEventArgs : EventArgs
    {
        public bool IsSuccess;
        public string Message;

        public MessageSentEventArgs(bool isSuccess, string Message)
        {
            IsSuccess = isSuccess;
            this.Message = Message;
        }
    }

    public class ConversationPresenter : IDisposable
    {
        #region Private services 

        private readonly IConversationService _conversationService = ServiceContainer.Resolve<IConversationService>();
        private readonly IContactNameProvider _contactNameProvider = ServiceContainer.Resolve<IContactNameProvider>();
        private readonly IMessagesService _messagesService = ServiceContainer.Resolve<IMessagesService>();
        private readonly IPhoneFormatter _formatter = ServiceContainer.Resolve<IPhoneFormatter>();

        #endregion

        #region Private variables

        private DateTime _currentDate;
        private int _currentPage;
        private bool _isLoading;
        private const int DefaultCount = 50;
        private const string DateFormat = "MM/dd/yyyy";

        private const string SameNumberMessage =
            "Oops, It looks like you've sent this message to your own number. Please update the recipient phone number and try again.";

        private Dictionary<DateTime, List<IChatMessage>> _rawData;

        private long? _conversationId;
        private string _phoneNumber;

        #endregion

        #region Public events

        public event EventHandler ContactsUpdated;
        public event EventHandler ItemsChanged;
        public event EventHandler MessageSent;
        public event EventHandler ServerError;

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
                var newValue = _formatter.Normalize(value);
                if (_phoneNumber != null && newValue == _phoneNumber)
                    return;
                _phoneNumber = newValue;
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
            if (e.Message == null || !e.Message.CreatedAt.HasValue ||
                !_rawData.ContainsKey(e.Message.CreatedAt.Value) ||
                e.Message.Conversation?.Id != _conversationId.Value) return;
            var chatMessages = _rawData[e.Message.CreatedAt.Value];
            if (chatMessages == null)
            {
                _addOutgoingMessage(e.Message);
                return;
            }

            var visibleItemIndex = chatMessages.FindIndex(chatMessage => chatMessage.MessageId == e.Message.Id);
            if (visibleItemIndex == -1)
            {
                _addOutgoingMessage(e.Message);
                return;
            }

            chatMessages[visibleItemIndex] = CreateChatMessage(e.Message);
            _updateItems();
            ItemsChanged?.Invoke(this, new ConversationCollectionEventArgs(Items));
        }

        private void OnNewMessageEventHandler(object sender, ConversationEventArg e)
        {
            var message = e.Conversation.Messages.LastOrDefault();
            if (message == null || !_conversationId.HasValue || e.Conversation.Id != _conversationId.Value)
            {
                return;
            }

            var rawTo = _formatter.Normalize(message.To.PhoneNumber);

            var rawFrom = _formatter.Normalize(message.From.PhoneNumber);
            var current = _formatter.Normalize(PhoneNumber);

            if (rawFrom.Equals(current) || !rawTo.Equals(current))
            {
                return;
            }

            var model = new IncomingMessageViewModel(e.Conversation.Messages.Last());
            _addMessage(model);
        }

        private void _addOutgoingMessage(Message message)
        {
            var rawTo = _formatter.Normalize(message.To.PhoneNumber);
            var rawFrom = _formatter.Normalize(message.From.PhoneNumber);
            var current = _formatter.Normalize(PhoneNumber);

            if (rawTo.Equals(current) || !rawFrom.Equals(current))
            {
                return;
            }

            var model = new OutgoingMessageViewModel(message);
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

        public async Task<long?> SendMessageAsync(string text)
        {
            if (!_conversationId.HasValue || _conversationId.Value <= 0)
            {
                return null;
            }

            var res = await _messagesService.SendMessage(_conversationId.Value, text);

            if (res == null || res.Entity == null)
            {
                MessagedSentError(null);
                return null;
            }

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

            return res.Entity?.Id;
        }

        public async Task<long?> SendMessage(string currentPhone, string toPhone, string text)
        {
            var clearedCurrentPhone = GetClearPhoneNumber(currentPhone);
            var clearedToPhone = GetClearPhoneNumber(toPhone);

            if (string.IsNullOrWhiteSpace(clearedCurrentPhone) || string.IsNullOrWhiteSpace(clearedToPhone))
                return null;

            if (clearedCurrentPhone.Equals(clearedToPhone))
            {
                MessagedSentError(null, SameNumberMessage);
                return null;
            }

            var res = await _messagesService.SendMessage(clearedCurrentPhone, clearedToPhone, text);

            if (res == null || res.Entity == null)
            {
                MessagedSentError(null);
                return null;
            }

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
            var entity = ServiceContainer.Resolve<IMapper>()
                .Map<FreedomVoice.Entities.Response.Conversation>(conversation);
            NotificationMessageService.Instance().ReceivedNotification(PushType.NewMessage, entity);
            var lastMessage = conversation?.Messages?.Last();
            if (lastMessage == null)
            {
                return;
            }

            _addMessage(new OutgoingMessageViewModel(lastMessage));
        }

        private void MessagedSentError(Conversation conversation, string MessageText = ConversationsPresenter.DefaultError)
        {
            var lastMessage = conversation?.Messages?.Last();
            if (lastMessage != null)
                _addMessage(new OutgoingMessageViewModel(lastMessage));
            MessageSent?.Invoke(this, new MessageSentEventArgs(false, MessageText));
        }

        private void MessagedSentSending(Conversation conversation)
        {
            MessagedSentSuccess(conversation);
        }

        #endregion

        private void _addMessage(IChatMessage message)
        {
            var date = message.Date;
            date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            var pack = _rawData.ContainsKey(date) ? _rawData[date] : new List<IChatMessage>();
            pack.Insert(0, message);
            _rawData[date] = pack;
            _updateItems();
            ItemsChanged?.Invoke(this, new ConversationCollectionEventArgs(Items));
        }

        private void ResetState()
        {
            _currentDate = DateTime.Now;
            _currentPage = 1;
            Items = new List<IChatMessage>();
            _rawData = new Dictionary<DateTime, List<IChatMessage>>();
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
            if (res.ResponseCode != Entities.Enums.ErrorCodes.Ok)
            {
                _isLoading = false;
                ServerError?.Invoke(this, null);
            }
            HasMore = !res.IsEnd;

            foreach (var row in res.Messages)
            {
                if (row.From == null) continue;
                var date = row.CreatedAt.Value.ToLocalTime();
                date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                var pack = _rawData.ContainsKey(date) ? _rawData[date] : new List<IChatMessage>();
                pack.Add(CreateChatMessage(row));
                _rawData[date] = pack;
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
                Items.AddRange(group.Value);
                Items.Add(new DateMessageViewModel(group.Key));
            }

            Items = Items.GroupBy(p => p.MessageId).Select(g => g.OrderBy(y => y.MessageId).First()).ToList();
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

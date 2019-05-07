using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.DAL.DbEntities;

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

    public class ConversationPresenter
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
        private const int DEFAULT_COUNT = 50;
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
            ResetState();
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
                case FreedomVoice.Entities.Enums.SendingState.Error:
                    MessagedSentError(res.Entity);
                    break;
                
                case FreedomVoice.Entities.Enums.SendingState.Sending:
                    MessagedSentSending(res.Entity);
                    break;
                
                case FreedomVoice.Entities.Enums.SendingState.Success:
                    MessagedSentSuccess(res.Entity);
                    break;
            }
        }
        
        public async Task<long?> SendMessage(string currentPhone, string collocutorPhone, string text)
        {
            var res = await _messagesService.SendMessage(currentPhone, collocutorPhone, text);
            
            switch (res.State)
            {
                case FreedomVoice.Entities.Enums.SendingState.Error:
                    MessagedSentError(res.Entity);
                    break;
                
                case FreedomVoice.Entities.Enums.SendingState.Sending:
                    MessagedSentSending(res.Entity);
                    break;
                
                case FreedomVoice.Entities.Enums.SendingState.Success:
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

        public async Task<long?> GetConversationId(string currentPhone, string collocutorPhone)
        {
            var conversation = await _conversationService.Get(currentPhone, collocutorPhone);
            return conversation?.Conversation?.Id;
        }

        #region Send message handlers

        private void MessagedSentSuccess(FreedomVoice.Entities.Response.Conversation conversation)
        {
            var lastMessage = conversation?.Messages?.Last();
            if (lastMessage == null)
            {
                //TODO Show error?
                return;
            }
            
            Items.Insert(0, new OutgoingMessageViewModel(lastMessage));
            ItemsChanged?.Invoke(this, new ConversationCollectionEventArgs(Items)); 
        }

        private void MessagedSentError(FreedomVoice.Entities.Response.Conversation conversation)
        {
            //TODO
        }

        private void MessagedSentSending(FreedomVoice.Entities.Response.Conversation conversation)
        {
            //TODO
        }

        #endregion

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
            var res = await _messagesService.GetList(_conversationId.Value, _currentDate, DEFAULT_COUNT, _currentPage);
            HasMore = !res.IsEnd;

            foreach (var row in res.Messages)
            {
                if (row.From == null) continue;
                var dateStr = row.CreatedAt?.ToString("MM/dd/yyyy");
                if (dateStr == null) continue;
                var pack = _rawData.ContainsKey(dateStr) ? _rawData[dateStr] : new List<IChatMessage>();
                if (row.From.PhoneNumber.Equals(PhoneNumber))
                {
                    pack.Add(new OutgoingMessageViewModel(row));
                }
                else
                {
                    pack.Add(new IncomingMessageViewModel(row));
                }
                _rawData[dateStr] = pack;
            }

            _updateItems();
            ItemsChanged?.Invoke(this, new ConversationCollectionEventArgs(Items));
            _isLoading = false;
        }

        private void _updateItems()
        {
            Items = new List<IChatMessage>();
            foreach (var group in _rawData)
            {
                var date = DateTime.MinValue;
                DateTime.TryParseExact(group.Key, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out date);
                Items.AddRange(group.Value);
                Items.Add(new DateMessageViewModel(date));
            }
        }
        
        private void ContactNameProviderOnContactsUpdated(object sender, EventArgs e)
        {
            ContactsUpdated?.Invoke(sender, e);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreedomVoice.Core.Services;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;
using FreedomVoice.Core.ViewModels;

namespace FreedomVoice.Core.Presenters
{
    public class ConversationsEventArgs : EventArgs
    {
        public IEnumerable<ConversationViewModel> Conversations;

        public ConversationsEventArgs(IEnumerable<ConversationViewModel> conversations)
        {
            Conversations = conversations;
        }
    }

    public class ConversationsPresenter : IDisposable
    {
        public List<ConversationViewModel> Items;
        public event EventHandler ItemsChanged;
        private readonly IConversationService _service;
        private readonly IContactNameProvider _nameProvider;
        private readonly IPhoneFormatter _formatter;

        private DateTime _currentDate;
        private int _currentPage;
        private bool _isLoading;
        private const int DefaultCount = 50;

        private string _phoneNumber;

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

        public bool IsLoading
        {
            get => _isLoading;
        }

        public string AccountNumber { get; set; }

        public bool HasMore { get; private set; }
        public string Query { get; set; } = "";

        public ConversationsPresenter()
        {
            _currentDate = DateTime.Now;
            _currentPage = 1;
            Items = new List<ConversationViewModel>();
            HasMore = false;
            _service = ServiceContainer.Resolve<IConversationService>();
            _nameProvider = ServiceContainer.Resolve<IContactNameProvider>();
            _formatter = ServiceContainer.Resolve<IPhoneFormatter>();

            NotificationMessageService.Instance().NewMessageEventHandler += OnNewMessageEventHandler;
            NotificationMessageService.Instance().MessageUpdatedHandler += OnMessageUpdatedHandler;
        }

        ~ConversationsPresenter()
        {
            NotificationMessageService.Instance().NewMessageEventHandler -= OnNewMessageEventHandler;
            NotificationMessageService.Instance().MessageUpdatedHandler -= OnMessageUpdatedHandler;
        }

        public void Dispose()
        {
            NotificationMessageService.Instance().NewMessageEventHandler -= OnNewMessageEventHandler;
            NotificationMessageService.Instance().MessageUpdatedHandler -= OnMessageUpdatedHandler;
        }

        private void OnNewMessageEventHandler(object sender, ConversationEventArg e)
        {
            var conversation = e.Conversation;
            var message = conversation.Messages.FirstOrDefault();
            if (message == null) return;
            if (!message.To.PhoneNumber.Equals(PhoneNumber)) return;

            _updateConversation(conversation);
        }

        void OnMessageUpdatedHandler(object sender, MessageEventArg e)
        {
            var conversation = e.Message.Conversation;
            var message = e.Message;
            if (conversation == null) return;
            if (message == null) return;
            if (!message.From.PhoneNumber.Equals(PhoneNumber)) return;
            conversation.Messages = new List<DAL.DbEntities.Message> {message};
            conversation.SystemPhone = message.From;
            conversation.ToPhone = message.To;

            _updateConversation(conversation);
        }

        private void _updateConversation(DAL.DbEntities.Conversation conversation)
        {
            var viewModel = new ConversationViewModel(conversation, _nameProvider, _formatter);
            var index = Items.FindIndex(model => model.ConversationId == conversation.Id);
            if (index < 0)
            {
                Items.Add(viewModel);
            }
            else
            {
                Items[index] = viewModel;
            }

            Items = Items.OrderByDescending(item => item.DateTime).ToList();
            ItemsChanged?.Invoke(this, new ConversationsEventArgs(Items));
        }


        public async void ReloadAsync()
        {
            if (_isLoading) return;
            _currentDate = DateTime.Now;
            _currentPage = 1;
            Items = new List<ConversationViewModel>();
            await _PerformLoading();
        }

        public async void LoadMoreAsync()
        {
            if (!HasMore && !_isLoading) return;
            _currentPage++;
            await _PerformLoading();
        }

        private async Task _PerformLoading()
        {
            _isLoading = true;
            Entities.Texting.ConversationListResponse res;

            res = string.IsNullOrEmpty(Query)
                ? await _service.GetList(_phoneNumber, _currentDate, DefaultCount, _currentPage)
                : await _service.Search(_phoneNumber, Query, _nameProvider.SearchNumbers(Query).ToArray(), _currentDate,
                    DefaultCount, _currentPage);

            HasMore = !res.IsEnd;

            Items.AddRange(res.Conversations?.Select(row =>
                                   new ConversationViewModel(row, _nameProvider, _formatter))
                               .Where(row => row.ConversationId > 0) ?? throw new Exception()
            );

            ItemsChanged?.Invoke(this, new ConversationsEventArgs(Items));
            _isLoading = false;
        }
    }
}
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
        private readonly IMessagesService _service;
        private readonly IContactNameProvider _nameProvider;
        private readonly DateTime _currentDate;
        private int _currentPage;
        private bool _isLoading = false;

        public event EventHandler ItemsChanged;
        public List<IChatMessage> Items;
        public bool HasMore { get; private set; }
        private long _conversationId;
        public long ConversationId
        {
            get => _conversationId;
            set
            {
                if (value == _conversationId)
                    return;
                _conversationId = value;

            }
        }

        private string _phoneNumber;
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

        public ConversationPresenter()
        {
            _currentDate = DateTime.Now;
            _currentPage = 1;
            HasMore = false;
            _service = ServiceContainer.Resolve<IMessagesService>();
            _nameProvider = ServiceContainer.Resolve<IContactNameProvider>();
        }

        public async void ReloadAsync()
        {
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
            _currentPage = 1;
            var res = await _service.GetList(_conversationId, _currentDate, 50, _currentPage);
            HasMore = !res.IsEnd;
            Items = new List<IChatMessage>();

            var groups = res.Messages.GroupBy(arg =>
            {
                var date = arg.From.PhoneNumber.Equals(PhoneNumber) ? arg.SentAt : arg.ReceivedAt;
                return date?.ToString("MM/dd/yyyy") ?? "";
            });
            foreach (var group in groups)
            {
                Items.Add(new DateMessageViewModel(DateTime.Parse(group.Key)));
                foreach (var row in group.ToList()) 
                {
                    if (row.From.PhoneNumber.Equals(PhoneNumber))
                    {
                        Items.Add(new OutgoingMessageViewModel(row));
                    }
                    else
                    {
                        Items.Add(new IncomingMessageViewModel(row));
                    }
                }
            }
            _isLoading = false;
            ItemsChanged?.Invoke(this, new ConversationCollectionEventArgs(Items));
        }
    }
}

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
        private DateTime _currentDate;
        private int _currentPage;
        private bool _isLoading;
        private const int DEFAULT_COUNT = 50;
        private Dictionary<string, List<IChatMessage>> _rawData;

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
            ResetState();
            _service = ServiceContainer.Resolve<IMessagesService>();
            _nameProvider = ServiceContainer.Resolve<IContactNameProvider>();
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
            _isLoading = true;
            var res = await _service.GetList(_conversationId, _currentDate, DEFAULT_COUNT, _currentPage);
            HasMore = !res.IsEnd;

            foreach (var row in res.Messages)
            {
                if (row.From == null) continue;
                var date = row.From.PhoneNumber.Equals(PhoneNumber) ? row.SentAt : row.ReceivedAt;
                var dateStr = date?.ToString("MM/dd/yyyy");
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
                Items.AddRange(group.Value);
                Items.Add(new DateMessageViewModel(DateTime.Parse(group.Key)));
            }
        }
    }
}

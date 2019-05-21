using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
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

    public class ConversationsPresenter
    {
        public List<ConversationViewModel> Items;
        public event EventHandler ItemsChanged;
        private readonly IConversationService _service;
        private readonly IContactNameProvider _nameProvider;
        private DateTime _currentDate;
        private int _currentPage;
        private bool _isLoading = false;
        private const int DEFAULT_COUNT = 50;

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

        public bool HasMore { get; private set; }


        public ConversationsPresenter()
        {
            _currentDate = DateTime.Now;
            Console.WriteLine(_currentDate.ToString("t"));
            _currentPage = 1;
            Items = new List<ConversationViewModel>();
            HasMore = false;
            _service = ServiceContainer.Resolve<IConversationService>();
            _nameProvider = ServiceContainer.Resolve<IContactNameProvider>();
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
            var res = await _service.GetList(_phoneNumber, _currentDate, DEFAULT_COUNT, _currentPage);
            HasMore = !res.IsEnd;


            Items.AddRange(res.Conversations?.Select(row =>
                new ConversationViewModel(row, _nameProvider)));
            
            ItemsChanged?.Invoke(this, new ConversationsEventArgs(Items));
            _isLoading = false;
        }
    }
}

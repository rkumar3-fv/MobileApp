using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.Core.Services.Interfaces;
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

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (value == _phoneNumber)
                    return;
                _phoneNumber = value;

            }
        }

        public bool HasMore { get; private set; }


        public ConversationsPresenter(IConversationService service, IContactNameProvider nameProvider)
        {
            Items = new List<ConversationViewModel>();
            HasMore = false;
            _service = service;
            _nameProvider = nameProvider;
        }

        public async Task ReloadAsync() {
            var res = await _service.GetList("test", 50, 1);
            HasMore = !res.IsEnd;
            Items = new List<ConversationViewModel>();
            if (res.Conversations != null)
            {
                foreach (var row in res.Conversations)
                {
                    Items.Add(new ConversationViewModel(row, _nameProvider));
                }
            }

            ItemsChanged?.Invoke(this, new ConversationsEventArgs(Items));
        }

        public void LoadMore() { 
        }


    }
}

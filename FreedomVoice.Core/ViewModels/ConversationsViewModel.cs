using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.Core.Services;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.DAL.DbEntities;

namespace FreedomVoice.Core.ViewModels
{

    public class ConversationsEventArgs : EventArgs
    {
        public IEnumerable<Conversation> Conversations;

        public ConversationsEventArgs(IEnumerable<Conversation> conversations)
        {
            Conversations = conversations;
        }
    }

    public class ConversationsViewModel
    {

        public List<Conversation> Items;
        public event EventHandler ItemsChanged;
        private IConversationService _service;

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

        private bool _hasMore;
        public bool HasMore {
            get => _hasMore;
        }


        public ConversationsViewModel(IConversationService service)
        {
            Items = new List<Conversation>();
            _hasMore = false;
            _service = service;
        }

        public async Task ReloadAsync() {
            var res = await _service.GetList("test", 50, 1);
            _hasMore = !res.IsEnd;
            Items = new List<Conversation>(res.Conversations);
            ItemsChanged.Invoke(this, new ConversationsEventArgs(res.Conversations));
        }

        public void LoadMore() { 
        }


    }
}

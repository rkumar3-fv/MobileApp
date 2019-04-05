using System;
namespace FreedomVoice.Core.ViewModels
{
    public class ConversationsViewModel
    {
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


        public ConversationsViewModel()
        {

        }

        public void ReloadData() {
        
        }

    }
}

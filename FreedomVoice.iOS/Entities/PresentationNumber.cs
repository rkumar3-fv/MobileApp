using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;

namespace FreedomVoice.iOS.Entities
{
    public class PresentationNumber
    {
        private readonly IPhoneFormatter _phoneFormatter = ServiceContainer.Resolve<IPhoneFormatter>();
        
        public string PhoneNumber { get; }
        public string FormattedPhoneNumber => _phoneFormatter.Format(PhoneNumber);

        public bool IsSelected { get; set; }
        
        public PresentationNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }
    }
}
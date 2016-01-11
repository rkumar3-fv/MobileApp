namespace FreedomVoice.iOS.Entities
{
    public class Account
    {
        public Account(string phoneNumber)
        {
            PhoneNumber = phoneNumber;  
        }

        public string PhoneNumber { get; }

        public string FormattedPhoneNumber => $"({PhoneNumber.Substring(0, 3)}) {PhoneNumber.Substring(3, 3)}-{PhoneNumber.Substring(6, 4)}";
    }
}
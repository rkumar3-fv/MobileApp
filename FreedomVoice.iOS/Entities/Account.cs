namespace FreedomVoice.iOS.Entities
{
    public class Account
    {
        public string PhoneNumber { get; set; }

        public string FormattedPhoneNumber => $"({PhoneNumber.Substring(0, 3)}) {PhoneNumber.Substring(3, 3)}-{PhoneNumber.Substring(6, 4)}";
    }
}
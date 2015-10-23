namespace FreedomVoice.iOS.Entities
{
    public class PresentationNumber
    {
        public PresentationNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public string PhoneNumber { get; set; }

        public string FormattedPhoneNumber => $"({PhoneNumber.Substring(0, 3)}) {PhoneNumber.Substring(3, 3)}-{PhoneNumber.Substring(6, 4)}";

        public bool IsSelected { get; set; } = false;
    }
}
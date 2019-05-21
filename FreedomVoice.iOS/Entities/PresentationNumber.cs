namespace FreedomVoice.iOS.Entities
{
    public class PresentationNumber
    {
        public PresentationNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public string PhoneNumber { get; }

        public string FormattedPhoneNumber {
            get
            {
                if (string.IsNullOrWhiteSpace(PhoneNumber))
                {
                    return "";
                }
                if (PhoneNumber.Length <= 3)
                {
                    return PhoneNumber;
                }
                 
                if (PhoneNumber.Length <= 6)
                {
                   return $"({PhoneNumber.Substring(0, 3)})" +
                          $" {PhoneNumber.Substring(3, PhoneNumber.Length - 3)}";
                }
                
                if(PhoneNumber.Length <= 9)
                {
                    return $"({PhoneNumber.Substring(0, 3)}) " +
                           $"{PhoneNumber.Substring(3, 3)}-" +
                           $"{PhoneNumber.Substring(6, PhoneNumber.Length - 6)}";
                }
                
                return $"({PhoneNumber.Substring(0, 3)})" +
                       $" {PhoneNumber.Substring(3, 3)}-" +
                       $"{PhoneNumber.Substring(6, 4)} " +
                       $"{PhoneNumber.Substring(10, PhoneNumber.Length - 10)}";
            }
        }

    public bool IsSelected { get; set; }
    }
}
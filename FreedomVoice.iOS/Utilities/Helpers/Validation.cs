using System.Text.RegularExpressions;
using FreedomVoice.Core.Utils;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Validation
    {
        public static bool IsValidEmail(string emailAddress)
        {
            return DataFormatUtils.IsValidEmail(emailAddress);
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            return !string.IsNullOrEmpty(phoneNumber) && Regex.Match(phoneNumber, @"^\d{10}$").Success;
        }

        public static bool IsValidPassword(string password)
        {
            return !string.IsNullOrEmpty(password);
        }
    }
}
using System.Text.RegularExpressions;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Validation
    {
        public static bool IsValidEmail(string emailAddress)
        {
            return !string.IsNullOrEmpty(emailAddress) && Regex.Match(emailAddress, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").Success;
        }

        public static bool IsValidPassword(string password)
        {
            return !string.IsNullOrEmpty(password);
        }
    }
}
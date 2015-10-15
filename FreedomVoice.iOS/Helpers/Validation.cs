using System.Text.RegularExpressions;

namespace FreedomVoice.iOS.Helpers
{
    public static class Validation
    {
        public static bool IsValidEmail(string emailAddress)
        {
            return !string.IsNullOrEmpty(emailAddress) && Regex.Match(emailAddress, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$").Success;
        }
    }
}
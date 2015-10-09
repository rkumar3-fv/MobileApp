using System.Text.RegularExpressions;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    /// <summary>
    /// Data format actions
    /// </summary>
    public static class DataFormatUtils
    {
        private const string PhoneRegex = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";
        private const string PhoneExtRegex = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})[x-. ]?([0-9]{3})$";
        /// <summary>
        /// Convert string to phone number
        /// </summary>
        /// <param name="unformatted">unformatted string</param>
        /// <returns>formatted phone number</returns>
        public static string ToPhoneNumber(string unformatted)
        {
            var phoneRegex = new Regex(PhoneRegex);
            return phoneRegex.IsMatch(unformatted) ? phoneRegex.Replace(unformatted, "($1) $2-$3") : unformatted;
        }

        /// <summary>
        /// Convert string to phone number with extension
        /// </summary>
        /// <param name="unformatted">unformatted string</param>
        /// <returns>formatted phone number</returns>
        public static string ToPhoneNumberWithExt(string unformatted)
        {
            var phoneRegex = new Regex(PhoneExtRegex);
            return phoneRegex.IsMatch(unformatted) ? phoneRegex.Replace(unformatted, "($1) $2-$3 x$4") : unformatted;
        }
    }
}
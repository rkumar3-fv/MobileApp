using System.Text.RegularExpressions;

namespace FreedomVoice.Core.Utils
{
    public static class DataFormatUtils
    {
        private const string Phone3Regex = @"^\(?([0-9]{3})\)$";
        private const string Phone4Regex = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{1,3})$";
        private const string Phone7Regex = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{1,4})$";
        private const string PhoneExtRegex = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})[x-. ]?([0-9]{3})$";

        /// <summary>
        /// Convert string to phone number
        /// </summary>
        /// <param name="unformatted">unformatted string</param>
        /// <returns>formatted phone number</returns>
        public static string ToPhoneNumber(string unformatted)
        {
            var phone7Regex = new Regex(Phone7Regex);
            var phone4Regex = new Regex(Phone4Regex);
            var phone3Regex = new Regex(Phone3Regex);
            if (phone7Regex.IsMatch(unformatted))
                return phone7Regex.Replace(unformatted, "($1) $2-$3");
            if (phone4Regex.IsMatch(unformatted))
                return phone4Regex.Replace(unformatted, "($1) $2");
            if (phone3Regex.IsMatch(unformatted))
                return phone3Regex.Replace(unformatted, "($1)");
            return unformatted;
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

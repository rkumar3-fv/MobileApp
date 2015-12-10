using System;
using System.Text.RegularExpressions;

namespace FreedomVoice.Core.Utils
{
    /// <summary>
    /// Data format actions
    /// </summary>
    public static class DataFormatUtils
    {
        private const string Phone3Regex = @"^\(?([0-9]{3})\)$";
        private const string Phone4Regex = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{1,3})$";
        private const string Phone7Regex = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{1,4})$";
        private const string Phone11Regex= @"^([0-9]{1})[-. ]?\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";

        private const string PlusPhone4Regex = @"^([+]{1})?([0-9]{1})[-. ]?\(?([0-9]{1,3})\)$";
        private const string PlusPhone7Regex = @"^([+]{1})?([0-9]{1})[-. ]?\(?([0-9]{3})\)?[-. ]?([0-9]{1,3})$";
        private const string PlusPhone11Regex = @"^([+]{1})?([0-9]{1})[-. ]?\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{1,4})$";

        private const string PhoneExtRegex = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})[x-. ]?([0-9]{3})$";

        /// <summary>
        /// Convert string to phone number
        /// </summary>
        /// <param name="unformatted">unformatted string</param>
        /// <returns>formatted phone number</returns>
        public static string ToPhoneNumber(string unformatted)
        {
            if (unformatted.StartsWith("+"))
            {
                var phone11Regex = new Regex(PlusPhone11Regex);
                var phone7Regex = new Regex(PlusPhone7Regex);
                var phone4Regex = new Regex(PlusPhone4Regex);
                if (phone11Regex.IsMatch(unformatted))
                    return phone11Regex.Replace(unformatted, "$1$2 ($3) $4-$5");
                if (phone7Regex.IsMatch(unformatted))
                    return phone7Regex.Replace(unformatted, "$1$2 ($3) $4");
                if (phone4Regex.IsMatch(unformatted))
                    return phone4Regex.Replace(unformatted, "$1$2 ($3)");
            }
            else
            {
                var phone11Regex = new Regex(Phone11Regex);
                var phone7Regex = new Regex(Phone7Regex);
                var phone4Regex = new Regex(Phone4Regex);
                var phone3Regex = new Regex(Phone3Regex);
                if (phone11Regex.IsMatch(unformatted))
                    return phone11Regex.Replace(unformatted, "$1 ($2) $3-$4");
                if (phone7Regex.IsMatch(unformatted))
                    return phone7Regex.Replace(unformatted, "($1) $2-$3");
                if (phone4Regex.IsMatch(unformatted))
                    return phone4Regex.Replace(unformatted, "($1) $2");
                if (phone3Regex.IsMatch(unformatted))
                    return phone3Regex.Replace(unformatted, "($1)");
            }
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

        public static string NormalizePhone(string phone)
        {
            var r = new Regex("(?:[^0-9+]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return r.Replace(phone, string.Empty);
        }

        /// <summary>
        /// Get formatted duration
        /// </summary>
        /// <param name="length">unformatted duration</param>
        /// <returns>formated duration</returns>
        public static string ToDuration(int length)
        {
            return length / 3600 == 0 ? $"{(length / 60) % 60}:{length % 60:00}" : $"{length / 3600}:{(length / 60) % 60:00}:{length % 60:00}";
        }

        /// <summary>
        /// Date formatter
        /// </summary>
        /// <param name="date">date for formatting</param>
        /// <returns>formatted date</returns>
        public static string ToFullFormattedDate( DateTime date)
        {
            return date.ToString("MM/dd/yyyy hh:mm tt");
        }

        /// <summary>
        /// Date formatter
        /// </summary>
        /// <param name="yesterdayLabel">label for "yesterday"</param>
        /// <param name="date">date for formatting</param>
        /// <returns>formatted date</returns>
        public static string ToFormattedDate(string yesterdayLabel, DateTime date)
        {
            var current = DateTime.Now;

            if ((date.DayOfYear == current.DayOfYear) && (date.Year == current.Year))
                return date.ToString("hh:mm tt");

            if ((date.DayOfYear == current.DayOfYear - 1) && (date.Year == current.Year))
                return $"{yesterdayLabel} {date.ToString("hh:mm tt")}";

            return date.ToString("MM/dd/yyyy hh:mm tt");
        }

        /// <summary>
        /// Date short formatter
        /// </summary>
        /// <param name="yesterdayLabel">label for "yesterday"</param>
        /// <param name="date">date for formatting</param>
        /// <returns>formatted date</returns>
        public static string ToShortFormattedDate(string yesterdayLabel, DateTime date)
        {
            var current = DateTime.Now;

            if ((date.DayOfYear == current.DayOfYear) && (date.Year == current.Year))
                return date.ToString("hh:mm tt");

            if ((date.DayOfYear == current.DayOfYear - 1) && (date.Year == current.Year))
                return yesterdayLabel;

            return date.ToString("MM/dd/yy");
        }

        /// <summary>
        /// Pages formatter
        /// </summary>
        /// <param name="pages">pages for formatting</param>
        /// <returns>formatted pages string</returns>
        public static string PagesToFormattedString(int pages)
        {
            return pages == 1 ? $"{pages} page" : $"{pages} pages";
        }
    }
}
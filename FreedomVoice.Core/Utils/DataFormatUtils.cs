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
        /// <param name="yesterdayLabel">label for "yesterday"</param>
        /// <param name="date">date for formatting</param>
        /// <returns>formatted date</returns>
        public static string ToFormattedDate(string yesterdayLabel, DateTime date)
        {
            var current = DateTime.Now;
            if ((date.DayOfYear == current.DayOfYear) && (date.Year == current.Year))
                return date.ToString("HH:mm tt");
            if ((date.DayOfYear == current.DayOfYear) && (date.Year == current.Year))
                return $"{yesterdayLabel} {date.ToString("HH:mm tt")}";
            return date.ToString("MM/dd/hhhh HH:mm tt");
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
                return date.ToString("HH:mm tt");
            if ((date.DayOfYear == current.DayOfYear) && (date.Year == current.Year))
                return yesterdayLabel;
            return date.ToString("MM/dd/hh");
        }
    }
}
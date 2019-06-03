using System;
using System.Net;
using System.Text.RegularExpressions;

namespace FreedomVoice.Core.Utils
{
    /// <summary>
    /// Data format actions
    /// </summary>
    public static class DataFormatUtils
    {
        private const string SpaceFlag = "--SPACE--";

        public static string NormalizeSearchText(string text)
        {
            var r = new Regex("[^*#0-9a-z+]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return r.Replace(text, string.Empty);
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
            return date.ToString("MM/dd/yyyy hh:mm tt").ToUpper();
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
                return date.ToString("hh:mm tt").ToUpper();

            if ((date.DayOfYear == current.DayOfYear - 1) && (date.Year == current.Year))
                return $"{yesterdayLabel} {date.ToString("hh:mm tt").ToUpper()}";

            return date.ToString("MM/dd/yyyy hh:mm tt").ToUpper();
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
                return date.ToString("hh:mm tt").ToUpper();

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

        public static bool IsValidEmail(string strIn)
        {
            if (string.IsNullOrEmpty(strIn))
                return false;

            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static string UrlEncodeWithSpaces(string value)
        {
            var url = value.Replace(" ", SpaceFlag);
            url = WebUtility.UrlEncode(url);
            return url.Replace(SpaceFlag, "%20").Replace("(", "%28").Replace(")", "%29").Replace("&", "%26");
        }
    }
}
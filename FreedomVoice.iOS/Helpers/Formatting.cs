using System;
using Foundation;

namespace FreedomVoice.iOS.Helpers
{
    public static class Formatting
    {
        public static string DateTimeFormat(NSDate dateTime)
        {
            var dateFormatter = new NSDateFormatter { DoesRelativeDateFormatting = true, DateStyle = NSDateFormatterStyle.Short };
            var timeFormatter = new NSDateFormatter { DateFormat = @" hh:mm a" };

            return dateFormatter.ToString(dateTime) + timeFormatter.ToString(dateTime);
        }

        public static string SecondsToFormattedString(double seconds)
        {
            return TimeSpan.FromSeconds(seconds).ToString(seconds < 600 ? @"m\:ss" : @"mm\:ss");
        }

        public static string PagesToFormattedString(double pages)
        {
            return (int)pages == 1 ? $"{pages} page" : $"{pages} pages";
        }
    }
}
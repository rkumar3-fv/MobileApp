using System;
using Foundation;

namespace FreedomVoice.iOS.Utilities
{
    public static class NSDateDateTimeExtensions
    {
        private static DateTime _reference = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Local); // last zero is milliseconds

        /// <summary>Convert a DateTime to NSDate</summary>
        /// <param name="dt">The DateTime to convert</param>
        /// <returns>An NSDate</returns>
        public static NSDate ToNSDate(this DateTime dt)
        {
            return NSDate.FromTimeIntervalSinceReferenceDate(dt.SecondsSinceNSRefenceDate());
        }

        /// <summary>Convert an NSDate to DateTime</summary>
        /// <param name="nsDate">The NSDate to convert</param>
        /// <returns>A DateTime</returns>
        public static DateTime ToDateTime(this NSDate nsDate)
        {
            // We loose granularity below millisecond range but that is probably ok
            return _reference.AddSeconds(nsDate.SecondsSinceReferenceDate);
        }

        /// <summary>Returns the seconds interval for a DateTime from NSDate reference data of January 1, 2001</summary>
        /// <param name="dt">The DateTime to evaluate</param>
        /// <returns>The seconds since NSDate reference date</returns>
        private static double SecondsSinceNSRefenceDate(this DateTime dt)
        {
            return (dt - _reference).TotalSeconds;
        }
    }
}
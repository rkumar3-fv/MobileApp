using System;
using Android.Runtime;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Recent entity
    /// </summary>
    [Preserve(AllMembers = true)]
    public class Recent : IEquatable<Recent>
    {
        /// <summary>
        /// Destination phone number
        /// </summary>
        public string PhoneNumber { get; }

        /// <summary>
        /// Date of start calling
        /// </summary>
        public DateTime CallDate { get; }

        /// <summary>
        /// Create recent
        /// </summary>
        /// <param name="phoneNumber">Destination phone number</param>
        public Recent(string phoneNumber) : this (phoneNumber, DateTime.Now)
        {}

        /// <summary>
        /// Create recent
        /// </summary>
        /// <param name="phoneNumber">Destination phone number</param>
        /// <param name="date">call date</param>
        public Recent(string phoneNumber, DateTime date)
        {
            PhoneNumber = phoneNumber;
            CallDate = date;
        }

        public bool Equals(Recent other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || string.Equals(PhoneNumber, other.PhoneNumber);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Recent) obj);
        }

        public override int GetHashCode()
        {
            return PhoneNumber?.GetHashCode() ?? 0;
        }
    }
}
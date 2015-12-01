using System;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Recent entity
    /// </summary>
    public class Recent : IEquatable<Recent>
    {
        private int _count;

        /// <summary>
        /// CallReservation - OK
        /// </summary>
        public const int ResultOk = 1;

        /// <summary>
        /// CallReservation - BadRequest
        /// </summary>
        public const int ResultFail = 0;

        /// <summary>
        /// Destination phone number
        /// </summary>
        public string PhoneNumber { get; }

        /// <summary>
        /// Date of start calling
        /// </summary>
        public DateTime CallDate { get; }

        /// <summary>
        /// Recents counter
        /// </summary>
        public int Count {
            get { return _count; }
            set
            {
                if (_count < value)
                    _count = value;
            }
        }

        /// <summary>
        /// Create recent
        /// </summary>
        /// <param name="phoneNumber">Destination phone number</param>
        public Recent(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
            CallDate = DateTime.Now;
            Count = 1;
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
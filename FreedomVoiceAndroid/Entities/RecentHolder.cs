using System;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    public class RecentHolder : IEquatable<RecentHolder>
    {
        private int _counter;

        public Recent SingleRecent { get; }

        public int Count
        {
            get
            {
                return _counter;
            }
            set
            {
                if ((value > 1)&&(value > _counter))
                    _counter = value;
            }
        }

        public RecentHolder(Recent recent, int count)
        {
            SingleRecent = recent;
            _counter = count;
        }

        public RecentHolder(Recent recent) : this (recent, 1)
        { }

        public bool Equals(RecentHolder other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _counter == other._counter && Equals(SingleRecent, other.SingleRecent);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((RecentHolder) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return SingleRecent?.GetHashCode() ?? 0;
            }
        }
    }
}
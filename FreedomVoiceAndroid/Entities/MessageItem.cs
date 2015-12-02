using System;
using Android.OS;
using Android.Runtime;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Abstract item for messages tab
    /// </summary>
    [Preserve(AllMembers = true)]
    public abstract class MessageItem : Entity, IEquatable<MessageItem>
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; }

        protected MessageItem(int id)
        {
            Id = id;
        }

        protected MessageItem(Parcel parcel)
        {
            Id = parcel.ReadInt();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            dest.WriteInt(Id);
        }

        public bool Equals(MessageItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((MessageItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ Id;
            }
        }
    }
}
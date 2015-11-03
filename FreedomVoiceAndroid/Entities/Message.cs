using System;
using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    public class Message : MessageItem, IEquatable<Message>
    {
        public const int TypeFax = 0;
        public const int TypeVoice = 1;
        public const int TypeRec = 2;

        /// <summary>
        /// Unread flag
        /// </summary>
        public bool Unread { get; }

        /// <summary>
        /// Message name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Recipient's name
        /// </summary>
        public string FromName { get; }
        
        /// <summary>
        /// Recipient's number
        /// </summary>
        public string FromNumber { get; }

        /// <summary>
        /// Message date
        /// </summary>
        public DateTime MessageDate { get; }

        /// <summary>
        /// Message type
        /// </summary>
        public int MessageType { get; }

        /// <summary>
        /// Message length
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Link to the media content
        /// </summary>
        public string AttachUrl { get; }

        public Message(int id, string name, string fromName, string fromNumber, DateTime date, int type, bool unread, int length, string attach) : base(id)
        {
            Name = name;
            FromName = fromName;
            FromNumber = fromNumber;
            MessageDate = date;
            MessageType = type;
            Unread = unread;
            Length = length;
            AttachUrl = attach;
        }

        private Message(Parcel parcel) : base (parcel)
        {
            Name = parcel.ReadString();
            FromName = parcel.ReadString();
            FromNumber = parcel.ReadString();
            MessageDate = DateTime.FromFileTime(parcel.ReadLong());
            MessageType = parcel.ReadInt();
            Unread = (parcel.ReadByte() == 1);
            Length = parcel.ReadInt();
            AttachUrl = parcel.ReadString();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(Name);
            dest.WriteString(FromName);
            dest.WriteString(FromNumber);
            dest.WriteLong(MessageDate.ToFileTime());
            dest.WriteInt(MessageType);
            dest.WriteByte(Unread?(sbyte)1:(sbyte)0);
            dest.WriteInt(Length);
            dest.WriteString(AttachUrl);
        }

        [ExportField("CREATOR")]
        public static ParcelableMessageCreator InitializeCreator()
        {
            return new ParcelableMessageCreator();
        }

        public class ParcelableMessageCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new Message(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }

        public bool Equals(Message other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Unread == other.Unread && string.Equals(Name, other.Name) && string.Equals(FromName, other.FromName) 
                && string.Equals(FromNumber, other.FromNumber) && MessageDate.Equals(other.MessageDate) && MessageType == other.MessageType && Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Message) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ Unread.GetHashCode();
                hashCode = (hashCode*397) ^ (Name?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (FromName?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (FromNumber?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ MessageDate.GetHashCode();
                hashCode = (hashCode*397) ^ MessageType;
                hashCode = (hashCode*397) ^ Length;
                return hashCode;
            }
        }
    }
}
using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    public class Message : MessageItem
    {
        public const int TypeFax = 0;
        public const int TypeVoice = 1;
        public const int TypeRec = 2;

        /// <summary>
        /// Unread flag
        /// </summary>
        public bool Unread { get; set; }

        /// <summary>
        /// Message name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Recipient's name
        /// </summary>
        public string FromName { get; set; }
        
        /// <summary>
        /// Recipient's number
        /// </summary>
        public string FromNumber { get; set; }

        /// <summary>
        /// Message date
        /// </summary>
        public string MessageDate { get; set; }

        /// <summary>
        /// Message type
        /// </summary>
        public int MessageType { get; set; }

        /// <summary>
        /// Message length
        /// </summary>
        public int Length { get; set; }

        public Message(int id, string name, string fromName, string fromNumber, string date, int type, bool unread, int length) : base(id)
        {
            Name = name;
            FromName = fromName;
            FromNumber = fromNumber;
            MessageDate = date;
            MessageType = type;
            Unread = unread;
            Length = length;
        }

        private Message(Parcel parcel) : base (parcel)
        {
            Name = parcel.ReadString();
            FromName = parcel.ReadString();
            FromNumber = parcel.ReadString();
            MessageDate = parcel.ReadString();
            MessageType = parcel.ReadInt();
            Unread = (parcel.ReadByte() == 1);
            Length = parcel.ReadInt();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(Name);
            dest.WriteString(FromName);
            dest.WriteString(FromNumber);
            dest.WriteString(MessageDate);
            dest.WriteInt(MessageType);
            dest.WriteByte(Unread?(sbyte)1:(sbyte)0);
            dest.WriteInt(Length);
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
    }
}
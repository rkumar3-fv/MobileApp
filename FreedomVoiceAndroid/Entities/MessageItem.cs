using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Abstract item for messages tab
    /// </summary>
    public abstract class MessageItem : Entity
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

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
    }
}
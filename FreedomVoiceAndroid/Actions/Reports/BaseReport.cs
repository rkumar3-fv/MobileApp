using Android.OS;
using Android.Runtime;
using Java.Lang;
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;

namespace com.FreedomVoice.MobileApp.Android.Actions.Reports
{
    /// <summary>
    /// Base file action state report
    /// </summary>
    [Preserve(AllMembers = true)]
    public abstract class BaseReport : BaseAction
    {
        /// <summary>
        /// Message ID
        /// </summary>
        public int Id { get; }

        public Message Msg { get; }

        protected BaseReport(int id, Message msg)
        {
            Id = id;
            Msg = msg;
        }

        protected BaseReport(Parcel parcel)
        {
            Id = parcel.ReadInt();
            Msg = parcel.ReadParcelable(ClassLoader.SystemClassLoader).JavaCast<Message>();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            dest.WriteInt(Id);
            dest.WriteParcelable(Msg, flags);
        }
    }
}
using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Actions.Reports
{
    /// <summary>
    /// Base file action state report
    /// </summary>
    public abstract class BaseReport : BaseAction
    {
        /// <summary>
        /// Message ID
        /// </summary>
        public int Id { get; }

        protected BaseReport(int id)
        {
            Id = id;
        }

        protected BaseReport(Parcel parcel)
        {
            Id = parcel.ReadInt();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            dest.WriteInt(Id);
        }
    }
}
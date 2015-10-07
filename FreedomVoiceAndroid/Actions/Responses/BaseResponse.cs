using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Abstract response action
    /// </summary>
    public abstract class BaseResponse : BaseAction
    {
        protected BaseResponse(long requestId)
        {
            RequestId = requestId;
        }

        protected BaseResponse(Parcel parcel)
        {
            RequestId = parcel.ReadLong();
        }

        /// <summary>
        /// Request ID for response
        /// </summary>
        public long RequestId { get; }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            dest.WriteLong(RequestId);
        }
    }
}
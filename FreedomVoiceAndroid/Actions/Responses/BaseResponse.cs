using System;
using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Abstract response action
    /// <see href="https://api.freedomvoice.com/Help">FreedomVoice REST API</see>
    /// </summary>
    public abstract class BaseResponse : BaseAction, IEquatable<BaseResponse>
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

        public bool Equals(BaseResponse other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RequestId == other.RequestId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((BaseResponse) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ RequestId.GetHashCode();
            }
        }
    }
}
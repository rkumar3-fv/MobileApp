using System;
using Android.OS;
using Android.Runtime;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Polling interval response
    /// <see href="https://api.freedomvoice.com/Help/Api/GET-api-v1-settings-pollingInterval">API - Get polling interval</see>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class GetPollingResponse : BaseResponse, IEquatable<GetPollingResponse>
    {
        /// <summary>
        /// Polling interval in miliseconds
        /// </summary>
        public double PollingInterval { get; }

        public GetPollingResponse(long requestId, double polling) : base(requestId)
        {
            PollingInterval = polling;
        }

        private GetPollingResponse(Parcel parcel) : base (parcel)
        {
            PollingInterval = parcel.ReadDouble();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteDouble(PollingInterval);
        }

        [ExportField("CREATOR")]
        public static ParcelablePollingResponseCreator InitializeCreator()
        {
            return new ParcelablePollingResponseCreator();
        }

        public class ParcelablePollingResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new GetPollingResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }

        public bool Equals(GetPollingResponse other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && PollingInterval.Equals(other.PollingInterval);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((GetPollingResponse) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ PollingInterval.GetHashCode();
            }
        }
    }
}
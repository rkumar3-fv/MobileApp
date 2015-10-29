using System;
using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Accounts list response
    /// <see href="https://api.freedomvoice.com/Help/Api/POST-api-v1-systems-systemPhoneNumber-createCallReservation">API - Create call reservation</see>
    /// </summary>
    public class CallReservationResponse : BaseResponse, IEquatable<CallReservationResponse>
    {
        /// <summary>
        /// Service number for dialing
        /// </summary>
        public string ServiceNumber { get; }

        /// <summary>
        /// Response for calling reservation
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="serviceNumber">Number for dialing</param>
        public CallReservationResponse(long requestId, string serviceNumber) : base(requestId)
        {
#if DEBUG
            ServiceNumber = $"+1{serviceNumber}";
#else
            ServiceNumber = serviceNumber;
#endif
        }

        public CallReservationResponse(Parcel parcel) : base(parcel)
        {
            ServiceNumber = parcel.ReadString();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(ServiceNumber);
        }

        [ExportField("CREATOR")]
        public static ParcelableCallReservationResponseCreator InitializeCreator()
        {
            return new ParcelableCallReservationResponseCreator();
        }

        public class ParcelableCallReservationResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new CallReservationResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }

        public bool Equals(CallReservationResponse other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && string.Equals(ServiceNumber, other.ServiceNumber);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CallReservationResponse) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (ServiceNumber != null ? ServiceNumber.GetHashCode() : 0);
            }
        }
    }
}
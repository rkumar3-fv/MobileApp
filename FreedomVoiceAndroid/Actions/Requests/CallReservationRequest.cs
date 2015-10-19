using System.Threading.Tasks;
using Android.OS;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using FreedomVoice.Core;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Requests
{
    /// <summary>
    /// Call reservation request
    /// <see href="https://api.freedomvoice.com/Help/Api/POST-api-v1-systems-systemPhoneNumber-createCallReservation">API - Create call reservation</see>
    /// </summary>
    public class CallReservationRequest : BaseRequest
    {
        /// <summary>
        /// Current account number
        /// </summary>
        public string Account { get; }

        /// <summary>
        /// Number for showing
        /// </summary>
        public string PresentationNumber { get; }

        /// <summary>
        /// Real phone number (SIM)
        /// </summary>
        public string RealSIMNumber { get; }

        /// <summary>
        /// Number for dialing
        /// </summary>
        public string DialingNumber { get; }

        public CallReservationRequest(long id, string account, string presentationNumber, string simNumber,
            string dialingNumber) : base(id)
        {
            Account = account;
            PresentationNumber = presentationNumber;
            RealSIMNumber = simNumber;
            DialingNumber = dialingNumber;
        }

        private CallReservationRequest(Parcel parcel) : base(parcel)
        {
            Account = parcel.ReadString();
            PresentationNumber = parcel.ReadString();
            RealSIMNumber = parcel.ReadString();
            DialingNumber = parcel.ReadString();
        }

        public override async Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.CreateCallReservation(Account, RealSIMNumber, PresentationNumber, DialingNumber);
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;
            return new CallReservationResponse(Id, asyncRes.Result.SwitchboardPhoneNumber);
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(Account);
            dest.WriteString(PresentationNumber);
            dest.WriteString(RealSIMNumber);
            dest.WriteString(DialingNumber);
        }

        [ExportField("CREATOR")]
        public static ParcelableCallReservationRequestCreator InitializeCreator()
        {
            return new ParcelableCallReservationRequestCreator();
        }

        public class ParcelableCallReservationRequestCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new CallReservationRequest(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
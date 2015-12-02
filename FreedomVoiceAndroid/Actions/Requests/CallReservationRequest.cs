using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
#if DEBUG
using Android.Util;
#endif
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
    [Preserve(AllMembers = true)]
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
        public string RealSimNumber { get; }

        /// <summary>
        /// Number for dialing
        /// </summary>
        public string DialingNumber { get; }

        public CallReservationRequest(long id, string account, string presentationNumber, string simNumber,
            string dialingNumber) : base(id)
        {
            Account = account;
            PresentationNumber = presentationNumber;
            RealSimNumber = simNumber;
            DialingNumber = dialingNumber;
        }

        private CallReservationRequest(Parcel parcel) : base(parcel)
        {
            Account = parcel.ReadString();
            PresentationNumber = parcel.ReadString();
            RealSimNumber = parcel.ReadString();
            DialingNumber = parcel.ReadString();
        }

        public override async Task<BaseResponse> ExecuteRequest()
        {
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} executes request");
#endif
            var asyncRes = await ApiHelper.CreateCallReservation(Account, RealSimNumber, PresentationNumber, DialingNumber);
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} GetResponse {(asyncRes == null ? "NULL" : "NOT NULL")}");
#endif
            if (asyncRes == null) return new ErrorResponse(Id, ErrorResponse.ErrorInternal);
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
            dest.WriteString(RealSimNumber);
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
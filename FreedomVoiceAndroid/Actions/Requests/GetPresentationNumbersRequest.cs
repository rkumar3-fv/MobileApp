using System.Threading.Tasks;
using Android.OS;
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
    /// Presentation numbers list request
    /// <see href="https://api.freedomvoice.com/Help/Api/GET-api-v1-systems-systemPhoneNumber-presentationPhoneNumbers">API - Get presentation numbers</see>
    /// </summary>
    public class GetPresentationNumbersRequest : BaseRequest
    {
        public string Account { get; }

        public GetPresentationNumbersRequest(long id, string account) : base(id)
        {
            Account = account;
        }

        private GetPresentationNumbersRequest(Parcel parcel) : base(parcel)
        {
            Account = parcel.ReadString();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(Account);
        }

        public async override Task<BaseResponse> ExecuteRequest()
        {
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} executes request");
#endif
            var asyncRes = await ApiHelper.GetPresentationPhoneNumbers(Account);
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} GetResponse {(asyncRes == null ? "NULL" : "NOT NULL")}");
#endif
            if (asyncRes == null) return new ErrorResponse(Id, ErrorResponse.ErrorConnection);
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;
            return new GetPresentationNumbersResponse(Id, asyncRes.Result.PhoneNumbers);
        }

        [ExportField("CREATOR")]
        public static ParcelablePresentationNumbersRequestCreator InitializeCreator()
        {
            return new ParcelablePresentationNumbersRequestCreator();
        }

        public class ParcelablePresentationNumbersRequestCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new GetPresentationNumbersRequest(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
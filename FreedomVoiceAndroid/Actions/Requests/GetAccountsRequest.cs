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
    /// Accounts list request
    /// <see href="https://api.freedomvoice.com/Help/Api/GET-api-v1-systems">API - Get accounts request</see>
    /// </summary>
    public class GetAccountsRequest : BaseRequest
    {
        public GetAccountsRequest(long id) : base(id)
        {}

        private GetAccountsRequest(Parcel parcel) : base(parcel)
        {}

        public override async Task<BaseResponse> ExecuteRequest()
        {
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} executes request");
#endif
            var asyncRes = await ApiHelper.GetSystems();
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} GetResponse {(asyncRes == null ? "NULL" : "NOT NULL")}");
#endif
            if (asyncRes == null) return new ErrorResponse(Id, ErrorResponse.ErrorInternal);
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;
            return new GetAccountsResponse(Id, asyncRes.Result.PhoneNumbers);
        }

        [ExportField("CREATOR")]
        public static ParcelableAccountsRequestCreator InitializeCreator()
        {
            return new ParcelableAccountsRequestCreator();
        }

        public class ParcelableAccountsRequestCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new GetAccountsRequest(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
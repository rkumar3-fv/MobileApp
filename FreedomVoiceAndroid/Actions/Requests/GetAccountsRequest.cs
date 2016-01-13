using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
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
    [Preserve(AllMembers = true)]
    public class GetAccountsRequest : BaseRequest
    {
        public GetAccountsRequest(long id) : base(id)
        {}

        private GetAccountsRequest(Parcel parcel) : base(parcel)
        {}

        public override async Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.GetSystems();
            if (asyncRes == null) return new ErrorResponse(Id, ErrorResponse.ErrorInternal, "Response is NULL");
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code, $"{asyncRes.HttpCode} - {asyncRes.JsonText}");
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
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
    /// Polling interval request
    /// <see href="https://api.freedomvoice.com/Help/Api/GET-api-v1-settings-pollingInterval">API - Get polling interval</see>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class GetPollingRequest : BaseRequest
    {
        public GetPollingRequest(long id) : base(id)
        { }

        private GetPollingRequest(Parcel parcel) : base(parcel)
        { }

        public override async Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.GetPollingInterval();
            if (asyncRes == null) return new ErrorResponse(Id, ErrorResponse.ErrorInternal, "Response is NULL");
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code, $"{asyncRes.HttpCode} - {asyncRes.JsonText}");
            if (errorResponse != null)
                return errorResponse;
            return new GetPollingResponse(Id, (double)asyncRes.Result.PollingIntervalSeconds * 1000);
        }

        [ExportField("CREATOR")]
        public static ParcelablePollingRequestCreator InitializeCreator()
        {
            return new ParcelablePollingRequestCreator();
        }

        public class ParcelablePollingRequestCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new GetPollingRequest(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
using System.Threading.Tasks;
using Android.OS;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using FreedomVoice.Core;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Requests
{
    /// <summary>
    /// Logout action
    /// </summary>
    public class LogoutRequest : BaseRequest
    {
        public LogoutRequest(long id) : base(id)
        {}

        public LogoutRequest(Parcel parcel) : base(parcel)
        {}

        public override async Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.Logout();
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;
            return new LogoutResponse(Id);
        }

        [ExportField("CREATOR")]
        public static ParcelableLogoutCreator InitializeCreator()
        {
            return new ParcelableLogoutCreator();
        }

        public class ParcelableLogoutCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new LogoutRequest(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
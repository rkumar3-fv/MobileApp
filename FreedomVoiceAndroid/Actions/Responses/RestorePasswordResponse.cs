using Android.OS;
using Android.Runtime;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Sucessful password restoration action
    /// <see href="https://api.freedomvoice.com/Help/Api/POST-api-v1-passwordReset">API - Restore password request</see>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class RestorePasswordResponse : BaseResponse
    {
        public RestorePasswordResponse(long requestId) : base(requestId)
        {}

        private RestorePasswordResponse(Parcel parcel) : base(parcel)
        {}

        [ExportField("CREATOR")]
        public static ParcelableRestoreResponseCreator InitializeCreator()
        {
            return new ParcelableRestoreResponseCreator();
        }

        public class ParcelableRestoreResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new RestorePasswordResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
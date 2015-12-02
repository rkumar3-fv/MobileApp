using Android.OS;
using Android.Runtime;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Response for logout request
    /// </summary>
    [Preserve(AllMembers = true)]
    public class LogoutResponse : BaseResponse
    {
        public LogoutResponse(long requestId) : base(requestId)
        { }

        private LogoutResponse(Parcel parcel) : base(parcel)
        { }

        [ExportField("CREATOR")]
        public static ParcelableLogoutResponseCreator InitializeCreator()
        {
            return new ParcelableLogoutResponseCreator();
        }

        public class ParcelableLogoutResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new LogoutResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
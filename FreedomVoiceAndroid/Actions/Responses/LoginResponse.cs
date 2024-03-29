using Android.OS;
using Android.Runtime;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Successful login action response
    /// <see href="https://api.freedomvoice.com/Help/Api/POST-api-v1-login">API - Login request</see>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class LoginResponse : BaseResponse
    {
        public LoginResponse(long requestId) : base(requestId)
        {}

        private LoginResponse(Parcel parcel) : base(parcel)
        {}

        [ExportField("CREATOR")]
        public static ParcelableLoginResponseCreator InitializeCreator()
        {
            return new ParcelableLoginResponseCreator();
        }

        public class ParcelableLoginResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new LoginResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
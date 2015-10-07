using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Successful login action response
    /// </summary>
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
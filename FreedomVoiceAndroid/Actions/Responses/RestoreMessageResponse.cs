using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Move from trash action response
    /// <see href="https://api.freedomvoice.com/Help/Api/POST-api-v1-systems-systemPhoneNumber-mailboxes-mailboxNumber-moveMessages">API - Move message request</see>
    /// </summary>
    public class RestoreMessageResponse : BaseResponse
    {
        public RestoreMessageResponse(long requestId) : base(requestId)
        { }

        private RestoreMessageResponse(Parcel parcel) : base(parcel)
        { }

        [ExportField("CREATOR")]
        public static ParcelableRestoreMessageResponseCreator InitializeCreator()
        {
            return new ParcelableRestoreMessageResponseCreator();
        }

        public class ParcelableRestoreMessageResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new RestoreMessageResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
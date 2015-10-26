using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Move message to trash action response
    /// <see href="https://api.freedomvoice.com/Help/Api/POST-api-v1-systems-systemPhoneNumber-mailboxes-mailboxNumber-moveMessages">API - Move message request</see>
    /// </summary>
    public class RemoveMessageResponse : BaseResponse
    {
        public RemoveMessageResponse(long requestId) : base(requestId)
        { }

        private RemoveMessageResponse(Parcel parcel) : base(parcel)
        { }

        [ExportField("CREATOR")]
        public static ParcelableRemoveMessageResponseCreator InitializeCreator()
        {
            return new ParcelableRemoveMessageResponseCreator();
        }

        public class ParcelableRemoveMessageResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new RemoveMessageResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
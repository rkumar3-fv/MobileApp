using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Move message to trash action response
    /// <see href="https://api.freedomvoice.com/Help/Api/POST-api-v1-systems-systemPhoneNumber-mailboxes-mailboxNumber-deleteMessages">API - Delete message request</see>
    /// </summary>
    public class DeleteMessageResponse : BaseResponse
    {
        public DeleteMessageResponse(long requestId) : base(requestId)
        { }

        private DeleteMessageResponse(Parcel parcel) : base(parcel)
        { }

        [ExportField("CREATOR")]
        public static ParcelableDeleteMessageResponseCreator InitializeCreator()
        {
            return new ParcelableDeleteMessageResponseCreator();
        }

        public class ParcelableDeleteMessageResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new DeleteMessageResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
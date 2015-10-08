using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Sucessful password restoration action
    /// </summary>
    public class RestoreResponse : BaseResponse
    {
        public RestoreResponse(long requestId) : base(requestId)
        {}

        private RestoreResponse(Parcel parcel) : base(parcel)
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
                return new RestoreResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
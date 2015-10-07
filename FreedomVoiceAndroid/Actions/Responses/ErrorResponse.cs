using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Response with error code
    /// </summary>
    public class ErrorResponse : BaseResponse
    {
        public const int ErrorBadRequest = 1;
        public const int ErrorCancelled = 2;
        public const int ErrorConnection = 3;
        public const int ErrorUnauthorized = 4;
        public const int ErrorUnknown = 0;

        public ErrorResponse(long requestId, int errorCode) : base(requestId)
        {
            ErrorCode = errorCode;
        }

        private ErrorResponse(Parcel parcel) : base(parcel)
        {
            ErrorCode = parcel.ReadInt();
        }

        /// <summary>
        /// Response error code
        /// </summary>
        public int ErrorCode { get; }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteInt(ErrorCode);
        }

        [ExportField("CREATOR")]
        public static ParcelableErrorResponseCreator InitializeCreator()
        {
            return new ParcelableErrorResponseCreator();
        }

        public class ParcelableErrorResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new ErrorResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
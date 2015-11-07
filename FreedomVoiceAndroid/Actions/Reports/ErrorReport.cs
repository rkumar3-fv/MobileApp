using Android.OS;
using FreedomVoice.Core.Entities.Enums;
using Java.Interop;
using Java.Lang;
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;

namespace com.FreedomVoice.MobileApp.Android.Actions.Reports
{
    /// <summary>
    /// Error in file loading
    /// </summary>
    public class ErrorReport : BaseReport
    {
        public const int ErrorBadRequest = 1;
        public const int ErrorCancelled = 2;
        public const int ErrorConnection = 3;
        public const int ErrorUnauthorized = 4;
        public const int ErrorNotFound = 5;
        public const int ErrorNotPaid = 6;
        public const int Forbidden = 7;
        public const int ErrorInternal = 8;
        public const int ErrorUnknown = 0;

        /// <summary>
        /// Error code
        /// </summary>
        public int ErrorCode { get; }

        public ErrorReport(int id, Message msg, int code) : base(id, msg)
        {
            ErrorCode = code;
        }

        public ErrorReport(int id, Message msg, ErrorCodes code) : base(id, msg)
        {
            switch (code)
            {
                case ErrorCodes.BadRequest:
                    ErrorCode = ErrorBadRequest;
                    break;
                case ErrorCodes.Cancelled:
                    ErrorCode = ErrorCancelled;
                    break;
                case ErrorCodes.ConnectionLost:
                    ErrorCode = ErrorConnection;
                    break;
                case ErrorCodes.Unauthorized:
                    ErrorCode = ErrorUnauthorized;
                    break;
                case ErrorCodes.NotFound:
                    ErrorCode = ErrorNotFound;
                    break;
                case ErrorCodes.PaymentRequired:
                    ErrorCode = ErrorNotPaid;
                    break;
                case ErrorCodes.Forbidden:
                    ErrorCode = Forbidden;
                    break;
                case ErrorCodes.InternalServerError:
                    ErrorCode = ErrorInternal;
                    break;
                default:
                    ErrorCode = ErrorUnknown;
                    break;
            }
        }

        private ErrorReport(Parcel parcel) : base(parcel)
        {
            ErrorCode = parcel.ReadInt();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteInt(ErrorCode);
        }

        [ExportField("CREATOR")]
        public static ParcelableErrorReportCreator InitializeCreator()
        {
            return new ParcelableErrorReportCreator();
        }

        public class ParcelableErrorReportCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new ErrorReport(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
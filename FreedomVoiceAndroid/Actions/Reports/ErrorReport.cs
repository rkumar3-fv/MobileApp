using Android.OS;
using Java.Interop;
using Java.Lang;

namespace com.FreedomVoice.MobileApp.Android.Actions.Reports
{
    /// <summary>
    /// Error in file loading
    /// </summary>
    public class ErrorReport : BaseReport
    {
        /// <summary>
        /// Error code
        /// </summary>
        public int ErrorCode { get; }

        public ErrorReport(int id, int code) : base(id)
        {
            ErrorCode = code;
        }

        public ErrorReport(Parcel parcel) : base(parcel)
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
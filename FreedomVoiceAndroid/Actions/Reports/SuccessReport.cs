using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;

namespace com.FreedomVoice.MobileApp.Android.Actions.Reports
{
    /// <summary>
    /// Success file operation
    /// </summary>
    public class SuccessReport : BaseReport
    {
        /// <summary>
        /// Path to file
        /// </summary>
        public string Path { get; }

        public SuccessReport(int id, Message msg, string path) : base(id, msg)
        {
            Path = path;
        }

        private SuccessReport(Parcel parcel) : base(parcel)
        {
            Path = parcel.ReadString();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(Path);
        }

        [ExportField("CREATOR")]
        public static ParcelableSuccessReportCreator InitializeCreator()
        {
            return new ParcelableSuccessReportCreator();
        }

        public class ParcelableSuccessReportCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new SuccessReport(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
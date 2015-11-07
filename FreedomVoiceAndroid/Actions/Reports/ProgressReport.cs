using Android.OS;
using Java.Interop;
using Java.Lang;
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;

namespace com.FreedomVoice.MobileApp.Android.Actions.Reports
{
    public class ProgressReport : BaseReport
    {
        /// <summary>
        /// Progress value
        /// </summary>
        public int ProgressValue { get; }

        public ProgressReport(int id, Message msg, int progress) : base(id, msg)
        {
            ProgressValue = progress;
        }

        private ProgressReport(Parcel parcel) : base(parcel)
        {
            ProgressValue = parcel.ReadInt();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteInt(ProgressValue);
        }

        [ExportField("CREATOR")]
        public static ParcelableProgressReportCreator InitializeCreator()
        {
            return new ParcelableProgressReportCreator();
        }

        public class ParcelableProgressReportCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new ProgressReport(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
using Android.OS;
using Java.Interop;
using Java.Lang;

namespace com.FreedomVoice.MobileApp.Android.Actions.Reports
{
    public class ProgressReport : BaseReport
    {
        /// <summary>
        /// Progress value
        /// </summary>
        public int ProgressValue { get; }

        public ProgressReport(int id, int progress) : base(id)
        {
            ProgressValue = progress;
        }

        public ProgressReport(Parcel parcel) : base(parcel)
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
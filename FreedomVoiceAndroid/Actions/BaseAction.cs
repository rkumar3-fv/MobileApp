using Android.OS;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions
{
    /// <summary>
    /// Abstract parcelable action
    /// </summary>
    public abstract class BaseAction : Object, IParcelable
    {
        public int DescribeContents()
        {
            return 0;
        }

        public abstract void WriteToParcel(Parcel dest, ParcelableWriteFlags flags);
    }
}
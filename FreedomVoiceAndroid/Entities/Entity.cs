using Android.OS;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    public abstract class Entity : Object, IParcelable
    {
        public int DescribeContents()
        {
            return 0;
        }

        public abstract void WriteToParcel(Parcel dest, ParcelableWriteFlags flags);
    }
}
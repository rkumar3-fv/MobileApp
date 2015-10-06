using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Abstract response action
    /// </summary>
    public abstract class BaseResponse : BaseAction
    {
        protected BaseResponse(Parcel parcel)
        {
            
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            
        }
    }
}
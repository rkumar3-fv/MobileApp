using System.Threading.Tasks;
using Android.OS;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;

namespace com.FreedomVoice.MobileApp.Android.Actions.Requests
{
    /// <summary>
    /// Abstract request action
    /// <see href="https://webservices.freedomvoice.com/FreedomAPI/FreedomAPI.asmx">FreedomAPI</see>
    /// </summary>
    public abstract class BaseRequest : BaseAction
    {
        private readonly long _id;

        protected BaseRequest(long id)
        {
            _id = id;
        }

        protected BaseRequest(Parcel parcel)
        {
            _id = parcel.ReadLong();
        }

        public abstract Task<BaseResponse> ExecuteRequest();

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            dest.WriteLong(_id);
        }
    }
}
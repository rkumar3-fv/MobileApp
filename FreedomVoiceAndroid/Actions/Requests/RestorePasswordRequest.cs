using System;
using System.Threading.Tasks;
using Android.OS;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Requests
{
    /// <summary>
    /// Restore password action
    /// </summary>
    public class RestorePasswordRequest : BaseRequest
    {
        private readonly string _email;

        public RestorePasswordRequest(long id, string email) : base(id)
        {
            _email = email;
        }

        public RestorePasswordRequest(Parcel parcel) : base(parcel)
        {
            _email = parcel.ReadString();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(_email);
        }

        public override Task<BaseResponse> ExecuteRequest()
        {
            throw new NotImplementedException();
        }

        [ExportField("CREATOR")]
        public static ParcelableResotreCreator InitializeCreator()
        {
            return new ParcelableResotreCreator();
        }

        public class ParcelableResotreCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new RestorePasswordRequest(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
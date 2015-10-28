using System;
using System.Threading.Tasks;
using Android.OS;
#if DEBUG
using Android.Util;
#endif
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using FreedomVoice.Core;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Requests
{
    /// <summary>
    /// Restore password action
    /// <see href="https://api.freedomvoice.com/Help/Api/POST-api-v1-passwordReset">API - Restore password request</see>
    /// </summary>
    public class RestorePasswordRequest : BaseRequest, IEquatable<RestorePasswordRequest>
    {
        private readonly string _email;

        public RestorePasswordRequest(long id, string email) : base(id)
        {
            _email = email;
        }

        private RestorePasswordRequest(Parcel parcel) : base(parcel)
        {
            _email = parcel.ReadString();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(_email);
        }

        public override async Task<BaseResponse> ExecuteRequest()
        {
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} executes request");
#endif
            var asyncRes = await ApiHelper.PasswordReset(_email);
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} GetResponse {(asyncRes == null ? "NULL" : "NOT NULL")}");
#endif
            if (asyncRes == null) return new ErrorResponse(Id, ErrorResponse.ErrorConnection);
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;
            return new RestorePasswordResponse(Id);
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

        public bool Equals(RestorePasswordRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && string.Equals(_email, other._email);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((RestorePasswordRequest) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (_email?.GetHashCode() ?? 0);
            }
        }
    }
}
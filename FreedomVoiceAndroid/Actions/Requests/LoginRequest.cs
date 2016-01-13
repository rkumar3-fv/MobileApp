using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using FreedomVoice.Core;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Requests
{
    /// <summary>
    /// Login action
    /// <see href="https://api.freedomvoice.com/Help/Api/POST-api-v1-login">API - Login request</see>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class LoginRequest : BaseRequest, IEquatable<LoginRequest>
    {
        private readonly string _login;
        private readonly string _password;

        public LoginRequest(long id, string login, string password) : base(id)
        {
            _login = login;
            _password = password;
        }

        private LoginRequest(Parcel parcel) : base(parcel)
        {
            _login = parcel.ReadString();
            _password = parcel.ReadString();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(_login);
            dest.WriteString(_password);
        }

        /// <summary>
        /// Execute login action async
        /// </summary>
        /// <returns>LoginResponse or ErrorResponse</returns>
        public override async Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.Login(_login, _password);
            if (asyncRes == null) return new ErrorResponse(Id, ErrorResponse.ErrorInternal, "Response is NULL");
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code, $"{asyncRes.HttpCode} - {asyncRes.JsonText}");
            if (errorResponse != null)
            {
                return errorResponse;
            }
            return new LoginResponse(Id);
        }

        [ExportField("CREATOR")]
        public static ParcelableLoginCreator InitializeCreator()
        {
            return new ParcelableLoginCreator();
        }
        
        public class ParcelableLoginCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new LoginRequest(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }

        public bool Equals(LoginRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && string.Equals(_login, other._login) && string.Equals(_password, other._password);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((LoginRequest) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (_login?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (_password?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
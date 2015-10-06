using System.Threading.Tasks;
using Android.OS;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Requests
{
    /// <summary>
    /// Login action
    /// <see href="https://webservices.freedomvoice.com/FreedomAPI/FreedomAPI.asmx?op=Login">FreedomAPI - login</see>
    /// </summary>
    public class LoginRequest : BaseRequest
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

        public override Task<BaseResponse> ExecuteRequest()
        {
            throw new System.NotImplementedException();
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
    }
}
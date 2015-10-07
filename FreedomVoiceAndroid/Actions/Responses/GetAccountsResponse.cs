using System.Collections.Generic;
using Android.OS;
using com.FreedomVoice.MobileApp.Android.Entities;
using Java.Interop;
using Java.Lang;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Accounts list response
    /// </summary>
    public class GetAccountsResponse : BaseResponse
    {
        public List<Account>AccountsList { get; }

        public GetAccountsResponse(long requestId, string[] accounts) : base(requestId)
        {
            AccountsList = new List<Account>();
            foreach (var account in accounts)
            {
                AccountsList.Add(new Account(account));
            }
        }

        public GetAccountsResponse(Parcel parcel) : base(parcel)
        {
            parcel.ReadList(AccountsList, ClassLoader.SystemClassLoader);
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteList(AccountsList);
        }

        [ExportField("CREATOR")]
        public static ParcelableAccountsResponseCreator InitializeCreator()
        {
            return new ParcelableAccountsResponseCreator();
        }

        public class ParcelableAccountsResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new GetAccountsResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
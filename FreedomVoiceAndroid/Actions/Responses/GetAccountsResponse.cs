using System;
using System.Collections.Generic;
using Android.OS;
using Android.Runtime;
using com.FreedomVoice.MobileApp.Android.Entities;
using Java.Interop;
using Java.Lang;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Accounts list response
    /// <see href="https://api.freedomvoice.com/Help/Api/GET-api-v1-systems">API - Get accounts request</see>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class GetAccountsResponse : BaseResponse, IEquatable<GetAccountsResponse>
    {
        public List<Account>AccountsList { get; }

        /// <summary>
        /// Response init for GetAccountsRequest
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="accounts">Accounts' numbers enum</param>
        public GetAccountsResponse(long requestId, IEnumerable<string> accounts) : base(requestId)
        {
            AccountsList = new List<Account>();
            foreach (var account in accounts)
            {
                AccountsList.Add(new Account(account, new List<string>()));
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

        public bool Equals(GetAccountsResponse other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(AccountsList, other.AccountsList);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((GetAccountsResponse) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (AccountsList?.GetHashCode() ?? 0);
            }
        }
    }
}
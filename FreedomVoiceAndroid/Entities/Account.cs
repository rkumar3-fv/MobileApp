using System;
using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Account entity
    /// </summary>
    public class Account : Entity, IEquatable<Account>
    {
        /// <summary>
        /// Account name
        /// </summary>
        public string AccountName { get; }

        public Account(string name)
        {
            AccountName = name;
        }

        private Account(Parcel parcel)
        {
            AccountName = parcel.ReadString();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            dest.WriteString(AccountName);
        }

        [ExportField("CREATOR")]
        public static ParcelableAccountCreator InitializeCreator()
        {
            return new ParcelableAccountCreator();
        }

        public class ParcelableAccountCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new Account(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }

        public bool Equals(Account other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || string.Equals(AccountName, other.AccountName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Account) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (AccountName?.GetHashCode() ?? 0);
            }
        }
    }
}
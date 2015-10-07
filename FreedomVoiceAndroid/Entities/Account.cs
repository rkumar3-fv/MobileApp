using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Account entity
    /// </summary>
    public class Account : Entity
    {
        /// <summary>
        /// Account name
        /// </summary>
        public string AccountName { get; set; }

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
    }
}
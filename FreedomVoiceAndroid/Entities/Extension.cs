using Android.OS;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Extension entity
    /// </summary>
    public class Extension : Entity
    {
        /// <summary>
        /// Extension name
        /// </summary>
        public string ExtensionName { get; set; }
        
        /// <summary>
        /// Mails counter
        /// </summary>
        public int MailsCount { get; set; }

        public Extension(string name, int count)
        {
            ExtensionName = name;
            MailsCount = count;
        }

        private Extension(Parcel parcel)
        {
            ExtensionName = parcel.ReadString();
            MailsCount = parcel.ReadInt();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            dest.WriteString(ExtensionName);
            dest.WriteInt(MailsCount);
        }

        [ExportField("CREATOR")]
        public static ParcelableExtensionCreator InitializeCreator()
        {
            return new ParcelableExtensionCreator();
        }

        public class ParcelableExtensionCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new Extension(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
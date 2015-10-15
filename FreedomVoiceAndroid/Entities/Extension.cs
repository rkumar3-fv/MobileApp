using System.Collections.Generic;
using Android.OS;
using Java.Interop;
using Java.Lang;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Extension entity
    /// </summary>
    public class Extension : MessageItem
    {
        /// <summary>
        /// Extension name
        /// </summary>
        public string ExtensionName { get; set; }
        
        /// <summary>
        /// Mails counter
        /// </summary>
        public int MailsCount { get; set; }

        /// <summary>
        /// Folders
        /// </summary>
        public List<Folder> Folders {get; set; } 

        public Extension(int id, string name, int count, List<Folder> folders ) : base (id)
        {
            ExtensionName = name;
            Folders = folders;
            MailsCount = count;
        }

        public Extension(int id, string name, int count) : this(id, name, count, new List<Folder>())
        { }

        private Extension(Parcel parcel) : base (parcel)
        {
            ExtensionName = parcel.ReadString();
            parcel.ReadList(Folders, ClassLoader.SystemClassLoader);
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(ExtensionName);
            dest.WriteList(Folders);
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
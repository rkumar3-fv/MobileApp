using System;
using System.Collections.Generic;
using Android.OS;
using Android.Runtime;
using Java.Interop;
using Java.Lang;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Extension entity
    /// </summary>
    [Preserve(AllMembers = true)]
    public class Extension : MessageItem, IEquatable<Extension>
    {
        /// <summary>
        /// Extension name
        /// </summary>
        public string ExtensionName { get; }
        
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
            MailsCount = parcel.ReadInt();
            parcel.ReadList(Folders, ClassLoader.SystemClassLoader);
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(ExtensionName);
            dest.WriteInt(MailsCount);
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

        public bool Equals(Extension other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && string.Equals(ExtensionName, other.ExtensionName) && MailsCount == other.MailsCount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Extension) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (ExtensionName?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
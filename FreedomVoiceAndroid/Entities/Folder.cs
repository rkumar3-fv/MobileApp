using System;
using System.Collections.Generic;
using Android.OS;
using Java.Interop;
using Java.Lang;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Folder entity
    /// </summary>
    public class Folder : MessageItem, IEquatable<Folder>
    {
        /// <summary>
        /// Folder name
        /// </summary>
        public string FolderName { get; }

        /// <summary>
        /// Unreaded mails
        /// </summary>
        public int MailsCount { get; set; }

        /// <summary>
        /// Messages in folder
        /// </summary>
        public List<Message> MessagesList { get; set; }

        public Folder(string name, int count, List<Message> messages) : base(0)
        {
            FolderName = name;
            MessagesList = messages;
            MailsCount = count;
        }

        public Folder(string name, int count) : this(name, count, new List<Message>())
        { }

        private Folder(Parcel parcel) : base(parcel)
        {
            FolderName = parcel.ReadString();
            parcel.ReadList(MessagesList, ClassLoader.SystemClassLoader);
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(FolderName);
            dest.WriteList(MessagesList);
        }

        [ExportField("CREATOR")]
        public static ParcelableFolderCreator InitializeCreator()
        {
            return new ParcelableFolderCreator();
        }

        public class ParcelableFolderCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new Folder(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }

        public bool Equals(Folder other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && string.Equals(FolderName, other.FolderName) && MailsCount == other.MailsCount;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Folder) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (FolderName?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
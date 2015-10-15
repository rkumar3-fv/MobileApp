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
    public class Folder : MessageItem
    {
        /// <summary>
        /// Folder name
        /// </summary>
        public string FolderName { get; set; }

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
    }
}
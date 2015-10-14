using System;
using System.Collections.Generic;
using Android.OS;
using Java.Interop;
using Java.Lang;
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Responses
{
    /// <summary>
    /// Messages list response
    /// </summary>
    public class GetMessagesResponse : BaseResponse, IEquatable<GetMessagesResponse>
    {
        /// <summary>
        /// Messages list
        /// </summary>
        public List<Message> MessagesList { get; }

        public GetMessagesResponse(long requestId, List<Message> messages) : base(requestId)
        {
            MessagesList = messages;
        }

        public GetMessagesResponse(Parcel parcel) : base(parcel)
        {
            parcel.ReadList(MessagesList, ClassLoader.SystemClassLoader);
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteList(MessagesList);
        }

        [ExportField("CREATOR")]
        public static ParcelableFoldersResponseCreator InitializeCreator()
        {
            return new ParcelableFoldersResponseCreator();
        }

        public class ParcelableFoldersResponseCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new GetFoldersResponse(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }

        public bool Equals(GetMessagesResponse other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(MessagesList, other.MessagesList);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((GetMessagesResponse) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (MessagesList?.GetHashCode() ?? 0);
            }
        }
    }
}
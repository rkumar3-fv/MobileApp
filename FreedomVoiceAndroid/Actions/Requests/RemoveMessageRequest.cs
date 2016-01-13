using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
#if DEBUG
using Android.Util;
#endif
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using FreedomVoice.Core;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Requests
{
    /// <summary>
    /// Move message to trash action
    /// <see href="https://api.freedomvoice.com/Help/Api/POST-api-v1-systems-systemPhoneNumber-mailboxes-mailboxNumber-moveMessages">API - Move message request</see>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class RemoveMessageRequest : BaseRequest, IEquatable<RemoveMessageRequest>
    {
        /// <summary>
        /// Account number
        /// </summary>
        public string Account { get; }

        /// <summary>
        /// Extension code
        /// </summary>
        public int Extension { get; }

        /// <summary>
        /// Message ID
        /// </summary>
        public string MessageName { get; }

        public RemoveMessageRequest(long id, string account, int extension, string message) : base(id)
        {
            Account = account;
            Extension = extension;
            MessageName = message;
        }

        protected RemoveMessageRequest(Parcel parcel) : base(parcel)
        {
            Account = parcel.ReadString();
            Extension = parcel.ReadInt();
            MessageName = parcel.ReadString();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(Account);
            dest.WriteInt(Extension);
            dest.WriteString(MessageName);
        }

        /// <summary>
        /// Execute remove message action async
        /// </summary>
        /// <returns>RemoveMessageResponse or ErrorResponse</returns>
        public override async Task<BaseResponse> ExecuteRequest()
        {
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} executes request");
#endif
            var asyncRes = await ApiHelper.MoveMessages(Account, Extension, "Trash", new List<string> {MessageName});
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} GetResponse {(asyncRes == null ? "NULL" : "NOT NULL")}");
#endif
            if (asyncRes == null) return new ErrorResponse(Id, ErrorResponse.ErrorInternal, "Response is NULL");
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code, asyncRes.JsonText);
            if (errorResponse != null)
                return errorResponse;
            return new RemoveMessageResponse(Id);
        }

        [ExportField("CREATOR")]
        public static ParcelableRemoveMessageCreator InitializeCreator()
        {
            return new ParcelableRemoveMessageCreator();
        }

        public class ParcelableRemoveMessageCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new RemoveMessageRequest(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }

        public bool Equals(RemoveMessageRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && string.Equals(Account, other.Account) && Extension == other.Extension && string.Equals(MessageName, other.MessageName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((RemoveMessageRequest) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (Account?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ Extension;
                hashCode = (hashCode*397) ^ (MessageName?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
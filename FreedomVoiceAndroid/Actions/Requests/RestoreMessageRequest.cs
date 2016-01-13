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
    /// Restore message from trash action
    /// <see href="https://api.freedomvoice.com/Help/Api/POST-api-v1-systems-systemPhoneNumber-mailboxes-mailboxNumber-moveMessages">API - Move message request</see>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class RestoreMessageRequest : RemoveMessageRequest, IEquatable<RestoreMessageRequest>
    {
        /// <summary>
        /// Folder for restoration
        /// </summary>
        public string Folder { get; }

        public RestoreMessageRequest(long id, string account, int extension, string message, string folder) : base(id, account, extension, message)
        {
            Folder = folder;
        }

        private RestoreMessageRequest(Parcel parcel) : base(parcel)
        {
            Folder = parcel.ReadString();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(Folder);
        }

        /// <summary>
        /// Execute restore message action async
        /// </summary>
        /// <returns>RestoreMessageResponse or ErrorResponse</returns>
        public override async Task<BaseResponse> ExecuteRequest()
        {
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} executes request");
#endif
            var asyncRes = await ApiHelper.MoveMessages(Account, Extension, Folder, new List<string> { MessageName });
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} GetResponse {(asyncRes == null ? "NULL" : "NOT NULL")}");
#endif
            if (asyncRes == null) return new ErrorResponse(Id, ErrorResponse.ErrorInternal, "Response is NULL");
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code, asyncRes.JsonText);
            if (errorResponse != null)
                return errorResponse;
            return new RestoreMessageResponse(Id);
        }

        [ExportField("CREATOR")]
        public static ParcelableRestoreMessageCreator InitializeChildCreator()
        {
            return new ParcelableRestoreMessageCreator();
        }

        public class ParcelableRestoreMessageCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new RestoreMessageRequest(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }

        public bool Equals(RestoreMessageRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && string.Equals(Folder, other.Folder);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((RestoreMessageRequest) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (Folder?.GetHashCode() ?? 0);
            }
        }
    }
}
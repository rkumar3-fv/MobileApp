using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
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
    /// Delete message action
    /// <see href="https://api.freedomvoice.com/Help/Api/POST-api-v1-systems-systemPhoneNumber-mailboxes-mailboxNumber-deleteMessages">API - Delete message request</see>
    /// </summary>
    public class DeleteMessageRequest : RemoveMessageRequest
    {
        public DeleteMessageRequest(long id, string account, int extension, string message) : base(id, account, extension, message)
        {}

        protected DeleteMessageRequest(Parcel parcel) : base(parcel)
        { }

        /// <summary>
        /// Execute delete message action async
        /// </summary>
        /// <returns>DeleteMessageResponse or ErrorResponse</returns>
        public override async Task<BaseResponse> ExecuteRequest()
        {
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} executes request");
#endif
            var asyncRes = await ApiHelper.DeleteMessages(Account, Extension, new List<string> { MessageName });
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} GetResponse {(asyncRes == null ? "NULL" : "NOT NULL")}");
#endif
            if (asyncRes == null) return new ErrorResponse(Id, ErrorResponse.ErrorInternal);
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;
            return new DeleteMessageResponse(Id);
        }

        [ExportField("CREATOR")]
        public static ParcelableDeleteMessageCreator InitializeChildCreator()
        {
            return new ParcelableDeleteMessageCreator();
        }

        public class ParcelableDeleteMessageCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new DeleteMessageRequest(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
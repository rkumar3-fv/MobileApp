using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
#if DEBUG
using Android.Util;
#endif
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities.Enums;
using Java.Interop;
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Requests
{
    /// <summary>
    /// Messages list request
    /// <see href="https://api.freedomvoice.com/Help/Api/GET-api-v1-systems-systemPhoneNumber-mailboxes-mailboxNumber-folders-folderName-messages_PageSize_PageNumber_SortAsc">API - Get messages request</see>
    /// </summary>
    public class GetMessagesRequest : BaseRequest
    {
        /// <summary>
        /// Used account name
        /// </summary>
        public string AccountName { get; }

        /// <summary>
        /// Used extension ID
        /// </summary>
        public int ExtensionId { get; }

        public string Folder { get; }

        public GetMessagesRequest(long id, string account, int extensionId, string folder) : base(id)
        {
            AccountName = account;
            ExtensionId = extensionId;
            Folder = folder;
        }

        private GetMessagesRequest(Parcel parcel) : base(parcel)
        {
            AccountName = parcel.ReadString();
            ExtensionId = parcel.ReadInt();
            Folder = parcel.ReadString();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(AccountName);
            dest.WriteInt(ExtensionId);
            dest.WriteString(Folder);
        }

        public async override Task<BaseResponse> ExecuteRequest()
        {
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} REQUESTS MESSAGES FOR x{ExtensionId} from {Folder}");
#endif
            var asyncRes = await ApiHelper.GetMesages(AccountName, ExtensionId, Folder, 30, 1, false);
#if DEBUG
            Log.Debug(App.AppPackage, $"{GetType().Name} GetResponse {(asyncRes == null ? "NULL" : "NOT NULL")}");
#endif
            if (asyncRes == null) return new ErrorResponse(Id, ErrorResponse.ErrorConnection);
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;
            var listMsg = asyncRes.Result;
            var resList = new List<Message>();
            foreach (var message in listMsg)
            {
                int type;
                switch (message.Type)
                {
                    case MessageType.Fax:
                        type = Message.TypeFax;
                        break;
                    case MessageType.Recording:
                        type = Message.TypeRec;
                        break;
                    case MessageType.Voicemail:
                        type = Message.TypeVoice;
                        break;
                    default:
                        type = -1;
                        break;
                }
                resList.Add(new Message(Convert.ToInt32(message.Id.Substring(1)), 
                    message.Id, 
                    message.SourceName,
                    message.SourceNumber,
                    message.ReceivedOn,
                    type,
                    message.Unread,
                    message.Length));
            }
            return new GetMessagesResponse(Id, resList);
        }

        [ExportField("CREATOR")]
        public static ParcelableMessagesRequestCreator InitializeCreator()
        {
            return new ParcelableMessagesRequestCreator();
        }

        public class ParcelableMessagesRequestCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new GetMessagesRequest(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
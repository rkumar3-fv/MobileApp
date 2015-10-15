using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using com.FreedomVoice.MobileApp.Android.Entities;
using FreedomVoice.Core;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Requests
{
    /// <summary>
    /// Folders list request
    /// <see href="https://api.freedomvoice.com/Help/Api/GET-api-v1-systems-systemPhoneNumber-mailboxes-mailboxNumber-folders">API - Get folders request</see>
    /// </summary>
    public class GetFoldersRequest : BaseRequest
    {
        /// <summary>
        /// Used account name
        /// </summary>
        public string AccountName { get; }

        /// <summary>
        /// Used extension ID
        /// </summary>
        public int ExtensionId { get; }

        public GetFoldersRequest(long id, string account, int extensionId) : base(id)
        {
            AccountName = account;
            ExtensionId = extensionId;
        }

        private GetFoldersRequest(Parcel parcel) : base(parcel)
        {
            AccountName = parcel.ReadString();
            ExtensionId = parcel.ReadInt();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(AccountName);
            dest.WriteInt(ExtensionId);
        }

        public async override Task<BaseResponse> ExecuteRequest()
        {
            Log.Debug(App.AppPackage, $"REQUEST FOLDERS FOR x{ExtensionId}");
            var asyncRes = await ApiHelper.GetFolders(AccountName, ExtensionId);
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;
            var listFold = asyncRes.Result;
            var resList = listFold.Select(folder => new Folder(folder.Name, folder.MessageCount)).ToList();
            return new GetFoldersResponse(Id, resList);
        }

        [ExportField("CREATOR")]
        public static ParcelableFoldersRequestCreator InitializeCreator()
        {
            return new ParcelableFoldersRequestCreator();
        }

        public class ParcelableFoldersRequestCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new GetFoldersRequest(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
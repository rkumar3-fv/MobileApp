using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using com.FreedomVoice.MobileApp.Android.Entities;
using FreedomVoice.Core;
using Java.Interop;
using Object = Java.Lang.Object;

namespace com.FreedomVoice.MobileApp.Android.Actions.Requests
{
    /// <summary>
    /// Folders list request
    /// <see href="https://api.freedomvoice.com/Help/Api/GET-api-v1-systems-systemPhoneNumber-mailboxes-mailboxNumber-foldersWithCounts">API - Get folders with count request</see>
    /// </summary>
    [Preserve(AllMembers = true)]
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

        public override async Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.GetFoldersWithCount(AccountName, ExtensionId);
            if (asyncRes == null) return new ErrorResponse(Id, ErrorResponse.ErrorInternal, "Response is NULL");
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code, asyncRes.JsonText);
            if (errorResponse != null)
                return errorResponse;
            var listFold = asyncRes.Result;
            var resList = listFold.Select(folder => new Folder(folder.Name, folder.UnreadMessages, folder.MessageCount)).ToList();
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
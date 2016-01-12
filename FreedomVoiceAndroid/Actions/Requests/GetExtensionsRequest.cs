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
    /// Extensions list request
    /// <see href="https://api.freedomvoice.com/Help/Api/GET-api-v1-systems-systemPhoneNumber-mailboxesWithCounts">API - Get extensions request</see>
    /// </summary>
    [Preserve(AllMembers = true)]
    public class GetExtensionsRequest : BaseRequest
    {
        /// <summary>
        /// Used account name
        /// </summary>
        public string AccountName { get; }

        public GetExtensionsRequest(long id, string account) : base(id)
        {
            AccountName = account;
        }

        private GetExtensionsRequest(Parcel parcel) : base(parcel)
        {
            AccountName = parcel.ReadString();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(AccountName);
        }

        public override async Task<BaseResponse> ExecuteRequest()
        {
            var asyncRes = await ApiHelper.GetMailboxesWithCounts(AccountName);
            if (asyncRes == null) return new ErrorResponse(Id, ErrorResponse.ErrorInternal);
            var errorResponse = CheckErrorResponse(Id, asyncRes.Code);
            if (errorResponse != null)
                return errorResponse;
            var listExt = asyncRes.Result;
            var resList = listExt.Select(mailboxWithCount => new Extension(mailboxWithCount.MailboxNumber, mailboxWithCount.DisplayName, mailboxWithCount.UnreadMessages)).ToList();
            return new GetExtensionsResponse(Id, resList);
        }

        [ExportField("CREATOR")]
        public static ParcelableExtensionsRequestCreator InitializeCreator()
        {
            return new ParcelableExtensionsRequestCreator();
        }

        public class ParcelableExtensionsRequestCreator : Object, IParcelableCreator
        {
            public Object CreateFromParcel(Parcel source)
            {
                return new GetExtensionsRequest(source);
            }

            public Object[] NewArray(int size)
            {
                return new Object[size];
            }
        }
    }
}
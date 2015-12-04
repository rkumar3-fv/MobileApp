using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.Services
{
    public interface IMessagesService
    {
        /// <summary>
        /// Asynchronous retrieving of messages
        /// </summary>
        Task<BaseResponse> ExecuteRequest(string systemNumber, int mailboxNumber, string folderName, List<Contact> contactList);
    }
}
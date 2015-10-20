using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.Core.Entities;
using FreedomVoice.Core.Entities.Base;

namespace FreedomVoice.iOS.Data
{
    public interface IExtensionsService
    {
        /// <summary>
        /// Asynchronous retrieving of mailboxes
        /// </summary>
        Task<BaseResult<List<MailboxWithCount>>> GetMailboxesWithCountsAsync(string systemPhoneNumber);
    }
}
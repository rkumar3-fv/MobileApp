using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities;
using FreedomVoice.Core.Entities.Base;

namespace FreedomVoice.iOS.Data.Implementations
{
    public class ExtensionsService : IExtensionsService
    {
        public Task<BaseResult<List<MailboxWithCount>>> GetMailboxesWithCountsAsync(string systemPhoneNumber)
        {
            return Task.Factory.StartNew(async () => await ApiHelper.GetMailboxesWithCounts(systemPhoneNumber)).Unwrap();
        }
    }
}
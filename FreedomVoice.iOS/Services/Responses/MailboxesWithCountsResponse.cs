using System.Collections.Generic;
using FreedomVoice.Core.Entities;
using FreedomVoice.iOS.Entities;

namespace FreedomVoice.iOS.Services.Responses
{
    public class MailboxesWithCountsResponse : BaseResponse
    {
        public List<ExtensionsWithCount> ExtensionsWithCount { get; }

        /// <summary>
        /// Response init for ExtensionsService
        /// </summary>
        /// <param name="mailboxesWithCount">Mailboxes With Count</param>
        public MailboxesWithCountsResponse(IEnumerable<MailboxWithCount> mailboxesWithCount)
        {
            ExtensionsWithCount = new List<ExtensionsWithCount>();

            foreach (var mailbox in mailboxesWithCount)
                ExtensionsWithCount.Add(new ExtensionsWithCount(mailbox));
        }
    }
}
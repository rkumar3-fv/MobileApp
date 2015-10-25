using System.Collections.Generic;
using FreedomVoice.Core.Entities;
using FreedomVoice.iOS.Entities;

namespace FreedomVoice.iOS.Services.Responses
{
    public class ExtensionsWithCountResponse : BaseResponse
    {
        public List<ExtensionWithCount> ExtensionsWithCount { get; }

        /// <summary>
        /// Response init for ExtensionsService
        /// </summary>
        /// <param name="mailboxesWithCount">Mailboxes With Count</param>
        public ExtensionsWithCountResponse(IEnumerable<MailboxWithCount> mailboxesWithCount)
        {
            ExtensionsWithCount = new List<ExtensionWithCount>();

            foreach (var mailbox in mailboxesWithCount)
                ExtensionsWithCount.Add(new ExtensionWithCount(mailbox));
        }
    }
}
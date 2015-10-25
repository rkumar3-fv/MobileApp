using FreedomVoice.Core.Entities;

namespace FreedomVoice.iOS.Entities
{
    public class ExtensionWithCount
    {
        public ExtensionWithCount(MailboxWithCount mailboxWithCount)
        {
            ExtensionNumber = mailboxWithCount.MailboxNumber;
            DisplayName = mailboxWithCount.DisplayName;
            UnreadMessagesCount = mailboxWithCount.UnreadMessages;
        }

        public int ExtensionNumber { get; set; }

        public string DisplayName { get; set; }

        public int UnreadMessagesCount { get; set; }
    }
}
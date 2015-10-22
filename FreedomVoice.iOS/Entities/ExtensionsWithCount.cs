using FreedomVoice.Core.Entities;

namespace FreedomVoice.iOS.Entities
{
    public class ExtensionsWithCount
    {
        public ExtensionsWithCount(MailboxWithCount mailboxWithCount)
        {
            MailboxNumber = mailboxWithCount.MailboxNumber;
            DisplayName = mailboxWithCount.DisplayName;
            UnreadMessagesCount = mailboxWithCount.UnreadMessages;
        }

        public int MailboxNumber { get; set; }

        public string DisplayName { get; set; }

        public int UnreadMessagesCount { get; set; }
    }
}
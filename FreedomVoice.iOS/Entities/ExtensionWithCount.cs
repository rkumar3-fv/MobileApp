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

        public int ExtensionNumber { get; private set; }

        public string DisplayName { get; private set; }

        public int UnreadMessagesCount { get; private set; }
    }
}
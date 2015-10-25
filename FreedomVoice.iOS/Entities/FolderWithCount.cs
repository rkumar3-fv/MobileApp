using FreedomVoice.Core.Entities;

namespace FreedomVoice.iOS.Entities
{
    public class FolderWithCount
    {
        public FolderWithCount(MessageFolderWithCounts messageFolder)
        {
            MessageCount = messageFolder.MessageCount;
            DisplayName = messageFolder.Name;
            UnreadMessagesCount = messageFolder.UnreadMessages;
        }

        public int MessageCount { get; set; }

        public string DisplayName { get; set; }

        public int UnreadMessagesCount { get; set; }
    }
}

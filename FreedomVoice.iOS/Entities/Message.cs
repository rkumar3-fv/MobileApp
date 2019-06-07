using System;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;
using ContactsHelper = FreedomVoice.iOS.Core.Utilities.Helpers.Contacts;

namespace FreedomVoice.iOS.Entities
{
    public class Message
    {
        public Message(FreedomVoice.Core.Entities.Message message)
        {
            Id = message.Id;
            Name = message.Name;
            Folder = message.Folder;
            SourceName = message.SourceName.Trim();
            SourceNumber = message.SourceNumber.Trim();
            Length = message.Length;
            Mailbox = message.Mailbox;
            ReceivedOn = message.ReceivedOn;
            Type = message.Type;
            Unread = message.Unread;

            Title = GetMessageTitle();
        }

        public string Title { get; private set; }
        public string Id { get; private set; }
        public int Length { get; private set; }
        public string SourceNumber { get; }
        public DateTime ReceivedOn { get; private set; }
        public MessageType Type { get; private set; }
        public bool Unread { get; set; }
        public string Folder { get; private set; }
        public int Mailbox { get; private set; }

        public string Name { get; set; }

        private string SourceName { get; }

        private string GetMessageTitle()
        {
            if (!string.IsNullOrEmpty(SourceNumber))
            {
                var contact = ContactsHelper.FindContactByNumber(SourceNumber);
                if (!string.IsNullOrEmpty(contact?.DisplayName))
                    return contact.DisplayName;
            }

            if (!string.IsNullOrEmpty(SourceName))
                return SourceName;

            return !string.IsNullOrEmpty(SourceNumber) ? ServiceContainer.Resolve<IPhoneFormatter>().Format(SourceNumber) : "Unavailable";
        }
    }
}
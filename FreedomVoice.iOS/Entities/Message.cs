using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.Core.Utils;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.Entities
{
    public class Message
    {
        public Message(Core.Entities.Message message, List<Contact> contactList)
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

            ContactList = contactList;
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
        private List<Contact> ContactList { get; }

        private string GetMessageTitle()
        {
            if (!string.IsNullOrEmpty(SourceNumber))
            {
                var contact = FindContactByNumber(SourceNumber);
                if (contact != null)
                    return contact.DisplayName;
            }

            if (!string.IsNullOrEmpty(SourceName))
                return SourceName;

            return !string.IsNullOrEmpty(SourceNumber) ? DataFormatUtils.ToPhoneNumber(SourceNumber) : "Unavailable";
        }

        private Contact FindContactByNumber(string number)
        {
            var numberToCompareWith = Regex.Replace(number, @"[^\d]", "");

            return ContactList?.FirstOrDefault(c => c.Phones.Any(p => Regex.Replace(p.Number, @"[^\d]", "").EndsWith(numberToCompareWith)));
        }
    }
}
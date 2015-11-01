﻿using Foundation;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Utilities.Extensions;

namespace FreedomVoice.iOS.Entities
{
    public class Message
    {
        public Message(Core.Entities.Message message)
        {
            Id = message.Id;
            Name = message.Name;
            Folder = message.Folder;
            SourceName = message.SourceName;
            SourceNumber = message.SourceNumber;
            Length = message.Length;
            Mailbox = message.Mailbox;
            ReceivedOn = message.ReceivedOn.ToNSDate();
            Type = message.Type;
            Unread = message.Unread;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Folder { get; set; }
        public string SourceName { get; set; }
        public string SourceNumber { get; set; }

        public int Length { get; set; }
        public int Mailbox { get; set; }

        public NSDate ReceivedOn { get; set; }

        public MessageType Type { get; set; }

        public bool Unread { get; set; }
    }
}
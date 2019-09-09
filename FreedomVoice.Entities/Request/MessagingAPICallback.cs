using FreedomVoice.Entities.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FreedomVoice.Entities.Request
{
    public class MessagingAPICallback
    {
        [JsonProperty(Required = Required.Always)]
        public string From { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string To { get; set; }

        [JsonProperty(Required = Required.Always)]
        public EventType EventType { get; set; }

        public string Text { get; set; }

        public ICollection<Uri> Media { get; set; }

        [JsonProperty(Required = Required.Always)]
        public MessageDirection Direction { get; set; }

        [JsonProperty(Required = Required.Always)]
        public MessageState State { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DateTime Time { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string MessageId { get; set; }

        public string Tag { get; set; }

        public string ApplicationNumber { get; set; }
    }
}

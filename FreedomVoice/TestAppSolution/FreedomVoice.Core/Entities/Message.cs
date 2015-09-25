namespace FreedomVoice.Core.Entities
{
    using System;
    using Enums;
    using Newtonsoft.Json;

    public class Message
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("MessageType")]
        public MessageType Type { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Length")]
        public int Length { get; set; }

        [JsonProperty("SourceName")]
        public string SourceName { get; set; }

        [JsonProperty("SourceNumber")]
        public string SourceNumber { get; set; }

        [JsonProperty("ReceivedOn")]
        public DateTime ReceivedOn { get; set; }

        [JsonProperty("Unread")]
        public bool Unread { get; set; }

        [JsonProperty("Mailbox")]
        public int Mailbox { get; set; }

        [JsonProperty("Folder")]
        public string Folder { get; set; }
    }
}

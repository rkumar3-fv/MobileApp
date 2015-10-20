namespace FreedomVoice.Core.Entities
{
    using Newtonsoft.Json;

    public class MessageFolderWithCounts
    {
        [JsonProperty("MessageCount")]
        public int MessageCount { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("UnreadMessages")]
        public int UnreadMessages { get; set; }
    }
}

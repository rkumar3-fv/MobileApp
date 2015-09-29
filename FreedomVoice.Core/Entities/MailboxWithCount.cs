namespace FreedomVoice.Core.Entities
{
    using Newtonsoft.Json;

    public class MailboxWithCount
    {
        [JsonProperty("MailboxNumber")]
        public int MailboxNumber { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("UnreadMessages")]
        public int UnreadMessages { get; set; }
    }
}

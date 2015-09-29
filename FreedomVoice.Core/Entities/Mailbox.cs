namespace FreedomVoice.Core.Entities
{
    using Newtonsoft.Json;

    public class Mailbox
    {
        [JsonProperty("MailboxNumber")]
        public int MailboxNumber { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }
    }
}

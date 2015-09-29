namespace FreedomVoice.Core.Entities.Enums
{
    using Newtonsoft.Json;

    public enum MessageType
    {
        [JsonProperty("Voicemail")]
        Voicemail,
        [JsonProperty("Fax")]
        Fax,
        [JsonProperty("Recording")]
        Recording
    }
}
namespace FreedomVoice.Core.Entities
{
    using Newtonsoft.Json;

    public class PollingInterval
    {
        [JsonProperty("PollingIntervalSeconds")]
        public int PollingIntervalSeconds { get; set; }
    }
}

namespace FreedomVoice.Core.Entities
{
    using Newtonsoft.Json;

    public class Folder
    {
        [JsonProperty("MessageCount")]
        public int MessageCount { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }
}

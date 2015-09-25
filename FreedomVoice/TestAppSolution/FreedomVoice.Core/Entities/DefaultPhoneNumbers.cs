namespace FreedomVoice.Core.Entities
{
    using Newtonsoft.Json;

    public class DefaultPhoneNumbers
    {
        [JsonProperty("DefaultPhoneNumbers")]
        public string[] PhoneNumbers { get; set; }
    }
}

namespace FreedomVoice.Core.Entities
{
    using Newtonsoft.Json;

    public class PresentationPhoneNumbers
    {
        [JsonProperty("PresentationPhoneNumbers")]
        public string[] PhoneNumbers { get; set; }
    }
}

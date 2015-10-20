namespace FreedomVoice.Core.Entities
{
    using Newtonsoft.Json;

    public class CreateCallReservationSetting
    {
        [JsonProperty("SwitchboardPhoneNumber")]
        public string SwitchboardPhoneNumber { get; set; }
    }
}

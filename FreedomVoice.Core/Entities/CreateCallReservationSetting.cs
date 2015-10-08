namespace FreedomVoice.Core.Entities
{
    using Newtonsoft.Json;

    public class CreateCallReservationSetting
    {
        [JsonProperty("CreateCallReservationSetting")]
        public string CallReservationSetting { get; set; }
    }
}

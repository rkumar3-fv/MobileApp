using Newtonsoft.Json;

namespace FreedomVoice.Entities.PushContract
{
    public class PushNotification<T>
    {
        public Aps Aps { get; set; }
        public T Data { get; set; }
    }

    public class Aps
    {
        public Alert Alert { get; set; }
        [JsonProperty("mutable-content")]
        public int MutableContent { get; set; }
        [JsonProperty("content-available")]
        public int ContentAvailable { get; set; }
        public int Badge { get; set; }
        public string Sound { get; set; }
    }

    public class Alert
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Body { get; set; }
    }

}



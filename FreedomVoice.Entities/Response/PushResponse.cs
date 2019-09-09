using FreedomVoice.Entities.Enums;

namespace FreedomVoice.Entities.Response
{
    public class PushResponse<T>
    {
        public T Data { get; set; }
        public PushType PushType { get; set; }
    }
}

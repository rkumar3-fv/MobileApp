using FreedomVoice.Entities.Enums;

namespace FreedomVoice.Entities.Response
{
    public class BaseResponse
    {
        public SendingState State { get; set; }
        public string ErrorMessage { get; set; }
    }
}

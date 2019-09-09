namespace FreedomVoice.Entities.Response
{
    public class SendingResponse<T> : BaseResponse
    {
        public T Entity { get; set; }
    }
}

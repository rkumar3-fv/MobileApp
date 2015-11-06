namespace FreedomVoice.iOS.Services.Responses
{
    public class MessageOperationsResponse : BaseResponse
    {
        public string Result { private set; get; }

        /// <summary>
        /// Response init for MessageOperationsService
        /// </summary>
        /// <param name="responseResult">Response result</param>
        public MessageOperationsResponse(string responseResult)
        {
            Result = responseResult;
        }
    }
}
namespace FreedomVoice.iOS.Services.Responses
{
    public class GetMediaResponse : BaseResponse
    {
        public string FilePath { get; }

        /// <summary>
        /// Response init for MediaService
        /// </summary>
        /// <param name="filePath">Existing file path</param>
        public GetMediaResponse(string filePath)
        {
            FilePath = filePath;
        }
    }
}
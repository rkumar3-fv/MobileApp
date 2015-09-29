namespace FreedomVoice.Core.Entities.Base
{
    using System.Net;

    public class BaseResult<T>
    {
        public HttpStatusCode Code { get; set; }

        public T Result { get; set; }
    }
}

using System.IO;

namespace FreedomVoice.Core.Entities
{
    public class MediaResponse
    {
        public Stream ReceivedStream { get; }

        public long Length { get; }

        public MediaResponse(long length, Stream stream)
        {
            Length = length;
            ReceivedStream = stream;
        }
    }
}
